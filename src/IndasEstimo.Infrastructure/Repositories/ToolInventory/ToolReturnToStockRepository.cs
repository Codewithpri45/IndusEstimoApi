using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Application.Interfaces.Services;
using Dapper;

namespace IndasEstimo.Infrastructure.Repositories.ToolInventory;

public class ToolReturnToStockRepository : IToolReturnToStockRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolReturnToStockRepository> _logger;

    public ToolReturnToStockRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolReturnToStockRepository> logger)
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

    public async Task<long> SaveToolReturnToStockAsync(
        ToolReturnToStock main,
        List<ToolReturnToStockDetail> details)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            // VB SavePlateReturnToStockData (lines 139-153)
            // 1. Insert Main record (ToolTransactionMain)
            var transactionId = await _dbOperations.InsertDataAsync("ToolTransactionMain", main, connection, transaction, "TransactionID");

            // 2. Set TransactionID for all details
            foreach (var detail in details)
            {
                detail.TransactionID = transactionId;
            }

            // 3. Insert Detail records (ToolTransactionDetail)
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool return to stock");
            throw;
        }
    }

    public async Task<bool> DeleteToolReturnToStockAsync(long transactionId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // VB DeletePlateReturnData (lines 218-219) - MSSQL branch
            // Soft delete main and detail tables
            var sql = @"
                Update ToolTransactionMain
                Set DeletedBy=@UserID, DeletedDate=Getdate(), IsDeletedTransaction=1
                WHERE ProductionUnitID=@ProductionUnitID And TransactionID=@TransactionID;

                Update ToolTransactionDetail
                Set DeletedBy=@UserID, DeletedDate=Getdate(), IsDeletedTransaction=1
                WHERE ProductionUnitID=@ProductionUnitID And TransactionID=@TransactionID";

            var parameters = new { UserID = userId, ProductionUnitID = productionUnitId, TransactionID = transactionId };

            await connection.ExecuteAsync(sql, parameters, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool return to stock {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextReturnNoAsync(string prefix)
    {
        // VB GetVoucherNoForPlateReturnToStock (line 120)
        // VoucherID=-44, prefix="TRS"
        return await _dbOperations.GenerateVoucherNoAsync(
            "ToolTransactionMain",
            -44,
            prefix);
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<ToolAvailableForReturnDto>> GetAvailableForReturnAsync()
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        // VB GetFirstGridData (lines 82-84) - MSSQL branch
        // Complex LEFT JOIN with subquery calculating available stock = (IssueQuantity - ReceiptQuantity)
        // Filters VoucherID IN(-43,-44) and excludes rejected batches
        var sql = @"
            Select PM.ToolCode, PM.ToolID, LM.LedgerID, LM.LedgerName, JEJ.JobName, JEJC.JobCardContentNo,
                   PTD.ParentTransactionID, IM.ItemName, IM.ItemID, ISNULL(PTD.Stock,0) As Stock,
                   PTD.BatchNo, GM.WarehouseID, GM.WarehouseName
            From LedgerMaster As LM
            INNER JOIN JobBookingJobCard As JEJ On JEJ.LedgerID = LM.LedgerID
            INNER JOIN JobBookingJobCardContents As JEJC On JEJC.JobBookingID = JEJ.JobBookingID
                And JEJC.CompanyID = JEJ.CompanyID
            Left JOIN (
                Select P2.ParentTransactionID, Isnull(P2.ItemID,0) As ItemID,
                       P2.JobBookingJobCardContentsID, P2.CompanyID, P2.ToolID,
                       P2.BatchNO, P2.WarehouseID,
                       (Sum(Isnull(P2.IssueQuantity,0))-Sum(Isnull(P2.ReceiptQuantity,0))) As Stock
                From ToolTransactionDetail As P2
                INNER JOIN ToolTransactionMain As P1 On P2.TransactionID = P1.TransactionID
                    And P2.CompanyID = P1.CompanyID
                WHERE P1.VoucherID In(-43,-44) And Isnull(P2.IsDeletedTransaction,0) = 0
                Group By P2.ParentTransactionID, Isnull(P2.ItemID,0),
                         P2.JobBookingJobCardContentsID, P2.CompanyID, P2.ToolID,
                         P2.BatchNO, P2.WarehouseID
                HAVING (Sum(Isnull(P2.IssueQuantity,0))-Sum(Isnull(P2.ReceiptQuantity,0))) > 0
            ) As PTD On JEJC.JobBookingJobCardContentsID = PTD.JobBookingJobCardContentsID
                And JEJC.CompanyID = PTD.CompanyID
            INNER JOIN ToolMaster As PM On PM.ToolID = PTD.ToolID
            INNER JOIN WarehouseMaster As GM On GM.WarehouseID = PTD.WarehouseID
            INNER JOIN ItemMaster As IM On IM.ItemID = PTD.ItemID
            Where JEJC.ProductionUnitID IN(" + productionUnitIdStr + @")
                And isnull(JEJ.IsDeletedTransaction,0)<>1
                And PTD.BatchNo Not In (
                    Select BatchNo from ToolTransactionDetail
                    Where RejectedQuantity = 1
                        And ToolID = PTD.ToolID
                        And BatchNo = PTD.BatchNo
                )
            Order By isnull(JEJC.JobBookingJobCardContentsID,0), IM.ItemName, PTD.BatchNo";

        var result = await connection.QueryAsync<ToolAvailableForReturnDto>(sql);
        return result.ToList();
    }

    public async Task<List<ToolReturnToStockListDto>> GetReturnToStockListAsync()
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        // VB PlateReturnToStockShowlist (lines 178-180) - MSSQL branch
        // Multi-table JOIN through Job Booking system
        var sql = @"
            Select PTM.TransactionID, PTM.FYear, PTM.MaxVoucherNo, PTM.VoucherNo,
                   replace(convert(nvarchar(30),PTM.VoucherDate,106),' ','-') as VoucherDate,
                   isnull(LM.LedgerID,0) as LedgerID, nullif(LM.LedgerName,'') as LedgerName,
                   nullif(PM.ToolCode,'') as ToolCode, nullif(PM.ToolID,'') as ToolID,
                   IM.ItemName, PTD.BatchNo, PTD.WarehouseID, GM.WarehouseName, GM.BinName,
                   nullif(PTM.VoucherPrefix,'') as VoucherPrefix,
                   replace(convert(nvarchar(30),PTM.ModifiedDate,106),' ','-') as ModifiedDate,
                   replace(convert(nvarchar(30), PTM.CreatedDate,106),' ','-') as CreatedDate,
                   isnull(PTM.UserID,0) As UserID
            From LedgerMaster As LM
            INNER Join JobBookingJobCard As JEJ On JEJ.LedgerID = LM.LedgerID
            INNER Join JobBookingJobCardContents As JEJC On JEJC.JobBookingID = JEJ.JobBookingID
                And JEJC.CompanyID = JEJ.CompanyID
            INNER Join ToolTransactionDetail As PTD On JEJC.JobBookingJobCardContentsID = PTD.JobBookingJobCardContentsID
                And JEJC.CompanyID = PTD.CompanyID
            INNER Join ToolTransactionMain As PTM On PTM.TransactionID = PTD.TransactionID
                And PTM.CompanyID = PTD.CompanyID
                And PTM.VoucherID = -44
                And Isnull(PTM.IsDeletedTransaction,0) = 0
            INNER Join WarehouseMaster As GM On GM.WarehouseID = PTD.WarehouseID
            INNER Join ItemMaster As IM On IM.ItemID = PTD.ItemID
            INNER Join ToolMaster As PM On PM.ToolID = PTD.ToolID
            Where JEJC.ProductionUnitID IN(" + productionUnitIdStr + @")
                And isnull(JEJ.ISDeletedTransaction,0)<>1
            Order By PTM.FYear Desc, PTM.MaxVoucherNo Desc";

        var result = await connection.QueryAsync<ToolReturnToStockListDto>(sql);
        return result.ToList();
    }
}
