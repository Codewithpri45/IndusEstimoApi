using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Domain.Entities.Inventory;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.DTOs.Inventory;
using Dapper;
using Newtonsoft.Json;

namespace IndasEstimo.Infrastructure.Repositories.Inventory;


public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PurchaseRequisitionRepository> _logger;

    public PurchaseRequisitionRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<PurchaseRequisitionRepository> logger)
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



    public async Task<long> SavePurchaseRequisitionAsync(
        PurchaseRequisition main,
        List<PurchaseRequisitionDetail> details,
        List<PurchaseRequisitionIndentUpdate> indentUpdates)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Insert Main record (Audit fields handled by DbOperationsService)
            var transactionId = await _dbOperations.InsertDataAsync("ItemTransactionMain", main, connection, transaction, "TransactionID");

            // 2. Insert Detail records (Parent linkage and audit fields handled by DbOperationsService)
            await _dbOperations.InsertDataAsync("ItemTransactionDetail", details, (SqlConnection)connection, (SqlTransaction)transaction, "TransactionDetailID", parentTransactionId: transactionId);

            // 3. Update Indent Details
            if (indentUpdates != null && indentUpdates.Count > 0)
            {
                await UpdateIndentDetailsAsync(transactionId, indentUpdates, connection, transaction);
            }

            await transaction.CommitAsync();
            return transactionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving purchase requisition");
            throw;
        }
    }

    private async Task UpdateIndentDetailsAsync(long transactionId, List<PurchaseRequisitionIndentUpdate> updates, SqlConnection connection, SqlTransaction transaction)
    {
        var sql = @"
            UPDATE ItemTransactionDetail 
            SET RequisitionTransactionID = @RequisitionTransactionID
            WHERE TransactionID = @TransactionID 
              AND ItemID = @ItemID 
              AND CompanyID = @CompanyID";

        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var parameters = updates.Select(u => new
        {
            RequisitionTransactionID = transactionId,
            u.TransactionID,
            u.ItemID,
            CompanyID = companyId
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }

    public async Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPRNumberAsync(string prefix)
    {
        return await _dbOperations.GenerateVoucherNoAsync(
            "ItemTransactionMain",
            -9,
            prefix);
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
        string itemName,
        decimal purchaseQty)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        var command = new SqlCommand("UserApprovalProcessMultiUnit", connection);
        command.CommandType = System.Data.CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@DisplayModuleName", displayModuleName);
        command.Parameters.AddWithValue("@ModuleID", moduleId);
        command.Parameters.AddWithValue("@CompanyID", _currentUserService.GetCompanyId() ?? 0);
        command.Parameters.AddWithValue("@ProductionUnitID", _currentUserService.GetProductionUnitId() ?? 0);
        command.Parameters.AddWithValue("@TransactionID", transactionId);
        command.Parameters.AddWithValue("@TransactionDetailID", transactionDetailId);
        command.Parameters.AddWithValue("@VoucherID", -9);
        command.Parameters.AddWithValue("@VoucherName", "Paper Purchase Requisition");
        command.Parameters.AddWithValue("@VoucherNo", voucherNo);
        command.Parameters.AddWithValue("@ItemID", itemId);
        command.Parameters.AddWithValue("@ItemDescription", itemDescription);
        command.Parameters.AddWithValue("@FYear", _currentUserService.GetFYear() ?? string.Empty);
        command.Parameters.AddWithValue("@UserID", _currentUserService.GetUserId() ?? 0);
        command.Parameters.AddWithValue("@TableName", "ItemTransactionDetail");
        command.Parameters.AddWithValue("@ApprovalColumnName", "IsVoucherItemApproved");
        command.Parameters.AddWithValue("@ItemNameLine", itemName);
        command.Parameters.AddWithValue("@ItemQtyLine", purchaseQty.ToString());
        command.Parameters.AddWithValue("@PageUrl", $"PurchaseRequisition.aspx?TransactionID={transactionId}");

        try
        {
            command.LogQuery(_logger);
            await command.ExecuteNonQueryAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating approval workflow for PR");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<List<JobCardDto>> GetJobCardListAsync(string productionUnitIds)
    {
        if (string.IsNullOrWhiteSpace(productionUnitIds))
        {
            return new List<JobCardDto>();
        }

        // Format the production unit IDs list for the IN clause
        var arrIDs = productionUnitIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        string formattedIDs = string.Join(",", arrIDs.Select(id => id.Trim()));

        if (string.IsNullOrEmpty(formattedIDs))
        {
            return new List<JobCardDto>();
        }

        using var connection = GetConnection();

        string query = $@"
            SELECT DISTINCT 
                JobCardContentNo As RefJobCardContentNo,
                JobBookingJobCardContentsID As RefJobBookingJobCardContentsID 
            FROM JobBookingJobCardContents 
            WHERE ProductionUnitID IN ({formattedIDs}) 
              AND ISNULL(IsDeletedTransaction,0) = 0";

        var results = await connection.QueryAsync<JobCardDto>(query);
        return results.ToList();
    }

    public async Task<List<ClientListDto>> GetClientListAsync()
    {
        using var connection = GetConnection();

        string query = @"
            SELECT LedgerID AS ClientID, LedgerName 
            FROM LedgerMaster 
            WHERE LedgerGroupID = 1 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
              AND ISNULL(LedgerName, '') <> '' 
              AND CompanyID = @CompanyID
            ORDER BY LedgerName";

        var results = await connection.QueryAsync<ClientListDto>(query, new { CompanyID = _currentUserService.GetCompanyId() ?? 0 });
        return results.ToList();
    }

    public async Task<bool> CloseIndentsAsync(List<long> itemIds)
    {
        if (itemIds == null || !itemIds.Any()) return false;

        using var connection = GetConnection();
        string query = @"
            UPDATE ItemMaster 
            SET IsBlocked = '1' 
            WHERE ItemID IN @ItemIDs 
              AND CompanyID = @CompanyID";

        var rowsAffected = await connection.ExecuteAsync(query, new
        {
            ItemIDs = itemIds,
            CompanyID = _currentUserService.GetCompanyId() ?? 0
        });

        return rowsAffected > 0;
    }

    public async Task<bool> CloseRequisitionsAsync(List<RequisitionItemDto> requisitions)
    {
        if (requisitions == null || !requisitions.Any()) return false;

        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            string query = @"
                UPDATE ItemTransactionDetail 
                SET IsCancelled = 1 
                WHERE TransactionID = @TransactionID 
                  AND ItemID = @ItemID 
                  AND TransactionID > 0
                  AND CompanyID = @CompanyID";

            var parameters = requisitions.Select(r => new
            {
                r.TransactionID,
                r.ItemID,
                CompanyID = companyId
            });

            await connection.ExecuteAsync(query, parameters, transaction);
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error closing requisitions");
            throw;
        }
    }

    public async Task<DateTime?> GetLastTransactionDateAsync()
    {
        using var connection = GetConnection();
        string query = @"
            SELECT MAX(VoucherDate) 
            FROM ItemTransactionMain 
            WHERE VoucherID = -9 
              AND CompanyID = @CompanyID 
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        return await connection.ExecuteScalarAsync<DateTime?>(query, new { CompanyID = _currentUserService.GetCompanyId() ?? 0 });
    }

    public async Task<List<RequisitionDataDto>> GetRequisitionDataAsync(long transactionId)
    {
        using var connection = GetConnection();
        string query = @"
            SELECT DISTINCT 
                ISNULL(IEM.TransactionID, 0) AS RequisitionTransactionID,
                ISNULL(IED.IsvoucherItemApproved, 0) AS VoucherItemApproved,
                ISNULL(IEM.MaxVoucherNo, 0) AS RequisitionMaxVoucherNo,
                ISNULL(IEM.VoucherID, 0) AS RequisitionVoucherID, 
                ISNULL(ID.TransactionID, 0) AS TransactionID,
                ISNULL(I.MaxVoucherNo, 0) AS MaxVoucherNo, 
                ISNULL(I.VoucherID, 0) AS VoucherID,   
                ISNULL(IED.ItemID, 0) AS RequisitionItemID,
                ISNULL(ID.ItemID, 0) AS ItemID,
                ISNULL(IED.TransID, 0) AS TransID, 
                ISNULL(IM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID, 
                ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID,  
                NULLIF(I.VoucherNo, '') AS VoucherNo, 
                REPLACE(CONVERT(VARCHAR(30), I.VoucherDate, 106), ' ', '-') AS VoucherDate,
                NULLIF(IEM.VoucherNo, '') AS RequisitionVoucherNo, 
                REPLACE(CONVERT(VARCHAR(30), IEM.VoucherDate, 106), ' ', '-') AS RequisitionVoucherDate,  
                NULLIF(IGM.ItemGroupName, '') AS ItemGroupName,
                NULLIF(IM.ItemCode, '') AS RequisitionItemCode,  
                NULLIF(IM.ItemName, '') AS RequisitionItemName,
                NULLIF(IM.ItemDescription, '') AS RequisitionItemDescription,
                NULLIF(M.ItemCode, '') AS ItemCode,
                NULLIF(M.ItemName, '') AS ItemName,
                NULLIF(M.ItemDescription, '') AS ItemDescription,
                ISNULL(IED.RequiredNoOfPacks, 0) AS RequiredNoOfPacks,
                ISNULL(IED.QuantityPerPack, 0) AS QuantityPerPack,
                ISNULL(IED.RequiredQuantity, 0) AS PurchaseQty,  
                ISNULL((SELECT ROUND(SUM(ISNULL(RequiredQuantity,0)),3) 
                        FROM ItemTransactionDetail 
                        WHERE RequisitionTransactionID = IED.TransactionID 
                          AND RequisitionItemID = IED.ItemID 
                          AND CompanyID = @CompanyID), 0) AS TotalRequisitionQty,
                ISNULL(ID.RequiredQuantity, 0) AS RequisitionQty,
                ISNULL(IMS.BookedStock, 0) AS RequisitionBookedStock, 
                ISNULL(IMS.AllocatedStock, 0) AS RequisitionAllocatedStock, 
                ISNULL(IED.CurrentStockInStockUnit, 0) AS RequisitionPhysicalStock, 
                ISNULL(IED.CurrentStockInPurchaseUnit, 0) AS RequisitionPhysicalStockInPurchaseUnit, 
                ISNULL(IMS.BookedStock, 0) AS BookedStock, 
                ISNULL(IMS.AllocatedStock, 0) AS AllocatedStock, 
                ISNULL(IMS.PhysicalStock, 0) AS PhysicalStock, 
                NULLIF(IM.StockUnit, '') AS StockUnit,  
                NULLIF(IED.StockUnit, '') AS OrderUnit,
                REPLACE(CONVERT(VARCHAR(30), IED.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate, 
                NULLIF(IED.ItemNarration, '') AS ItemNarration,
                NULLIF(IM.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(UOM.DecimalPlace, 0) AS UnitDecimalPlace,
                NULLIF(IEM.FYear, '') AS FYear,
                NULLIF(JBC.JobCardContentNo, '') AS JobCardNo,
                NULLIF(IED.RefJobCardContentNo, '') AS RefJobCardContentNo,
                ISNULL(IED.RefJobBookingJobCardContentsID, 0) AS RefJobBookingJobCardContentsID,
                ISNULL(ID.JobBookingJobCardContentsID, 0) AS JobBookingJobCardContentsID,
                (SELECT TOP 1 REPLACE(CONVERT(VARCHAR(13), A.VoucherDate, 106), ' ', '-') 
                 FROM ItemTransactionMain AS A 
                 INNER JOIN ItemTransactionDetail AS B ON A.TransactionID = B.TransactionID 
                   AND A.CompanyID = B.CompanyID 
                   AND B.ItemID = IED.ItemID 
                 WHERE A.VoucherID = -11 
                   AND A.CompanyID = @CompanyID 
                   AND ISNULL(A.IsDeletedTransaction, 0) = 0 
                   AND CAST(FLOOR(CAST(A.VoucherDate AS FLOAT)) AS DATETIME) < CAST(FLOOR(CAST(IEM.VoucherDate AS FLOAT)) AS DATETIME) 
                 ORDER BY A.VoucherDate DESC) AS LastPurchaseDate,
                ISNULL(IM.UnitPerPacking, 0) AS UnitPerPacking,
                ISNULL(IM.WtPerPacking, 0) AS WtPerPacking,
                ISNULL(IM.SizeW, 0) AS SizeW,
                ISNULL(IM.GSM, 0) AS GSM,
                ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                ISNULL(IM.Thickness, 0) AS Thickness,
                ISNULL(IM.Density, 0) AS Density,
                ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                NULLIF(PUM.ProductionUnitName, '') AS ProductionUnitName,
                NULLIF(CM.CompanyName, '') AS CompanyName
            FROM ItemTransactionMain AS IEM  
            INNER JOIN ItemTransactionDetail AS IED ON IEM.TransactionID = IED.TransactionID AND IEM.CompanyID = IED.CompanyID 
            INNER JOIN ItemMaster AS IM ON IM.ItemID = IED.ItemID
            LEFT JOIN ItemMasterStock AS IMS ON IMS.ItemID = IM.ItemID AND IMS.ProductionUnitID = IED.ProductionUnitID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID 
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = IEM.ProductionUnitID 
            INNER JOIN CompanyMaster AS CM ON CM.CompanyID = PUM.CompanyID 
            LEFT JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID = IED.TransactionID AND ID.RequisitionItemID = IED.ItemID AND ID.CompanyID = IED.CompanyID 
            LEFT JOIN ItemTransactionMain AS I ON I.TransactionID = ID.TransactionID AND I.CompanyID = ID.CompanyID 
            LEFT JOIN JobBookingJobCardContents AS JBC ON JBC.JobBookingJobCardContentsID = ID.JobBookingJobCardContentsID AND JBC.JobBookingID = ID.JobBookingID AND JBC.CompanyID = ID.CompanyID 
            LEFT JOIN ItemMaster AS M ON M.ItemID = ID.ItemID  
            LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol = IED.StockUnit   
            WHERE IEM.VoucherID IN (-9) 
              AND IEM.TransactionID = @TransactionID 
              AND ISNULL(IM.IsDeletedTransaction,0) <> 1 
              AND IEM.CompanyID = @CompanyID
            ORDER BY FYear, RequisitionMaxVoucherNo DESC, TransID";

        var results = await connection.QueryAsync<RequisitionDataDto>(query, new { TransactionID = transactionId, CompanyID = _currentUserService.GetCompanyId() ?? 0 });
        return results.ToList();
    }

    public async Task<List<ItemLookupDto>> GetItemLookupListAsync(long? itemGroupId, string productionUnitId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var parameters = new DynamicParameters();
        parameters.Add("ProductionUnitID", productionUnitId);
        parameters.Add("CompanyID", companyId);

        string groupFilter = "";
        if (itemGroupId.HasValue)
        {
            groupFilter = " IM.ItemGroupID = @ItemGroupID AND ";
            parameters.Add("ItemGroupID", itemGroupId);
        }

        string query = $@"
            SELECT DISTINCT 
                IM.ItemID, IM.ItemGroupID, IGM.ItemGroupNameID, IM.ItemSubGroupID, IGM.ItemGroupName, 
                NULLIF(ISGM.ItemSubGroupName, '') AS ItemSubGroupName, IM.ItemCode, IM.ItemName, 
                IM.Quality, IM.GSM, IM.ReleaseGSM, IM.AdhesiveGSM, IM.Thickness, IM.Density, 
                IM.Manufecturer AS Manufacturer, IM.Finish, IM.SizeW, IM.SizeL, 
                IMS.BookedStock, IMS.AllocatedStock, IMS.PhysicalStock, 
                NULLIF(IM.StockUnit, '') AS StockUnit, UOM.DecimalPlace AS UnitDecimalPlace, 
                IM.PurchaseUnit, IM.WtPerPacking, IM.UnitPerPacking, IM.ConversionFactor, 
                NULLIF(C.ConversionFormula, '') AS ConversionFormula, C.ConvertedUnitDecimalPlace,
                (SELECT TOP 1 REPLACE(CONVERT(VARCHAR(13), A.VoucherDate, 106), ' ', '-') 
                 FROM ItemTransactionMain AS A 
                 INNER JOIN ItemTransactionDetail AS B ON A.TransactionID = B.TransactionID AND A.CompanyID = B.CompanyID 
                 WHERE A.VoucherID = -11 AND B.ItemID = IM.ItemID AND ISNULL(A.IsDeletedTransaction, 0) = 0
                 ORDER BY A.VoucherDate DESC) AS LastPurchaseDate
            FROM ItemMaster AS IM 
            INNER JOIN ItemMasterStock AS IMS ON IMS.ItemID = IM.ItemID 
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID 
            LEFT OUTER JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0 
            LEFT OUTER JOIN UnitMaster AS UOM ON UOM.UnitSymbol = IM.StockUnit 
            LEFT OUTER JOIN ConversionMaster AS C ON IM.StockUnit = C.BaseUnitSymbol AND IM.PurchaseUnit = C.ConvertedUnitSymbol 
            WHERE {groupFilter} 
              ISNULL(IM.IsDeletedTransaction, 0) <> 1  
              AND ISNULL(IM.ISItemActive, 0) <> 0 
              AND IMS.ProductionUnitID = @ProductionUnitID 
              AND IM.CompanyID = @CompanyID
            ORDER BY ItemGroupID, ItemSubGroupName, ItemCode, ItemName";

        var results = await connection.QueryAsync<ItemLookupDto>(query, parameters);
        return results.ToList();
    }

    public async Task<bool> IsRequisitionUsedAsync(long transactionId)
    {
        using var connection = GetConnection();
        string query = @"
            SELECT COUNT(1) 
            FROM ItemPurchaseRequisitionDetail 
            WHERE RequisitionTransactionID = @TransactionID 
              AND CompanyID = @CompanyID 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
              AND ISNULL(IsBlocked, 0) = 0 
              AND ISNULL(IsLocked, 0) = 0";

        var count = await connection.ExecuteScalarAsync<int>(query, new
        {
            TransactionID = transactionId,
            CompanyID = _currentUserService.GetCompanyId() ?? 0
        });

        return count > 0;
    }

    public async Task<bool> IsRequisitionApprovedAsync(long transactionId)
    {
        using var connection = GetConnection();
        string query = @"
            SELECT TOP 1 IsApproved 
            FROM UserApprovalTransactionsDetail 
            WHERE RecordTransactionDetailID IN (
                SELECT TransactionDetailID 
                FROM ItemTransactionDetail 
                WHERE TransactionID = @TransactionID 
                  AND ISNULL(IsvoucherItemApproved, 0) <> 0 
                  AND ISNULL(IsDeletedTransaction, 0) = 0 
                  AND CompanyID = @CompanyID
            ) 
            AND CompanyID = @CompanyID";

        var isApproved = await connection.ExecuteScalarAsync<int?>(query, new
        {
            TransactionID = transactionId,
            CompanyID = _currentUserService.GetCompanyId() ?? 0
        });

        return isApproved == 1;
    }


    public async Task<bool> DeletePurchaseRequisitionAsync(long transactionId)
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
            // Modernized: This is now inside the transaction
            string deleteApprovalsSql = @"
                DELETE FROM UserApprovalTransactionsDetail 
                WHERE RecordTransactionID = @TransactionID 
                  AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(deleteApprovalsSql, new { TransactionID = transactionId, CompanyID = companyId }, transaction);

            // 3. Soft Delete Details
            string deleteDetailSql = @"
                UPDATE ItemTransactionDetail 
                SET DeletedBy = @DeletedBy, 
                    DeletedDate = GETDATE(), 
                    IsDeletedTransaction = 1 
                WHERE TransactionID = @TransactionID AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(deleteDetailSql, new { DeletedBy = userId, TransactionID = transactionId, CompanyID = companyId }, transaction);

            // 4. Reset linked indents (Release indents so they can be reused)
            string resetIndentSql = @"
                UPDATE ItemTransactionDetail 
                SET RequisitionTransactionID = 0 
                WHERE RequisitionTransactionID = @TransactionID AND CompanyID = @CompanyID";

            await connection.ExecuteAsync(resetIndentSql, new { TransactionID = transactionId, CompanyID = companyId }, transaction);

            // 4. Update Stock SP
            //try
            //{
            //    await connection.ExecuteAsync("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE @CompanyID, @TransactionID, 0", 
            //        new { CompanyID = companyId, TransactionID = transactionId }, transaction);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogWarning(ex, "Could not execute UPDATE_ITEM_STOCK_VALUES_UNIT_WISE during delete.");
            //}

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting purchase requisition {TransactionID}", transactionId);
            throw;
        }
    }

    public async Task<List<CommentDataDto>> GetCommentDataAsync(string purchaseTransactionId, string requisitionIds)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string docType = (purchaseTransactionId != "0" && !string.IsNullOrEmpty(purchaseTransactionId))
            ? "Purchase Order"
            : "Purchase Requisition";

        string id = (purchaseTransactionId != "0" && !string.IsNullOrEmpty(purchaseTransactionId))
            ? purchaseTransactionId
            : requisitionIds;

        var parameters = new DynamicParameters();
        parameters.Add("CompanyID", companyId);
        parameters.Add("ModuleName", docType); // "Purchase Order" or "Purchase Requisition"

        // Logic to pass the correct ID to the correct parameter
        if (docType == "Purchase Order")
        {
            parameters.Add("RequisitionIDs", 0);
            parameters.Add("PurchaseTransactionIDs", id);
        }
        else
        {
            parameters.Add("RequisitionIDs", id);
            parameters.Add("PurchaseTransactionIDs", 0);
        }

        // Default all other ID parameters to 0 as per SP signature
        parameters.Add("GRNTransactionIDs", 0);
        parameters.Add("BookingIDs", 0);
        parameters.Add("PriceApprovalIDs", 0);
        parameters.Add("OrderBookingIDs", 0);
        parameters.Add("ProductMasterIDs", 0);
        parameters.Add("JobBookingIDs", 0);

        // Legacy call: GetCommentData(CompanyID, DocType, 0, ID, 0, 0, 0, 0, 0, 0)
        var results = await connection.QueryAsync<CommentDataDto>(
            "GetCommentData",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure
        );

        return results.ToList();
    }

    public async Task<string> FillGridAsync(string radioValue, string filterString, string fromDateValue, string toDateValue)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;
        var fYear = _currentUserService.GetFYear() ?? "";
        filterString ??= "";

        var parameters = new DynamicParameters();
        parameters.Add("ProductionUnitID", productionUnitId);
        parameters.Add("FYear", fYear);
        parameters.Add("CompanyID", companyId);
        parameters.Add("FromDate", fromDateValue);
        parameters.Add("ToDate", toDateValue);

        string query = string.Empty;

        if (radioValue == "Indent List")
        {
            query = @"
                Select distinct Isnull(IEM.TransactionID,0) as TransactionID,Isnull(IEM.MaxVoucherNo,0) as MaxVoucherNo,Isnull(IEM.VoucherID,0) as VoucherID,nullif(IEM.VoucherNo,'') as VoucherNo,replace(convert(nvarchar(30),IEM.VoucherDate,106),' ','-') as VoucherDate,Isnull(IED.ItemID,0) as ItemID,Isnull(IM.ItemGroupID,0) as ItemGroupID,
                Isnull(IGM.ItemGroupNameID,0) as ItemGroupNameID,Isnull(JBC.JobBookingID,0) as BookingID,Isnull(JBC.JobBookingJobCardContentsID,0) as JobBookingJobCardContentsID,nullif(IGM.ItemGroupName,'') as ItemGroupName,nullif(ISGM.ItemSubGroupName,'') as ItemSubGroupName,nullif(IM.ItemCode,'') as ItemCode,	 nullif(IM.ItemName,'') as ItemName,  nullif(IM.ItemDescription,'') as ItemDescription
                ,nullif(JBC.JobCardContentNo,'') AS JobBookingContentNo,Isnull(IED.RequiredQuantity,0) as RequiredQuantity,Isnull(IMS.BookedStock,0) as BookedStock,Isnull(IMS.AllocatedStock,0) as AllocatedStock,Isnull(IMS.PhysicalStock,0) as PhysicalStock,nullif(IED.StockUnit,'') as StockUnit,Isnull(UOM.DecimalPlace,0) AS UnitDecimalPlace,nullif(IM.PurchaseUnit,'') as PurchaseUnit,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,0) AS UnitPerPacking,Isnull(IM.ConversionFactor,0) AS ConversionFactor,Isnull(IM.SizeW,0) AS SizeW,Nullif(C.ConversionFormula,'') AS ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS ConvertedUnitDecimalPlace,(Select Top 1 Replace(Convert(Varchar(13),A.VoucherDate,106),' ','-') FRom ItemTransactionMain AS A INNER JOIN ItemTransactionDetail as B ON A.TransactionID=B.TransactionID AND A.CompanyID=B.CompanyID AND B.ItemID=IED.ItemID Where A.VoucherID=-11 AND A.CompanyID =IED.CompanyID AND Isnull(A.IsDeletedTransaction,0)=0 Order By A.VoucherDate Desc) AS LastPurchaseDate,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density,PUM.ProductionUnitName,PUM.ProductionUnitID,CM.CompanyName,CM.CompanyID 
                From ItemTransactionMain AS IEM INNER JOIN ItemTransactionDetail AS IED ON IEM.TransactionID=IED.TransactionID AND IEM.CompanyID=IED.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=IED.ItemID INNER JOIN ItemMasterStock As IMS ON IMS.ItemID = IM.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = IEM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0	LEFT JOIN JobBookingJobCardContents AS JBC ON JBC.JobBookingJobCardContentsID=IED.JobBookingJobCardContentsID 
                LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IED.StockUnit  LEFT JOIN ConversionMaster AS C ON IM.StockUnit=C.BaseUnitSymbol AND IM.PurchaseUnit=C.ConvertedUnitSymbol Where IEM.VoucherID IN(-8) AND ISNULL(IED.IsCancelled, 0) = 0 And Isnull(IED.RequisitionTransactionID,0)=0 and Isnull(IEM.IsDeletedTransaction,0)<>1	And IEM.ProductionUnitID = @ProductionUnitID 
                UNION ALL Select 0 as TransactionID,0 as MaxVoucherNo,0 as VoucherID,nullif('','') as VoucherNo,nullif('','') as VoucherDate,Isnull(IM.ItemID,0) as ItemID,Isnull(IM.ItemGroupID,0) as ItemGroupID,Isnull(IGM.ItemGroupNameID,0) as ItemGroupNameID, 0 as BookingID,0 as JobContentsID,nullif(IGM.ItemGroupName,'') as ItemGroupName,  nullif(ISGM.ItemSubGroupName,'') as ItemSubGroupName,nullif(IM.ItemCode,'') as ItemCode,nullif(IM.ItemName,'') as ItemName,nullif(IM.ItemDescription,'') as ItemDescription, 
                nullif('','') AS JobBookingContentNo,Isnull(Nullif(IM.PurchaseOrderQuantity,''),0) as RequiredQuantity,nullif(IMS.BookedStock,'') as BookedStock,  nullif(IMS.AllocatedStock,'') as AllocatedStock,	Isnull(IMS.PhysicalStock,0) as PhysicalStock,nullif(IM.StockUnit,'') as StockUnit,Isnull(U.DecimalPlace,0) AS UnitDecimalPlace,nullif(IM.PurchaseUnit,'') as PurchaseUnit,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,0) AS UnitPerPacking,Isnull(IM.ConversionFactor,0) AS ConversionFactor,Isnull(IM.SizeW,0) AS SizeW,Nullif(C.ConversionFormula,'') AS ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS ConvertedUnitDecimalPlace,/*(Select Top 1 Replace(Convert(Varchar(13),A.VoucherDate,106),' ','-') FRom ItemTransactionMain AS A INNER JOIN ItemTransactionDetail as B ON A.TransactionID=B.TransactionID AND A.CompanyID=B.CompanyID AND B.ItemID=IM.ItemID Where A.VoucherID=-11  AND Isnull(A.IsDeletedTransaction,0)=0 Order By A.VoucherDate Desc)*/ REPLACE(CONVERT(Varchar(13), LLP.LastPurchaseDate, 106), ' ', '-') AS LastPurchaseDate,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density, PUM.ProductionUnitName,PUM.ProductionUnitID, CM.CompanyName, CM.CompanyID 
                From ItemMaster AS IM INNER JOIN ItemMasterStock As IMS ON IMS.ItemID = IM.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID=IMS.ProductionUnitID  INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN UnitMaster AS U ON U.UnitSymbol=IM.StockUnit LEFT JOIN ConversionMaster AS C ON IM.StockUnit=C.BaseUnitSymbol AND IM.PurchaseUnit=C.ConvertedUnitSymbol 
                LEFT JOIN (Select B.CompanyID,B.ItemID,Max(A.VoucherDate) AS LastPurchaseDate From ItemTransactionMain AS A INNER JOIN ItemTransactionDetail as B ON A.TransactionID=B.TransactionID AND A.CompanyID=B.CompanyID Where A.VoucherID=-11  AND Isnull(A.IsDeletedTransaction,0)=0 Group BY B.CompanyID,B.ItemID) AS LLP ON LLP.ItemID=IM.ItemID AND LLP.CompanyID=Isnull(PUM.CompanyID,0)  
                Where Isnull(IM.IsDeletedTransaction,0)<>1 AND IMS.ProductionUnitID = @ProductionUnitID AND  Isnull(IM.IsRegularItem,0)='1' AND Isnull(IM.MinimumStockQty,0)>Isnull(IMS.PhysicalStock,0) 
                AND Not Exists(Select Distinct IED.ItemID From ItemTransactionMain AS IEM INNER JOIN ItemTransactionDetail AS IED ON IEM.TransactionID=IED.TransactionID AND IEM.CompanyID=IED.CompanyID Where IEM.VoucherID IN(-8) And IEM.FYear=@FYear And IEM.ProductionUnitID = @ProductionUnitID And Isnull(IED.IsDeletedTransaction,0)<>1 AND Isnull(IED.RequisitionTransactionID,0)=0 AND IED.ItemID=IM.ItemID) 
                Order By ItemGroupName,ItemName ,VoucherDate";
        }
        else
        {
            query = $@"
                Select Distinct Isnull(IEM.TransactionID,0) AS TransactionID,Isnull(IEM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(IEM.VoucherID,0) AS VoucherID,Isnull(IED.ItemID,0) As ItemID,Isnull(IED.TransID,0) As TransID, Isnull(IM.ItemGroupID,0) As ItemGroupID,Isnull(IM.ItemSubGroupID,0) As ItemSubGroupID, Isnull(IGM.ItemGroupNameID, 0) As ItemGroupNameID,  NullIf(IEM.VoucherNo,'') AS VoucherNo, Replace(Convert(Varchar(30), IEM.VoucherDate, 106),' ','-') AS VoucherDate,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(IM.ItemCode,'') AS ItemCode, NullIf(IM.ItemName,'') AS ItemName, NullIf(IM.ItemDescription,'') AS ItemDescription,Nullif(IED.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(IED.RequiredQuantity,0) AS PurchaseQty,  Isnull(IED.RequiredQuantity,0) AS RequisitionQty,Isnull(IMS.BookedStock, 0) As BookedStock, Isnull(IMS.AllocatedStock, 0) As AllocatedStock, Isnull(IMS.PhysicalStock, 0) As PhysicalStock,  NullIf(IM.StockUnit,'') AS StockUnit, NullIf(IED.StockUnit,'') AS OrderUnit,Replace(Convert(Varchar(30),IED.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,  
                Nullif(IED.ItemNarration,'') AS ItemNarration,Nullif(IEM.Narration,'') AS Narration,Isnull(UOM.DecimalPlace,0) AS UnitDecimalPlace,NullIf(IEM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,NullIf(UA.UserName,'') AS ApprovedBy, Isnull(IED.IsAuditApproved,0) As AuditApproved,Isnull(IED.AuditApprovedBy,0) AS AuditApprovedBy,Isnull(IED.IsAuditCancelled,0) AS IsAuditCancelled, Isnull(IED.IsVoucherItemApproved,0) AS IsVoucherItemApproved,Isnull(IED.IsCancelled,0) AS IsCancelled,NullIf(IM.PurchaseUnit,'') AS PurchaseUnit,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName  
                From ItemTransactionMain AS IEM INNER JOIN ItemTransactionDetail AS IED ON IEM.TransactionID=IED.TransactionID  And IEM.CompanyID=IED.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=IED.ItemID And ISNULL(IM.IsDeletedTransaction,0) = 0  INNER JOIN (SELECT ItemID,SUM(BookedStock) AS BookedStock,SUM(AllocatedStock) AS AllocatedStock,SUM(PhysicalStock) AS PhysicalStock FROM ItemMasterStock WHERE ISNULL(IsDeletedTransaction, 0) = 0 GROUP BY ItemID) AS IMS ON IMS.ItemID = IM.ItemID INNER JOIN  ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID  INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = IEM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IED.StockUnit LEFT JOIN UserMaster AS UM ON UM.UserID=IEM.CreatedBy LEFT JOIN UserMaster AS UA ON UA.UserID=IED.VoucherItemApprovedBy 
                Where IEM.VoucherID IN(-9) And IEM.ProductionUnitID = @ProductionUnitID AND IM.IsBlocked = 0 And Isnull(IEM.IsDeletedTransaction,0)<>1 AND IEM.VoucherDate BETWEEN @FromDate AND @ToDate {filterString} Order By FYear Desc,MaxVoucherNo Desc";
        }

        // Just for logging the query with values
        using (var logCmd = new SqlCommand(query, connection))
        {
            logCmd.Parameters.AddWithValue("@ProductionUnitID", productionUnitId);
            logCmd.Parameters.AddWithValue("@FYear", fYear);
            logCmd.Parameters.AddWithValue("@FromDate", fromDateValue);
            logCmd.Parameters.AddWithValue("@ToDate", toDateValue);
            logCmd.LogQuery(_logger);
        }

        var results = await connection.QueryAsync<dynamic>(query, parameters);
        return JsonConvert.SerializeObject(results);
    }
}