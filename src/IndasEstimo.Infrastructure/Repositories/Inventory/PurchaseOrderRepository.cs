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

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PurchaseOrderRepository> _logger;
    public PurchaseOrderRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<PurchaseOrderRepository> logger)
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
    public async Task<long> SavePurchaseOrderAsync(
        PurchaseOrder main,
        List<PurchaseOrderDetail> details,
        List<PurchaseOrderTax> taxes,
        List<PurchaseOrderSchedule> schedules,
        List<PurchaseOrderOverhead> overheads,
        List<PurchaseOrderRequisition> requisitions)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Insert Main record (Audit fields handled by DbOperationsService)
            var transactionId = await _dbOperations.InsertDataAsync("ItemTransactionMain", main, connection, transaction, "TransactionID");

            // 2. Insert related detail records (Parent linkage and audit fields mapping handled by DbOperationsService)
            await _dbOperations.InsertDataAsync("ItemTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseOrderTaxes", taxes, connection, transaction, "POTaxID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseDeliverySchedule", schedules, connection, transaction, "ScheduleID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseOverheadCharges", overheads, connection, transaction, "OverheadID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseRequisitionDetail", requisitions, connection, transaction, "RequisitionDetailID", parentTransactionId: transactionId);

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving purchase order");
            throw;
        }
    }
    /*
    private async Task InsertPurchaseOrderAttachmentsAsync(long transactionId, List<PurchaseOrderAttachment> attachments, SqlConnection connection, SqlTransaction transaction)
    {
        foreach (var attachment in attachments)
        {
            var command = new SqlCommand(@"
                INSERT INTO ItemTransactionAttachments (
                    TransactionID, AttachmentFilesName, AttachedFileRemark,
                    CompanyID, ProductionUnitID, UserID,
                    CreatedBy, ModifiedBy, CreatedDate, ModifiedDate
                )
                VALUES (
                    @TransactionID, @AttachmentFilesName, @AttachedFileRemark,
                    @CompanyID, @ProductionUnitID, @UserID,
                    @CreatedBy, @ModifiedBy, GETDATE(), GETDATE()
                )", connection, transaction);
            command.Parameters.AddWithValue("@TransactionID", transactionId);
            command.Parameters.AddWithValue("@AttachmentFilesName", attachment.AttachmentFilesName);
            command.Parameters.AddWithValue("@AttachedFileRemark", (object?)attachment.AttachedFileRemark ?? DBNull.Value);
            command.Parameters.AddWithValue("@CompanyID", attachment.CompanyID);
            command.Parameters.AddWithValue("@ProductionUnitID", attachment.ProductionUnitID);
            command.Parameters.AddWithValue("@UserID", attachment.UserID);
            command.Parameters.AddWithValue("@CreatedBy", attachment.CreatedBy);
            command.Parameters.AddWithValue("@ModifiedBy", attachment.ModifiedBy);
            await command.ExecuteNonQueryAsync();
        }
    }
    */
    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPONumberAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ItemTransactionMain",
            -11,
            prefix);
    }
    public async Task<string?> GetVoucherNoAsync(long transactionId)
    {
        using var connection = GetConnection();
        var sql = "SELECT VoucherNo FROM ItemTransactionMain WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID";
        return await connection.ExecuteScalarAsync<string>(sql, new { TransactionID = transactionId, CompanyID = _currentUserService.GetCompanyId() });
    }
    public async Task UpdateStockValuesAsync(long transactionId)
    {
        /*
        using var connection = GetConnection();
        await connection.OpenAsync();
        var command = new SqlCommand("UPDATE_ITEM_STOCK_VALUES_UNIT_WISE", connection);
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CompanyID", _currentUserService.GetCompanyId() ?? 0);
        command.Parameters.AddWithValue("@TransactionID", transactionId);
        command.Parameters.AddWithValue("@Type", 0);
        command.LogQuery(_logger);
        await command.ExecuteNonQueryAsync();
        */
        await Task.CompletedTask;
    }
    public async Task<string> CreateApprovalWorkflowAsync(
        long transactionId,
        long transactionDetailId,
        long itemId,
        string itemDescription,
        string displayModuleName,
        long moduleId,
        string voucherNo,
        long ledgerId,
        string itemName,
        decimal purchaseQty,
        decimal itemRate,
        decimal itemAmount)
    {
        using var connection = GetConnection();
        var parameters = new DynamicParameters();
        parameters.Add("DisplayModuleName", displayModuleName);
        parameters.Add("ModuleID", moduleId);
        parameters.Add("CompanyID", _currentUserService.GetCompanyId() ?? 0);
        parameters.Add("ProductionUnitID", _currentUserService.GetProductionUnitId() ?? 0);
        parameters.Add("TransactionID", transactionId);
        parameters.Add("TransactionDetailID", transactionDetailId);
        parameters.Add("VoucherID", -11);
        parameters.Add("VoucherName", "Paper Purchase Order");
        parameters.Add("VoucherNo", voucherNo);
        parameters.Add("ItemID", itemId);
        parameters.Add("ItemDescription", itemDescription);
        parameters.Add("FYear", _currentUserService.GetFYear() ?? string.Empty);
        parameters.Add("UserID", _currentUserService.GetUserId() ?? 0);
        parameters.Add("TableName", "ItemTransactionDetail");
        parameters.Add("ApprovalColumnName", "IsVoucherItemApproved");
        parameters.Add("LedgerID", ledgerId);
        parameters.Add("ItemName", itemName);
        parameters.Add("PurchaseQty", purchaseQty);
        parameters.Add("ItemRate", itemRate);
        parameters.Add("ItemAmount", itemAmount);
        parameters.Add("PageUrl", $"PurchaseOrder.aspx?TransactionID={transactionId}");

        try
        {
            await connection.ExecuteAsync("UserApprovalProcessMultiUnit", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating approval workflow for PO {TransactionID}", transactionId);
            return $"Error: {ex.Message}";
        }
    }
    public async Task<long> UpdatePurchaseOrderAsync(
        long transactionId,
        PurchaseOrder main,
        List<PurchaseOrderDetail> details,
        List<PurchaseOrderTax> taxes,
        List<PurchaseOrderSchedule> schedules,
        List<PurchaseOrderOverhead> overheads,
        List<PurchaseOrderRequisition> requisitions)
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

            await _dbOperations.UpdateDataAsync("ItemTransactionMain", main, connection, transaction, new[] { "TransactionID", "CompanyID" });
            // 2. Delete existing details and related records
            await connection.ExecuteAsync("DELETE FROM ItemTransactionDetail WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId", new { TransactionID = transactionId, CompanyId = companyId }, transaction);
            await connection.ExecuteAsync("DELETE FROM ItemPurchaseOrderTaxes WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId", new { TransactionID = transactionId, CompanyId = companyId }, transaction);
            await connection.ExecuteAsync("DELETE FROM ItemPurchaseDeliverySchedule WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId", new { TransactionID = transactionId, CompanyId = companyId }, transaction);
            await connection.ExecuteAsync("DELETE FROM ItemPurchaseOverheadCharges WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId", new { TransactionID = transactionId, CompanyId = companyId }, transaction);
            await connection.ExecuteAsync("DELETE FROM ItemPurchaseRequisitionDetail WHERE TransactionID = @TransactionID AND CompanyID = @CompanyId", new { TransactionID = transactionId, CompanyId = companyId }, transaction);
            // 3. Re-insert everything (Audit fields and parent linking handled by DbOperationsService)
            await _dbOperations.InsertDataAsync("ItemTransactionDetail", details, connection, transaction, "TransactionDetailID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseOrderTaxes", taxes, connection, transaction, "POTaxID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseDeliverySchedule", schedules, connection, transaction, "ScheduleID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseOverheadCharges", overheads, connection, transaction, "OverheadID", parentTransactionId: transactionId);
            await _dbOperations.InsertDataAsync("ItemPurchaseRequisitionDetail", requisitions, connection, transaction, "RequisitionDetailID", parentTransactionId: transactionId);
            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating purchase order {TransactionId}", transactionId);
            throw;
        }
    }
    public async Task<bool> IsPurchaseOrderUsedAsync(long transactionId)
    {
        using var connection = GetConnection();
        // Check if this PO is referenced as a requisition in other details (e.g. GRN)
        var sql = "SELECT COUNT(1) FROM ItemTransactionDetail WHERE RequisitionTransactionID = @TransactionID AND CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction,0)=0";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { TransactionID = transactionId, CompanyID = _currentUserService.GetCompanyId() });
        return count > 0;
    }
    public async Task<bool> IsPurchaseOrderApprovedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var sql = @"
            SELECT TOP 1 IsApproved 
            FROM UserApprovalTransactionsDetail 
            WHERE RecordTransactionDetailID IN (
                SELECT TransactionDetailID FROM ItemTransactionDetail 
                WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction,0)=0
            ) AND CompanyID = @CompanyID";
        var isApproved = await connection.ExecuteScalarAsync<bool?>(sql, new { TransactionID = transactionId, CompanyID = _currentUserService.GetCompanyId() });
        return isApproved ?? false;
    }

    public async Task<bool> DeletePurchaseOrderAsync(long transactionId)
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

            await connection.ExecuteAsync(deleteMainSql, new { DeletedBy = userId, TransactionID = transactionId, CompanyID = companyId }, transaction);

            // 2. Clear existing approvals
            string deleteApprovalsSql = @"
                DELETE FROM UserApprovalTransactionsDetail 
                WHERE RecordTransactionID = @TransactionID 
                  AND CompanyID = @CompanyID";
            
            await connection.ExecuteAsync(deleteApprovalsSql, new { TransactionID = transactionId, CompanyID = companyId }, transaction);

            // 3. Soft Delete Details
            string deleteDetailSql = @"
                UPDATE ItemTransactionDetail 
                SET IsDeletedTransaction = 1 
                WHERE TransactionID = @TransactionID 
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(deleteDetailSql, new { TransactionID = transactionId, CompanyID = companyId }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting purchase order {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<(bool IsApprovalRequired, int IsVoucherItemApproved, long ApprovalByUserId, long ModuleId, string DisplayModuleName)>
        CheckApprovalRequirementAsync(long companyId, string formName)
    {
        using var connection = GetConnection();

        try
        {
            var sql = @"
                SELECT
                    ISNULL(DTA.IsApprovalRequired, 0) AS IsApprovalRequired,
                    ISNULL(DTA.IsVoucherItemApproved, 0) AS IsVoucherItemApproved,
                    ISNULL(DTA.VoucherItemApprovedBy, 0) AS VoucherItemApprovedBy,
                    ISNULL(DTA.ModuleID, 0) AS ModuleID,
                    ISNULL(MM.DisplayModuleName, '') AS DisplayModuleName
                FROM DynamicTransactionApproval AS DTA
                INNER JOIN ModuleMaster AS MM ON MM.ModuleID = DTA.ModuleID
                WHERE DTA.CompanyID = @CompanyID
                  AND MM.FormName = @FormName
                  AND ISNULL(DTA.IsDeletedTransaction, 0) = 0";

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { CompanyID = companyId, FormName = formName });

            if (result == null)
            {
                return (false, 1, 0, 0, "Purchase Order"); // Default: no approval required
            }

            return (
                (bool)result.IsApprovalRequired,
                (int)result.IsVoucherItemApproved,
                (long)result.VoucherItemApprovedBy,
                (long)result.ModuleID,
                (string)result.DisplayModuleName
            );
        }
        catch (Exception ex)
        {
            // If DynamicTransactionApproval table doesn't exist, return defaults (no approval required)
            _logger.LogWarning(ex, "DynamicTransactionApproval table not found. Defaulting to no approval required.");
            return (false, 1, 0, 0, "Purchase Order");
        }
    }

    // ==================== Retrieve Operations ====================

    public async Task<List<Application.DTOs.Inventory.PurchaseOrderDataDto>> GetPurchaseOrderDataAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // This query matches the RetrivePoCreateGrid WebMethod - complex join across multiple tables
        var sql = @"
            SELECT
                itm.TransactionID,
                itd.TransactionDetailID AS TransID,
                itm.VoucherID,
                itm.VoucherNo,
                itm.VoucherDate,
                itm.LedgerID,
                lm.LedgerName,
                itm.ContactPersonID,
                ISNULL(cp.Name, '') AS ContactPersonName,
                itd.ItemID,
                im.ItemCode,
                im.ItemName,
                ISNULL(im.ItemDescription, '') AS ItemDescription,
                itd.ItemGroupID,
                itd.RequiredQuantity,
                itd.RequiredNoOfPacks,
                itd.QuantityPerPack,
                itd.PurchaseOrderQuantity,
                itd.ChallanWeight,
                itd.PurchaseUnit,
                itd.StockUnit,
                itd.PurchaseRate,
                itd.PurchaseTolerance,
                itd.GrossAmount,
                itd.DiscountPercentage,
                itd.DiscountAmount,
                itd.BasicAmount,
                itd.TaxableAmount,
                itd.GSTPercentage,
                itd.CGSTPercentage,
                itd.SGSTPercentage,
                itd.IGSTPercentage,
                itd.CGSTAmount,
                itd.SGSTAmount,
                itd.IGSTAmount,
                itd.NetAmount,
                ISNULL(itd.ItemNarration, '') AS ItemNarration,
                itd.ExpectedDeliveryDate,
                ISNULL(itd.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(itd.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(itd.ClientID, 0) AS ClientID,
                ISNULL(cl.LedgerName, '') AS ClientName,
                ISNULL(itd.Remark, '') AS Remark,
                ISNULL(itd.ProductHSNID, 0) AS ProductHSNID,
                ISNULL(hsn.HSNCode, '') AS HSNCode,
                itm.TotalQuantity,
                itm.TotalBasicAmount,
                itm.TotalCGSTTaxAmount,
                itm.TotalSGSTTaxAmount,
                itm.TotalIGSTTaxAmount,
                itm.TotalTaxAmount,
                itm.TotalOverheadAmount,
                itm.NetAmount AS TotalNetAmount,
                itm.PurchaseDivision,
                ISNULL(itm.PurchaseReferenceRemark, '') AS PurchaseReferenceRemark,
                ISNULL(itm.DeliveryAddress, '') AS DeliveryAddress,
                ISNULL(itm.TermsOfPayment, '') AS TermsOfPayment,
                ISNULL(itm.CurrencyCode, 'INR') AS CurrencyCode,
                itm.ModeOfTransport,
                ISNULL(itm.DealerID, 0) AS DealerID,
                ISNULL(itm.VoucherApprovalByEmployeeID, 0) AS VoucherApprovalByEmployeeID,
                ISNULL(itm.Narration, '') AS Narration,
                ISNULL(itd.IsVoucherItemApproved, 0) AS IsVoucherItemApproved,
                ISNULL(itd.VoucherItemApprovedBy, 0) AS VoucherItemApprovedBy,
                itd.VoucherItemApprovedDate,
                ISNULL(im.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(im.UnitPerPacking, 0) AS UnitPerPacking,
                ISNULL(im.ConversionFactor, 0) AS ConversionFactor,
                ISNULL(cm.ConversionFormula, '') AS ConversionFormula
            FROM ItemTransactionMain itm
            INNER JOIN ItemTransactionDetail itd ON itm.TransactionID = itd.TransactionID AND itd.CompanyID = @CompanyID
            INNER JOIN ItemMaster im ON itd.ItemID = im.ItemID
            LEFT JOIN LedgerMaster lm ON itm.LedgerID = lm.LedgerID
            LEFT JOIN ConcernPersonMaster cp ON itm.ContactPersonID = cp.ConcernPersonID
            LEFT JOIN LedgerMaster cl ON itd.ClientID = cl.LedgerID
            LEFT JOIN ProductHSNMaster hsn ON itd.ProductHSNID = hsn.ProductHSNID
            LEFT JOIN ConversionMaster cm ON im.PurchaseUnit = cm.BaseUnitSymbol AND im.StockUnit = cm.ConvertedUnitSymbol
            WHERE itm.TransactionID = @TransactionID
              AND itm.CompanyID = @CompanyID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.PurchaseOrderDataDto>(
            sql,
            new { TransactionID = transactionId, CompanyID = companyId }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.PurchaseOrderListDto>> GetPurchaseOrderListAsync(
        string fromDate,
        string toDate,
        string filterStr,
        bool detail)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // This matches the ProcessFillGrid WebMethod
        var sql = detail ?
            // Detail view - item level
            @"SELECT
                itm.TransactionID,
                itd.TransactionDetailID AS TransID,
                itm.VoucherNo,
                itm.VoucherDate,
                itm.LedgerID,
                lm.LedgerName,
                itd.ItemID,
                im.ItemCode,
                im.ItemName,
                itd.PurchaseOrderQuantity,
                itd.PurchaseRate,
                itd.NetAmount,
                itm.NetAmount AS TotalNetAmount,
                ISNULL(itd.IsVoucherItemApproved, 0) AS IsVoucherItemApproved,
                (itd.PurchaseOrderQuantity - ISNULL((
                    SELECT SUM(ChallanQuantity)
                    FROM ItemTransactionDetail
                    WHERE PurchaseTransactionID = itd.TransactionDetailID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)) AS PendingToReceiveQty,
                itd.ExpectedDeliveryDate,
                ISNULL(itm.PurchaseReferenceRemark, '') AS PurchaseReferenceRemark,
                itm.ProductionUnitID,
                pu.ProductionUnitName
            FROM ItemTransactionMain itm
            INNER JOIN ItemTransactionDetail itd ON itm.TransactionID = itd.TransactionID AND itd.CompanyID = @CompanyID
            INNER JOIN ItemMaster im ON itd.ItemID = im.ItemID
            LEFT JOIN LedgerMaster lm ON itm.LedgerID = lm.LedgerID
            LEFT JOIN ProductionUnitMaster pu ON itm.ProductionUnitID = pu.ProductionUnitID
            WHERE itm.VoucherID = -11
              AND itm.CompanyID = @CompanyID
              AND itm.ProductionUnitID = @ProductionUnitID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0
              AND (@FromDate = '' OR itm.VoucherDate >= CAST(@FromDate AS DATE))
              AND (@ToDate = '' OR itm.VoucherDate <= CAST(@ToDate AS DATE))
              AND (@FilterStr = '' OR itm.VoucherNo LIKE '%' + @FilterStr + '%' OR lm.LedgerName LIKE '%' + @FilterStr + '%')
            ORDER BY itm.VoucherDate DESC, itm.TransactionID DESC" :
            // Summary view - header level
            @"SELECT DISTINCT
                itm.TransactionID,
                0 AS TransID,
                itm.VoucherNo,
                itm.VoucherDate,
                itm.LedgerID,
                lm.LedgerName,
                0 AS ItemID,
                '' AS ItemCode,
                '' AS ItemName,
                itm.TotalQuantity AS PurchaseOrderQuantity,
                0 AS PurchaseRate,
                itm.NetAmount,
                itm.NetAmount AS TotalNetAmount,
                CAST(CASE WHEN EXISTS(
                    SELECT 1 FROM ItemTransactionDetail
                    WHERE TransactionID = itm.TransactionID
                      AND CompanyID = @CompanyID
                      AND IsVoucherItemApproved = 0
                ) THEN 0 ELSE 1 END AS INT) AS IsVoucherItemApproved,
                0 AS PendingToReceiveQty,
                NULL AS ExpectedDeliveryDate,
                ISNULL(itm.PurchaseReferenceRemark, '') AS PurchaseReferenceRemark,
                itm.ProductionUnitID,
                pu.ProductionUnitName
            FROM ItemTransactionMain itm
            LEFT JOIN LedgerMaster lm ON itm.LedgerID = lm.LedgerID
            LEFT JOIN ProductionUnitMaster pu ON itm.ProductionUnitID = pu.ProductionUnitID
            WHERE itm.VoucherID = -11
              AND itm.CompanyID = @CompanyID
              AND itm.ProductionUnitID = @ProductionUnitID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0
              AND (@FromDate = '' OR itm.VoucherDate >= CAST(@FromDate AS DATE))
              AND (@ToDate = '' OR itm.VoucherDate <= CAST(@ToDate AS DATE))
              AND (@FilterStr = '' OR itm.VoucherNo LIKE '%' + @FilterStr + '%' OR lm.LedgerName LIKE '%' + @FilterStr + '%')
            ORDER BY itm.VoucherDate DESC, itm.TransactionID DESC";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.PurchaseOrderListDto>(
            sql,
            new
            {
                CompanyID = companyId,
                ProductionUnitID = productionUnitId,
                FromDate = fromDate,
                ToDate = toDate,
                FilterStr = filterStr
            }
        );

        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.PendingRequisitionDto>> GetPendingRequisitionsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        // This matches the FillGrid WebMethod for pending requisitions
        var sql = @"
            SELECT
                itm.TransactionID,
                itd.TransactionDetailID AS TransID,
                itm.VoucherNo,
                itm.VoucherDate,
                itd.ItemID,
                im.ItemCode,
                im.ItemName,
                ISNULL(im.ItemDescription, '') AS ItemDescription,
                itd.ItemGroupID,
                itd.RequiredQuantity,
                (itd.RequiredQuantity - ISNULL((
                    SELECT SUM(RequisitionProcessQuantity)
                    FROM ItemPurchaseRequisitionDetail
                    WHERE RequisitionTransactionID = itm.TransactionID
                      AND ItemID = itd.ItemID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)) AS PurchaseQuantity,
                (itd.RequiredQuantity - ISNULL((
                    SELECT SUM(RequisitionProcessQuantity)
                    FROM ItemPurchaseRequisitionDetail
                    WHERE RequisitionTransactionID = itm.TransactionID
                      AND ItemID = itd.ItemID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)) AS PendingQuantity,
                ISNULL(im.PurchaseRate, 0) AS PurchaseRate,
                itd.StockUnit,
                im.PurchaseUnit,
                itd.ExpectedDeliveryDate,
                ISNULL(itd.ProductHSNID, 0) AS ProductHSNID,
                ISNULL(hsn.HSNCode, '') AS HSNCode,
                ISNULL(hsn.GSTTaxPercentage, 0) AS GSTPercentage,
                ISNULL(hsn.CGSTTaxPercentage, 0) AS CGSTPercentage,
                ISNULL(hsn.SGSTTaxPercentage, 0) AS SGSTPercentage,
                ISNULL(hsn.IGSTTaxPercentage, 0) AS IGSTPercentage,
                ISNULL(itd.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(itd.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(itd.ClientID, 0) AS ClientID,
                ISNULL(cl.LedgerName, '') AS ClientName,
                ISNULL(im.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(im.UnitPerPacking, 0) AS UnitPerPacking,
                ISNULL(im.ConversionFactor, 0) AS ConversionFactor,
                ISNULL(cm.ConversionFormula, '') AS ConversionFormula
            FROM ItemTransactionMain itm
            INNER JOIN ItemTransactionDetail itd ON itm.TransactionID = itd.TransactionID AND itd.CompanyID = @CompanyID
            INNER JOIN ItemMaster im ON itd.ItemID = im.ItemID
            LEFT JOIN ProductHSNMaster hsn ON itd.ProductHSNID = hsn.ProductHSNID
            LEFT JOIN LedgerMaster cl ON itd.ClientID = cl.LedgerID
            LEFT JOIN ConversionMaster cm ON im.PurchaseUnit = cm.BaseUnitSymbol AND im.StockUnit = cm.ConvertedUnitSymbol
            WHERE itm.VoucherID = -9
              AND itm.CompanyID = @CompanyID
              AND itm.ProductionUnitID = @ProductionUnitID
              AND ISNULL(itm.IsDeletedTransaction, 0) = 0
              AND ISNULL(itd.IsVoucherItemApproved, 0) = 1
              AND itd.RequiredQuantity > ISNULL((
                    SELECT SUM(RequisitionProcessQuantity)
                    FROM ItemPurchaseRequisitionDetail
                    WHERE RequisitionTransactionID = itm.TransactionID
                      AND ItemID = itd.ItemID
                      AND CompanyID = @CompanyID
                      AND ISNULL(IsDeletedTransaction, 0) = 0
                ), 0)
            ORDER BY itm.VoucherDate DESC, itd.ItemID";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.PendingRequisitionDto>(
            sql,
            new { CompanyID = companyId, ProductionUnitID = productionUnitId }
        );

        return result.ToList();
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<string> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 CONVERT(VARCHAR(10), VoucherDate, 120) AS VoucherDate
            FROM ItemTransactionMain
            WHERE VoucherID = -11
              AND CompanyID = @CompanyID
              AND ProductionUnitID = @ProductionUnitID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY VoucherDate DESC";

        var result = await connection.ExecuteScalarAsync<string>(sql, new { CompanyID = companyId, ProductionUnitID = productionUnitId });
        return result ?? DateTime.Now.ToString("yyyy-MM-dd");
    }

    public async Task<List<Application.DTOs.Inventory.SupplierDto>> GetSuppliersAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // LedgerGroupNameID = 23 is for Suppliers (from legacy code)
        // Fixed: Join with LedgerGroupMaster to get LedgerGroupNameID, and CountryStateMaster for StateCode/StateTinNo
        var sql = @"
            SELECT
                A.LedgerID,
                A.LedgerName,
                ISNULL(A.City, '') AS City,
                ISNULL(A.State, '') AS State,
                ISNULL(A.Country, '') AS Country,
                ISNULL(A.MobileNo, '') AS MobileNo,
                ISNULL(A.GSTNo, '') AS GSTNo,
                ISNULL(A.CurrencyCode, 'INR') AS CurrencyCode,
                ISNULL(A.GSTApplicable, 0) AS GSTApplicable,
                ISNULL(S.StateCode, '') AS StateCode,
                ISNULL(S.StateTinNo, 0) AS StateTinNo
            FROM LedgerMaster AS A
            INNER JOIN LedgerGroupMaster AS B ON A.LedgerGroupID = B.LedgerGroupID
            LEFT JOIN CountryStateMaster AS S ON S.State = A.State
            WHERE B.LedgerGroupNameID = 23
              AND A.CompanyID = @CompanyID
              AND ISNULL(A.IsLedgerActive, 1) = 1
              AND ISNULL(A.IsDeletedTransaction, 0) = 0
            ORDER BY A.LedgerName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.SupplierDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.ContactPersonDto>> GetContactPersonsAsync(long ledgerId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ConcernPersonID,
                Name
            FROM ConcernPersonMaster
            WHERE LedgerID = @LedgerID
            ORDER BY Name";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.ContactPersonDto>(sql, new { LedgerID = ledgerId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.DeliveryAddressDto>> GetDeliveryAddressesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT
                DeliveryAddress,
                CompanyID
            FROM DeliveryAddresses
            WHERE CompanyID = @CompanyID";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.DeliveryAddressDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.OverheadChargeHeadDto>> GetOverheadChargeHeadsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // This would typically come from a master table - using a simple query for now
        var sql = @"
            SELECT DISTINCT
                HeadID,
                HeadName
            FROM ItemPurchaseOverheadCharges
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY HeadName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.OverheadChargeHeadDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.TaxChargeLedgerDto>> GetTaxChargeLedgersAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // LedgerGroupNameID = 43 is for Indirect Expenses/Charges (from legacy code)
        // Fixed: Use subquery to filter by LedgerGroupNameID which is in LedgerGroupMaster
        var sql = @"
            SELECT
                LedgerID,
                LedgerName,
                ISNULL(TaxPercentage, 0) AS TaxPercentage,
                ISNULL(TaxType, '') AS TaxType,
                ISNULL(GSTApplicable, 0) AS GSTApplicable,
                ISNULL(GSTLedgerType, '') AS GSTLedgerType
            FROM LedgerMaster
            WHERE IsLedgerActive = 1
              AND IsDeletedTransaction = 0
              AND LedgerGroupID IN (
                  SELECT DISTINCT LedgerGroupID
                  FROM LedgerGroupMaster
                  WHERE LedgerGroupNameID = 43
              )
            ORDER BY LedgerName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.TaxChargeLedgerDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.CurrencyDto>> GetCurrenciesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                CurrencyCode,
                CurrencyHeadName,
                ISNULL(CurrencySymbol, '') AS CurrencySymbol
            FROM CurrencyMaster
            ORDER BY CurrencyHeadName";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.CurrencyDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.HSNCodeDto>> GetHSNCodesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ProductHSNID,
                HSNCode,
                ProductHSNName,
                ISNULL(GSTTaxPercentage, 0) AS GSTTaxPercentage,
                ISNULL(CGSTTaxPercentage, 0) AS CGSTTaxPercentage,
                ISNULL(SGSTTaxPercentage, 0) AS SGSTTaxPercentage,
                ISNULL(IGSTTaxPercentage, 0) AS IGSTTaxPercentage
            FROM ProductHSNMaster
            ORDER BY HSNCode";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.HSNCodeDto>(sql);
        return result.ToList();
    }

    public async Task<List<Application.DTOs.Inventory.AttachmentFileDto>> GetAttachmentsAsync(long transactionId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT
                AttachmentFileID,
                AttachmentFilesName,
                ISNULL(AttachedFileRemark, '') AS AttachedFileRemark
            FROM ItemTransactionAttachments
            WHERE TransactionID = @TransactionID
              AND CompanyID = @CompanyID";

        var result = await connection.QueryAsync<Application.DTOs.Inventory.AttachmentFileDto>(sql, new { TransactionID = transactionId, CompanyID = companyId });
        return result.ToList();
    }
}
