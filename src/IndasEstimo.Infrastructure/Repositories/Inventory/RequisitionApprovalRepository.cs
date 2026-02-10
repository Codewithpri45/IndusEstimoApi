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

public class RequisitionApprovalRepository : IRequisitionApprovalRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RequisitionApprovalRepository> _logger;

    public RequisitionApprovalRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<RequisitionApprovalRepository> logger)
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
    /// Get unapproved requisitions - matches UnApprovedRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 0 AND IsCancelled = 0 AND IsAuditApproved = 1
    /// </summary>
    public async Task<List<UnapprovedRequisitionDto>> GetUnapprovedRequisitionsAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = @"
            SELECT DISTINCT
                ITM.TransactionID,
                ITM.VoucherID,
                ITM.MaxVoucherNo,
                ITD.ItemGroupID,
                ITD.ItemID,
                ITM.VoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IM.ItemCode, '') AS ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                NULLIF(IM.ItemName, '') AS ItemName,
                NULLIF(IM.ItemDescription, '') AS ItemDescription,
                NULLIF(ITD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(ITD.RequiredQuantity, 0) AS RequiredQuantity,
                NULLIF(ITD.StockUnit, '') AS StockUnit,
                NULLIF(ITD.ItemNarration, '') AS ItemNarration,
                REPLACE(CONVERT(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                NULLIF(ITM.Narration, '') AS Narration,
                (SELECT COUNT(TransactionID) AS Expr1
                 FROM ItemTransactionDetail
                 WHERE (TransactionID = ITM.TransactionID) AND (CompanyID = ITM.CompanyID)) AS TotalItems,
                ISNULL(ITM.TotalQuantity, 0) AS TotalQuantity,
                NULLIF(ITM.FYear, '') AS FYear,
                NULLIF(UA.UserName, '') AS CreatedBy,
                ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                NULLIF(PUM.ProductionUnitName, '') AS ProductionUnitName,
                NULLIF(CM.CompanyName, '') AS CompanyName,
                ISNULL(CM.CompanyID, 0) AS CompanyID
            FROM ItemTransactionMain AS ITM
            INNER JOIN ItemTransactionDetail AS ITD ON ITD.TransactionID = ITM.TransactionID AND ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN UserMaster AS UA ON UA.UserID = ITM.CreatedBy
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
            WHERE (ISNULL(ITM.VoucherID, 0) = -9)
              AND (ISNULL(ITM.IsDeletedTransaction, 0) = 0)
              AND (ISNULL(ITD.IsVoucherItemApproved, 0) = 0)
              AND (ISNULL(ITD.IsCancelled, 0) = 0)
              AND (ISNULL(ITD.IsAuditApproved, 0) = 1)
              AND (CAST(FLOOR(CAST(ITM.VoucherDate AS float)) AS DateTime) >= @FromDate)
              AND (CAST(FLOOR(CAST(ITM.VoucherDate AS float)) AS DateTime) <= @ToDate)
              AND (ITM.ProductionUnitID IN (" + productionUnitIdStr + @"))
            ORDER BY FYear DESC, ITM.MaxVoucherNo DESC";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<UnapprovedRequisitionDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Get approved requisitions - matches ApprovedRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 1
    /// </summary>
    public async Task<List<ApprovedRequisitionDto>> GetApprovedRequisitionsAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = @"
            SELECT Distinct
                ITM.TransactionID,
                ITM.VoucherID,
                ITM.MaxVoucherNo,
                ITD.ItemGroupID,
                ITD.ItemID,
                ITM.VoucherNo,
                REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                IM.ItemCode,
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                IM.ItemName,
                NULLIF(ITD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ITD.RequiredQuantity,
                NULLIF(ITD.StockUnit, '') AS StockUnit,
                REPLACE(CONVERT(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                NULLIF(ITD.ItemNarration, '') AS ItemNarration,
                NULLIF(ITM.Narration, '') AS Narration,
                (SELECT COUNT(TransactionID) AS Expr1
                 FROM ItemTransactionDetail
                 WHERE (TransactionID = ITM.TransactionID) AND (CompanyID = ITM.CompanyID)) AS TotalItems,
                ITM.TotalQuantity,
                NULLIF(ITM.FYear, '') AS FYear,
                NULLIF(UA.UserName, '') AS CreatedBy,
                NULLIF(U.UserName, '') AS ApprovedBy,
                REPLACE(CONVERT(Varchar(13), ITD.VoucherItemApprovedDate, 106), ' ', '-') AS ApprovalDate,
                Isnull(PUM.ProductionUnitID, 0) as ProductionUnitID,
                Nullif(PUM.ProductionUnitName, '') AS ProductionUnitName,
                Nullif(CM.CompanyName, '') AS CompanyName,
                ISNULL(CM.CompanyID, 0) AS CompanyID,
                (SELECT TOP (1) TransactionID
                 FROM ItemPurchaseRequisitionDetail
                 WHERE (RequisitionTransactionID = ITD.TransactionID)
                   AND (ItemID = ITD.ItemID)
                   AND (CompanyID = ITD.CompanyID)
                   AND IsDeletedTransaction = 0) AS PurchaseTransactionID
            FROM ItemTransactionMain As ITM
            INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID = ITM.TransactionID And ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemMaster As IM On IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN UserMaster As UA On UA.UserID = ITM.CreatedBy
            INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID
            Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID
            LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID = IM.ItemSubGroupID
            LEFT JOIN UserMaster As U On U.UserID = ITD.VoucherItemApprovedBy
            Where Isnull(ITM.VoucherID, 0) = -9
              And Isnull(ITM.IsDeletedTransaction, 0) = 0
              And Isnull(ITD.IsVoucherItemApproved, 0) = 1
              And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= @FromDate))
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate))
              And ITM.ProductionUnitID IN(" + productionUnitIdStr + @")
            Order By FYear Desc, ITM.TransactionID Desc";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<ApprovedRequisitionDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Get cancelled requisitions - matches CancelledRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 0 AND IsCancelled = 1
    /// </summary>
    public async Task<List<CancelledRequisitionDto>> GetCancelledRequisitionsAsync(string fromDate, string toDate)
    {
        using var connection = GetConnection();
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        var sql = @"
            SELECT Distinct
                ITM.TransactionID,
                ITM.VoucherID,
                ITM.MaxVoucherNo,
                ITD.ItemGroupID,
                ITD.ItemID,
                ITM.VoucherNo,
                Replace(Convert(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate,
                Nullif(IM.ItemCode, '') AS ItemCode,
                Nullif(IGM.ItemGroupName, '') AS ItemGroupName,
                Nullif(ISGM.ItemSubGroupName, '') AS ItemSubGroupName,
                Nullif(IM.ItemName, '') AS ItemName,
                Nullif(IM.ItemDescription, '') AS ItemDescription,
                Nullif(ITD.RefJobCardContentNo, '') AS RefJobCardContentNo,
                Isnull(ITD.RequiredQuantity, 0) As RequiredQuantity,
                NullIf(ITD.StockUnit, '') AS StockUnit,
                Replace(Convert(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate,
                NullIf(ITD.ItemNarration, '') AS ItemNarration,
                NullIf(ITM.Narration, '') AS Narration,
                (Select Count(TransactionID)
                 From ItemTransactionDetail
                 Where TransactionID = ITM.TransactionID AND CompanyID = ITM.CompanyID) AS TotalItems,
                Isnull(ITM.TotalQuantity, 0) AS TotalQuantity,
                NullIf(ITM.FYear, '') AS FYear,
                NullIf(UA.UserName, '') AS CreatedBy,
                NullIf(U.UserName, '') AS ApprovedBy,
                Replace(Convert(Varchar(13), ITD.VoucherItemApprovedDate, 106), ' ', '-') AS ApprovalDate,
                Isnull((Select Top 1 TransactionID
                        From ItemPurchaseRequisitionDetail
                        Where RequisitionTransactionID = ITD.TransactionID
                          AND ItemID = ITD.ItemID
                          AND CompanyID = ITD.CompanyID), 0) AS PurchaseTransactionID,
                Isnull(PUM.ProductionUnitID, 0) as ProductionUnitID,
                Nullif(PUM.ProductionUnitName, '') AS ProductionUnitName,
                Nullif(CM.CompanyName, '') AS CompanyName,
                ISNULL(CM.CompanyID, 0) AS CompanyID
            From ItemTransactionMain As ITM
            INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID = ITM.TransactionID And ITD.CompanyID = ITM.CompanyID
            INNER JOIN ItemMaster As IM On IM.ItemID = ITD.ItemID
            INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN UserMaster As UA On UA.UserID = ITM.CreatedBy
            INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID
            Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID
            LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID = IM.ItemSubGroupID
            LEFT JOIN UserMaster As U On U.UserID = ITD.VoucherItemApprovedBy
            Where Isnull(ITM.VoucherID, 0) = -9
              And Isnull(ITM.IsDeletedTransaction, 0) = 0
              And Isnull(ITD.IsVoucherItemApproved, 0) = 0
              And Isnull(ITD.IsCancelled, 0) = 1
              And ((Cast(Floor(cast(ITM.VoucherDate As float)) As DateTime) >= @FromDate))
              AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= @ToDate))
              And ITM.ProductionUnitID IN(" + productionUnitIdStr + @")
            Order By FYear Desc, ITM.TransactionID Desc";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FromDate", fromDate);
        cmd.Parameters.AddWithValue("@ToDate", toDate);
        cmd.LogQuery(_logger);

        var result = await connection.QueryAsync<CancelledRequisitionDto>(sql, new { FromDate = fromDate, ToDate = toDate });
        return result.ToList();
    }

    /// <summary>
    /// Approve requisitions - matches UpdateData WebMethod with approval action
    /// </summary>
    public async Task<bool> ApproveRequisitionsAsync(List<RequisitionApprovalItem> items)
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
            _logger.LogError(ex, "Error approving requisitions");
            throw;
        }
    }

    /// <summary>
    /// Cancel requisitions - matches UpdateData WebMethod with cancel action
    /// </summary>
    public async Task<bool> CancelRequisitionsAsync(List<RequisitionCancellationItem> items)
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
            _logger.LogError(ex, "Error cancelling requisitions");
            throw;
        }
    }

    /// <summary>
    /// Unapprove requisitions - reverse the approval
    /// </summary>
    public async Task<bool> UnapproveRequisitionsAsync(List<RequisitionApprovalItem> items)
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
            _logger.LogError(ex, "Error unapproving requisitions");
            throw;
        }
    }

    /// <summary>
    /// Uncancel requisitions - reverse the cancellation
    /// </summary>
    public async Task<bool> UncancelRequisitionsAsync(List<RequisitionCancellationItem> items)
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
            _logger.LogError(ex, "Error uncancelling requisitions");
            throw;
        }
    }
}
