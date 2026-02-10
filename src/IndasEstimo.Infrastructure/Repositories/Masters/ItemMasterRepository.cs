using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using System.Data;
using System.Text;
using System.Text.Json;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ItemMasterRepository : IItemMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ItemMasterRepository> _logger;

    public ItemMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ItemMasterRepository> logger)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    public async Task<List<MasterListDto>> GetMasterListAsync()
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = @"
            SELECT DISTINCT
                IGM.ItemGroupID,
                IGM.ItemGroupName,
                NULLIF(GridColumnName,'') as GridColumnName,
                NULLIF(GridColumnHide,'') as GridColumnHide
            FROM ItemGroupMaster As IGM
            INNER JOIN UserSubModuleAuthentication As UMA
                ON UMA.ItemGroupID = IGM.ItemGroupID
                AND UMA.CompanyID = IGM.CompanyID
                AND UMA.CanView = 1
            WHERE IGM.IsDeletedTransaction = 0
                AND UMA.UserID = @UserID
            ORDER BY IGM.ItemGroupID";

        var result = await connection.QueryAsync<MasterListDto>(sql, new { UserID = userId });
        return result.ToList();
    }

    public async Task<object> GetMasterGridAsync(string masterID)
    {
        using var connection = GetConnection();

        // Get dynamic query from ItemGroupMaster
        var queryConfigSql = @"
            SELECT NULLIF(SelectQuery, '') as SelectQuery
            FROM ItemGroupMaster
            WHERE ItemGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var queryConfig = await connection.QueryFirstOrDefaultAsync<string>(
            queryConfigSql,
            new { MasterID = masterID });

        if (string.IsNullOrEmpty(queryConfig))
            return new List<object>();

        // Execute dynamic query (preserve existing logic)
        string executeSql;
        if (queryConfig.ToUpper().Contains("EXECUTE"))
        {
            executeSql = queryConfig + " '', " + masterID;
        }
        else
        {
            executeSql = queryConfig;
        }

        try
        {
            // Execute and return dynamic results
            var result = await connection.QueryAsync<dynamic>(executeSql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dynamic grid query for masterID {MasterID}", masterID);
            throw;
        }
    }

    public async Task<List<GridColumnHideDto>> GetGridColumnHideAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                NULLIF(GridColumnHide,'') as GridColumnHide,
                NULLIF(TabName,'') as TabName,
                NULLIF(ItemNameFormula,'') as ItemNameFormula,
                NULLIF(ItemDescriptionFormula,'') as ItemDescriptionFormula
            FROM ItemGroupMaster
            WHERE ItemGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<GridColumnHideDto>(sql, new { MasterID = masterID });
        return result.ToList();
    }

    public async Task<List<GridColumnDto>> GetGridColumnAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT NULLIF(GridColumnName,'') as GridColumnName
            FROM ItemGroupMaster
            WHERE ItemGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<GridColumnDto>(sql, new { MasterID = masterID });
        return result.ToList();
    }

    public async Task<string> SaveItemAsync(SaveItemRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Check item-group specific authorization
            var canSave = await CheckAuthorizationAsync(connection, transaction, userId, companyId, request.ItemGroupID, "CanSave");
            if (!canSave)
            {
                return "You are not authorized to save..!";
            }

            // Check StockRefCode duplicate
            var duplicateSql = @"
                SELECT NULLIF(StockRefCode, '') as StockRefCode
                FROM ItemMaster
                WHERE StockRefCode = @StockRefCode
                AND ItemGroupID = @ItemGroupID
                AND IsDeletedTransaction = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                duplicateSql,
                new { request.StockRefCode, request.ItemGroupID },
                transaction);

            if (!string.IsNullOrEmpty(exists))
            {
                return "Stock Ref. Code already exists.";
            }

            // Handle special ItemGroupID cases
            var itemMasterData = new Dictionary<string, object>(request.CostingDataItemMaster[0]);
            if (Convert.ToInt32(request.ItemGroupID) == 1)
            {
                itemMasterData["ItemSubGroupID"] = -1;
            }
            else if (Convert.ToInt32(request.ItemGroupID) == 2)
            {
                itemMasterData["ItemSubGroupID"] = -2;
            }

            // Get ItemCode prefix
            var itemCodePrefixSql = @"
                SELECT NULLIF(ItemGroupPrefix,'') as ItemGroupPrefix
                FROM ItemGroupMaster
                WHERE ItemGroupID = @ItemGroupID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var itemCodePrefix = await connection.QueryFirstOrDefaultAsync<string>(
                itemCodePrefixSql,
                new { request.ItemGroupID },
                transaction);

            // Generate ItemCode
            var maxItemNoSql = @"
                SELECT ISNULL(MAX(MaxItemNo), 0) + 1 AS MaxItemNo
                FROM ItemMaster
                WHERE ItemCodePrefix = @ItemCodePrefix
                AND ISNULL(IsDeletedTransaction, 0) = 0";

            var maxItemNo = await connection.QueryFirstOrDefaultAsync<int>(
                maxItemNoSql,
                new { ItemCodePrefix = itemCodePrefix },
                transaction);

            var itemCode = $"{itemCodePrefix}{maxItemNo}";

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "ItemMaster");

            // Build dynamic INSERT statement
            var columns = new List<string>
            {
                "CreatedDate", "UserID", "CompanyID", "FYear",
                "CreatedBy", "ItemCode", "ItemCodePrefix", "MaxItemNo"
            };
            var values = new List<string>
            {
                "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                "@CreatedBy", "@ItemCode", "@ItemCodePrefix", "@MaxItemNo"
            };
            var parameters = new DynamicParameters();
            parameters.Add("@CreatedDate", DateTime.Now);
            parameters.Add("@UserID", userId);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@FYear", fYear);
            parameters.Add("@CreatedBy", userId);
            parameters.Add("@ItemCode", itemCode);
            parameters.Add("@ItemCodePrefix", itemCodePrefix);
            parameters.Add("@MaxItemNo", maxItemNo);

            // Only add columns that exist in the table
            int paramIndex = 0;
            foreach (var kvp in itemMasterData)
            {
                if (validColumns.Contains(kvp.Key))
                {
                    columns.Add(kvp.Key);
                    var paramName = $"@param{paramIndex}";
                    values.Add(paramName);
                    parameters.Add(paramName, ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            var insertSql = $@"
                INSERT INTO ItemMaster ({string.Join(", ", columns)})
                OUTPUT INSERTED.ItemID
                VALUES ({string.Join(", ", values)})";

            var itemId = await connection.ExecuteScalarAsync<long>(insertSql, parameters, transaction);

            if (itemId <= 0)
            {
                return "Error in main";
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving item");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateItemAsync(UpdateItemRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            // Check item-group specific authorization
            var canEditGroup = await CheckAuthorizationAsync(connection, transaction, userId, companyId, request.UnderGroupID, "CanEdit");
            if (!canEditGroup)
            {
                return "You are not authorized to update..!";
            }

            // Check StockRefCode duplicate
            var duplicateSql = @"
                SELECT NULLIF(StockRefCode, '') as StockRefCode
                FROM ItemMaster
                WHERE StockRefCode = @StockRefCode
                AND ItemID <> @ItemID
                AND ItemGroupID = @ItemGroupID
                AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                duplicateSql,
                new { request.StockRefCode, request.ItemID, ItemGroupID = request.UnderGroupID },
                transaction);

            if (!string.IsNullOrEmpty(exists))
            {
                return "Stock Ref. Code already exists.";
            }

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "ItemMaster");

            // Build dynamic UPDATE statement
            var itemMasterData = request.CostingDataItemMaster[0];
            var setClause = new StringBuilder();
            var parameters = new DynamicParameters();

            setClause.Append("ModifiedDate = GETDATE(), ");
            setClause.Append($"UserID = {userId}, ");
            setClause.Append($"CompanyID = {companyId}, ");
            setClause.Append($"ModifiedBy = {userId}");

            // Only update columns that exist in the table
            int paramIndex = 0;
            foreach (var kvp in itemMasterData)
            {
                if (validColumns.Contains(kvp.Key))
                {
                    setClause.Append($", {kvp.Key} = @param{paramIndex}");
                    parameters.Add($"@param{paramIndex}", ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            parameters.Add("@ItemID", request.ItemID);
            parameters.Add("@UnderGroupID", request.UnderGroupID);

            var updateSql = $@"
                UPDATE ItemMaster
                SET {setClause}
                WHERE ItemID = @ItemID
                AND ItemGroupID = @UnderGroupID";

            await connection.ExecuteAsync(updateSql, parameters, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating item");
            return "fail";
        }
    }

    public async Task<string> DeleteItemAsync(string itemID, string itemgroupID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            await connection.OpenAsync();

            // Check item-group specific authorization
            var canDeleteGroup = await CheckAuthorizationAsync(connection, null, userId, companyId, itemgroupID, "CanDelete");
            if (!canDeleteGroup)
            {
                return "You are not authorized to delete..!";
            }

            // Soft delete
            var sql = @"
                UPDATE ItemMaster
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ItemID = @ItemID
                AND ItemGroupID = @ItemGroupID";

            await connection.ExecuteAsync(sql, new { UserID = userId, ItemID = itemID, ItemGroupID = itemgroupID });
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> CheckPermissionAsync(string transactionID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            // Check in ItemTransactionDetail
            var sql1 = @"
                SELECT TOP 1 ISNULL(ITD.ItemID, 0) as ItemID
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID = ITD.TransactionID
                WHERE ITM.CompanyID = @CompanyID
                    AND ITM.VoucherID <> -8
                    AND ITD.ItemID = @TransactionID
                    AND ISNULL(ITM.IsDeletedTransaction, 0) <> 1";

            var exists1 = await connection.QueryFirstOrDefaultAsync<long?>(sql1,
                new { CompanyID = companyId, TransactionID = transactionID });

            if (exists1.HasValue && exists1.Value > 0)
            {
                return "Exist";
            }

            // Check QC approval
            var sql2 = @"
                SELECT TOP 1 TransactionID
                FROM ItemTransactionDetail
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                    AND ISNULL(QCApprovalNo, '') <> ''
                    AND TransactionID = @TransactionID
                    AND (ISNULL(ApprovedQuantity, 0) > 0 OR ISNULL(RejectedQuantity, 0) > 0)";

            var exists2 = await connection.QueryFirstOrDefaultAsync<long?>(sql2,
                new { TransactionID = transactionID });

            if (exists2.HasValue)
            {
                return "Exist";
            }

            return "";
        }
        catch (Exception)
        {
            return "fail";
        }
    }

    public async Task<object> GetLoadedDataAsync(string masterID, string itemId)
    {
        using var connection = GetConnection();

        // Execute stored procedure SelectedRowMultiUnit
        var sql = $"EXECUTE SelectedRowMultiUnit '', {masterID}, {itemId}";

        try
        {
            var result = await connection.QueryAsync<dynamic>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectedRowMultiUnit for masterID {MasterID}, itemId {ItemId}", masterID, itemId);
            throw;
        }
    }

    public async Task<object> GetDrillDownDataAsync(string masterID, string tabID)
    {
        using var connection = GetConnection();

        // Get dynamic query from DrilDown table
        var queryConfigSql = @"
            SELECT NULLIF(SelectQuery, '') as SelectQuery
            FROM DrilDown
            WHERE ItemGroupID = @MasterID
            AND TabName = @TabID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var queryConfig = await connection.QueryFirstOrDefaultAsync<string>(
            queryConfigSql,
            new { MasterID = masterID, TabID = tabID });

        if (string.IsNullOrEmpty(queryConfig))
            return new List<object>();

        // Execute dynamic query (preserve existing logic)
        string executeSql;
        if (queryConfig.ToUpper().Contains("EXECUTE"))
        {
            executeSql = queryConfig + " '', " + masterID;
        }
        else
        {
            executeSql = queryConfig;
        }

        try
        {
            var result = await connection.QueryAsync<dynamic>(executeSql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing drill-down query for masterID {MasterID}, tabID {TabID}", masterID, tabID);
            throw;
        }
    }

    public async Task<List<MasterFieldDto>> GetMasterFieldsAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                NULLIF(ItemGroupFieldID,'') as ItemGroupFieldID,
                NULLIF(ItemGroupID,'') as ItemGroupID,
                NULLIF(FieldName,'') as FieldName,
                NULLIF(FieldDataType,'') as FieldDataType,
                NULLIF(FieldDescription,'') as FieldDescription,
                NULLIF(IsDisplay,'') as IsDisplay,
                NULLIF(IsCalculated,'') as IsCalculated,
                NULLIF(FieldFormula,'') as FieldFormula,
                NULLIF(FieldTabIndex,'') as FieldTabIndex,
                NULLIF(FieldDrawSequence,'') as FieldDrawSequence,
                NULLIF(FieldDefaultValue,'') as FieldDefaultValue,
                NULLIF(CompanyID,'') as CompanyID,
                NULLIF(UserID,'') as UserID,
                NULLIF(ModifiedDate,'') as ModifiedDate,
                NULLIF(FYear,'') as FYear,
                NULLIF(IsActive,'') as IsActive,
                NULLIF(IsDeleted,'') as IsDeleted,
                NULLIF(FieldDisplayName,'') as FieldDisplayName,
                NULLIF(FieldType,'') as FieldType,
                NULLIF(SelectBoxQueryDB,'') as SelectBoxQueryDB,
                NULLIF(SelectBoxDefault,'') as SelectBoxDefault,
                NULLIF(ControllValidation,'') as ControllValidation,
                NULLIF(FieldFormulaString,'') as FieldFormulaString,
                NULLIF(IsRequiredFieldValidator,'') as IsRequiredFieldValidator,
                NULLIF(UnitMeasurement,'') as UnitMeasurement,
                IsLocked,
                ISNULL(MinimumValue, 0) AS MinimumValue,
                ISNULL(MaximumValue, 0) AS MaximumValue
            FROM ItemGroupFieldMaster
            WHERE ItemGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY FieldDrawSequence";

        var result = await connection.QueryAsync<MasterFieldDto>(sql, new { MasterID = masterID });
        return result.ToList();
    }

    public async Task<object> SelectBoxLoadAsync(JArray jsonData)
    {
        using var connection = GetConnection();
        var dataset = new Dictionary<string, List<dynamic>>();

        for (int i = 0; i < jsonData.Count; i++)
        {
            var fieldId = jsonData[i]["FieldID"]?.ToString();
            var fieldName = jsonData[i]["FieldName"]?.ToString();

            if (string.IsNullOrEmpty(fieldId) || string.IsNullOrEmpty(fieldName))
                continue;

            // Get dynamic query for this field
            var querySql = @"
                SELECT NULLIF(SelectboxQueryDB, '') as SelectboxQueryDB
                FROM ItemGroupFieldMaster
                WHERE ItemGroupFieldID = @FieldID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var query = await connection.QueryFirstOrDefaultAsync<string>(
                querySql, new { FieldID = fieldId });

            if (string.IsNullOrEmpty(query))
                continue;

            // Replace # with '
            query = query.Replace("#", "'");

            // Handle MySQL CALL syntax
            if (query.ToUpper().Contains("CALL "))
            {
                query = query + ");";
            }

            try
            {
                // Execute dynamic dropdown query
                var data = await connection.QueryAsync<dynamic>(query);
                var dataList = data.ToList();

                // Add field name as first row if 2 columns (preserve legacy logic)
                if (dataList.Count > 0)
                {
                    var firstItem = (IDictionary<string, object>)dataList[0];
                    if (firstItem.Count == 2)
                    {
                        var dynamicItem = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                        dynamicItem[firstItem.Keys.ElementAt(0)] = firstItem.Values.ElementAt(0);
                        dynamicItem[firstItem.Keys.ElementAt(1)] = fieldName;
                        dataList.Insert(0, dynamicItem);
                    }
                }

                dataset.Add($"tbl_{fieldName}", dataList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing selectbox query for field {FieldName}", fieldName);
            }
        }

        return dataset;
    }

    public async Task<List<UnderGroupDto>> GetUnderGroupAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ItemSubGroupID,
                ItemSubGroupDisplayName
            FROM ItemSubGroupMaster";

        var result = await connection.QueryAsync<UnderGroupDto>(sql);
        return result.ToList();
    }

    public async Task<List<GroupDto>> GetGroupAsync()
    {
        using var connection = GetConnection();
        var productionUnitIds = await _dbOperations.GetProductionUnitIdsAsync();

        var sql = $@"
            SELECT DISTINCT
                ISGM.ItemSubGroupUniqueID,
                ISGM.ItemSubGroupID,
                ISGM.ItemSubGroupDisplayName,
                ISGM.UnderSubGroupID,
                ISGM.ItemSubGroupName,
                ISGM.ItemSubGroupLevel,
                (SELECT TOP 1 ItemSubGroupDisplayName
                 FROM ItemSubGroupMaster
                 WHERE ItemSubGroupID = ISGM.UnderSubGroupID) as GroupName
            FROM ItemSubGroupMaster as ISGM
            WHERE ISGM.ProductionUnitID IN({productionUnitIds})
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<GroupDto>(sql);
        return result.ToList();
    }

    public async Task<string> SaveGroupAsync(SaveGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // Check if group name exists
            var existsSql = @"
                SELECT DISTINCT NULLIF(ItemSubGroupName, '') as ItemSubGroupName
                FROM ItemSubGroupMaster
                WHERE ItemSubGroupName = @GroupName
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                existsSql,
                new { request.GroupName },
                transaction);

            if (!string.IsNullOrEmpty(exists))
            {
                return "Exist";
            }

            // Get next ItemSubGroupID
            var nextIdSql = @"
                SELECT ISNULL(MAX(ItemSubGroupID), 0) + 1 AS ItemSubGroupID
                FROM ItemSubGroupMaster
                WHERE ISNULL(IsDeletedTransaction, 0) <> 1";

            var itemSubGroupID = await connection.QueryFirstOrDefaultAsync<int>(nextIdSql, transaction: transaction);

            // Get group level
            var levelSql = @"
                SELECT ISNULL(ItemSubGroupLevel, 0) ItemSubGroupLevel
                FROM ItemSubGroupMaster
                WHERE ItemSubGroupID = @UnderGroupID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var level = await connection.QueryFirstOrDefaultAsync<int?>(
                levelSql,
                new { request.UnderGroupID },
                transaction);

            var groupLevel = (level ?? 0) + 1;

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "ItemSubGroupMaster");

            // Build dynamic INSERT
            var groupMasterData = request.CostingDataGroupMaster[0];
            var columns = new List<string>
            {
                "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "ItemSubGroupID",
                "FYear", "CreatedBy", "ModifiedBy", "ItemSubGroupLevel", "ProductionUnitID"
            };
            var values = new List<string>
            {
                "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@ItemSubGroupID",
                "@FYear", "@CreatedBy", "@ModifiedBy", "@ItemSubGroupLevel", "@ProductionUnitID"
            };
            var parameters = new DynamicParameters();
            parameters.Add("@ModifiedDate", DateTime.Now);
            parameters.Add("@CreatedDate", DateTime.Now);
            parameters.Add("@UserID", userId);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@ItemSubGroupID", itemSubGroupID);
            parameters.Add("@FYear", fYear);
            parameters.Add("@CreatedBy", userId);
            parameters.Add("@ModifiedBy", userId);
            parameters.Add("@ItemSubGroupLevel", groupLevel);
            parameters.Add("@ProductionUnitID", productionUnitId);

            // Only add columns that exist in the table
            int paramIndex = 0;
            foreach (var kvp in groupMasterData)
            {
                if (validColumns.Contains(kvp.Key))
                {
                    columns.Add(kvp.Key);
                    var paramName = $"@param{paramIndex}";
                    values.Add(paramName);
                    parameters.Add(paramName, ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            var insertSql = $@"
                INSERT INTO ItemSubGroupMaster ({string.Join(", ", columns)})
                VALUES ({string.Join(", ", values)})";

            await connection.ExecuteAsync(insertSql, parameters, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving group");
            return "fail";
        }
    }

    public async Task<string> UpdateGroupAsync(UpdateGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // Check if group name exists (exclude current group)
            var existsSql = @"
                SELECT DISTINCT NULLIF(ItemSubGroupName, '') as ItemSubGroupName
                FROM ItemSubGroupMaster
                WHERE ItemSubGroupName = @GroupName
                AND ItemSubGroupUniqueID <> @ItemSubGroupUniqueID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                existsSql,
                new { request.GroupName, request.ItemSubGroupUniqueID },
                transaction);

            if (!string.IsNullOrEmpty(exists))
            {
                return "Exist";
            }

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "ItemSubGroupMaster");

            // Build dynamic UPDATE
            var groupMasterData = request.CostingDataGroupMaster[0];
            var setClause = new StringBuilder();
            var parameters = new DynamicParameters();

            setClause.Append("ModifiedDate = GETDATE(), ");
            setClause.Append($"UserID = {userId}, ");
            setClause.Append($"CompanyID = {companyId}, ");
            setClause.Append($"ModifiedBy = {userId}, ");
            setClause.Append($"ItemSubGroupLevel = '{request.ItemSubGroupLevel}', ");
            setClause.Append($"ProductionUnitID = '{productionUnitId}'");

            // Only update columns that exist in the table
            int paramIndex = 0;
            foreach (var kvp in groupMasterData)
            {
                if (validColumns.Contains(kvp.Key))
                {
                    setClause.Append($", {kvp.Key} = @param{paramIndex}");
                    parameters.Add($"@param{paramIndex}", ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            parameters.Add("@ItemSubGroupUniqueID", request.ItemSubGroupUniqueID);

            var updateSql = $@"
                UPDATE ItemSubGroupMaster
                SET {setClause}
                WHERE ItemSubGroupUniqueID = @ItemSubGroupUniqueID";

            await connection.ExecuteAsync(updateSql, parameters, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating group");
            return "fail";
        }
    }

    public async Task<string> DeleteGroupAsync(DeleteGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var sql = @"
                UPDATE ItemSubGroupMaster
                SET ModifiedBy = @UserID,
                    DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ItemSubGroupUniqueID = @ItemSubGroupUniqueID";

            await connection.ExecuteAsync(sql,
                new { UserID = userId, request.ItemSubGroupUniqueID },
                transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting group");
            return "fail";
        }
    }

    public async Task<List<ItemGroupDto>> GetItemsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT ItemGroupID, ItemGroupName
            FROM ItemGroupMaster
            WHERE ISNULL(IsDeleted, 0) = 0";

        var result = await connection.QueryAsync<ItemGroupDto>(sql);
        return result.ToList();
    }

    public async Task<List<LedgerGroupDto>> GetLedgersAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT LedgerGroupID, LedgerGroupName
            FROM LedgerGroupMaster
            WHERE CompanyID = @CompanyID
            AND ISNULL(IsDeleted, 0) = 0";

        var result = await connection.QueryAsync<LedgerGroupDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<string> CheckPermissionForUpdateAsync(string itemID)
    {
        using var connection = GetConnection();

        try
        {
            // Check JobBookingContent
            var sql1 = @"
                SELECT TOP 1 paperID
                FROM JobBookingContents
                WHERE IsDeletedTransaction = 0
                AND paperID = @ItemID";

            var exists1 = await connection.QueryFirstOrDefaultAsync<long?>(sql1, new { ItemID = itemID });
            if (exists1.HasValue)
            {
                return "Exist";
            }

            // Check JobBookingJobCardContents
            var sql2 = @"
                SELECT TOP 1 paperID
                FROM JobBookingJobCardContents
                WHERE IsDeletedTransaction = 0
                AND paperID = @ItemID";

            var exists2 = await connection.QueryFirstOrDefaultAsync<long?>(sql2, new { ItemID = itemID });
            if (exists2.HasValue)
            {
                return "Exist";
            }

            // Check ProductMasterContents
            var sql3 = @"
                SELECT TOP 1 paperID
                FROM ProductMasterContents
                WHERE IsDeletedTransaction = 0
                AND paperID = @ItemID";

            var exists3 = await connection.QueryFirstOrDefaultAsync<long?>(sql3, new { ItemID = itemID });
            if (exists3.HasValue)
            {
                return "Exist";
            }

            // Check ItemTransactionDetail
            var sql4 = @"
                SELECT TOP 1 ITD.ItemID
                FROM ItemTransactionMain AS ITM
                INNER JOIN ItemTransactionDetail AS ITD
                    ON ITM.TransactionID = ITD.TransactionID
                WHERE ISNULL(ITM.IsDeletedTransaction, 0) = 0
                    AND ITM.VoucherID <> -8
                    AND ITD.ItemID = @ItemID";

            var exists4 = await connection.QueryFirstOrDefaultAsync<long?>(sql4, new { ItemID = itemID });
            if (exists4.HasValue)
            {
                return "Exist";
            }

            return "Success";
        }
        catch (Exception)
        {
            return "fail";
        }
    }

    public async Task<string> UpdateUserItemAsync(UpdateUserItemRequest request)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;

        try
        {
            // Check StockRefCode duplicate
            var duplicateSql = @"
                SELECT NULLIF(StockRefCode, '') as StockRefCode
                FROM ItemMaster
                WHERE StockRefCode = @StockRefCode
                AND ItemID <> @ItemID
                AND IsDeletedTransaction = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                duplicateSql,
                new { request.StockRefCode, request.ItemID });

            if (!string.IsNullOrEmpty(exists))
            {
                return "Stock Ref. Code already exists.";
            }

            // Build dynamic UPDATE
            var itemData = request.ItemName[0];
            var setClause = new StringBuilder();
            var parameters = new DynamicParameters();

            setClause.Append("ModifiedDate = GETDATE(), ");
            setClause.Append($"UserID = {userId}, ");
            setClause.Append($"ModifiedBy = {userId}");

            int paramIndex = 0;
            foreach (var kvp in itemData)
            {
                setClause.Append($", {kvp.Key} = @param{paramIndex}");
                parameters.Add($"@param{paramIndex}", kvp.Value);
                paramIndex++;
            }

            parameters.Add("@ItemID", request.ItemID);

            var updateSql = $@"
                UPDATE ItemMaster
                SET {setClause}
                WHERE ItemID = @ItemID";

            await connection.ExecuteAsync(updateSql, parameters);
            return "Success";
        }
        catch (Exception)
        {
            return "fail";
        }
    }

    // Helper Methods

    private async Task<bool> CheckAuthorizationAsync(SqlConnection connection, SqlTransaction? transaction, long userId, long companyId, string itemGroupID, string permission)
    {
        var sql = $@"
            SELECT {permission}
            FROM UserSubModuleAuthentication
            WHERE UserID = @UserID
            AND ItemGroupID = @ItemGroupID
            AND CompanyID = @CompanyID";

        var result = await connection.QueryFirstOrDefaultAsync<bool?>(
            sql,
            new { UserID = userId, ItemGroupID = itemGroupID, CompanyID = companyId },
            transaction);

        return result ?? false;
    }

    private object? ConvertJsonElement(object? value)
    {
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.TryGetInt64(out long l) ? l : jsonElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => jsonElement.ToString()
            };
        }
        return value;
    }

    private async Task<HashSet<string>> GetTableColumnsAsync(SqlConnection connection, SqlTransaction? transaction, string tableName)
    {
        var sql = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName";

        var columns = await connection.QueryAsync<string>(sql, new { TableName = tableName }, transaction);
        return new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase);
    }
}
