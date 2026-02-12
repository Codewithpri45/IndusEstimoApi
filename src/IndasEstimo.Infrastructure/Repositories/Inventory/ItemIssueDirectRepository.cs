using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Domain.Entities.Inventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Application.Interfaces.Services;
using Dapper;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class ItemIssueDirectRepository : IItemIssueDirectRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ItemIssueDirectRepository> _logger;

    public ItemIssueDirectRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ItemIssueDirectRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // ==================== CRUD Operations ====================

    public async Task<long> SaveItemIssueDirectAsync(
        ItemIssueDirectMain main,
        List<ItemIssueDirectDetail> details,
        List<ItemIssueDirectConsumeMain>? consumeMain,
        List<ItemIssueDirectConsumeDetail>? consumeDetails)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Insert Main record (Audit fields handled by DbOperationsService)
            var transactionId = await _dbOperations.InsertDataAsync(
                "ItemTransactionMain",
                main,
                connection,
                transaction,
                "TransactionID");

            // 2. Insert Detail records
            await _dbOperations.InsertDataAsync(
                "ItemTransactionDetail",
                details,
                connection,
                transaction,
                "TransactionDetailID",
                parentTransactionId: transactionId);

            // 3. Insert Consume Main records (if present)
            if (consumeMain != null && consumeMain.Any())
            {
                await _dbOperations.InsertDataAsync(
                    "ItemTransactionConsume",
                    consumeMain,
                    connection,
                    transaction,
                    "ConsumeID",
                    parentTransactionId: transactionId);
            }

            // 4. Insert Consume Detail records (if present)
            if (consumeDetails != null && consumeDetails.Any())
            {
                await _dbOperations.InsertDataAsync(
                    "ItemTransactionConsumeDetail",
                    consumeDetails,
                    connection,
                    transaction,
                    "ConsumeDetailID",
                    parentTransactionId: transactionId);
            }

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving item issue direct");
            throw;
        }
    }

    public async Task<long> UpdateItemIssueDirectAsync(
        long transactionId,
        ItemIssueDirectMain main,
        List<ItemIssueDirectDetail> details,
        List<ItemIssueDirectConsumeMain>? consumeMain,
        List<ItemIssueDirectConsumeDetail>? consumeDetails)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            // 1. Update main record
            main.TransactionID = transactionId;
            main.ModifiedBy = _currentUserService.GetUserId() ?? 0;
            main.ProductionUnitID = _currentUserService.GetProductionUnitId() ?? 0;
            main.CompanyID = companyId;

            await _dbOperations.UpdateDataAsync(
                "ItemTransactionMain",
                main,
                connection,
                transaction,
                new[] { "TransactionID", "CompanyID" });

            // 2. Delete existing details and related records
            await connection.ExecuteAsync(
                "DELETE FROM ItemTransactionDetail WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId",
                new { TransactionID = transactionId, CompanyId = companyId },
                transaction);

            await connection.ExecuteAsync(
                "DELETE FROM ItemTransactionConsume WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId",
                new { TransactionID = transactionId, CompanyId = companyId },
                transaction);

            await connection.ExecuteAsync(
                "DELETE FROM ItemTransactionConsumeDetail WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId",
                new { TransactionID = transactionId, CompanyId = companyId },
                transaction);

            // 3. Re-insert everything
            await _dbOperations.InsertDataAsync(
                "ItemTransactionDetail",
                details,
                connection,
                transaction,
                "TransactionDetailID",
                parentTransactionId: transactionId);

            if (consumeMain != null && consumeMain.Any())
            {
                await _dbOperations.InsertDataAsync(
                    "ItemTransactionConsume",
                    consumeMain,
                    connection,
                    transaction,
                    "ConsumeID",
                    parentTransactionId: transactionId);
            }

            if (consumeDetails != null && consumeDetails.Any())
            {
                await _dbOperations.InsertDataAsync(
                    "ItemTransactionConsumeDetail",
                    consumeDetails,
                    connection,
                    transaction,
                    "ConsumeDetailID",
                    parentTransactionId: transactionId);
            }

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating item issue direct {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<bool> DeleteItemIssueDirectAsync(long transactionId, long? jobContID)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // 1. Soft Delete Main
            string deleteMainSql = @"
                UPDATE ItemTransactionMain
                SET DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(
                deleteMainSql,
                new { DeletedBy = userId, TransactionID = transactionId, CompanyID = companyId },
                transaction);

            // 2. Soft Delete Details
            string deleteDetailSql = @"
                UPDATE ItemTransactionDetail
                SET IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(
                deleteDetailSql,
                new { TransactionID = transactionId, CompanyID = companyId },
                transaction);

            // 3. Soft Delete Consume records if present
            string deleteConsumeSql = @"
                UPDATE ItemTransactionConsume
                SET IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(
                deleteConsumeSql,
                new { TransactionID = transactionId, CompanyID = companyId },
                transaction);

            string deleteConsumeDetailSql = @"
                UPDATE ItemTransactionConsumeDetail
                SET IsDeletedTransaction = 1
                WHERE TransactionID = @TransactionID
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(
                deleteConsumeDetailSql,
                new { TransactionID = transactionId, CompanyID = companyId },
                transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting item issue direct {TransactionID}", transactionId);
            throw;
        }
    }

    // ==================== Voucher Number Generation ====================

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextIssueNumberAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ItemTransactionMain",
            -19,  // VoucherID for Item Issue
            prefix);
    }

    public async Task<(string SlipNo, long MaxSlipNo)> GenerateNextSlipNumberAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT ISNULL(MAX(CAST(SlipNo AS BIGINT)), 0) + 1 AS NextSlipNo
            FROM ItemTransactionMain
            WHERE VoucherID = -19
              AND CompanyID = @CompanyID
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNUMERIC(SlipNo) = 1";

        var nextSlipNo = await connection.ExecuteScalarAsync<long>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId });

        return (nextSlipNo.ToString(), nextSlipNo);
    }

    public async Task<string?> GetVoucherNoAsync(long transactionId)
    {
        using var connection = GetConnection();
        var sql = "SELECT VoucherNo FROM ItemTransactionMain WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID";
        return await connection.ExecuteScalarAsync<string>(
            sql,
            new { TransactionID = transactionId, CompanyID = _currentUserService.GetCompanyId() });
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<Application.DTOs.Inventory.ItemIssueDirectDataDto>> GetItemIssueDirectDataAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT
                itm.TransactionID,
                itd.TransactionDetailID AS TransID,
                itm.VoucherID,
                itm.VoucherNo,
                itm.VoucherDate,
                ISNULL(itm.SlipNo, '') AS SlipNo,
                itm.DepartmentID,
                ISNULL(dm.DepartmentName, '') AS DepartmentName,
                itm.MachineID,
                ISNULL(mm.MachineName, '') AS MachineName,
                ISNULL(itm.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(itm.RefJobCardContentNo, '') AS RefJobCardContentNo,
                itd.ItemID,
                im.ItemCode,
                im.ItemName,
                ISNULL(im.ItemDescription, '') AS ItemDescription,
                itd.ItemGroupID,
                itd.IssueQuantity,
                itd.IssueWeight,
                itd.IssueUnit,
                itd.IssueRate,
                itd.IssueAmount,
                ISNULL(itd.StockBatchID, 0) AS StockBatchID,
                ISNULL(sb.BatchNo, '') AS BatchNo,
                ISNULL(itd.WarehouseID, 0) AS WarehouseID,
                ISNULL(wh.WarehouseName, '') AS WarehouseName,
                ISNULL(itd.BinID, 0) AS BinID,
                ISNULL(bn.BinName, '') AS BinName,
                ISNULL(itd.ProcessID, 0) AS ProcessID,
                ISNULL(pm.ProcessName, '') AS ProcessName,
                ISNULL(itd.ItemNarration, '') AS ItemNarration,
                ISNULL(itd.Remark, '') AS Remark,
                itm.TotalQuantity,
                itm.TotalWeight,
                itm.TotalAmount,
                ISNULL(itm.Narration, '') AS Narration,
                ISNULL(itm.StockType, '') AS StockType
            FROM ItemTransactionMain itm
            INNER JOIN ItemTransactionDetail itd ON itm.TransactionID = itd.TransactionID AND itd.CompanyID = @CompanyID
            INNER JOIN ItemMaster im ON itd.ItemID = im.ItemID
            LEFT JOIN DepartmentMaster dm ON itm.DepartmentID = dm.DepartmentID
            LEFT JOIN MachineMaster mm ON itm.MachineID = mm.MachineID
            LEFT JOIN StockBatch sb ON itd.StockBatchID = sb.StockBatchID
            LEFT JOIN WarehouseMaster wh ON itd.WarehouseID = wh.WarehouseID
            LEFT JOIN BinMaster bn ON itd.BinID = bn.BinID
            LEFT JOIN ProcessMaster pm ON itd.ProcessID = pm.ProcessID
            WHERE itm.TransactionID = @TransactionID
              AND itm.CompanyID = @CompanyID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ItemIssueDirectDataDto>(
            sql,
            new { TransactionID = transactionId, CompanyID = companyId }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.ItemIssueDirectListDto>> GetItemIssuesDirectListAsync(
        string fromDate,
        string toDate,
        bool applyDateFilter)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                itm.TransactionID,
                itm.VoucherNo,
                itm.VoucherDate,
                ISNULL(itm.SlipNo, '') AS SlipNo,
                ISNULL(dm.DepartmentName, '') AS DepartmentName,
                ISNULL(mm.MachineName, '') AS MachineName,
                ISNULL(itm.RefJobCardContentNo, '') AS RefJobCardContentNo,
                itm.TotalQuantity,
                itm.TotalWeight,
                itm.TotalAmount,
                ISNULL(itm.StockType, '') AS StockType,
                itm.ProductionUnitID,
                pu.ProductionUnitName
            FROM ItemTransactionMain itm
            LEFT JOIN DepartmentMaster dm ON itm.DepartmentID = dm.DepartmentID
            LEFT JOIN MachineMaster mm ON itm.MachineID = mm.MachineID
            LEFT JOIN ProductionUnitMaster pu ON itm.ProductionUnitID = pu.ProductionUnitID
            WHERE itm.VoucherID = -19
              AND itm.CompanyID = @CompanyID
              AND itm.ProductionUnitID = @ProductionUnitID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0
              AND (@ApplyDateFilter = 0 OR (itm.VoucherDate >= CAST(@FromDate AS DATE) AND itm.VoucherDate <= CAST(@ToDate AS DATE)))
            ORDER BY itm.VoucherDate DESC, itm.TransactionID DESC";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ItemIssueDirectListDto>(
            sql,
            new
            {
                CompanyID = companyId,
                ProductionUnitID = productionUnitId,
                FromDate = fromDate,
                ToDate = toDate,
                ApplyDateFilter = applyDateFilter
            }
        );

        return result.ToList();
    }

    public async Task<Application.DTOs.Inventory.ItemIssueDirectDataDto?> GetItemIssueDirectHeaderAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT TOP 1
                itm.TransactionID,
                itm.VoucherNo,
                itm.VoucherDate,
                ISNULL(itm.SlipNo, '') AS SlipNo,
                itm.DepartmentID,
                ISNULL(dm.DepartmentName, '') AS DepartmentName,
                itm.MachineID,
                ISNULL(mm.MachineName, '') AS MachineName,
                ISNULL(itm.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(itm.RefJobCardContentNo, '') AS RefJobCardContentNo,
                itm.TotalQuantity,
                itm.TotalWeight,
                itm.TotalAmount,
                ISNULL(itm.Narration, '') AS Narration,
                ISNULL(itm.StockType, '') AS StockType
            FROM ItemTransactionMain itm
            LEFT JOIN DepartmentMaster dm ON itm.DepartmentID = dm.DepartmentID
            LEFT JOIN MachineMaster mm ON itm.MachineID = mm.MachineID
            WHERE itm.TransactionID = @TransactionID
              AND itm.CompanyID = @CompanyID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<Application.DTOs.Inventory.ItemIssueDirectDataDto>(
            sql,
            new { TransactionID = transactionId, CompanyID = companyId }
        );

        return result;
    }

    // ==================== Picklist Operations ====================

    public async Task<List<Application.DTOs.Inventory.DirectPicklistDto>> GetJobAllocatedPicklistAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                pr.PicklistReleaseID,
                pr.PicklistNo,
                pr.PicklistDate,
                pr.RefJobBookingJobCardContentsID,
                ISNULL(pr.RefJobCardContentNo, '') AS RefJobCardContentNo,
                prd.PicklistReleaseDetailID,
                prd.ItemID,
                im.ItemCode,
                im.ItemName,
                ISNULL(im.ItemDescription, '') AS ItemDescription,
                prd.RequiredQuantity,
                (prd.RequiredQuantity - ISNULL((
                    SELECT SUM(IssueQuantity)
                    FROM ItemTransactionDetail
                    WHERE PicklistReleaseDetailID = prd.PicklistReleaseDetailID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)) AS PendingQuantity,
                prd.IssueUnit AS StockUnit,
                ISNULL(prd.StockBatchID, 0) AS StockBatchID,
                ISNULL(sb.BatchNo, '') AS BatchNo,
                ISNULL(im.IssueRate, 0) AS IssueRate
            FROM PicklistReleaseMaster pr
            INNER JOIN PicklistReleaseDetail prd ON pr.PicklistReleaseID = prd.PicklistReleaseID
            INNER JOIN ItemMaster im ON prd.ItemID = im.ItemID
            LEFT JOIN StockBatch sb ON prd.StockBatchID = sb.StockBatchID
            WHERE pr.CompanyID = @CompanyID
              AND pr.ProductionUnitID = @ProductionUnitID
              AND ISNULL(pr.IsDeletedTransaction, 0) = 0
              AND ISNULL(pr.IsPicklistReleased, 0) = 1
              AND prd.RequiredQuantity > ISNULL((
                    SELECT SUM(IssueQuantity)
                    FROM ItemTransactionDetail
                    WHERE PicklistReleaseDetailID = prd.PicklistReleaseDetailID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)
            ORDER BY pr.PicklistDate DESC, prd.ItemID";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.DirectPicklistDto>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.DirectPicklistDto>> GetAllPicklistByStockTypeAsync(string stockType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                im.ItemID,
                im.ItemCode,
                im.ItemName,
                ISNULL(im.ItemDescription, '') AS ItemDescription,
                ISNULL(im.StockUnit, '') AS StockUnit,
                ISNULL(im.IssueRate, 0) AS IssueRate,
                ISNULL(sb.StockBatchID, 0) AS BatchID,
                ISNULL(sb.BatchNo, '') AS BatchNo,
                ISNULL(sb.AvailableQuantity, 0) AS PhysicalStock,
                ISNULL(sb.AvailableWeight, 0) AS AvailableWeight
            FROM ItemMaster im
            LEFT JOIN StockBatch sb ON im.ItemID = sb.ItemID
            WHERE im.CompanyID = @CompanyID
              AND ISNULL(im.IsDeletedTransaction, 0) = 0
              AND (@StockType = '' OR im.StockType = @StockType)
              AND ISNULL(sb.AvailableQuantity, 0) > 0
            ORDER BY im.ItemCode, im.ItemName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.DirectPicklistDto>(
            sql,
            new
            {
                CompanyID = companyId,
                ProductionUnitID = productionUnitId,
                StockType = stockType
            }
        );

        return result.ToList();
    }

    // ==================== Stock Operations ====================

    public async Task<List<Application.DTOs.Inventory.StockBatchDirectDto>> GetStockBatchWiseAsync(
        long itemId,
        long? jobBookingJobCardContentsID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // DTO fields: BatchID, BatchNo, Warehouse, Bin, BatchStock
        // WarehouseID in StockBatch references WarehouseMaster.WarehouseID (WarehouseName → 'Warehouse')
        // BinName in old schema is stored in WarehouseMaster (not separate BinMaster table)
        var sql = @"
            SELECT
                sb.StockBatchID AS BatchID,
                sb.BatchNo,
                sb.ItemID,
                ISNULL(sb.AvailableQuantity, 0) AS BatchStock,
                ISNULL(sb.WarehouseID, 0) AS WarehouseID,
                ISNULL(wh.WarehouseName, '') AS Warehouse,
                ISNULL(wh.BinName, '') AS Bin,
                ISNULL(sb.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(sb.RefJobCardContentNo, '') AS RefJobCardContentNo,
                sb.BatchDate,
                ISNULL(sb.IssueRate, 0) AS IssueRate
            FROM StockBatch sb
            LEFT JOIN WarehouseMaster wh ON sb.WarehouseID = wh.WarehouseID
            WHERE sb.ItemID = @ItemID
              AND sb.CompanyID = @CompanyID
              AND sb.ProductionUnitID = @ProductionUnitID
              AND ISNULL(sb.IsDeletedTransaction, 0) = 0
              AND ISNULL(sb.AvailableQuantity, 0) > 0
              AND (@JobBookingJobCardContentsID IS NULL OR sb.RefJobBookingJobCardContentsID = @JobBookingJobCardContentsID)
            ORDER BY sb.BatchDate ASC, sb.BatchNo ASC";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.StockBatchDirectDto>(
            sql,
            new
            {
                ItemID = itemId,
                CompanyID = companyId,
                ProductionUnitID = productionUnitId,
                JobBookingJobCardContentsID = jobBookingJobCardContentsID
            }
        );

        return result.ToList();
    }

    // ==================== Lookup Operations ====================

    public async Task<List<Application.DTOs.Inventory.JobCardDirectDto>> GetJobCardFilterListAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Note: JobBookingJobCardContents does NOT have ClientID, JobCardDate or JobCardStatus.
        // Client info comes from JobBookingJobCard (parent table) via LedgerID.
        // Old code used stored proc: Exec ItemIssueDirectJobCardRender
        var sql = @"
            SELECT
                jbc.JobBookingID,
                jbc.JobBookingJobCardContentsID,
                jbj.LedgerID,
                ISNULL(jbj.JobBookingNo, '') AS BookingNo,
                ISNULL(jbc.JobCardContentNo, '') AS JobCardNo,
                ISNULL(jbj.JobName, '') AS JobName,
                ISNULL(jbc.PlanContName, '') AS ContentName,
                ISNULL(CONVERT(VARCHAR(10), jbc.ReleasedDate, 120), '') AS ReleasedDate
            FROM JobBookingJobCardContents jbc
            INNER JOIN JobBookingJobCard jbj ON jbc.JobBookingID = jbj.JobBookingID
                AND jbj.CompanyID = jbc.CompanyID
            WHERE jbc.CompanyID = @CompanyID
              AND jbc.ProductionUnitID = @ProductionUnitID
              AND ISNULL(jbc.IsDeletedTransaction, 0) = 0
              AND ISNULL(jbc.IsRelease, 0) = 1
              AND ISNULL(jbj.IsClose, 0) = 0
              AND ISNULL(jbj.IsDeletedTransaction, 0) = 0
            ORDER BY jbc.ReleasedDate DESC, jbc.JobCardContentNo DESC";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.JobCardDirectDto>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.DepartmentDto>> GetDepartmentsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Old VB query: select DepartmentID, nullif(DepartmentName,'') as DepartmentName from DepartmentMaster where IsDeletedTransaction=0
        var sql = @"
            SELECT
                DepartmentID,
                ISNULL(DepartmentName, '') AS DepartmentName
            FROM DepartmentMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY DepartmentName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.DepartmentDto>(
            sql,
            new { CompanyID = companyId });

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.MachineDto>> GetMachinesByDepartmentAsync(long departmentId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT
                MachineID,
                ISNULL(MachineName, '') AS MachineName,
                DepartmentID
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND DepartmentID = @DepartmentID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY MachineName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.MachineDto>(
            sql,
            new { CompanyID = companyId, DepartmentID = departmentId });

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.ProcessDto>> GetProcessListJobWiseAsync(long jobCardContentsId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT DISTINCT
                pm.ProcessID,
                ISNULL(pm.ProcessName, '') AS ProcessName
            FROM ProcessMaster pm
            INNER JOIN JobBookingJobCardProcess jcpd ON pm.ProcessID = jcpd.ProcessID
            WHERE jcpd.JobBookingJobCardContentsID = @JobCardContentsID
              AND pm.CompanyID = @CompanyID
              AND ISNULL(pm.IsDeletedTransaction, 0) = 0
            ORDER BY pm.ProcessName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ProcessDto>(
            sql,
            new { JobCardContentsID = jobCardContentsId, CompanyID = companyId });

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.ItemIssueWarehouseDto>> GetWarehousesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Old VB query: Select DISTINCT WarehouseName As Warehouse From WarehouseMaster
        // DTO field is 'Warehouse' (not 'WarehouseName') - must alias correctly
        var sql = @"
            SELECT DISTINCT
                WarehouseID,
                WarehouseName AS Warehouse
            FROM WarehouseMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(WarehouseName, '') <> ''
            ORDER BY WarehouseName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ItemIssueWarehouseDto>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId });

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.ItemIssueBinDto>> GetBinsAsync(string warehouseName)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Old VB query: SELECT Distinct BinName AS Bin, WarehouseID FROM WarehouseMaster
        // Bins are stored in WarehouseMaster (BinName column) in the legacy schema.
        // DTO field is 'Bin' (not 'BinName') - must alias correctly.
        // WarehouseID is a self-reference in WarehouseMaster for bin-to-warehouse mapping.
        var sql = @"
            SELECT DISTINCT
                WarehouseID AS BinID,
                BinName AS Bin,
                WarehouseID
            FROM WarehouseMaster
            WHERE CompanyID = @CompanyID
              AND WarehouseName = @WarehouseName
              AND ISNULL(BinName, '') <> ''
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY BinName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ItemIssueBinDto>(
            sql,
            new
            {
                CompanyID = companyId,
                ProductionUnitID = productionUnitId,
                WarehouseName = warehouseName
            });

        return result.ToList();
    }

    // ==================== Utility Operations ====================

    public async Task<string> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 CONVERT(VARCHAR(10), VoucherDate, 120) AS VoucherDate
            FROM ItemTransactionMain
            WHERE VoucherID = -19
              AND CompanyID = @CompanyID
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY VoucherDate DESC";

        var result = await connection.ExecuteScalarAsync<string>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId });

        return result ?? DateTime.Now.ToString("yyyy-MM-dd");
    }

    public async Task<bool> CheckUserAuthorityAsync()
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Old VB query: select UserID, ISNULL(IsExtraPaperIssue,0) As IsExtraPaperIssue from UserMaster where UserID='...'
        var sql = @"
            SELECT ISNULL(IsExtraPaperIssue, 0)
            FROM UserMaster
            WHERE UserID = @UserID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var hasAuthority = await connection.ExecuteScalarAsync<bool>(
            sql,
            new { UserID = userId, CompanyID = companyId });

        return hasAuthority;
    }

    public async Task<bool> IsItemIssueUsedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Check if this issue is referenced in any subsequent transactions (e.g. returns, consumption tracking)
        var sql = @"
            SELECT COUNT(1)
            FROM ItemTransactionDetail
            WHERE IssueTransactionID = @TransactionID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { TransactionID = transactionId, CompanyID = companyId });

        return count > 0;
    }

    public async Task UpdateStockValuesAsync(long transactionId)
    {
        // Placeholder for stock value update stored procedure
        // Similar pattern to PurchaseOrderRepository
        await Task.CompletedTask;
    }
}
