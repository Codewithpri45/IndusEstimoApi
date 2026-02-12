using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Application.Interfaces.Services;
using Dapper;
namespace IndasEstimo.Infrastructure.Repositories.ToolInventory;

public class ToolRequisitionRepository : IToolRequisitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolRequisitionRepository> _logger;
    public ToolRequisitionRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolRequisitionRepository> logger)
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

    public async Task<long> SaveToolRequisitionAsync(
        ToolRequisition main,
        List<ToolRequisitionDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Insert Main record
            var transactionId = await _dbOperations.InsertDataAsync("ToolTransactionMain", main, connection, transaction, "TransactionID");

            // 2. Insert Detail records
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool requisition");
            throw;
        }
    }

    public async Task UpdateIndentLinkageAsync(long requisitionTransactionId, List<Application.DTOs.ToolInventory.ToolRequisitionIndentUpdateDto> indentUpdates)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            UPDATE ToolTransactionDetail
            SET RequisitionTransactionID = @RequisitionTransactionID
            WHERE TransactionID = @TransactionID
              AND ToolID = @ToolID
              AND ProductionUnitID = @ProductionUnitID";

        foreach (var indent in indentUpdates)
        {
            await connection.ExecuteAsync(sql, new
            {
                RequisitionTransactionID = requisitionTransactionId,
                TransactionID = indent.TransactionID,
                ToolID = indent.ToolID,
                ProductionUnitID = productionUnitId
            });
        }
    }

    public async Task<long> UpdateToolRequisitionAsync(
        long transactionId,
        ToolRequisition main,
        List<ToolRequisitionDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // 1. Update main record
            main.TransactionID = transactionId;
            main.ModifiedBy = _currentUserService.GetUserId() ?? 0;
            main.ProductionUnitID = productionUnitId;
            main.CompanyID = _currentUserService.GetCompanyId() ?? 0;

            await _dbOperations.UpdateDataAsync("ToolTransactionMain", main, connection, transaction, new[] { "TransactionID", "ProductionUnitID" });

            // 2. Delete existing details
            await connection.ExecuteAsync(
                "DELETE FROM ToolTransactionDetail WHERE ProductionUnitID = @ProductionUnitID AND TransactionID = @TransactionID",
                new { ProductionUnitID = productionUnitId, TransactionID = transactionId },
                transaction);

            // 3. Re-insert details
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating tool requisition {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task ClearIndentLinkageAsync(long requisitionTransactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            UPDATE ToolTransactionDetail
            SET RequisitionTransactionID = 0
            WHERE ProductionUnitID = @ProductionUnitID
              AND RequisitionTransactionID = @RequisitionTransactionID";

        await connection.ExecuteAsync(sql, new
        {
            ProductionUnitID = productionUnitId,
            RequisitionTransactionID = requisitionTransactionId
        });
    }

    public async Task<bool> DeleteToolRequisitionAsync(long transactionId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // 1. Soft Delete Main
            string deleteMainSql = @"
                UPDATE ToolTransactionMain
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ProductionUnitID = @ProductionUnitID
                  AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteMainSql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                ProductionUnitID = productionUnitId,
                TransactionID = transactionId
            }, transaction);

            // 2. Soft Delete Details
            string deleteDetailSql = @"
                UPDATE ToolTransactionDetail
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ProductionUnitID = @ProductionUnitID
                  AND TransactionID = @TransactionID";

            await connection.ExecuteAsync(deleteDetailSql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                ProductionUnitID = productionUnitId,
                TransactionID = transactionId
            }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool requisition {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<bool> IsToolRequisitionApprovedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ProductionUnitID = @ProductionUnitID
              AND TransactionID = @TransactionID
              AND ISNULL(IsvoucherToolApproved, 0) <> 0
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            ProductionUnitID = productionUnitId,
            TransactionID = transactionId
        });
        return count > 0;
    }

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextVoucherNoAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ToolTransactionMain",
            -115,
            prefix);
    }

    public async Task<string?> GetVoucherNoAsync(long transactionId)
    {
        using var connection = GetConnection();
        var sql = "SELECT VoucherNo FROM ToolTransactionMain WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID";
        return await connection.ExecuteScalarAsync<string>(sql, new
        {
            TransactionID = transactionId,
            CompanyID = _currentUserService.GetCompanyId()
        });
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<Application.DTOs.ToolInventory.ToolIndentListDto>> GetIndentListAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var fYear = _currentUserService.GetFYear() ?? string.Empty;

        // Matches FillGrid "Indent List" mode from legacy VB:
        // Part 1: Tools from indents (VoucherID=-8) that haven't been requisitioned yet
        // Part 2: UNION ALL tools from ToolMaster not yet in any indent
        var sql = @"
            SELECT
                ISNULL(IEM.TransactionID, 0) AS TransactionID,
                ISNULL(IEM.MaxVoucherNo, 0) AS MaxVoucherNo,
                ISNULL(IEM.VoucherID, 0) AS VoucherID,
                NULLIF(IEM.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(NVARCHAR(30), IEM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                ISNULL(IED.ToolID, 0) AS ToolID,
                ISNULL(IM.ToolGroupID, 0) AS ToolGroupID,
                ISNULL(IGM.ToolGroupNameID, 0) AS ToolGroupNameID,
                ISNULL(JBC.JobBookingID, 0) AS BookingID,
                ISNULL(JBC.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                NULLIF(IGM.ToolGroupName, '') AS ToolGroupName,
                NULLIF(IM.ToolCode, '') AS ToolCode,
                NULLIF(IM.ToolName, '') AS ToolName,
                NULLIF(IM.ToolDescription, '') AS ToolDescription,
                NULLIF(JBC.JobCardContentNo, '') AS JobBookingContentNo,
                ISNULL(IED.RequiredQuantity, 0) AS RequiredQuantity,
                ISNULL(IM.BookedStock, 0) AS BookedStock,
                ISNULL(IM.AllocatedStock, 0) AS AllocatedStock,
                ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                NULLIF(IED.StockUnit, '') AS StockUnit,
                ISNULL(UOM.DecimalPlace, 0) AS UnitDecimalPlace,
                NULLIF(IM.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(IM.SizeW, 0) AS SizeW,
                NULLIF(C.ConversionFormula, '') AS ConversionFormula,
                ISNULL(C.ConvertedUnitDecimalPlace, 0) AS ConvertedUnitDecimalPlace,
                (SELECT TOP 1 REPLACE(CONVERT(VARCHAR(13), A.VoucherDate, 106), ' ', '-')
                 FROM ToolTransactionMain AS A
                 INNER JOIN ToolTransactionDetail AS B ON A.TransactionID = B.TransactionID AND A.CompanyID = B.CompanyID AND B.ToolID = IED.ToolID
                 WHERE A.VoucherID = -11 AND A.CompanyID = IED.CompanyID AND ISNULL(A.IsDeletedTransaction, 0) = 0
                 ORDER BY A.VoucherDate DESC) AS LastPurchaseDate
            FROM ToolTransactionMain AS IEM
            INNER JOIN ToolTransactionDetail AS IED ON IEM.TransactionID = IED.TransactionID AND IEM.CompanyID = IED.CompanyID
            INNER JOIN ToolMaster AS IM ON IM.ToolID = IED.ToolID AND IM.CompanyID = IED.CompanyID
            INNER JOIN ToolGroupMaster AS IGM ON IGM.ToolGroupID = IM.ToolGroupID AND IGM.CompanyID = IM.CompanyID
            LEFT JOIN JobBookingJobCardContents AS JBC ON JBC.JobBookingJobCardContentsID = IED.JobBookingJobCardContentsID AND JBC.JobBookingID = IED.JobBookingID AND JBC.CompanyID = IED.CompanyID
            LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol = IED.StockUnit AND UOM.CompanyID = IEM.CompanyID
            LEFT JOIN ConversionMaster AS C ON IM.StockUnit = C.BaseUnitSymbol AND IM.PurchaseUnit = C.ConvertedUnitSymbol AND IM.CompanyID = C.CompanyID
            WHERE IEM.VoucherID IN (-8)
              AND ISNULL(IED.IsCancelled, 0) = 0
              AND IM.CompanyID = @CompanyID
              AND IEM.FYear = @FYear
              AND ISNULL(IED.RequisitionTransactionID, 0) = 0
              AND ISNULL(IEM.IsDeletedTransaction, 0) <> 1

            UNION ALL

            SELECT
                0 AS TransactionID,
                0 AS MaxVoucherNo,
                0 AS VoucherID,
                NULLIF('', '') AS VoucherNo,
                NULLIF('', '') AS VoucherDate,
                ISNULL(IM.ToolID, 0) AS ToolID,
                ISNULL(IM.ToolGroupID, 0) AS ToolGroupID,
                ISNULL(IGM.ToolGroupNameID, 0) AS ToolGroupNameID,
                0 AS BookingID,
                0 AS JobBookingJobCardContentsID,
                NULLIF(IGM.ToolGroupName, '') AS ToolGroupName,
                NULLIF(IM.ToolCode, '') AS ToolCode,
                NULLIF(IM.ToolName, '') AS ToolName,
                NULLIF(IM.ToolDescription, '') AS ToolDescription,
                NULLIF('', '') AS JobBookingContentNo,
                0 AS RequiredQuantity,
                ISNULL(IM.BookedStock, 0) AS BookedStock,
                ISNULL(IM.AllocatedStock, 0) AS AllocatedStock,
                ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                ISNULL(U.DecimalPlace, 0) AS UnitDecimalPlace,
                NULLIF(IM.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(IM.SizeW, 0) AS SizeW,
                NULLIF(C.ConversionFormula, '') AS ConversionFormula,
                ISNULL(C.ConvertedUnitDecimalPlace, 0) AS ConvertedUnitDecimalPlace,
                (SELECT TOP 1 REPLACE(CONVERT(VARCHAR(13), A.VoucherDate, 106), ' ', '-')
                 FROM ToolTransactionMain AS A
                 INNER JOIN ToolTransactionDetail AS B ON A.TransactionID = B.TransactionID AND A.CompanyID = B.CompanyID AND B.ToolID = IM.ToolID
                 WHERE A.VoucherID = -11 AND A.CompanyID = IM.CompanyID AND ISNULL(A.IsDeletedTransaction, 0) = 0
                 ORDER BY A.VoucherDate DESC) AS LastPurchaseDate
            FROM ToolMaster AS IM
            INNER JOIN ToolGroupMaster AS IGM ON IGM.ToolGroupID = IM.ToolGroupID AND IGM.CompanyID = IM.CompanyID
            LEFT JOIN UnitMaster AS U ON U.UnitSymbol = IM.StockUnit AND U.CompanyID = IM.CompanyID
            LEFT JOIN ConversionMaster AS C ON IM.StockUnit = C.BaseUnitSymbol AND IM.PurchaseUnit = C.ConvertedUnitSymbol AND IM.CompanyID = C.CompanyID
            WHERE IM.CompanyID = @CompanyID
              AND ISNULL(IM.IsDeletedTransaction, 0) <> 1
              AND IM.ToolID NOT IN (
                  SELECT DISTINCT IED.ToolID
                  FROM ToolTransactionMain AS IEM
                  INNER JOIN ToolTransactionDetail AS IED ON IEM.TransactionID = IED.TransactionID AND IEM.CompanyID = IED.CompanyID
                  WHERE IEM.VoucherID IN (-8)
                    AND IEM.FYear = @FYear
                    AND IEM.CompanyID = @CompanyID
                    AND ISNULL(IED.IsDeletedTransaction, 0) <> 1
                    AND ISNULL(IED.RequisitionTransactionID, 0) = 0
              )
            ORDER BY ToolGroupName, ToolName, VoucherDate";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolIndentListDto>(
            sql,
            new { CompanyID = companyId, FYear = fYear }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolRequisitionListDto>> GetToolRequisitionListAsync(
        string fromDate,
        string toDate)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Matches FillGrid requisition mode (VoucherID = -115)
        // DISTINCT added to prevent duplicate rows when multiple detail records exist
        var sql = @"
            SELECT DISTINCT
                ISNULL(STM.TransactionID, 0) AS TransactionID,
                ISNULL(STM.VoucherID, 0) AS VoucherID,
                ISNULL(STM.MaxVoucherNo, 0) AS MaxVoucherNo,
                NULLIF(STM.VoucherNo, '') AS VoucherNo,
                NULLIF(STM.FYear, '') AS FYear,
                ISNULL(STM.TotalQuantity, 0) AS TotalQuantity,
                REPLACE(CONVERT(NVARCHAR(13), STM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                REPLACE(CONVERT(NVARCHAR(13), STM.CreatedDate, 106), ' ', '-') AS CreatedDate,
                NULLIF(UM.UserName, '') AS CreatedBy,
                NULLIF(STM.Narration, '') AS Narration,
                NULLIF(UM2.UserName, '') AS ApprovedBy,
                ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                ISNULL(CM1.CompanyName, '') AS CompanyName
            FROM ToolTransactionMain AS STM
            INNER JOIN ToolTransactionDetail AS TTD ON TTD.TransactionID = STM.TransactionID AND TTD.CompanyID = STM.CompanyID
            INNER JOIN UserMaster AS UM ON UM.UserID = STM.UserID
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = STM.ProductionUnitID AND ISNULL(PUM.IsDeletedTransaction, 0) = 0
            INNER JOIN CompanyMaster AS CM1 ON CM1.CompanyID = PUM.CompanyID
            LEFT JOIN UserMaster AS UM2 ON UM2.UserID = TTD.VoucherToolApprovedBy AND UM2.CompanyID = TTD.CompanyID
            WHERE STM.ProductionUnitID = @ProductionUnitID
              AND ISNULL(STM.IsDeletedTransaction, 0) <> 1
              AND STM.VoucherID = -115
              AND (@FromDate = '' OR STM.VoucherDate >= CAST(@FromDate AS DATE))
              AND (@ToDate = '' OR STM.VoucherDate <= CAST(@ToDate AS DATE))
            ORDER BY FYear DESC, MaxVoucherNo DESC";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolRequisitionListDto>(
            sql,
            new
            {
                ProductionUnitID = productionUnitId,
                FromDate = fromDate,
                ToDate = toDate
            }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolRequisitionDataDto>> GetToolRequisitionDataAsync(long transactionId)
    {
        using var connection = GetConnection();

        // Matches RetriveRequisitionData WebMethod (SQL Server branch)
        var sql = @"
            SELECT DISTINCT
                ISNULL(IEM.TransactionID, 0) AS RequisitionTransactionID,
                ISNULL(IED.IsvoucherToolApproved, 0) AS VoucherToolApproved,
                ISNULL(IEM.MaxVoucherNo, 0) AS RequisitionMaxVoucherNo,
                ISNULL(IEM.VoucherID, 0) AS RequisitionVoucherID,
                ISNULL(ID.TransactionID, 0) AS TransactionID,
                ISNULL(I.MaxVoucherNo, 0) AS MaxVoucherNo,
                ISNULL(I.VoucherID, 0) AS VoucherID,
                ISNULL(IED.ToolID, 0) AS RequisitionToolID,
                ISNULL(ID.ToolID, 0) AS ToolID,
                ISNULL(IED.TransID, 0) AS TransID,
                ISNULL(IM.ToolGroupID, 0) AS ToolGroupID,
                ISNULL(IM.ToolSubGroupID, 0) AS ToolSubGroupID,
                ISNULL(IGM.ToolGroupNameID, 0) AS ToolGroupNameID,
                NULLIF(I.VoucherNo, '') AS VoucherNo,
                REPLACE(CONVERT(VARCHAR(30), I.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IEM.VoucherNo, '') AS RequisitionVoucherNo,
                REPLACE(CONVERT(VARCHAR(30), IEM.VoucherDate, 106), ' ', '-') AS RequisitionVoucherDate,
                NULLIF(IGM.ToolGroupName, '') AS ToolGroupName,
                NULLIF(IM.ToolCode, '') AS RequisitionToolCode,
                NULLIF(IM.ToolName, '') AS RequisitionToolName,
                NULLIF(IM.ToolDescription, '') AS RequisitionToolDescription,
                NULLIF(M.ToolCode, '') AS ToolCode,
                NULLIF(M.ToolName, '') AS ToolName,
                NULLIF(M.ToolDescription, '') AS ToolDescription,
                ISNULL(IED.RequiredQuantity, 0) AS PurchaseQty,
                ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                ISNULL(CM.CompanyName, '') AS CompanyName,
                ISNULL((SELECT ROUND(SUM(ISNULL(RequiredQuantity, 0)), 3)
                        FROM ToolTransactionDetail
                        WHERE RequisitionTransactionID = IED.TransactionID
                          AND RequisitionToolID = IED.ToolID
                          AND CompanyID = IED.CompanyID), 0) AS TotalRequisitionQty,
                ISNULL(ID.RequiredQuantity, 0) AS RequisitionQty,
                ISNULL(IM.BookedStock, 0) AS RequisitionBookedStock,
                ISNULL(IM.AllocatedStock, 0) AS RequisitionAllocatedStock,
                ISNULL(IED.CurrentStockInStockUnit, 0) AS RequisitionPhysicalStock,
                ISNULL(IED.CurrentStockInPurchaseUnit, 0) AS RequisitionPhysicalStockInPurchaseUnit,
                ISNULL(M.BookedStock, 0) AS BookedStock,
                ISNULL(M.AllocatedStock, 0) AS AllocatedStock,
                ISNULL(M.PhysicalStock, 0) AS PhysicalStock,
                NULLIF(IM.StockUnit, '') AS StockUnit,
                NULLIF(IED.StockUnit, '') AS OrderUnit,
                REPLACE(CONVERT(VARCHAR(30), IED.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                NULLIF(IED.ToolNarration, '') AS ToolNarration,
                NULLIF(IM.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(UOM.DecimalPlace, 0) AS UnitDecimalPlace,
                NULLIF(IEM.FYear, '') AS FYear,
                NULLIF(JBC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(IED.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(IED.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(ID.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                (SELECT TOP 1 REPLACE(CONVERT(VARCHAR(13), A.VoucherDate, 106), ' ', '-')
                 FROM ToolTransactionMain AS A
                 INNER JOIN ToolTransactionDetail AS B ON A.TransactionID = B.TransactionID AND A.CompanyID = B.CompanyID AND B.ToolID = IED.ToolID
                 WHERE A.VoucherID = -11 AND A.CompanyID = IED.CompanyID AND ISNULL(A.IsDeletedTransaction, 0) = 0
                   AND CAST(FLOOR(CAST(A.VoucherDate AS FLOAT)) AS DATETIME) < CAST(FLOOR(CAST(IEM.VoucherDate AS FLOAT)) AS DATETIME)
                 ORDER BY A.VoucherDate DESC) AS LastPurchaseDate,
                ISNULL(IM.SizeW, 0) AS SizeW
            FROM ToolTransactionMain AS IEM
            INNER JOIN ToolTransactionDetail AS IED ON IEM.TransactionID = IED.TransactionID AND IEM.CompanyID = IED.CompanyID
            INNER JOIN ToolMaster AS IM ON IM.ToolID = IED.ToolID
            INNER JOIN ToolGroupMaster AS IGM ON IGM.ToolGroupID = IM.ToolGroupID
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = IEM.ProductionUnitID AND PUM.CompanyID = IEM.CompanyID AND ISNULL(PUM.IsDeletedTransaction, 0) = 0
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            LEFT JOIN ToolTransactionDetail AS ID ON ID.RequisitionTransactionID = IED.TransactionID AND ID.RequisitionToolID = IED.ToolID AND ID.CompanyID = IED.CompanyID
            LEFT JOIN ToolTransactionMain AS I ON I.TransactionID = ID.TransactionID AND I.CompanyID = ID.CompanyID
            LEFT JOIN JobBookingJobCardContents AS JBC ON JBC.JobBookingJobCardContentsID = ID.JobBookingJobCardContentsID AND JBC.JobBookingID = ID.JobBookingID
            LEFT JOIN ToolMaster AS M ON M.ToolID = ID.ToolID
            LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol = IED.StockUnit
            WHERE IEM.VoucherID IN (-115)
              AND IEM.TransactionID = @TransactionID
              AND ISNULL(IM.IsDeletedTransaction, 0) <> 1
            ORDER BY FYear, RequisitionMaxVoucherNo DESC, TransID";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolRequisitionDataDto>(
            sql,
            new { TransactionID = transactionId }
        );

        return result.ToList();
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<string> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 CONVERT(VARCHAR(10), VoucherDate, 120) AS VoucherDate
            FROM ToolTransactionMain
            WHERE VoucherID = -115
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY VoucherDate DESC";

        var result = await connection.ExecuteScalarAsync<string>(sql, new { ProductionUnitID = productionUnitId });
        return result ?? DateTime.Now.ToString("yyyy-MM-dd");
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolMasterItemDto>> GetToolMasterListAsync()
    {
        using var connection = GetConnection();

        // Matches GetOverFlowGrid WebMethod
        var sql = @"
            SELECT
                ISNULL(TM.ToolID, 0) AS ToolID,
                ISNULL(TM.ToolGroupID, 0) AS ToolGroupID,
                NULLIF(TM.ToolCode, '') AS ToolCode,
                NULLIF(TM.ToolName, '') AS ToolName,
                NULLIF(TM.PurchaseUnit, '') AS Unit,
                NULLIF(TGM.ToolGroupName, '') AS ToolType,
                ISNULL(TM.PurchaseRate, 0) AS Rate,
                NULLIF(TM.Narration, '') AS Narration
            FROM ToolMaster AS TM
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID AND TGM.CompanyID = TM.CompanyID
            WHERE ISNULL(TM.IsDeletedTransaction, 0) = 0
            ORDER BY TM.ToolName";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolMasterItemDto>(sql);
        return result.ToList();
    }

    public async Task<bool> CheckPermissionAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Matches CheckPermission WebMethod - checks if any detail items are approved
        var sql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ProductionUnitID = @ProductionUnitID
              AND TransactionID = @TransactionID
              AND ISNULL(IsvoucherToolApproved, 0) <> 0
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            ProductionUnitID = productionUnitId,
            TransactionID = transactionId
        });
        return count > 0;
    }
}
