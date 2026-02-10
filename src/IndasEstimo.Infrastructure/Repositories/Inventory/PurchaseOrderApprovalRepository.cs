using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Extensions;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;

public class PurchaseOrderApprovalRepository : IPurchaseOrderApprovalRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PurchaseOrderApprovalRepository> _logger;

    public PurchaseOrderApprovalRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<PurchaseOrderApprovalRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    /// <summary>
    /// Get unapproved purchase orders - matches UnApprovedPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=0, IsCancelled=0
    /// </summary>
    public async Task<List<UnapprovedPurchaseOrderDto>> GetUnapprovedPurchaseOrdersAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ITM.TransactionID,
                ITD.ClientID,
                NULLIF(LM1.LedgerName,'') as ClientName,
                ITM.VoucherID,
                ITM.LedgerID,
                ITD.TransID,
                ITD.ItemID,
                ITD.ItemGroupID,
                (SELECT COUNT(TransactionDetailID)
                 FROM ItemTransactionDetail
                 WHERE (TransactionID = ITM.TransactionID)
                   AND (CompanyID = ITM.CompanyID)
                   AND (IsDeletedTransaction = 0)) AS TotalItems,
                LM.LedgerName,
                ITM.MaxVoucherNo,
                ITM.VoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                IM.ItemCode,
                IM.ItemName,
                ITD.PurchaseOrderQuantity,
                ITD.PurchaseUnit,
                ITD.PurchaseRate,
                ITD.GrossAmount,
                ITD.DiscountAmount,
                ITD.BasicAmount,
                ITD.GSTPercentage,
                ITD.CGSTAmount + ITD.SGSTAmount + ITD.IGSTAmount As GSTTaxAmount,
                ITD.NetAmount,
                ITD.RefJobCardContentNo,
                UA.UserName As CreatedBy,
                UM.UserName As ApprovedBy,
                ITM.FYear,
                0 As ReceiptTransactionID,
                ITM.PurchaseDivision,
                ITM.CurrencyCode,
                ITD.AuditApprovedBy,
                ITM.DealerID,
                ITM.PurchaseReferenceRemark,
                ITM.ModeOfTransport,
                ITM.DeliveryAddress,
                ITM.TermsOfDelivery,
                ITM.TermsOfPayment,
                ITD.TaxableAmount,
                ITM.TotalTaxAmount,
                ITD.BasicAmount As AfterDisAmt,
                ITM.Narration,
                (SELECT Replace(Convert(Varchar(13),Max(IT.VoucherDate),106),' ','-')
                 FROM ItemTransactionMain AS IT
                 INNER JOIN ItemTransactionDetail AS ID ON IT.TransactionID=ID.TransactionID
                 WHERE IT.VoucherID=-11
                   AND Isnull(IT.IsDeletedTransaction,0) = 0
                   AND Isnull(ID.IsDeletedTransaction,0) = 0
                   AND ID.ItemID=ITD.ItemID
                   AND IT.TransactionID < ITM.TransactionID) AS LastPODate,
                Isnull(PUM.ProductionUnitID, 0) As ProductionUnitID,
                Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,
                Nullif(CM.CompanyName,'') AS CompanyName,
                Isnull(CM.CompanyID, 0) As CompanyID
            FROM ItemTransactionMain As ITM
            INNER JOIN ItemTransactionDetail As ITD ON ITM.TransactionID=ITD.TransactionID
                AND ITM.CompanyID=ITD.CompanyID
            INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID
            INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy
            LEFT JOIN UserMaster As UM ON UM.UserID=ITD.VoucherItemApprovedBy
            INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID
            INNER JOIN LedgerMaster As LM ON LM.LedgerID=ITM.LedgerID
            LEFT JOIN LedgerMaster As LM1 ON LM1.LedgerID = ITD.ClientID
            WHERE ITM.VoucherID= -11
              AND ITM.ProductionUnitID In(" + productionUnitIdStr + @")
              AND ITD.IsDeletedTransaction=0
              AND ITD.IsVoucherItemApproved =0
              AND ITD.IsCancelled =0
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= @FromDate))
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate))
            ORDER BY ITM.FYear Desc, ITM.MaxVoucherNo Desc";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<UnapprovedPurchaseOrderDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Get approved purchase orders - matches ApprovedPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=1
    /// </summary>
    public async Task<List<ApprovedPurchaseOrderDto>> GetApprovedPurchaseOrdersAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ITM.TransactionID,
                ITM.MaxVoucherNo,
                ITD.ClientID,
                NULLIF(LM1.LedgerName,'') as ClientName,
                ITM.VoucherID,
                ITM.LedgerID,
                ITD.TransID,
                ITD.ItemID,
                ITD.ItemGroupID,
                (SELECT COUNT(TransactionDetailID)
                 FROM ItemTransactionDetail
                 WHERE TransactionID=ITM.TransactionID
                   AND CompanyID=ITM.CompanyID
                   AND IsDeletedTransaction=0) AS TotalItems,
                LM.LedgerName,
                ITM.VoucherNo,
                Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,
                IM.ItemCode,
                IM.ItemName,
                ITD.PurchaseOrderQuantity,
                ITD.PurchaseUnit,
                ITD.PurchaseRate,
                ITD.GrossAmount,
                ITD.DiscountAmount,
                ITD.BasicAmount,
                ITD.GSTPercentage,
                (Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,
                ITD.NetAmount,
                ITD.RefJobCardContentNo,
                UA.UserName AS CreatedBy,
                UM.UserName AS ApprovedBy,
                Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,
                ITM.FYear,
                PurchaseDivision,
                ITM.CurrencyCode,
                AuditApprovedBy,
                DealerID,
                PurchaseReferenceRemark,
                ModeOfTransport,
                DeliveryAddress,
                TermsOfDelivery,
                ITM.TermsOfPayment,
                TaxableAmount,
                ITM.NetAmount,
                TotalTaxAmount,
                ITD.BasicAmount As AfterDisAmt,
                Narration,
                PUM.ProductionUnitID,
                PUM.ProductionUnitName,
                CM.CompanyName,
                Isnull(CM.CompanyID, 0) As CompanyID
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID
                AND ITM.CompanyID=ITD.CompanyID
            INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy
            INNER JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID
            LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy
            INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID
            LEFT JOIN LedgerMaster as LM1 on LM1.LedgerID = ITD.ClientID
            WHERE ITM.VoucherID= -11
              AND ITM.ProductionUnitID In(" + productionUnitIdStr + @")
              AND ITD.IsDeletedTransaction = 0
              AND ITD.IsVoucherItemApproved = 1
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= @FromDate))
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate))
            ORDER BY ITM.FYear Desc,ITM.MaxVoucherNo Desc";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<ApprovedPurchaseOrderDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Get cancelled purchase orders - matches CancelledPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=0, IsCancelled=1
    /// </summary>
    public async Task<List<CancelledPurchaseOrderDto>> GetCancelledPurchaseOrdersAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = $@"
            SELECT
                ITM.TransactionID,
                ITD.ClientID,
                NULLIF(LM1.LedgerName,'') as ClientName,
                ITM.VoucherID,
                ITM.LedgerID,
                ITD.TransID,
                ITD.ItemID,
                ITD.ItemGroupID,
                (SELECT COUNT(TransactionDetailID)
                 FROM ItemTransactionDetail
                 WHERE TransactionID=ITM.TransactionID
                   AND CompanyID=ITM.CompanyID
                   AND Isnull(IsDeletedTransaction,0)=0) AS TotalItems,
                LM.LedgerName,
                ITM.MaxVoucherNo,
                ITM.VoucherNo,
                Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,
                NULLIF(IM.ItemCode,'') AS ItemCode,
                NULLIF(Isnull(IM.ItemName,''),'') AS ItemName,
                ITD.PurchaseOrderQuantity,
                ITD.PurchaseUnit,
                ITD.PurchaseRate,
                ITD.GrossAmount,
                ITD.DiscountAmount,
                ITD.BasicAmount,
                ITD.GSTPercentage,
                (Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,
                ITD.NetAmount,
                ITD.RefJobCardContentNo,
                UA.UserName AS CreatedBy,
                UM.UserName AS ApprovedBy,
                Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,
                ITM.FYear,
                PurchaseDivision,
                ITM.CurrencyCode,
                AuditApprovedBy,
                DealerID,
                PurchaseReferenceRemark,
                ModeOfTransport,
                DeliveryAddress,
                TermsOfDelivery,
                ITM.TermsOfPayment,
                TaxableAmount,
                ITM.NetAmount,
                TotalTaxAmount,
                ITD.BasicAmount As AfterDisAmt,
                Narration,
                (SELECT Replace(Convert(Varchar(13),Max(IT.VoucherDate),106),' ','-')
                 FROM ItemTransactionMain AS IT
                 INNER JOIN ItemTransactionDetail AS ID ON IT.TransactionID=ID.TransactionID
                 WHERE IT.VoucherID=-11
                   AND Isnull(IT.IsDeletedTransaction,0) = 0
                   AND Isnull(ID.IsDeletedTransaction,0) = 0
                   AND ID.ItemID=ITD.ItemID
                   AND IT.TransactionID < ITM.TransactionID) AS LastPODate,
                PUM.ProductionUnitID,
                PUM.ProductionUnitName,
                CM.CompanyName,
                Isnull(CM.CompanyID, 0) As CompanyID
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID
                AND ITM.CompanyID=ITD.CompanyID
            INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy
            INNER JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID
            LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy
            INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID
            INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID
            LEFT JOIN LedgerMaster as LM1 on LM1.LedgerID = ITD.ClientID
            WHERE ITM.VoucherID= -11
              AND ITM.ProductionUnitID In(" + productionUnitIdStr + @")
              AND ITD.IsDeletedTransaction=0
              AND ITD.IsVoucherItemApproved=0
              AND ITD.IsCancelled=1
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= @FromDate))
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate))
            ORDER BY ITM.FYear Desc,ITM.MaxVoucherNo Desc";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<CancelledPurchaseOrderDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Check if PO is used in receipts - matches IsPurchaseOrdersProcessed WebMethod
    /// </summary>
    public async Task<bool> IsPurchaseOrderProcessedAsync(long transactionId)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT COUNT(1)
            FROM ItemTransactionDetail
            WHERE PurchaseTransactionID = @TransactionID
              AND ProductionUnitID = @ProductionUnitID
              AND IsDeletedTransaction = 0";

        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            TransactionID = transactionId,
            ProductionUnitID = productionUnitId
        });

        return count > 0;
    }

    /// <summary>
    /// Approve purchase orders - matches UpdateData WebMethod with "Approve"
    /// </summary>
    public async Task<bool> ApprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            foreach (var item in items)
            {
                var sql = @"
                    UPDATE ItemTransactionDetail
                    SET IsVoucherItemApproved = 1,
                        VoucherItemApprovedBy = @UserId,
                        VoucherItemApprovedDate = GETDATE(),
                        IsCancelled = 0,
                        ModifiedBy = @UserId,
                        ModifiedDate = GETDATE()
                    WHERE TransactionID = @TransactionID
                      AND ItemID = @ItemID
                      AND TransID = @TransID
                      AND CompanyID = @CompanyID";

                await connection.ExecuteAsync(sql, new
                {
                    TransactionID = item.TransactionID,
                    ItemID = item.ItemID,
                    TransID = item.TransID,
                    CompanyID = companyId,
                    UserId = userId
                }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error approving purchase orders");
            throw;
        }
    }

    /// <summary>
    /// Cancel purchase orders - matches UpdateData WebMethod with "Cancel"
    /// </summary>
    public async Task<bool> CancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            foreach (var item in items)
            {
                var sql = @"
                    UPDATE ItemTransactionDetail
                    SET IsCancelled = 1,
                        ModifiedBy = @UserId,
                        ModifiedDate = GETDATE()
                    WHERE TransactionID = @TransactionID
                      AND ItemID = @ItemID
                      AND TransID = @TransID
                      AND CompanyID = @CompanyID";

                await connection.ExecuteAsync(sql, new
                {
                    TransactionID = item.TransactionID,
                    ItemID = item.ItemID,
                    TransID = item.TransID,
                    CompanyID = companyId,
                    UserId = userId
                }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error cancelling purchase orders");
            throw;
        }
    }

    /// <summary>
    /// Unapprove purchase orders - reverse the approval
    /// </summary>
    public async Task<bool> UnapprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            foreach (var item in items)
            {
                var sql = @"
                    UPDATE ItemTransactionDetail
                    SET IsVoucherItemApproved = 0,
                        VoucherItemApprovedBy = 0,
                        VoucherItemApprovedDate = NULL,
                        ModifiedBy = @UserId,
                        ModifiedDate = GETDATE()
                    WHERE TransactionID = @TransactionID
                      AND ItemID = @ItemID
                      AND TransID = @TransID
                      AND CompanyID = @CompanyID";

                await connection.ExecuteAsync(sql, new
                {
                    TransactionID = item.TransactionID,
                    ItemID = item.ItemID,
                    TransID = item.TransID,
                    CompanyID = companyId,
                    UserId = userId
                }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error unapproving purchase orders");
            throw;
        }
    }

    /// <summary>
    /// Uncancel purchase orders - reverse the cancellation
    /// </summary>
    public async Task<bool> UncancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            foreach (var item in items)
            {
                var sql = @"
                    UPDATE ItemTransactionDetail
                    SET IsCancelled = 0,
                        ModifiedBy = @UserId,
                        ModifiedDate = GETDATE()
                    WHERE TransactionID = @TransactionID
                      AND ItemID = @ItemID
                      AND TransID = @TransID
                      AND CompanyID = @CompanyID";

                await connection.ExecuteAsync(sql, new
                {
                    TransactionID = item.TransactionID,
                    ItemID = item.ItemID,
                    TransID = item.TransID,
                    CompanyID = companyId,
                    UserId = userId
                }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error uncancelling purchase orders");
            throw;
        }
    }
}
