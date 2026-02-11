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

public class ToolPurchaseOrderRepository : IToolPurchaseOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolPurchaseOrderRepository> _logger;

    public ToolPurchaseOrderRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolPurchaseOrderRepository> logger)
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

    public async Task<long> SaveToolPurchaseOrderAsync(
        ToolPurchaseOrder main,
        List<ToolPurchaseOrderDetail> details,
        List<ToolPurchaseOrderTax> taxes,
        List<ToolPurchaseOrderOverhead> overheads,
        List<ToolPurchaseOrderRequisition> requisitions)
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

            // 3. Insert Tax records
            await _dbOperations.InsertDataAsync("ToolPurchaseOrderTaxes", taxes, connection, transaction, "POTaxID", parentTransactionId: transactionId);

            // 4. Insert Overhead records
            await _dbOperations.InsertDataAsync("ToolPurchaseOverheadCharges", overheads, connection, transaction, "OverheadID", parentTransactionId: transactionId);

            // 5. Insert Requisition linkage records
            await _dbOperations.InsertDataAsync("ToolPurchaseRequisitionDetail", requisitions, connection, transaction, "RequisitionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool purchase order");
            throw;
        }
    }

    public async Task<long> UpdateToolPurchaseOrderAsync(
        long transactionId,
        ToolPurchaseOrder main,
        List<ToolPurchaseOrderDetail> details,
        List<ToolPurchaseOrderTax> taxes,
        List<ToolPurchaseOrderOverhead> overheads,
        List<ToolPurchaseOrderRequisition> requisitions)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // 1. Update Main record
            main.TransactionID = transactionId;
            main.ModifiedBy = _currentUserService.GetUserId() ?? 0;
            main.ProductionUnitID = productionUnitId;

            await _dbOperations.UpdateDataAsync("ToolTransactionMain", main, connection, transaction, new[] { "TransactionID", "ProductionUnitID" });

            // 2. Delete existing child records (delete-reinsert pattern)
            await connection.ExecuteAsync(
                "DELETE FROM ToolTransactionDetail WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID",
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId }, transaction);

            await connection.ExecuteAsync(
                "DELETE FROM ToolPurchaseOrderTaxes WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID",
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId }, transaction);

            await connection.ExecuteAsync(
                "DELETE FROM ToolPurchaseOverheadCharges WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID",
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId }, transaction);

            await connection.ExecuteAsync(
                "DELETE FROM ToolPurchaseRequisitionDetail WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID",
                new { TransactionID = transactionId, ProductionUnitID = productionUnitId }, transaction);

            // 3. Re-insert all child records
            await _dbOperations.InsertDataAsync("ToolTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ToolPurchaseOrderTaxes", taxes, connection, transaction, "POTaxID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ToolPurchaseOverheadCharges", overheads, connection, transaction, "OverheadID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ToolPurchaseRequisitionDetail", requisitions, connection, transaction, "RequisitionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating tool purchase order {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<bool> DeleteToolPurchaseOrderAsync(long transactionId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;

            // Soft delete all related tables
            var softDeleteSql = @"
                UPDATE {0}
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ProductionUnitID = @ProductionUnitID
                  AND TransactionID = @TransactionID";

            var parameters = new { ModifiedBy = userId, DeletedBy = userId, ProductionUnitID = productionUnitId, TransactionID = transactionId };

            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolTransactionMain"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolTransactionDetail"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolPurchaseOverheadCharges"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolPurchaseOrderTaxes"), parameters, transaction);
            await connection.ExecuteAsync(string.Format(softDeleteSql, "ToolPurchaseRequisitionDetail"), parameters, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool purchase order {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPONumberAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ToolTransactionMain",
            -117,
            prefix);
    }

    public async Task<string?> GetVoucherNoAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
        var sql = "SELECT VoucherNo FROM ToolTransactionMain WHERE TransactionID = @TransactionID AND ProductionUnitID = @ProductionUnitID";
        return await connection.ExecuteScalarAsync<string>(sql, new { TransactionID = transactionId, ProductionUnitID = productionUnitId });
    }

    public async Task<bool> IsToolPurchaseOrderApprovedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Check if any detail items are approved
        var sql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ProductionUnitID = @ProductionUnitID
              AND TransactionID = @TransactionID
              AND ISNULL(IsVoucherToolApproved, 0) = 1
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { ProductionUnitID = productionUnitId, TransactionID = transactionId });

        if (count > 0) return true;

        // Also check QC approval
        var qcSql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(QCApprovalNo, '') <> ''
              AND TransactionID = @TransactionID
              AND (ISNULL(ApprovedQuantity, 0) > 0 OR ISNULL(RejectedQuantity, 0) > 0)";

        var qcCount = await connection.ExecuteScalarAsync<int>(qcSql, new { TransactionID = transactionId });
        return qcCount > 0;
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<Application.DTOs.ToolInventory.ToolPurchaseOrderDataDto>> GetToolPurchaseOrderDataAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                Isnull(TTM.TransactionID, 0) AS PurchaseTransactionID,
                Isnull(TTM.VoucherID, 0) AS PurchaseVoucherID,
                Isnull(TPR.RequisitionTransactionID, 0) AS TransactionID,
                Isnull(TR.VoucherID, 0) AS VoucherID,
                Isnull(TTM.LedgerID, 0) AS LedgerID,
                Isnull(TTD.TransID, 0) AS TransID,
                Isnull(TTD.ToolID, 0) AS ToolID,
                Isnull(TTD.ToolGroupID, 0) AS ToolGroupID,
                NullIf(LM.LedgerName, '') AS LedgerName,
                Isnull(TTM.MaxVoucherNo, 0) AS PurchaseMaxVoucherNo,
                Isnull(TR.MaxVoucherNo, 0) AS MaxVoucherNo,
                NullIf(TTM.VoucherNo, '') AS PurchaseVoucherNo,
                NullIf(TR.VoucherNo, '') AS VoucherNo,
                Replace(Convert(Varchar(13), TTM.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate,
                Replace(Convert(Varchar(13), TR.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NullIf(TM.ToolCode, '') AS ToolCode,
                NullIf(TGM.ToolGroupName, '') AS ToolType,
                NullIf(Isnull(TM.ToolName, ''), '') AS ToolName,
                Isnull(TPR.RequisitionProcessQuantity, 0) AS RequiredQuantity,
                Isnull(TRD.RequiredQuantity, 0) AS RequisitionQty,
                NullIf(TPR.StockUnit, '') AS StockUnit,
                Isnull(TTD.PurchaseOrderQuantity, 0) AS PurchaseQuantity,
                Isnull(TTD.PurchaseUnit, '') AS PurchaseUnit,
                Isnull(TTD.PurchaseRate, 0) AS PurchaseRate,
                Isnull(TTD.GrossAmount, 0) AS BasicAmount,
                Isnull(TTD.DiscountPercentage, 0) AS Disc,
                Isnull(TTD.DiscountAmount, 0) AS DiscountAmount,
                Isnull(TTD.BasicAmount, 0) AS AfterDisAmt,
                Isnull(TTD.PurchaseTolerance, 0) AS Tolerance,
                Isnull(TTD.GSTPercentage, 0) AS GSTTaxPercentage,
                (Isnull(TTD.CGSTAmount, 0) + Isnull(TTD.SGSTAmount, 0) + Isnull(TTD.IGSTAmount, 0)) AS GSTTaxAmount,
                Isnull(TTD.NetAmount, 0) AS TotalAmount,
                NullIf(Isnull(UA.UserName, ''), '') AS CreatedBy,
                NullIf(Isnull(UM.UserName, ''), '') AS ApprovedBy,
                NullIf(TTD.FYear, '') AS FYear,
                0 AS ReceiptTransactionID,
                Isnull(TTD.IsVoucherToolApproved, 0) AS IsVoucherToolApproved,
                0 AS IsReworked,
                NullIf('', '') AS ReworkRemark,
                NullIf(TTM.PurchaseReferenceRemark, '') AS PurchaseReference,
                NullIf(TTM.Narration, '') AS Narration,
                NullIf(TTM.PurchaseDivision, '') AS PurchaseDivision,
                Isnull(TTD.RequiredQuantity, 0) AS TotalRequiredQuantity,
                NullIf(TTD.StockUnit, '') AS PurchaseStockUnit,
                Isnull(TTD.CGSTPercentage, 0) AS CGSTTaxPercentage,
                Isnull(TTD.SGSTPercentage, 0) AS SGSTTaxPercentage,
                Isnull(TTD.IGSTPercentage, 0) AS IGSTTaxPercentage,
                Isnull(TTD.CGSTAmount, 0) AS CGSTAmt,
                Isnull(TTD.SGSTAmount, 0) AS SGSTAmt,
                Isnull(TTD.IGSTAmount, 0) AS IGSTAmt,
                Isnull(TTD.TaxableAmount, 0) AS TaxableAmount,
                Replace(Convert(Varchar(13), TTD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                NullIf(PHM.ProductHSNName, '') AS ProductHSNName,
                NullIf(PHM.HSNCode, '') AS HSNCode,
                Isnull(PHM.ProductHSNID, 0) AS ProductHSNID
            FROM ToolTransactionMain AS TTM
            INNER JOIN ToolTransactionDetail AS TTD ON TTM.TransactionID = TTD.TransactionID AND TTM.CompanyID = TTD.CompanyID
            INNER JOIN ToolMaster AS TM ON TM.ToolID = TTD.ToolID
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID
            INNER JOIN UserMaster AS UA ON UA.UserID = TTM.CreatedBy
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID = TTM.LedgerID
            LEFT JOIN UserMaster AS UM ON UM.UserID = TTD.VoucherToolApprovedBy
            LEFT JOIN ToolPurchaseRequisitionDetail AS TPR ON TPR.TransactionID = TTD.TransactionID AND TPR.ToolID = TTD.ToolID AND TPR.CompanyID = TTD.CompanyID
            LEFT JOIN ToolTransactionMain AS TR ON TR.TransactionID = TPR.RequisitionTransactionID AND TR.CompanyID = TPR.CompanyID
            LEFT JOIN ToolTransactionDetail AS TRD ON TRD.TransactionID = TPR.RequisitionTransactionID AND TRD.ToolID = TPR.ToolID AND TRD.CompanyID = TPR.CompanyID
            LEFT JOIN ProductHSNMaster AS PHM ON PHM.ProductHSNID = TTD.ProductHSNID
            WHERE TTM.VoucherID = -117
              AND TTM.TransactionID = @TransactionID
              AND ISNULL(TTM.IsDeletedTransaction, 0) <> 1
            ORDER BY TransID";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPurchaseOrderDataDto>(
            sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolPOOverheadDataDto>> GetToolPurchaseOrderOverheadAsync(long transactionId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull(IPOHC.TransID, 0) AS TransID,
                Isnull(IPOHC.TransactionID, 0) AS TransactionID,
                Isnull(IPOHC.HeadID, 0) AS HeadID,
                Isnull(IPOHC.Quantity, 0) AS Weight,
                NullIf(IPOHC.ChargesType, '') AS RateType,
                Isnull(IPOHC.Amount, 0) AS HeadAmount,
                Isnull(IPOHC.Rate, 0) AS Rate,
                NullIf(IPOHC.HeadName, '') AS Head
            FROM ToolPurchaseOverheadCharges AS IPOHC
            WHERE IPOHC.TransactionID = @TransactionID
              AND ISNULL(IPOHC.IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPOOverheadDataDto>(
            sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolPOTaxDataDto>> GetToolPurchaseOrderTaxAsync(long transactionId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull(LedgerID, 0) AS LedgerID,
                Isnull(TransID, 0) AS TransID,
                Isnull(TransactionID, 0) AS TransactionID,
                Isnull(TaxRatePer, 0) AS TaxRatePer,
                Isnull(ChargesAmount, 0) AS ChargesAmount,
                Isnull(InAmount, 0) AS InAmount,
                ISNULL(IsCumulative, 0) AS IsCumulative,
                ISNULL(GSTApplicable, 0) AS GSTApplicable,
                NullIf(CalculateON, '') AS CalculateON,
                NullIf([LedgerName], '') AS LedgerName,
                NullIf([TaxType], '') AS TaxType,
                NullIf([GSTLedgerType], '') AS GSTLedgerType
            FROM (
                SELECT
                    Isnull(IPOT.LedgerID, 0) AS LedgerID,
                    Isnull(IPOT.TransID, 0) AS TransID,
                    Isnull(IPOT.TransactionID, 0) AS TransactionID,
                    Isnull(IPOT.TaxPercentage, 0) AS TaxRatePer,
                    Isnull(IPOT.Amount, 0) AS ChargesAmount,
                    Isnull(IPOT.TaxInAmount, 0) AS InAmount,
                    ISNULL(IPOT.IsComulative, 0) AS IsCumulative,
                    ISNULL(IPOT.GSTApplicable, 0) AS GSTApplicable,
                    NullIf(IPOT.CalculatedON, '') AS CalculateON,
                    [FieldName],
                    NullIf([FieldValue], '''') AS FieldValue
                FROM ToolPurchaseOrderTaxes AS IPOT
                INNER JOIN LedgerMasterDetails AS LMD ON LMD.LedgerID = IPOT.LedgerID
                WHERE IPOT.TransactionID = @TransactionID
                  AND ISNULL(IPOT.IsDeletedTransaction, 0) <> 1
            ) x
            UNPIVOT (value FOR name IN ([FieldValue])) up
            PIVOT (MAX(value) FOR FieldName IN ([LedgerName], [TaxType], [GSTLedgerType])) p
            ORDER BY TransID";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPOTaxDataDto>(
            sql, new { TransactionID = transactionId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolPurchaseOrderListDto>> GetToolPurchaseOrderListAsync(
        string fromDate, string toDate, string filterStr, bool detail)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        string sql;

        if (detail)
        {
            // Detail view - item level
            sql = @"
                SELECT
                    Isnull(ITM.TransactionID, 0) AS TransactionID,
                    Isnull(ITM.VoucherID, 0) AS VoucherID,
                    Isnull(ITM.LedgerID, 0) AS LedgerID,
                    Isnull(ITD.TransID, 0) AS TransID,
                    Isnull(ITD.ToolID, 0) AS ToolID,
                    NullIf(LM.LedgerName, '') AS LedgerName,
                    Isnull(ITM.MaxVoucherNo, 0) AS MaxVoucherNo,
                    NullIf(ITM.VoucherNo, '') AS VoucherNo,
                    Replace(Convert(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                    Isnull(ITD.PurchaseOrderQuantity, 0) AS PurchaseQuantity,
                    Isnull(ITD.PurchaseUnit, '') AS PurchaseUnit,
                    Isnull(ITD.PurchaseRate, 0) AS PurchaseRate,
                    Isnull(ITD.GrossAmount, 0) AS GrossAmount,
                    Isnull(ITD.DiscountAmount, 0) AS DiscountAmount,
                    Isnull(ITD.BasicAmount, 0) AS BasicAmount,
                    Isnull(ITD.GSTPercentage, 0) AS GSTPercentage,
                    (Isnull(ITD.CGSTAmount, 0) + Isnull(ITD.SGSTAmount, 0) + Isnull(ITD.IGSTAmount, 0)) AS GSTTaxAmount,
                    Isnull(ITD.NetAmount, 0) AS NetAmount,
                    NullIf(Isnull(UA.UserName, ''), '') AS CreatedBy,
                    NullIf(Isnull(UM.UserName, ''), '') AS ApprovedBy,
                    NullIf(ITD.FYear, '') AS FYear,
                    ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                    ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                    ISNULL(CM.CompanyName, '') AS CompanyName,
                    Isnull((SELECT TOP 1 TransactionID FROM ToolTransactionDetail
                            WHERE PurchaseTransactionID = ITM.TransactionID
                              AND CompanyID = ITD.CompanyID
                              AND Isnull(IsDeletedTransaction, 0) <> 1
                              AND Isnull(PurchaseTransactionID, 0) > 0), 0) AS ReceiptTransactionID,
                    Isnull(ITD.IsVoucherToolApproved, 0) AS IsVoucherToolApproved,
                    0 AS IsReworked,
                    NullIf('', '') AS ReworkRemark,
                    NullIf(ITM.PurchaseReferenceRemark, '') AS PurchaseReference,
                    NullIf(ITM.Narration, '') AS Narration,
                    NullIf(ITM.PurchaseDivision, '') AS PurchaseDivision,
                    NullIf(ITM.ContactPersonID, '') AS ContactPersonID,
                    (SELECT ROUND(SUM(Isnull(RequisitionProcessQuantity, 0)), 2)
                     FROM ToolPurchaseRequisitionDetail
                     WHERE TransactionID = ITD.TransactionID AND ToolID = ITD.ToolID AND CompanyID = ITD.CompanyID) AS RequiredQuantity,
                    Replace(Convert(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                    Isnull(ITM.TotalTaxAmount, 0) AS TotalTaxAmount,
                    Isnull(ITM.TotalOverheadAmount, 0) AS TotalOverheadAmount,
                    NullIf(ITM.DeliveryAddress, '') AS DeliveryAddress,
                    Isnull(ITM.TotalQuantity, '') AS TotalQuantity,
                    NullIf(ITM.TermsOfPayment, '') AS TermsOfPayment,
                    Isnull(ITD.TaxableAmount, 0) AS TaxableAmount,
                    NullIf(ITM.ModeOfTransport, '') AS ModeOfTransport,
                    NullIf(ITM.DealerID, '') AS DealerID,
                    Isnull(ITD.IsVoucherToolApproved, 0) AS VoucherToolApproved,
                    Isnull(ITD.IsCancelled, 0) AS VoucherCancelled,
                    Isnull(NullIf(ITM.CurrencyCode, ''), 'INR') AS CurrencyCode,
                    Isnull(ITM.VoucherApprovalByEmployeeID, 0) AS VoucherApprovalByEmployeeID
                FROM ToolTransactionMain AS ITM
                INNER JOIN ToolTransactionDetail AS ITD ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
                INNER JOIN UserMaster AS UA ON UA.UserID = ITM.CreatedBy
                INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID
                INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID AND ISNULL(PUM.IsDeletedTransaction, 0) = 0
                INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
                LEFT JOIN UserMaster AS UM ON UM.UserID = ITD.VoucherToolApprovedBy
                WHERE ITM.VoucherID = -117
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ITM.VoucherDate BETWEEN @FromDate AND @ToDate
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                ORDER BY FYear DESC, MaxVoucherNo DESC, TransID";
        }
        else
        {
            // Summary view - grouped by tool
            sql = @"
                SELECT
                    Isnull(ITM.TransactionID, 0) AS TransactionID,
                    Isnull(ITM.VoucherID, 0) AS VoucherID,
                    Isnull(ITM.LedgerID, 0) AS LedgerID,
                    0 AS TransID,
                    Isnull(SP.ToolID, 0) AS ToolID,
                    Isnull(SP.ToolGroupID, 0) AS ToolGroupID,
                    NullIf(LM.LedgerName, '') AS LedgerName,
                    Isnull(ITM.MaxVoucherNo, 0) AS MaxVoucherNo,
                    NullIf(ITM.VoucherNo, '') AS VoucherNo,
                    Replace(Convert(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                    NullIf(SP.ToolCode, '') AS ToolCode,
                    NullIf(SP.ToolType, '') AS ToolType,
                    NullIf(SP.ToolName, '') AS ToolName,
                    ROUND(SUM(Isnull(ITD.PurchaseOrderQuantity, 0)), 2) AS PurchaseQuantity,
                    NullIf('', '') AS PurchaseUnit,
                    0 AS PurchaseRate,
                    ROUND(SUM(Isnull(ITD.GrossAmount, 0)), 2) AS GrossAmount,
                    0 AS DiscountAmount,
                    ROUND(SUM(Isnull(ITD.BasicAmount, 0)), 2) AS BasicAmount,
                    0 AS GSTPercentage,
                    ROUND((SUM(Isnull(ITD.CGSTAmount, 0)) + SUM(Isnull(ITD.SGSTAmount, 0)) + SUM(Isnull(ITD.IGSTAmount, 0))), 2) AS GSTTaxAmount,
                    ROUND(SUM(Isnull(ITD.NetAmount, 0)), 2) AS NetAmount,
                    NullIf(Isnull(UA.UserName, ''), '') AS CreatedBy,
                    NullIf(Isnull(UM.UserName, ''), '') AS ApprovedBy,
                    NullIf(ITM.FYear, '') AS FYear,
                    ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                    ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                    ISNULL(CM.CompanyName, '') AS CompanyName,
                    0 AS ReceiptTransactionID,
                    Isnull(ITD.IsVoucherToolApproved, 0) AS IsVoucherToolApproved,
                    0 AS IsReworked,
                    NullIf(ITM.PurchaseReferenceRemark, '') AS PurchaseReference,
                    NullIf(ITM.Narration, '') AS Narration,
                    NullIf(ITM.PurchaseDivision, '') AS PurchaseDivision,
                    NullIf(ITM.ContactPersonID, '') AS ContactPersonID,
                    0 AS RequiredQuantity,
                    NullIf('', '') AS ExpectedDeliveryDate,
                    Isnull(ITM.TotalTaxAmount, 0) AS TotalTaxAmount,
                    Isnull(ITM.TotalOverheadAmount, 0) AS TotalOverheadAmount,
                    NullIf(ITM.DeliveryAddress, '') AS DeliveryAddress,
                    Isnull(ITM.TotalQuantity, '') AS TotalQuantity,
                    NullIf(ITM.TermsOfPayment, '') AS TermsOfPayment,
                    ROUND(SUM(Isnull(ITD.TaxableAmount, 0)), 2) AS TaxableAmount,
                    NullIf(ITM.ModeOfTransport, '') AS ModeOfTransport,
                    NullIf(ITM.DealerID, '') AS DealerID,
                    Isnull(ITD.IsVoucherToolApproved, 0) AS VoucherToolApproved,
                    Isnull(ITD.IsCancelled, 0) AS VoucherCancelled,
                    Isnull(NullIf(ITM.CurrencyCode, ''), 'INR') AS CurrencyCode,
                    Isnull(ITM.VoucherApprovalByEmployeeID, 0) AS VoucherApprovalByEmployeeID
                FROM ToolTransactionMain AS ITM
                INNER JOIN ToolTransactionDetail AS ITD ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
                INNER JOIN UserMaster AS UA ON UA.UserID = ITM.CreatedBy
                INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID
                LEFT JOIN UserMaster AS UM ON UM.UserID = ITD.VoucherToolApprovedBy
                INNER JOIN ToolMaster AS SP ON SP.ToolID = ITD.ToolID
                INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID AND ISNULL(PUM.IsDeletedTransaction, 0) = 0
                INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
                WHERE ITM.VoucherID = -117
                  AND ITM.ProductionUnitID = @ProductionUnitID
                  AND ITM.VoucherDate BETWEEN @FromDate AND @ToDate
                  AND ISNULL(ITD.IsDeletedTransaction, 0) <> 1
                GROUP BY Isnull(ITM.TransactionID, 0), Isnull(ITM.VoucherID, 0), Isnull(ITM.LedgerID, 0),
                    NullIf(LM.LedgerName, ''), Isnull(ITM.MaxVoucherNo, 0), NullIf(ITM.VoucherNo, ''),
                    Replace(Convert(Varchar(13), ITM.VoucherDate, 106), ' ', '-'),
                    NullIf(Isnull(UA.UserName, ''), ''), NullIf(Isnull(UM.UserName, ''), ''),
                    NullIf(ITM.FYear, ''), Isnull(ITD.IsVoucherToolApproved, 0),
                    NullIf(ITM.PurchaseReferenceRemark, ''), NullIf(ITM.Narration, ''),
                    NullIf(ITM.PurchaseDivision, ''), NullIf(ITM.ContactPersonID, ''),
                    Isnull(ITM.TotalTaxAmount, 0), Isnull(ITM.TotalOverheadAmount, 0),
                    NullIf(ITM.DeliveryAddress, ''), Isnull(ITM.TotalQuantity, ''),
                    NullIf(ITM.TermsOfPayment, ''), NullIf(ITM.ModeOfTransport, ''),
                    NullIf(ITM.DealerID, ''), Isnull(ITD.IsCancelled, 0),
                    Isnull(NullIf(ITM.CurrencyCode, ''), 'INR'), Isnull(ITM.VoucherApprovalByEmployeeID, 0),
                    NullIf(SP.ToolCode, ''), NullIf(SP.ToolType, ''), NullIf(SP.ToolName, ''),
                    Isnull(SP.ToolGroupID, 0), Isnull(SP.ToolID, 0),
                    ISNULL(PUM.ProductionUnitID, 0), ISNULL(PUM.ProductionUnitName, ''), ISNULL(CM.CompanyName, '')
                ORDER BY NullIf(ITM.FYear, '') DESC, Isnull(ITM.MaxVoucherNo, 0) DESC";
        }

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPurchaseOrderListDto>(
            sql, new { ProductionUnitID = productionUnitId, FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolPendingRequisitionDto>> GetPendingRequisitionsAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Approved tool requisitions (VoucherID=-115) with pending quantities
        var sql = @"
            SELECT
                Isnull(TTM.TransactionID, 0) AS TransactionID,
                Isnull(TTD.TransID, 0) AS TransID,
                NullIf(TTM.VoucherNo, '') AS VoucherNo,
                Replace(Convert(Varchar(13), TTM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                Isnull(TTD.ToolID, 0) AS ToolID,
                NullIf(TM.ToolCode, '') AS ToolCode,
                NullIf(TM.ToolName, '') AS ToolName,
                NullIf(TGM.ToolGroupName, '') AS ToolType,
                Isnull(TTD.ToolGroupID, 0) AS ToolGroupID,
                Isnull(TTD.RequiredQuantity, 0) AS RequiredQuantity,
                (Isnull(TTD.RequiredQuantity, 0) - Isnull(TRR.RequisitionProcessQuantity, 0)) AS PurchaseQuantity,
                (Isnull(TTD.RequiredQuantity, 0) - Isnull(TRR.RequisitionProcessQuantity, 0)) AS PendingQuantity,
                NullIf(TM.StockUnit, '') AS StockUnit,
                NullIf(TM.PurchaseUnit, '') AS PurchaseUnit,
                Replace(Convert(Nvarchar(30), TTD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                Isnull(PHM.ProductHSNID, 0) AS ProductHSNID,
                NullIf(PHM.HSNCode, '') AS HSNCode,
                Isnull(PHM.GSTTaxPercentage, 0) AS GSTTaxPercentage,
                Isnull(PHM.CGSTTaxPercentage, 0) AS CGSTTaxPercentage,
                Isnull(PHM.SGSTTaxPercentage, 0) AS SGSTTaxPercentage,
                Isnull(PHM.IGSTTaxPercentage, 0) AS IGSTTaxPercentage
            FROM ToolTransactionMain AS TTM
            INNER JOIN ToolTransactionDetail AS TTD ON TTD.TransactionID = TTM.TransactionID AND TTD.CompanyID = TTM.CompanyID
            INNER JOIN ToolMaster AS TM ON TM.ToolID = TTD.ToolID
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID
            LEFT JOIN ProductHSNMaster AS PHM ON PHM.ProductHSNID = TM.ProductHSNID
            LEFT JOIN (
                SELECT RequisitionTransactionID, ToolID, CompanyID,
                       SUM(Isnull(RequisitionProcessQuantity, 0)) AS RequisitionProcessQuantity
                FROM ToolPurchaseRequisitionDetail
                WHERE Isnull(IsDeletedTransaction, 0) = 0
                GROUP BY RequisitionTransactionID, ToolID, CompanyID
            ) AS TRR ON TRR.RequisitionTransactionID = TTD.TransactionID
                    AND TRR.ToolID = TTD.ToolID
                    AND TRR.CompanyID = TTD.CompanyID
            WHERE Isnull(TTM.VoucherID, 0) = -115
              AND TTM.ProductionUnitID = @ProductionUnitID
              AND Isnull(TTM.IsDeletedTransaction, 0) = 0
              AND Isnull(TTD.IsVoucherToolApproved, 0) = 1
              AND (Isnull(TTD.RequiredQuantity, 0) - Isnull(TRR.RequisitionProcessQuantity, 0)) > 0
            ORDER BY TTM.FYear DESC, TTM.MaxVoucherNo DESC, TTD.TransID";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPendingRequisitionDto>(
            sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<string> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 Replace(Convert(Varchar(13), VoucherDate, 106), ' ', '-')
            FROM ToolTransactionMain
            WHERE VoucherID = -117
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY VoucherDate DESC";

        var date = await connection.ExecuteScalarAsync<string>(sql, new { ProductionUnitID = productionUnitId });
        return date ?? string.Empty;
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolSupplierDto>> GetSuppliersAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                Isnull(LedgerID, 0) AS LedgerID,
                NullIf([LedgerName], '') AS LedgerName,
                NullIf([City], '') AS City,
                NullIf([State], '') AS State,
                NullIf([Country], '') AS Country,
                NullIf([MobileNo], '') AS MobileNo,
                NullIf([GSTNo], '') AS GSTNo,
                NullIf([CurrencyCode], '') AS CurrencyCode,
                NullIf([GSTApplicable], '') AS GSTApplicable,
                NullIf([StateCode], '') AS StateCode,
                NullIf([StateTinNo], '') AS StateTinNo
            FROM (
                SELECT [LedgerID], [LedgerGroupID], [CompanyID], [FieldName],
                    NullIf([FieldValue], '''') AS FieldValue
                FROM [LedgerMasterDetails]
                WHERE ISNULL(IsDeletedTransaction, 0) <> 1
                  AND LedgerGroupID IN (
                      SELECT DISTINCT LedgerGroupID FROM LedgerGroupMaster WHERE LedgerGroupNameID = 23
                  )
            ) x
            UNPIVOT (value FOR name IN ([FieldValue])) up
            PIVOT (MAX(value) FOR FieldName IN (
                [LedgerName], [City], [State], [Country], [MobileNo], [GSTNo],
                [CurrencyCode], [GSTApplicable], [StateCode], [StateTinNo]
            )) p";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolSupplierDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolContactPersonDto>> GetContactPersonsAsync(long ledgerId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull(ConcernPersonID, 0) AS ConcernPersonID,
                NullIf(Name, '') AS Name
            FROM ConcernPersonMaster
            WHERE LedgerID = @LedgerID
              AND ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY Name";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolContactPersonDto>(
            sql, new { LedgerID = ledgerId });
        return result.ToList();
    }

    public async Task<Application.DTOs.ToolInventory.ToolItemRateDto> GetItemRateAsync(long ledgerId, long toolId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT Isnull(PurchaseRate, 0) AS ItemRate
            FROM SupplierWisePurchaseSetting
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND LedgerID = @LedgerID
              AND ItemID = @ToolID";

        var result = await connection.QueryFirstOrDefaultAsync<Application.DTOs.ToolInventory.ToolItemRateDto>(
            sql, new { LedgerID = ledgerId, ToolID = toolId });
        return result ?? new Application.DTOs.ToolInventory.ToolItemRateDto();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolAllottedSupplierDto>> GetAllottedSuppliersAsync(long toolGroupId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT
                Isnull(LM.LedgerID, 0) AS LedgerID,
                NullIf(LM.LedgerName, '') AS LedgerName
            FROM SupplierItemGroupAllocation AS STGA
            INNER JOIN LedgerMaster AS LM ON STGA.LedgerID = LM.LedgerID
            WHERE STGA.ItemGroupID = @ToolGroupID
              AND ISNULL(STGA.IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolAllottedSupplierDto>(
            sql, new { ToolGroupID = toolGroupId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolOverflowGridDto>> GetOverflowGridAsync(long toolId, long toolGroupId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull(TM.ToolID, 0) AS ToolID,
                Isnull(TM.ToolGroupID, 0) AS ToolGroupID,
                NullIf(TM.ToolCode, '') AS ToolCode,
                NullIf(TM.ToolName, '') AS ToolName,
                NullIf(TGM.ToolGroupName, '') AS ToolType,
                NullIf(TM.PurchaseUnit, '') AS PurchaseUnit,
                NullIf(TM.StockUnit, '') AS StockUnit,
                Isnull(PHM.ProductHSNID, 0) AS ProductHSNID,
                NullIf(PHM.HSNCode, '') AS HSNCode,
                NullIf(PHM.ProductHSNName, '') AS ProductHSNName,
                Isnull(PHM.GSTTaxPercentage, 0) AS GSTTaxPercentage,
                Isnull(PHM.CGSTTaxPercentage, 0) AS CGSTTaxPercentage,
                Isnull(PHM.SGSTTaxPercentage, 0) AS SGSTTaxPercentage,
                Isnull(PHM.IGSTTaxPercentage, 0) AS IGSTTaxPercentage
            FROM ToolMaster AS TM
            INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID = TM.ToolGroupID
            LEFT JOIN ProductHSNMaster AS PHM ON PHM.ProductHSNID = TM.ProductHSNID
            WHERE TM.ToolID = @ToolID
              AND TM.ToolGroupID = @ToolGroupID
              AND ISNULL(TM.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolOverflowGridDto>(
            sql, new { ToolID = toolId, ToolGroupID = toolGroupId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolOverheadChargeHeadDto>> GetOverheadChargeHeadsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull(HeadID, 0) AS HeadID,
                NullIf(Head, '') AS Head,
                NullIf(RateType, '') AS RateType,
                0 AS Weight,
                0 AS Rate,
                0 AS HeadAmount
            FROM PurchaseHeadMaster";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolOverheadChargeHeadDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolTaxChargeLedgerDto>> GetTaxChargeLedgersAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull([LedgerID], 0) AS LedgerID,
                NullIf([LedgerName], '') AS LedgerName,
                Isnull([TaxPercentage], 0) AS TaxPercentage,
                NullIf([TaxType], '') AS TaxType,
                NullIf([GSTApplicable], '') AS GSTApplicable,
                NullIf([GSTLedgerType], '') AS GSTLedgerType,
                NullIf([GSTCalculationOn], '') AS GSTCalculationOn
            FROM (
                SELECT [LedgerID], [LedgerGroupID], [CompanyID], [FieldName],
                    NullIf([FieldValue], '''') AS FieldValue
                FROM [LedgerMasterDetails]
                WHERE ISNULL(IsDeletedTransaction, 0) <> 1
                  AND LedgerGroupID IN (
                      SELECT DISTINCT LedgerGroupID FROM LedgerGroupMaster WHERE LedgerGroupNameID = 43
                  )
            ) x
            UNPIVOT (value FOR name IN ([FieldValue])) up
            PIVOT (MAX(value) FOR FieldName IN (
                [LedgerName], [TaxPercentage], [TaxType], [GSTApplicable], [GSTLedgerType], [GSTCalculationOn]
            )) p";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolTaxChargeLedgerDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolHSNCodeDto>> GetHSNCodesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                Isnull(ProductHSNID, 0) AS ProductHSNID,
                NullIf(HSNCode, '') AS HSNCode,
                NullIf(ProductHSNName, '') AS ProductHSNName,
                Isnull(GSTTaxPercentage, 0) AS GSTTaxPercentage,
                Isnull(CGSTTaxPercentage, 0) AS CGSTTaxPercentage,
                Isnull(SGSTTaxPercentage, 0) AS SGSTTaxPercentage,
                Isnull(IGSTTaxPercentage, 0) AS IGSTTaxPercentage
            FROM ProductHSNMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY NullIf(ProductHSNName, '') ASC";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolHSNCodeDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolCurrencyDto>> GetCurrenciesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                NullIf(CurrencyCode, '') AS CurrencyCode,
                NullIf(CurrencyHeadName, '') AS CurrencyHeadName,
                NullIf(CurrencyChildName, '') AS CurrencyChildName,
                NullIf(CurrencySymbol, '') AS CurrencySymbol
            FROM CurrencyMaster
            WHERE Isnull(CurrencyCode, '') <> ''
            ORDER BY CurrencyCode";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolCurrencyDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.ToolInventory.ToolPOApprovalByDto>> GetPOApprovalByAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                Isnull([LedgerID], 0) AS LedgerID,
                NullIf([LedgerName], '') AS LedgerName
            FROM (
                SELECT [LedgerID], [LedgerGroupID], [CompanyID], [FieldName],
                    NullIf([FieldValue], '''') AS FieldValue
                FROM [LedgerMasterDetails]
                WHERE ISNULL(IsDeletedTransaction, 0) <> 1
                  AND LedgerGroupID IN (
                      SELECT DISTINCT LedgerGroupID FROM LedgerGroupMaster WHERE LedgerGroupNameID = 27
                  )
            ) x
            UNPIVOT (value FOR name IN ([FieldValue])) up
            PIVOT (MAX(value) FOR FieldName IN ([LedgerName])) p
            ORDER BY LedgerName";

        var result = await connection.QueryAsync<Application.DTOs.ToolInventory.ToolPOApprovalByDto>(sql);
        return result.ToList();
    }

    public async Task<bool> CheckPermissionAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // Check if any items are approved
        var approvedSql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ProductionUnitID = @ProductionUnitID
              AND TransactionID = @TransactionID
              AND ISNULL(IsVoucherToolApproved, 0) = 1
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var approvedCount = await connection.ExecuteScalarAsync<int>(approvedSql,
            new { ProductionUnitID = productionUnitId, TransactionID = transactionId });

        if (approvedCount > 0) return true;

        // Check QC approval
        var qcSql = @"
            SELECT COUNT(1) FROM ToolTransactionDetail
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND ISNULL(QCApprovalNo, '') <> ''
              AND TransactionID = @TransactionID
              AND (ISNULL(ApprovedQuantity, 0) > 0 OR ISNULL(RejectedQuantity, 0) > 0)";

        var qcCount = await connection.ExecuteScalarAsync<int>(qcSql, new { TransactionID = transactionId });
        return qcCount > 0;
    }
}
