using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class WarehouseMasterRepository : IWarehouseMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WarehouseMasterRepository> _logger;

    public WarehouseMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<WarehouseMasterRepository> logger)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    public async Task<WarehouseCodeDto> GetWarehouseNoAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT ISNULL(MAX(MaxWarehouseCode), 0) + 1 AS MaxWarehouseCode
            FROM WarehouseMaster
            WHERE CompanyID = @CompanyID
              AND Warehouseprefix = 'WH'
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var maxCode = await connection.QueryFirstOrDefaultAsync<long>(sql, new { CompanyID = companyId });
        var warehouseCode = $"WH{maxCode}";

        return new WarehouseCodeDto
        {
            WarehouseCode = warehouseCode,
            MaxWarehouseCode = maxCode,
            Prefix = "WH"
        };
    }

    public async Task<List<CityDto>> GetCityListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT NULLIF(City, '') AS City
            FROM CountryStateMaster
            WHERE ISNULL(City, '') <> ''
              AND ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY City";

        var result = await connection.QueryAsync<CityDto>(sql);
        return result.ToList();
    }

    public async Task<string> SaveWarehouseAsync(SaveWarehouseRequest request)
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

            // Validate production unit permission
            var canSave = await ValidateProductionUnitAsync(connection, transaction, userId, "Save");
            if (canSave != "Authorize")
            {
                return canSave;
            }

            if (request.SaveRecords.Length == 0)
            {
                return "No records to save";
            }

            // Generate warehouse code
            var maxCodeSql = @"
                SELECT ISNULL(MAX(MaxWarehouseCode), 0) + 1 AS MaxWarehouseCode
                FROM WarehouseMaster
                WHERE CompanyID = @CompanyID
                  AND Warehouseprefix = @Prefix
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var maxCode = await connection.QueryFirstOrDefaultAsync<long>(
                maxCodeSql,
                new { CompanyID = companyId, Prefix = request.Prefix },
                transaction);

            var warehouseCode = $"{request.Prefix}{maxCode}";

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "WarehouseMaster");

            foreach (var record in request.SaveRecords)
            {
                var columns = new List<string>
                {
                    "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                    "CreatedBy", "ModifiedBy", "Warehouseprefix", "MaxWarehouseCode", "WarehouseCode"
                };
                var values = new List<string>
                {
                    "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                    "@CreatedBy", "@ModifiedBy", "@Prefix", "@MaxCode", "@WarehouseCode"
                };
                var parameters = new DynamicParameters();
                parameters.Add("@ModifiedDate", DateTime.Now);
                parameters.Add("@CreatedDate", DateTime.Now);
                parameters.Add("@UserID", userId);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@FYear", fYear);
                parameters.Add("@CreatedBy", userId);
                parameters.Add("@ModifiedBy", userId);
                parameters.Add("@Prefix", request.Prefix);
                parameters.Add("@MaxCode", maxCode);
                parameters.Add("@WarehouseCode", warehouseCode);

                // Add dynamic columns from the request
                int paramIndex = 0;
                foreach (var kvp in record)
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
                    INSERT INTO WarehouseMaster ({string.Join(", ", columns)})
                    VALUES ({string.Join(", ", values)})";

                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving warehouse");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateWarehouseAsync(UpdateWarehouseRequest request)
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

            // Validate production unit permission
            var canSave = await ValidateProductionUnitAsync(connection, transaction, userId, "Save");
            if (canSave != "Authorize")
            {
                return canSave;
            }

            var validColumns = await GetTableColumnsAsync(connection, transaction, "WarehouseMaster");

            // Update existing records
            if (request.UpdateRecords.Length > 0)
            {
                foreach (var record in request.UpdateRecords)
                {
                    var setClause = new List<string> { "ModifiedDate = GETDATE()", $"ModifiedBy = {userId}" };
                    var parameters = new DynamicParameters();

                    // Extract WarehouseID for WHERE clause
                    if (!record.ContainsKey("WarehouseID"))
                    {
                        continue; // Skip if no ID
                    }

                    var warehouseId = ConvertJsonElement(record["WarehouseID"]);
                    parameters.Add("@WarehouseID", warehouseId);

                    int paramIndex = 0;
                    foreach (var kvp in record)
                    {
                        if (kvp.Key != "WarehouseID" && validColumns.Contains(kvp.Key))
                        {
                            var paramName = $"@param{paramIndex}";
                            setClause.Add($"{kvp.Key} = {paramName}");
                            parameters.Add(paramName, ConvertJsonElement(kvp.Value));
                            paramIndex++;
                        }
                    }

                    var updateSql = $@"
                        UPDATE WarehouseMaster
                        SET {string.Join(", ", setClause)}
                        WHERE WarehouseID = @WarehouseID
                          AND ProductionUnitID = {productionUnitId}";

                    await connection.ExecuteAsync(updateSql, parameters, transaction);
                }
            }

            // Insert new records (same as SaveWarehouse logic)
            if (request.SaveRecords.Length > 0)
            {
                foreach (var record in request.SaveRecords)
                {
                    var columns = new List<string>
                    {
                        "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                        "CreatedBy", "ModifiedBy"
                    };
                    var values = new List<string>
                    {
                        "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                        "@CreatedBy", "@ModifiedBy"
                    };
                    var parameters = new DynamicParameters();
                    parameters.Add("@ModifiedDate", DateTime.Now);
                    parameters.Add("@CreatedDate", DateTime.Now);
                    parameters.Add("@UserID", userId);
                    parameters.Add("@CompanyID", companyId);
                    parameters.Add("@FYear", fYear);
                    parameters.Add("@CreatedBy", userId);
                    parameters.Add("@ModifiedBy", userId);

                    int paramIndex = 0;
                    foreach (var kvp in record)
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
                        INSERT INTO WarehouseMaster ({string.Join(", ", columns)})
                        VALUES ({string.Join(", ", values)})";

                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                }
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating warehouse");
            return $"fail {ex.Message}";
        }
    }

    public async Task<List<WarehouseListDto>> GetWarehouseListAsync()
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var productionUnitIdStr = _currentUserService.GetProductionUnitIdStr() ?? "0";

        // Check if user can access multiple production units
        var userAccessSql = @"
            SELECT
                ISNULL(CanAccessMultipleProductionUnitData, 0) AS CanAccessMultipleProductionUnitData,
                ISNULL(ProductionUnitID, 0) AS ProductionUnitID
            FROM UserMaster
            WHERE UserID = @UserID";

        var userAccess = await connection.QueryFirstOrDefaultAsync<dynamic>(userAccessSql, new { UserID = userId });

        string productionUnitFilter = "";
        if (userAccess != null && userAccess.CanAccessMultipleProductionUnitData == false)
        {
            productionUnitFilter = $" AND PUM.ProductionUnitID = {userAccess.ProductionUnitID}";
        }

        var sql = $@"
            SELECT
                BM.BranchName,
                BM.BranchID,
                WM.RefWarehouseCode,
                ISNULL(WM.IsFloorWarehouse, 0) AS IsFloorWarehouse,
                PUM.ProductionUnitName,
                PUM.ProductionUnitID,
                WM.WarehouseID,
                WM.WarehouseName,
                WM.WarehouseCode,
                NULLIF(WM.City, '') AS City,
                NULLIF(WM.Address, '') AS Address,
                REPLACE(CONVERT(nvarchar(30), WM.ModifiedDate, 106), ' ', '-') AS ModifiedDate
            FROM WarehouseMaster AS WM
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = WM.ProductionUnitID
            LEFT JOIN BranchMaster AS BM ON BM.BranchID = WM.BranchID
            WHERE WM.ProductionUnitID IN ({productionUnitIdStr})
              AND ISNULL(WM.IsDeletedTransaction, 0) = 0
              {productionUnitFilter}
            ORDER BY WM.WarehouseID DESC";

        var result = await connection.QueryAsync<WarehouseListDto>(sql);
        return result.ToList();
    }

    public async Task<List<WarehouseBinDto>> GetBinNameAsync(string warehouseName)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(WarehouseID, 0) AS WarehouseID,
                NULLIF(BinName, '') AS BinName
            FROM WarehouseMaster
            WHERE WarehouseName = @WarehouseName
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<WarehouseBinDto>(
            sql,
            new { WarehouseName = warehouseName });

        return result.ToList();
    }

    public async Task<string> DeleteWarehouseAsync(string warehouseId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            // Validate production unit permission
            var canDelete = await ValidateProductionUnitAsync(connection, transaction, userId, "Save");
            if (canDelete != "Authorize")
            {
                return canDelete;
            }

            // Check if warehouse is used in any transaction
            var checkUsageSql = @"
                SELECT TOP 1 ISNULL(WarehouseID, 0) AS WarehouseID
                FROM ItemTransactionDetail
                WHERE WarehouseID = @WarehouseID
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var usageExists = await connection.QueryFirstOrDefaultAsync<int?>(
                checkUsageSql,
                new { WarehouseID = warehouseId },
                transaction);

            if (usageExists.HasValue && usageExists.Value > 0)
            {
                return "Exist"; // Warehouse is being used, cannot delete
            }

            // Soft delete
            var deleteSql = @"
                UPDATE WarehouseMaster
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE WarehouseID = @WarehouseID";

            await connection.ExecuteAsync(
                deleteSql,
                new { ModifiedBy = userId, DeletedBy = userId, WarehouseID = warehouseId },
                transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", warehouseId);
            return "fail";
        }
    }

    public async Task<List<BranchDto>> GetBranchListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                BranchID,
                BranchName,
                BranchCode,
                CompanyID,
                CAST(CASE WHEN ISNULL(IsDeletedTransaction, 0) = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive
            FROM BranchMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY BranchName";

        var result = await connection.QueryAsync<BranchDto>(sql);
        return result.ToList();
    }

    // Helper methods

    private async Task<string> ValidateProductionUnitAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int userId,
        string action)
    {
        // Note: This is a placeholder for production unit validation logic
        // The original VB code calls db.validateProductionUnit which isn't fully defined
        // You may need to implement this based on your business rules

        var sql = @"
            SELECT ISNULL(CanAccessMultipleProductionUnitData, 0) AS CanAccess
            FROM UserMaster
            WHERE UserID = @UserID";

        var canAccess = await connection.QueryFirstOrDefaultAsync<bool>(
            sql,
            new { UserID = userId },
            transaction);

        // Simplified validation - return "Authorize" if user has access
        return "Authorize";
    }

    private async Task<HashSet<string>> GetTableColumnsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string tableName)
    {
        var sql = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName";

        var columns = await connection.QueryAsync<string>(sql, new { TableName = tableName }, transaction);
        return new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase);
    }

    private object? ConvertJsonElement(object value)
    {
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.TryGetInt64(out var l) ? l : jsonElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => jsonElement.ToString()
            };
        }
        return value;
    }
}
