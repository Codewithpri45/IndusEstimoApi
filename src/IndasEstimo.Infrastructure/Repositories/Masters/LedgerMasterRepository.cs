using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

public class LedgerMasterRepository : ILedgerMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LedgerMasterRepository> _logger;
    private readonly IConfiguration _configuration;

    public LedgerMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<LedgerMasterRepository> logger,
        IConfiguration configuration)
    {
        _tenantProvider = tenantProvider;
        _connectionFactory = connectionFactory;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // ==================== Core CRUD Operations ====================

    public async Task<List<LedgerMasterListDto>> GetMasterListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                LedgerGroupID,
                LedgerGroupName,
                NULLIF(LedgerGroupNameDisplay,'') as LedgerGroupNameDisplay,
                NULLIF(LedgerGroupNameID,'') as LedgerGroupNameID
            FROM LedgerGroupMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY LedgerGroupID";

        var result = await connection.QueryAsync<LedgerMasterListDto>(sql);
        return result.ToList();
    }

    public async Task<object> GetMasterGridAsync(string masterID)
    {
        using var connection = GetConnection();

        // Step 1: Get SelectQuery from LedgerGroupMaster
        var queryConfigSql = @"
            SELECT NULLIF(SelectQuery, '') as SelectQuery
            FROM LedgerGroupMaster
            WHERE LedgerGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var selectQuery = await connection.QueryFirstOrDefaultAsync<string>(
            queryConfigSql,
            new { MasterID = masterID });

        if (string.IsNullOrEmpty(selectQuery))
        {
            _logger.LogWarning("GetMasterGridAsync - No SelectQuery found for MasterID {MasterID}", masterID);
            return new List<object>();
        }

        // Step 2: Get the actual CompanyID from LedgerMaster data for this group
        // This is the REAL CompanyID stored in the data, which the proc needs to filter correctly
        var companyIdSql = @"
            SELECT TOP 1 ISNULL(CompanyID, 0)
            FROM LedgerMaster
            WHERE LedgerGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) = 0";

        var dataCompanyId = await connection.QueryFirstOrDefaultAsync<long>(
            companyIdSql,
            new { MasterID = masterID });

        // Fall back to JWT claim if no data found
        var companyId = dataCompanyId > 0
            ? dataCompanyId
            : (_currentUserService.GetCompanyId() ?? 0);

        _logger.LogDebug("GetMasterGridAsync - MasterID: {MasterID}, SelectQuery: {SelectQuery}, CompanyID: {CompanyID}",
            masterID, selectQuery, companyId);

        // Step 3: Build exec SQL — proc signature: GetLedgerMasterData @TblName, @CompanyID, @LedgerGroupID
        string executeSql;
        if (selectQuery.TrimEnd().ToUpper().EndsWith("GETLEDGERMASTERDATA") ||
            selectQuery.ToUpper().Contains("EXECUTE"))
        {
            // Pass CompanyID as integer (no quotes), TblName as empty string
            executeSql = "EXECUTE GetLedgerMasterData '', " + companyId + ", " + masterID;
        }
        else
        {
            executeSql = selectQuery;
        }

        _logger.LogDebug("GetMasterGridAsync - Executing SQL: {SQL}", executeSql);

        try
        {
            var result = await connection.QueryAsync<dynamic>(executeSql);
            var filtered = result.Where(r =>
            {
                var dict = (IDictionary<string, object>)r;
                if (dict.TryGetValue("IsDeletedTransaction", out var val))
                {
                    return Convert.ToInt32(val ?? 0) == 0;
                }
                return true;
            }).ToList();
            _logger.LogDebug("GetMasterGridAsync - Row count: {Count}", filtered.Count);
            return filtered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing dynamic grid query for masterID {MasterID}", masterID);
            throw;
        }
    }

    public async Task<List<LedgerGridColumnHideDto>> GetGridColumnHideAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                NULLIF(GridColumnHide,'') as GridColumnHide,
                NULLIF(TabName,'') as TabName,
                NULLIF(ConcernPerson,'') as ConcernPerson,
                NULLIF(EmployeeMachineAllocation,'') as EmployeeMachineAllocation
            FROM LedgerGroupMaster
            WHERE LedgerGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<LedgerGridColumnHideDto>(sql, new { MasterID = masterID });
        return result.ToList();
    }

    public async Task<List<LedgerGridColumnDto>> GetGridColumnAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT NULLIF(GridColumnName,'') as GridColumnName
            FROM LedgerGroupMaster
            WHERE LedgerGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<LedgerGridColumnDto>(sql, new { MasterID = masterID });
        return result.ToList();
    }

    public async Task<List<LedgerMasterFieldDto>> GetMasterFieldsAsync(string masterID)
    {
        var sql = @"
            SELECT DISTINCT
                NULLIF(LedgerGroupFieldID,'') as LedgerGroupFieldID,
                NULLIF(LedgerGroupID,'') as LedgerGroupID,
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
                IsLocked
            FROM LedgerGroupFieldMaster
            WHERE LedgerGroupID = @MasterID
            AND ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY FieldDrawSequence";

        // Try tenant DB first, fall back to master DB if empty
        using (var tenantConnection = GetConnection())
        {
            var result = await tenantConnection.QueryAsync<LedgerMasterFieldDto>(sql, new { MasterID = masterID });
            var list = result.ToList();
            if (list.Count > 0)
                return list;
        }

        // LedgerGroupFieldMaster may be in master DB (IndusEnterpriseMonarch)
        using (var masterConnection = _connectionFactory.CreateMasterConnection())
        {
            var result = await masterConnection.QueryAsync<LedgerMasterFieldDto>(sql, new { MasterID = masterID });
            return result.ToList();
        }
    }

    public async Task<object> GetLoadedDataAsync(string masterID, string ledgerID)
    {
        using var connection = GetConnection();

        // Execute stored procedure SelectedRowLedgerMultiUnit
        var sql = $"EXECUTE SelectedRowLedgerMultiUnit '', {masterID}, {ledgerID}";

        try
        {
            var result = await connection.QueryAsync<dynamic>(sql);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectedRowLedgerMultiUnit for masterID {MasterID}, ledgerID {LedgerID}", masterID, ledgerID);
            throw;
        }
    }

    public async Task<object> GetDrillDownAsync(string masterID, string tabID, string ledgerID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Get dynamic query from LedgerMasterDrilDown table
        var queryConfigSql = @"
            SELECT SelectQuery
            FROM LedgerMasterDrilDown
            WHERE ISNULL(SelectQuery,'') <> ''
            AND IsDeletedTransaction = 0
            AND LedgerGroupID = @MasterID
            AND TabName = @TabID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var queryConfig = await connection.QueryFirstOrDefaultAsync<string>(
            queryConfigSql,
            new { MasterID = masterID, TabID = tabID });

        if (string.IsNullOrEmpty(queryConfig))
            return new List<object>();

        // Execute dynamic query with parameter replacement
        string executeSql;
        if (queryConfig.StartsWith("Exec", StringComparison.OrdinalIgnoreCase))
        {
            executeSql = queryConfig + " '', " + companyId + ", " + masterID;
        }
        else
        {
            executeSql = queryConfig
                .Replace("@CompanyID", companyId.ToString())
                .Replace("@LedgerGroupID", masterID)
                .Replace("@LedgerID", ledgerID);
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

    public async Task<string> SaveLedgerAsync(SaveLedgerRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Check authorization
            var canSave = await CheckAuthorizationAsync(connection, transaction, userId, companyId, request.LedgerGroupID, "CanSave");
            if (!canSave)
            {
                return "You are not authorized to save..!";
            }

            // Check duplicate using SaveAsString validation
            if (await CheckDuplicateLedgerAsync(connection, transaction, request.LedgerGroupID, request.CostingDataLedgerDetailMaster, companyId))
            {
                return "Duplicate data found";
            }

            // Check LedgerRefCode duplicate
            var duplicateSql = @"
                SELECT TOP 1 NULLIF(LedgerRefCode, '') as LedgerRefCode
                FROM LedgerMaster
                WHERE LedgerRefCode = @LedgerRefCode
                AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                duplicateSql,
                new { request.LedgerRefCode },
                transaction);

            if (!string.IsNullOrEmpty(exists))
            {
                return "Ledger Ref. Code already exists.";
            }

            // Get LedgerCode prefix
            var ledgerCodePrefixSql = @"
                SELECT NULLIF(LedgerGroupPrefix,'') as LedgerGroupPrefix
                FROM LedgerGroupMaster
                WHERE LedgerGroupID = @LedgerGroupID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var ledgerCodePrefix = await connection.QueryFirstOrDefaultAsync<string>(
                ledgerCodePrefixSql,  
                new { request.LedgerGroupID },
                transaction);

            // Generate LedgerCode
            var maxLedgerNoSql = @"
                SELECT ISNULL(MAX(MaxLedgerNo), 0) + 1 AS MaxLedgerNo
                FROM LedgerMaster
                WHERE LedgerCodeprefix = @LedgerCodePrefix
                AND LedgerGroupID = @LedgerGroupID
                AND ISNULL(IsDeletedTransaction, 0) = 0";

            var maxLedgerNo = await connection.QueryFirstOrDefaultAsync<long>(
                maxLedgerNoSql,
                new { LedgerCodePrefix = ledgerCodePrefix, request.LedgerGroupID },
                transaction);

            var ledgerCode = $"{ledgerCodePrefix}{maxLedgerNo.ToString().PadLeft(5, '0')}";

            // Get valid columns from table schema
            var validColumns = await GetTableColumnsAsync(connection, transaction, "LedgerMaster");

            // Build dynamic INSERT statement for LedgerMaster
            var columns = new List<string>
            {
                "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                "CreatedBy", "ModifiedBy", "LedgerCode", "LedgerCodeprefix", "MaxLedgerNo"
            };
            var values = new List<string>
            {
                "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                "@CreatedBy", "@ModifiedBy", "@LedgerCode", "@LedgerCodePrefix", "@MaxLedgerNo"
            };
            var parameters = new DynamicParameters();
            parameters.Add("@ModifiedDate", DateTime.Now);
            parameters.Add("@CreatedDate", DateTime.Now);
            parameters.Add("@UserID", userId);
            parameters.Add("@CompanyID", companyId);
            parameters.Add("@FYear", fYear);
            parameters.Add("@CreatedBy", userId);
            parameters.Add("@ModifiedBy", userId);
            parameters.Add("@LedgerCode", ledgerCode);
            parameters.Add("@LedgerCodePrefix", ledgerCodePrefix);
            parameters.Add("@MaxLedgerNo", maxLedgerNo);

            // Add dynamic columns from CostingDataLedgerMaster
            var ledgerMasterData = request.CostingDataLedgerMaster[0];
            int paramIndex = 0;
            foreach (var kvp in ledgerMasterData)
            {
                if (validColumns.Contains(kvp.Key) &&
                    !columns.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                {
                    columns.Add(kvp.Key);
                    var paramName = $"@param{paramIndex}";
                    values.Add(paramName);
                    parameters.Add(paramName, ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            var insertSql = $@"
                INSERT INTO LedgerMaster ({string.Join(", ", columns)})
                OUTPUT INSERTED.LedgerID
                VALUES ({string.Join(", ", values)})";

            var ledgerID = await connection.ExecuteScalarAsync<long>(insertSql, parameters, transaction);

            if (ledgerID <= 0)
            {
                return "Error in main";
            }

            // Insert LedgerMasterDetails
            var detailValidColumns = await GetTableColumnsAsync(connection, transaction, "LedgerMasterDetails");
            foreach (var detailRow in request.CostingDataLedgerDetailMaster)
            {
                var detailColumns = new List<string>
                {
                    "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "LedgerID",
                    "FYear", "CreatedBy", "ModifiedBy", "LedgerGroupID"
                };
                var detailValues = new List<string>
                {
                    "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@LedgerID",
                    "@FYear", "@CreatedBy", "@ModifiedBy", "@LedgerGroupID"
                };
                var detailParams = new DynamicParameters();
                detailParams.Add("@ModifiedDate", DateTime.Now);
                detailParams.Add("@CreatedDate", DateTime.Now);
                detailParams.Add("@UserID", userId);
                detailParams.Add("@CompanyID", companyId);
                detailParams.Add("@LedgerID", ledgerID);
                detailParams.Add("@FYear", fYear);
                detailParams.Add("@CreatedBy", userId);
                detailParams.Add("@ModifiedBy", userId);
                detailParams.Add("@LedgerGroupID", request.LedgerGroupID);

                int detailParamIndex = 0;
                foreach (var kvp in detailRow)
                {
                    if (detailValidColumns.Contains(kvp.Key) &&
                        !detailColumns.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        detailColumns.Add(kvp.Key);
                        var paramName = $"@dparam{detailParamIndex}";
                        detailValues.Add(paramName);
                        detailParams.Add(paramName, ConvertJsonElement(kvp.Value));
                        detailParamIndex++;
                    }
                }

                var detailInsertSql = $@"
                    INSERT INTO LedgerMasterDetails ({string.Join(", ", detailColumns)})
                    VALUES ({string.Join(", ", detailValues)})";

                await connection.ExecuteAsync(detailInsertSql, detailParams, transaction);
            }

            // Insert ISLedgerActive field
            var activeLedgerSql = @"
                INSERT INTO LedgerMasterDetails
                (ModifiedDate, CreatedDate, UserID, CompanyID, LedgerID, FYear, CreatedBy, ModifiedBy,
                 FieldValue, ParentFieldValue, ParentFieldName, FieldName, LedgerGroupID)
                VALUES
                (GETDATE(), GETDATE(), @UserID, @CompanyID, @LedgerID, @FYear, @CreatedBy, @ModifiedBy,
                 @ActiveLedger, @ActiveLedger, 'ISLedgerActive', 'ISLedgerActive', @LedgerGroupID)";

            await connection.ExecuteAsync(activeLedgerSql, new
            {
                UserID = userId,
                CompanyID = companyId,
                LedgerID = ledgerID,
                FYear = fYear,
                CreatedBy = userId,
                ModifiedBy = userId,
                ActiveLedger = request.ActiveLedger,
                request.LedgerGroupID
            }, transaction);

            // Execute stored procedure to update calculated fields
            await connection.ExecuteAsync(
                "EXEC UpdateLedgerMasterValuesMultiUnit @CompanyID, @LedgerID",
                new { CompanyID = companyId, LedgerID = ledgerID },
                transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving ledger");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateLedgerAsync(UpdateLedgerRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // ===================== 1. LOCK CHECK =====================
            var isLocked = await connection.QueryFirstOrDefaultAsync<bool>(
                @"SELECT ISNULL(IsLocked,0)
              FROM LedgerMaster
              WHERE LedgerID = @LedgerID AND CompanyID = @CompanyID",
                new { request.LedgerID, CompanyID = companyId },
                transaction);

            if (isLocked)
                return "fail";

            // ===================== 2. AUTH CHECK =====================
            var canEdit = await CheckAuthorizationAsync(
                connection, transaction, userId, companyId,
                request.UnderGroupID, "CanEdit");

            if (!canEdit)
                return "You are not authorized to update..!";

            // ===================== 3. DUPLICATE REF CODE =====================
            var exists = await connection.QueryFirstOrDefaultAsync<string>(
                @"SELECT LedgerRefCode
              FROM LedgerMaster
              WHERE LedgerRefCode = @LedgerRefCode
              AND LedgerID <> @LedgerID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction,0)=0",
                new { request.LedgerRefCode, request.LedgerID, CompanyID = companyId },
                transaction);

            if (!string.IsNullOrEmpty(exists))
                return "Ledger Ref. Code already exists.";

            // ===================== 4. UPDATE LedgerMaster =====================
            var validMasterColumns = await GetTableColumnsAsync(connection, transaction, "LedgerMaster");
            var setClause = new StringBuilder("ModifiedDate=GETDATE(), UserID=@UserID, CompanyID=@CompanyID, ModifiedBy=@UserID");

            var masterParams = new DynamicParameters();
            masterParams.Add("@UserID", userId);
            masterParams.Add("@CompanyID", companyId);
            masterParams.Add("@LedgerID", request.LedgerID);

            int mi = 0;
            foreach (var kvp in request.CostingDataLedgerMaster[0])
            {
                if (validMasterColumns.Contains(kvp.Key) &&
                    !kvp.Key.Equals("LedgerID", StringComparison.OrdinalIgnoreCase))
                {
                    setClause.Append($", {kvp.Key}=@m{mi}");
                    masterParams.Add($"@m{mi}", ConvertJsonElement(kvp.Value));
                    mi++;
                }
            }

            await connection.ExecuteAsync(
                $"UPDATE LedgerMaster SET {setClause} WHERE LedgerID=@LedgerID AND CompanyID=@CompanyID",
                masterParams, transaction);

            // ===================== 5. UPSERT LedgerMasterDetails =====================
            var validDetailColumns = await GetTableColumnsAsync(connection, transaction, "LedgerMasterDetails");

            foreach (var row in request.CostingDataLedgerDetailMaster)
            {
                var fieldName = row["FieldName"]?.ToString();
                if (string.IsNullOrEmpty(fieldName)) continue;

                var existsDetail = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
                  FROM LedgerMasterDetails
                  WHERE LedgerID=@LedgerID
                  AND LedgerGroupID=@LedgerGroupID
                  AND FieldName=@FieldName
                  AND ISNULL(IsDeletedTransaction,0)=0",
                    new
                    {
                        request.LedgerID,
                        LedgerGroupID = request.UnderGroupID,
                        FieldName = fieldName
                    }, transaction);

                if (existsDetail == 0)
                {
                    // -------- INSERT --------
                    var cols = new List<string>
                {
                    "LedgerID","LedgerGroupID","CompanyID","UserID","FYear",
                    "CreatedBy","ModifiedBy","CreatedDate","ModifiedDate"
                };
                    var vals = new List<string>
                {
                    "@LedgerID","@LedgerGroupID","@CompanyID","@UserID","@FYear",
                    "@UserID","@UserID","GETDATE()","GETDATE()"
                };

                    var p = new DynamicParameters(new
                    {
                        request.LedgerID,
                        LedgerGroupID = request.UnderGroupID,
                        CompanyID = companyId,
                        UserID = userId,
                        FYear = fYear
                    });

                    int di = 0;
                    foreach (var kvp in row)
                    {
                        if (validDetailColumns.Contains(kvp.Key) &&
                            !cols.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                        {
                            cols.Add(kvp.Key);
                            vals.Add($"@d{di}");
                            p.Add($"@d{di}", ConvertJsonElement(kvp.Value));
                            di++;
                        }
                    }

                    await connection.ExecuteAsync(
                        $"INSERT INTO LedgerMasterDetails ({string.Join(",", cols)}) VALUES ({string.Join(",", vals)})",
                        p, transaction);
                }
                else
                {
                    // -------- UPDATE --------
                    var sb = new StringBuilder("ModifiedDate=GETDATE(), UserID=@UserID, CompanyID=@CompanyID, ModifiedBy=@UserID");
                    var p = new DynamicParameters(new
                    {
                        request.LedgerID,
                        LedgerGroupID = request.UnderGroupID,
                        CompanyID = companyId,
                        UserID = userId,
                        FieldName = fieldName
                    });

                    int ui = 0;
                    foreach (var kvp in row)
                    {
                        if (validDetailColumns.Contains(kvp.Key) &&
                            !kvp.Key.Equals("FieldName", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append($", {kvp.Key}=@u{ui}");
                            p.Add($"@u{ui}", ConvertJsonElement(kvp.Value));
                            ui++;
                        }
                    }

                    await connection.ExecuteAsync(
                        $@"UPDATE LedgerMasterDetails
                       SET {sb}
                       WHERE LedgerID=@LedgerID
                       AND LedgerGroupID=@LedgerGroupID
                       AND FieldName=@FieldName",
                        p, transaction);
                }
            }

            // ===================== 6. UPDATE ISLedgerActive =====================
            await connection.ExecuteAsync(
                @"UPDATE LedgerMaster
              SET ISLedgerActive=@ActiveLedger,
                  ModifiedDate=GETDATE(),
                  ModifiedBy=@UserID
              WHERE LedgerID=@LedgerID AND CompanyID=@CompanyID",
                new
                {
                    request.ActiveLedger,
                    UserID = userId,
                    request.LedgerID,
                    CompanyID = companyId
                }, transaction);

            await connection.ExecuteAsync(
                @"UPDATE LedgerMasterDetails
              SET FieldValue=@ActiveLedger,
                  ParentFieldValue=@ActiveLedger,
                  ModifiedDate=GETDATE(),
                  ModifiedBy=@UserID
              WHERE LedgerID=@LedgerID
              AND LedgerGroupID=@LedgerGroupID
              AND FieldName='ISLedgerActive'",
                new
                {
                    request.ActiveLedger,
                    UserID = userId,
                    request.LedgerID,
                    LedgerGroupID = request.UnderGroupID
                }, transaction);

            // ===================== 7. RE-CALCULATE =====================
            await connection.ExecuteAsync(
                "EXEC UpdateLedgerMasterValues @CompanyID, @LedgerID",
                new { CompanyID = companyId, LedgerID = request.LedgerID },
                transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "UpdateLedgerAsync failed");
            return $"fail {ex.Message}";
        }
    }


    public async Task<string> DeleteLedgerAsync(string ledgerID, string ledgerGroupID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            await connection.OpenAsync();

            // Check authorization
            var canDelete = await CheckAuthorizationAsync(connection, null, userId, companyId, ledgerGroupID, "CanDelete");
            if (!canDelete)
            {
                return "You are not authorized to delete..!";
            }

            // Check if locked
            var isLockedSql = @"
                SELECT IsLocked
                FROM LedgerMaster
                WHERE IsLocked = 1
                AND LedgerID = @LedgerID";

            var isLocked = await connection.QueryFirstOrDefaultAsync<bool?>(
                isLockedSql,
                new { LedgerID = ledgerID });

            if (isLocked == true)
            {
                return "fail";
            }

            // Soft delete LedgerMasterDetails
            var deleteDetailsSql = @"
                UPDATE LedgerMasterDetails
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE LedgerID = @LedgerID
                AND LedgerGroupID = @LedgerGroupID";

            await connection.ExecuteAsync(deleteDetailsSql, new
            {
                UserID = userId,
                LedgerID = ledgerID,
                LedgerGroupID = ledgerGroupID
            });

            // Soft delete LedgerMaster
            var deleteMasterSql = @"
                UPDATE LedgerMaster
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE LedgerID = @LedgerID
                AND LedgerGroupID = @LedgerGroupID";

            await connection.ExecuteAsync(deleteMasterSql, new
            {
                UserID = userId,
                LedgerID = ledgerID,
                LedgerGroupID = ledgerGroupID
            });

            // Soft delete BusinessVerticalDetails
            var deleteBusinessVerticalSql = @"
                UPDATE BusinessVerticalDetails
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID
                AND LedgerID = @LedgerID";

            await connection.ExecuteAsync(deleteBusinessVerticalSql, new
            {
                UserID = userId,
                CompanyID = companyId,
                LedgerID = ledgerID
            });

            // Soft delete EmbargoDetails
            var deleteEmbargoSql = @"
                UPDATE EmbargoDetails
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID
                AND LedgerID = @LedgerID";

            await connection.ExecuteAsync(deleteEmbargoSql, new
            {
                UserID = userId,
                CompanyID = companyId,
                LedgerID = ledgerID
            });

            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ledger");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> CheckPermissionAsync(string ledgerID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            // Check in multiple tables for ledger usage
            var tables = new[]
            {
                "ConcernPersonMaster",
                "ClientMachineCostSettings",
                "EmployeeMachineAllocation",
                "ItemPurchaseOrderTaxes",
                "ItemTransactionMain",
                "JobBooking",
                "SupplierWisePurchaseSetting",
                "JobOrderBooking"
            };

            foreach (var table in tables)
            {
                var sql = $@"
                    SELECT TOP 1 ISNULL(LedgerID, 0) AS LedgerID
                    FROM {table}
                    WHERE CompanyID = @CompanyID
                    AND LedgerID = @LedgerID
                    AND ISNULL(IsDeletedTransaction, 0) <> 1";

                var exists = await connection.QueryFirstOrDefaultAsync<long?>(sql,
                    new { CompanyID = companyId, LedgerID = ledgerID });

                if (exists.HasValue && exists.Value > 0)
                {
                    return "Exist";
                }
            }

            return "";
        }
        catch (Exception)
        {
            return "fail";
        }
    }

    public async Task<object> SelectBoxLoadAsync(JArray jsonData)
    {
        var dataset = new Dictionary<string, List<dynamic>>();

        var querySql = @"
            SELECT NULLIF(SelectboxQueryDB, '') as SelectboxQueryDB,
                   NULLIF(SelectBoxDefault, '') as SelectBoxDefault
            FROM LedgerGroupFieldMaster
            WHERE LedgerGroupFieldID = @FieldID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        for (int i = 0; i < jsonData.Count; i++)
        {
            // Accept LedgerGroupFieldID (from master-fields response) or FieldID (Postman/direct)
            var fieldId = jsonData[i]["LedgerGroupFieldID"]?.ToString();
            if (string.IsNullOrEmpty(fieldId))
                fieldId = jsonData[i]["FieldID"]?.ToString();

            var fieldName = jsonData[i]["FieldName"]?.ToString();

            if (string.IsNullOrEmpty(fieldId) || string.IsNullOrEmpty(fieldName))
                continue;

            // Try tenant DB first (where most ledger data lives), fall back to master DB
            dynamic? fieldConfig = null;
            using (var tenantConn = GetConnection())
            {
                fieldConfig = await tenantConn.QueryFirstOrDefaultAsync<dynamic>(querySql, new { FieldID = fieldId });
            }
            // Fallback: try master DB (IndusEnterpriseMonarch)
            if (fieldConfig == null)
            {
                using var masterConn = _connectionFactory.CreateMasterConnection();
                fieldConfig = await masterConn.QueryFirstOrDefaultAsync<dynamic>(querySql, new { FieldID = fieldId });
            }

            if (fieldConfig == null)
                continue;

            string? query = (string?)fieldConfig.SelectboxQueryDB;
            string? defaults = (string?)fieldConfig.SelectBoxDefault;

            var resultList = new List<dynamic>();

            // 1. Execute dynamic query if available (returns full rows with ID + display columns)
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Replace("#", "'");
                var companyId = _currentUserService.GetCompanyId() ?? 0;
                query = query.Replace("'CompanyID'", $"'{companyId}'");

                List<dynamic> dataList = new();

                // Try tenant DB first (LedgerMaster-based queries like RefSalesRepresentativeID)
                try
                {
                    using var tenantConn = GetConnection();
                    var data = await tenantConn.QueryAsync<dynamic>(query);
                    dataList = data.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SelectBox tenant DB query failed for field {FieldName}, trying master DB", fieldName);
                }

                // Fallback to master DB (CountryStateMaster etc. are in master DB)
                if (dataList.Count == 0)
                {
                    try
                    {
                        using var masterConn = _connectionFactory.CreateMasterConnection();
                        var data = await masterConn.QueryAsync<dynamic>(query);
                        dataList = data.ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SelectBox master DB query failed for field {FieldName}, query: {Query}", fieldName, query);
                    }
                }

                resultList.AddRange(dataList);
            }

            // 2. If no query results, use SelectBoxDefault (comma-separated values)
            if (resultList.Count == 0 && !string.IsNullOrEmpty(defaults))
            {
                foreach (var val in defaults.Split(','))
                {
                    var trimmed = val.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    var item = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                    item[fieldName] = trimmed;
                    resultList.Add(item);
                }
            }

            if (resultList.Count > 0)
                dataset.Add($"tbl_{fieldName}", resultList);
        }

        return dataset;
    }

    // ==================== Ledger Operations ====================

    public async Task<string> ConvertLedgerToConsigneeAsync(int ledgerID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var fYear = _currentUserService.GetFYear() ?? "";

        try
        {
            await connection.OpenAsync();

            // Get consignee group details
            var ledgerGroupSql = @"
                SELECT NULLIF(LedgerGroupPrefix,'') As Prefix, LedgerGroupID
                FROM LedgerGroupMaster
                WHERE ISNULL(IsDeletedTransaction, 0) = 0
                AND LedgerGroupID = (
                    SELECT LedgerGroupID
                    FROM LedgerGroupMaster
                    WHERE LedgerGroupNameID = 44
                    AND ISNULL(IsDeletedTransaction, 0) = 0
                )";

            var ledgerGroup = await connection.QueryFirstOrDefaultAsync<(string Prefix, int LedgerGroupID)>(ledgerGroupSql);

            if (string.IsNullOrEmpty(ledgerGroup.Prefix))
            {
                return "No any consignee group found";
            }

            // Get clients that don't have consignees yet
            var clientsSql = @"
                SELECT LedgerID, LedgerName
                FROM LedgerMaster
                WHERE LedgerGroupID = (
                    SELECT LedgerGroupID
                    FROM LedgerGroupMaster
                    WHERE LedgerGroupNameID = 24
                )
                AND IsDeletedTransaction = 0
                AND LedgerName NOT IN (
                    SELECT DISTINCT LedgerName
                    FROM LedgerMaster
                    WHERE IsDeletedTransaction = 0
                    AND LedgerGroupID = @ConsigneeLedgerGroupID
                )";

            var clients = await connection.QueryAsync<(long LedgerID, string LedgerName)>(
                clientsSql,
                new { ConsigneeLedgerGroupID = ledgerGroup.LedgerGroupID });

            var clientList = clients.ToList();
            if (!clientList.Any())
            {
                return "Duplicate consignee found";
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var client in clientList)
                {
                    // Generate LedgerCode
                    var maxLedgerNoSql = @"
                        SELECT ISNULL(MAX(MaxLedgerNo), 0) + 1 AS MaxLedgerNo
                        FROM LedgerMaster
                        WHERE LedgerCodeprefix = @LedgerCodePrefix
                        AND ISNULL(IsDeletedTransaction, 0) = 0";

                    var maxLedgerNo = await connection.QueryFirstOrDefaultAsync<long>(
                        maxLedgerNoSql,
                        new { LedgerCodePrefix = ledgerGroup.Prefix },
                        transaction);

                    var ledgerCode = $"{ledgerGroup.Prefix}{maxLedgerNo}";

                    // Insert consignee ledger
                    var insertLedgerSql = @"
                        INSERT INTO LedgerMaster
                        (LedgerCode, MaxLedgerNo, LedgerCodePrefix, LedgerName, LedgerDescription, LedgerUnitID,
                         LedgerType, LedgerGroupID, ISLedgerActive, CompanyID, UserID, CreatedDate, IsBlocked,
                         FYear, IsLocked, CreatedBy)
                        SELECT @LedgerCode, @MaxLedgerNo, @LedgerCodePrefix, LedgerName,
                               REPLACE(LedgerDescription, 'Client:', 'Consignee:'), LedgerUnitID,
                               'Consignee', @LedgerGroupID, IsLedgerActive, CompanyID, @UserID, GETDATE(),
                               IsBlocked, FYear, IsLocked, @CreatedBy
                        FROM LedgerMaster
                        WHERE LedgerID = @SourceLedgerID
                        AND CompanyID = @CompanyID;
                        SELECT SCOPE_IDENTITY();";

                    var newLedgerID = await connection.ExecuteScalarAsync<long>(insertLedgerSql, new
                    {
                        LedgerCode = ledgerCode,
                        MaxLedgerNo = maxLedgerNo,
                        LedgerCodePrefix = ledgerGroup.Prefix,
                        LedgerGroupID = ledgerGroup.LedgerGroupID,
                        UserID = userId,
                        CreatedBy = userId,
                        SourceLedgerID = client.LedgerID,
                        CompanyID = companyId
                    }, transaction);

                    if (newLedgerID <= 0)
                    {
                        return $"Error creating consignee for {client.LedgerName}";
                    }

                    // Copy LedgerMasterDetails
                    var copyDetailsSql = @"
                        INSERT INTO LedgerMasterDetails
                        (ParentLedgerID, ParentFieldName, ParentFieldValue, LedgerID, FieldID, FieldName,
                         FieldValue, SequenceNo, LedgerGroupID, CompanyID, UserID, CreatedDate, FYear)
                        SELECT ParentLedgerID, ParentFieldName, ParentFieldValue, @NewLedgerID, FieldID,
                               FieldName, FieldValue, SequenceNo, @LedgerGroupID, @CompanyID, @UserID,
                               GETDATE(), @FYear
                        FROM LedgerMasterDetails
                        WHERE FieldName IN (
                            SELECT DISTINCT FieldName
                            FROM LedgerGroupFieldMaster
                            WHERE LedgerGroupID = @LedgerGroupID
                        )
                        AND LedgerID = @SourceLedgerID";

                    await connection.ExecuteAsync(copyDetailsSql, new
                    {
                        NewLedgerID = newLedgerID,
                        LedgerGroupID = ledgerGroup.LedgerGroupID,
                        CompanyID = companyId,
                        UserID = userId,
                        FYear = fYear,
                        SourceLedgerID = client.LedgerID
                    }, transaction);

                    // Insert RefClientID
                    var insertRefSql = @"
                        INSERT INTO LedgerMasterDetails
                        (CreatedDate, UserID, CompanyID, LedgerID, FYear, CreatedBy, ModifiedBy,
                         FieldValue, ParentFieldValue, ParentFieldName, FieldName, LedgerGroupID, SequenceNo)
                        VALUES
                        (GETDATE(), @UserID, @CompanyID, @NewLedgerID, @FYear, @CreatedBy, @ModifiedBy,
                         @RefClientID, @RefClientID, 'RefClientID', 'RefClientID', @LedgerGroupID, 20)";

                    await connection.ExecuteAsync(insertRefSql, new
                    {
                        UserID = userId,
                        CompanyID = companyId,
                        NewLedgerID = newLedgerID,
                        FYear = fYear,
                        CreatedBy = userId,
                        ModifiedBy = userId,
                        RefClientID = client.LedgerID,
                        LedgerGroupID = ledgerGroup.LedgerGroupID
                    }, transaction);

                    // Update calculated fields
                    await connection.ExecuteAsync(
                        "EXEC UpdateLedgerMasterValues @CompanyID, @LedgerID",
                        new { CompanyID = companyId, LedgerID = newLedgerID },
                        transaction);
                }

                await transaction.CommitAsync();
                return "Success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in transaction");
                return ex.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting ledger to consignee");
            return ex.Message;
        }
    }

    public async Task<object> GetLedgersByGroupAsync(string groupID)
    {
        using var connection = GetConnection();

        // Execute stored procedure GetFilteredLedgerMasterDataMultiUnit
        var sql = @"
            EXECUTE [GetFilteredLedgerMasterDataMultiUnit]
            '',
            @GroupID,
            ' And LedgerID In (Select Distinct LedgerID From LedgerMasterDetails Where FieldName=''ISLedgerActive'' And FieldValue=''True'')'";

        try
        {
            var result = await connection.QueryAsync<dynamic>(sql, new { GroupID = groupID });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing GetFilteredLedgerMasterDataMultiUnit for groupID {GroupID}", groupID);
            throw;
        }
    }

    // ==================== Concern Person Management ====================

    public async Task<List<ConcernPersonDto>> GetConcernPersonsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ConcernPersonID,
                NULLIF(LedgerID,'') as LedgerID,
                NULLIF(Name,'') as Name,
                NULLIF(Address1,'') as Address1,
                NULLIF(Address2,'') as Address2,
                NULLIF(Mobile,'') as Mobile,
                NULLIF(Email,'') as Email,
                NULLIF(Designation,'') as Designation,
                NULLIF(IsPrimaryConcernPerson,'') as IsPrimaryConcernPerson
            FROM ConcernPersonMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<ConcernPersonDto>(sql);
        return result.ToList();
    }

    public async Task<string> SaveConcernPersonAsync(SaveConcernPersonRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            var validColumns = await GetTableColumnsAsync(connection, transaction, "ConcernPersonMaster");

            // Insert new records
            foreach (var concernPerson in request.CostingDataSlab)
            {
                var columns = new List<string>
                {
                    "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                    "CreatedBy", "ModifiedBy", "LedgerID"
                };
                var values = new List<string>
                {
                    "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                    "@CreatedBy", "@ModifiedBy", "@LedgerID"
                };
                var parameters = new DynamicParameters();
                parameters.Add("@ModifiedDate", DateTime.Now);
                parameters.Add("@CreatedDate", DateTime.Now);
                parameters.Add("@UserID", userId);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@FYear", fYear);
                parameters.Add("@CreatedBy", userId);
                parameters.Add("@ModifiedBy", userId);
                parameters.Add("@LedgerID", request.LedgerID);

                int paramIndex = 0;
                foreach (var kvp in concernPerson)
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
                    INSERT INTO ConcernPersonMaster ({string.Join(", ", columns)})
                    VALUES ({string.Join(", ", values)})";

                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            // Update existing records
            foreach (var concernPerson in request.CostingDataSlabUpdate)
            {
                var setClause = new StringBuilder();
                setClause.Append("ModifiedDate = GETDATE(), ");
                setClause.Append($"UserID = {userId}, ");
                setClause.Append($"CompanyID = {companyId}, ");
                setClause.Append($"ModifiedBy = {userId}");

                var parameters = new DynamicParameters();
                int paramIndex = 0;
                foreach (var kvp in concernPerson)
                {
                    if (validColumns.Contains(kvp.Key) && kvp.Key != "ConcernPersonID")
                    {
                        setClause.Append($", {kvp.Key} = @param{paramIndex}");
                        parameters.Add($"@param{paramIndex}", ConvertJsonElement(kvp.Value));
                        paramIndex++;
                    }
                }

                parameters.Add("@LedgerID", request.LedgerID);
                parameters.Add("@ConcernPersonID", concernPerson["ConcernPersonID"]);

                var updateSql = $@"
                    UPDATE ConcernPersonMaster
                    SET {setClause}
                    WHERE LedgerID = @LedgerID
                    AND ConcernPersonID = @ConcernPersonID";

                await connection.ExecuteAsync(updateSql, parameters, transaction);
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving concern person");
            return "fail";
        }
    }

    public async Task<string> DeleteAllConcernPersonsAsync(string ledgerID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            var sql = @"
                UPDATE ConcernPersonMaster
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE LedgerID = @LedgerID";

            await connection.ExecuteAsync(sql, new { UserID = userId, LedgerID = ledgerID });
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all concern persons");
            return "fail";
        }
    }

    public async Task<string> DeleteConcernPersonAsync(string concernPersonID, string ledgerID)
    {
        using var connection = GetConnection();

        try
        {
            var sql = @"
                DELETE FROM ConcernPersonMaster
                WHERE ConcernPersonID = @ConcernPersonID
                AND LedgerID = @LedgerID";

            await connection.ExecuteAsync(sql, new { ConcernPersonID = concernPersonID, LedgerID = ledgerID });
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting concern person");
            return "Not match";
        }
    }

    // ==================== Employee Machine Allocation ====================

    public async Task<List<OperatorDto>> GetOperatorsAsync()
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT ISNULL(L.LedgerID, 0) As LedgerID,
                   NULLIF(L.LedgerName,'') As LedgerName
            FROM LedgerMaster As L
            INNER JOIN LedgerMasterDetails As LD
                ON L.LedgerID = LD.LedgerID
                AND L.CompanyID = LD.CompanyID
            INNER JOIN ProductionUnitMaster As PUM
                ON PUM.ProductionUnitID = L.ProductionUnitID
            WHERE LD.FieldName = 'Designation'
            AND UPPER(LD.FieldValue) = 'OPERATOR'
            AND ISNULL(L.IsDeletedTransaction, 0) <> 1
            AND L.ProductionUnitID = @ProductionUnitID";

        var result = await connection.QueryAsync<OperatorDto>(sql, new { ProductionUnitID = productionUnitId });
        return result.ToList();
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync(string groupID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT NULLIF(LedgerID,'') as LedgerID,
                   NULLIF(LedgerName,'') as LedgerName
            FROM LedgerMaster
            WHERE LedgerGroupID = @GroupID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryAsync<EmployeeDto>(sql, new { GroupID = groupID });
        return result.ToList();
    }

    public async Task<string> SaveMachineAllocationAsync(SaveMachineAllocationRequest request)
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

            // Delete existing allocation
            var deleteSql = @"
                DELETE FROM EmployeeMachineAllocation
                WHERE CompanyID = @CompanyID
                AND LedgerID = @EmployeeID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            await connection.ExecuteAsync(deleteSql, new
            {
                CompanyID = companyId,
                EmployeeID = request.EmployeeID
            }, transaction);

            if (!string.IsNullOrEmpty(request.GridRow))
            {
                // Get max ID
                var maxIdSql = @"
                    SELECT ISNULL(MAX(EmployeeMachineAllocationID), 0) + 1
                    FROM EmployeeMachineAllocation
                    WHERE CompanyID = @CompanyID
                    AND ISNULL(IsDeletedTransaction, 0) <> 1";

                var maxValue = await connection.ExecuteScalarAsync<long>(maxIdSql,
                    new { CompanyID = companyId },
                    transaction);

                var validColumns = await GetTableColumnsAsync(connection, transaction, "EmployeeMachineAllocation");

                foreach (var allocation in request.CostingDataMachinAllocation)
                {
                    var columns = new List<string>
                    {
                        "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                        "CreatedBy", "ModifiedBy", "MachineIDString", "EmployeeMachineAllocationID", "ProductionUnitID"
                    };
                    var values = new List<string>
                    {
                        "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                        "@CreatedBy", "@ModifiedBy", "@MachineIDString", "@EmployeeMachineAllocationID", "@ProductionUnitID"
                    };
                    var parameters = new DynamicParameters();
                    parameters.Add("@ModifiedDate", DateTime.Now);
                    parameters.Add("@CreatedDate", DateTime.Now);
                    parameters.Add("@UserID", userId);
                    parameters.Add("@CompanyID", companyId);
                    parameters.Add("@FYear", fYear);
                    parameters.Add("@CreatedBy", userId);
                    parameters.Add("@ModifiedBy", userId);
                    parameters.Add("@MachineIDString", request.GridRow);
                    parameters.Add("@EmployeeMachineAllocationID", maxValue);
                    parameters.Add("@ProductionUnitID", productionUnitId);

                    int paramIndex = 0;
                    foreach (var kvp in allocation)
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
                        INSERT INTO EmployeeMachineAllocation ({string.Join(", ", columns)})
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
            _logger.LogError(ex, "Error saving machine allocation");
            return "fail";
        }
    }

    public async Task<string> GetMachineAllocationAsync(string employeeID)
    {
        using var connection = GetConnection();
        var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

        var sql = @"
            SELECT TOP 1 NULLIF(MachineIDString,'') as MachineIDString
            FROM EmployeeMachineAllocation
            WHERE ProductionUnitID = @ProductionUnitID
            AND LedgerID = @EmployeeID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new
        {
            ProductionUnitID = productionUnitId,
            EmployeeID = employeeID
        });

        return result ?? "";
    }

    public async Task<string> DeleteMachineAllocationAsync(string ledgerID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;

        try
        {
            var sql = @"
                UPDATE EmployeeMachineAllocation
                SET ModifiedBy = @UserID,
                    DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE LedgerID = @LedgerID";

            await connection.ExecuteAsync(sql, new { UserID = userId, LedgerID = ledgerID });
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine allocation");
            return "Not match";
        }
    }

    // ==================== Supplier Group Allocation ====================

    public async Task<List<LedgerItemGroupDto>> GetItemGroupsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT ISNULL(ItemGroupID, 0) AS ItemGroupID,
                   NULLIF(ItemGroupName,'') AS ItemGroupName
            FROM ItemGroupMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY NULLIF(ItemGroupName,'')";

        var result = await connection.QueryAsync<LedgerItemGroupDto>(sql);
        return result.ToList();
    }

    public async Task<List<SpareGroupDto>> GetSpareGroupsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT SparePartGroup
            FROM SparePartMaster
            WHERE IsDeletedTransaction = 0
            ORDER BY SparePartGroup";

        var result = await connection.QueryAsync<SpareGroupDto>(sql);
        return result.ToList();
    }

    public async Task<string> GetGroupAllocationAsync(string supplierID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT TOP 1 NULLIF(GroupAllocationIDString,'') as GroupAllocationIDString
            FROM SupplierItemGroupAllocation
            WHERE LedgerID = @SupplierID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { SupplierID = supplierID });
        return result ?? "";
    }

    public async Task<string> GetSpareAllocationAsync(string supplierID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT STUFF((
                SELECT DISTINCT ',' + c.SparePartGroup
                FROM SupplierSpareGroupAllocation c
                WHERE c.LedgerID = @SupplierID
                AND c.IsDeletedTransaction = 0
                FOR XML PATH(''), TYPE
            ).value('.', 'nvarchar(max)'), 1, 1, '') As IDString";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { SupplierID = supplierID });
        return result ?? "";
    }

    public async Task<string> SaveGroupAllocationAsync(SaveGroupAllocationRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Delete existing allocations
            var deleteItemGroupSql = @"
                DELETE FROM SupplierItemGroupAllocation
                WHERE LedgerID = @SupplierID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            await connection.ExecuteAsync(deleteItemGroupSql, new { request.SupplierID }, transaction);

            var deleteSpareGroupSql = @"
                DELETE FROM SupplierSpareGroupAllocation
                WHERE LedgerID = @SupplierID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            await connection.ExecuteAsync(deleteSpareGroupSql, new { request.SupplierID }, transaction);

            // Insert item group allocation
            if (!string.IsNullOrEmpty(request.GridRow))
            {
                var maxIdSql = @"
                    SELECT ISNULL(MAX(SupplierItemGroupAllocationID), 0) + 1
                    FROM SupplierItemGroupAllocation
                    WHERE CompanyID = @CompanyID
                    AND IsDeletedTransaction = 0";

                var maxValue = await connection.ExecuteScalarAsync<long>(maxIdSql,
                    new { CompanyID = companyId },
                    transaction);

                var validColumns = await GetTableColumnsAsync(connection, transaction, "SupplierItemGroupAllocation");

                foreach (var allocation in request.CostingDataGroupAllocation)
                {
                    var columns = new List<string>
                    {
                        "ModifiedDate", "CreatedDate", "UserID", "CompanyID", "FYear",
                        "CreatedBy", "ModifiedBy", "GroupAllocationIDString", "SupplierItemGroupAllocationID"
                    };
                    var values = new List<string>
                    {
                        "@ModifiedDate", "@CreatedDate", "@UserID", "@CompanyID", "@FYear",
                        "@CreatedBy", "@ModifiedBy", "@GroupAllocationIDString", "@SupplierItemGroupAllocationID"
                    };
                    var parameters = new DynamicParameters();
                    parameters.Add("@ModifiedDate", DateTime.Now);
                    parameters.Add("@CreatedDate", DateTime.Now);
                    parameters.Add("@UserID", userId);
                    parameters.Add("@CompanyID", companyId);
                    parameters.Add("@FYear", fYear);
                    parameters.Add("@CreatedBy", userId);
                    parameters.Add("@ModifiedBy", userId);
                    parameters.Add("@GroupAllocationIDString", request.GridRow);
                    parameters.Add("@SupplierItemGroupAllocationID", maxValue);

                    int paramIndex = 0;
                    foreach (var kvp in allocation)
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
                        INSERT INTO SupplierItemGroupAllocation ({string.Join(", ", columns)})
                        VALUES ({string.Join(", ", values)})";

                    await connection.ExecuteAsync(insertSql, parameters, transaction);
                }
            }

            // Insert spare part allocation
            var maxSpareIdSql = @"
                SELECT ISNULL(MAX(SupplierSpareGroupAllocationID), 0) + 1
                FROM SupplierSpareGroupAllocation
                WHERE CompanyID = @CompanyID
                AND IsDeletedTransaction = 0";

            var maxSpareValue = await connection.ExecuteScalarAsync<long>(maxSpareIdSql,
                new { CompanyID = companyId },
                transaction);

            var spareValidColumns = await GetTableColumnsAsync(connection, transaction, "SupplierSpareGroupAllocation");

            foreach (var spareAllocation in request.ObjSparePartAllocation)
            {
                var columns = new List<string>
                {
                    "SupplierSpareGroupAllocationID", "ModifiedDate", "CreatedDate", "UserID",
                    "CompanyID", "FYear", "CreatedBy", "ModifiedBy"
                };
                var values = new List<string>
                {
                    "@SupplierSpareGroupAllocationID", "@ModifiedDate", "@CreatedDate", "@UserID",
                    "@CompanyID", "@FYear", "@CreatedBy", "@ModifiedBy"
                };
                var parameters = new DynamicParameters();
                parameters.Add("@SupplierSpareGroupAllocationID", maxSpareValue);
                parameters.Add("@ModifiedDate", DateTime.Now);
                parameters.Add("@CreatedDate", DateTime.Now);
                parameters.Add("@UserID", userId);
                parameters.Add("@CompanyID", companyId);
                parameters.Add("@FYear", fYear);
                parameters.Add("@CreatedBy", userId);
                parameters.Add("@ModifiedBy", userId);

                int paramIndex = 0;
                foreach (var kvp in spareAllocation)
                {
                    if (spareValidColumns.Contains(kvp.Key))
                    {
                        columns.Add(kvp.Key);
                        var paramName = $"@sparam{paramIndex}";
                        values.Add(paramName);
                        parameters.Add(paramName, ConvertJsonElement(kvp.Value));
                        paramIndex++;
                    }
                }

                var insertSql = $@"
                    INSERT INTO SupplierSpareGroupAllocation ({string.Join(", ", columns)})
                    VALUES ({string.Join(", ", values)})";

                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving group allocation");
            return "fail";
        }
    }

    public async Task<string> DeleteGroupAllocationAsync(string ledgerID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;

        try
        {
            var sql = @"
                UPDATE SupplierItemGroupAllocation
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE LedgerID = @LedgerID";

            await connection.ExecuteAsync(sql, new { UserID = userId, LedgerID = ledgerID });
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group allocation");
            return "Not match";
        }
    }

    // ==================== Business Vertical ====================

    public async Task<object> GetBusinessVerticalSettingsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql1 = @"
            SELECT ISNULL(LedgerID, 0) as LedgerID,
                   NULLIF(LedgerName,'') as LedgerName
            FROM LedgerMaster
            WHERE DepartmentID = -50
            AND CompanyID = @CompanyID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var sql2 = @"
            SELECT ISNULL(BusinessVerticalID, 0) as BusinessVerticalID,
                   NULLIF(BusinessVerticalName,'') as BusinessVerticalName
            FROM ClientBusinessVerticalMaster
            WHERE CompanyID = @CompanyID
            AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var salesPersonData = await connection.QueryAsync<dynamic>(sql1, new { CompanyID = companyId });
        var businessVerticalData = await connection.QueryAsync<dynamic>(sql2, new { CompanyID = companyId });

        return new
        {
            SalesPersondata = salesPersonData.ToList(),
            BusinessVerticalData = businessVerticalData.ToList()
        };
    }

    public async Task<object> GetBusinessVerticalDetailsAsync(string ledgerID, string verticalID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql1 = @"
            SELECT ISNULL(BVD.BusinessVerticalDetailID, 0) as BusinessVerticalDetailID,
                   ISNULL(BVD.LedgerID, 0) as LedgerID,
                   ISNULL(BVD.SalesPersonLedgerID, 0) as SalesPersonLedgerID,
                   ISNULL(BVD.BusinessVerticalID, 0) as BusinessVerticalID,
                   ISNULL(BVD.MaxCreditPeriod, 0) as MaxCreditPeriod,
                   ISNULL(BVD.MaxCreditLimit, 0) as MaxCreditLimit,
                   ISNULL(BVD.MaxFixedLimit, 0) as MaxFixedLimit,
                   ISNULL(BVD.TotalAvailableCredit, 0) as TotalAvailableCredit,
                   ISNULL(BVD.MaxOverdueDays, 0) as MaxOverdueDays,
                   ISNULL(BVD.TotalOverdueAmount, 0) as TotalOverdueAmount,
                   ISNULL(BVD.LastYearExposure, 0) as LastYearExposure,
                   NULLIF(BVD.Status,'') as Status,
                   NULLIF(BVD.EmbargoStatus,'') as EmbargoStatus,
                   NULLIF(LM.LedgerName,'') as LedgerName,
                   NULLIF(SP.LedgerName,'') as SalesPersonName,
                   NULLIF(CBVM.BusinessVerticalName,'') as BusinessVerticalName
            FROM BusinessVerticalDetails as BVD
            INNER JOIN LedgerMaster as LM
                ON LM.LedgerID = BVD.LedgerID
                AND ISNULL(LM.IsDeletedTransaction, 0) <> 1
            INNER JOIN ClientBusinessVerticalMaster as CBVM
                ON CBVM.BusinessVerticalID = BVD.BusinessVerticalID
                AND ISNULL(CBVM.IsDeletedTransaction, 0) <> 1
            INNER JOIN LedgerMaster as SP
                ON SP.LedgerID = BVD.SalesPersonLedgerID
                AND ISNULL(SP.IsDeletedTransaction, 0) <> 1
                AND SP.DepartmentID = -50
            WHERE BVD.CompanyID = @CompanyID
            AND BVD.LedgerID = @LedgerID
            AND ISNULL(BVD.IsDeletedTransaction, 0) <> 1";

        var sql2 = @"
            SELECT SUM(ISNULL(ID.BasicAmount, 0)) As TotalSale,
                   IM.LedgerID
            FROM InvoiceTRansactionDetail as ID
            INNER JOIN InvoiceTransactionMain as IM
                ON IM.InvoiceTransactionID = ID.InvoiceTransactionID
            INNER JOIN LedgerMaster as LM
                ON LM.LedgerID = IM.LedgerID
            INNER JOIN BusinessVerticalDetails as BVD
                ON BVD.LedgerID = LM.LedgerID
                AND ISNULL(BVD.IsDeletedTransaction, 0) = 0
                AND ISNULL(IM.IsDeletedTransaction, 0) = 0
                AND ISNULL(ID.IsDeletedTransaction, 0) = 0
            WHERE LM.LedgerID = @LedgerID
            AND BVD.BusinessVerticalID = @VerticalID
            AND IM.CompanyID = @CompanyID
            GROUP BY IM.LedgerID";

        var result1 = await connection.QueryAsync<dynamic>(sql1, new
        {
            CompanyID = companyId,
            LedgerID = ledgerID
        });

        var result2 = await connection.QueryAsync<dynamic>(sql2, new
        {
            CompanyID = companyId,
            LedgerID = ledgerID,
            VerticalID = verticalID
        });

        return new
        {
            Result1 = result1.ToList(),
            Result2 = result2.ToList()
        };
    }

    public async Task<string> SaveBusinessVerticalAsync(SaveBusinessVerticalRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            var validColumns = await GetTableColumnsAsync(connection, transaction, "BusinessVerticalDetails");

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
            foreach (var kvp in request.BusinessVerticalDetailsData[0])
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
                INSERT INTO BusinessVerticalDetails ({string.Join(", ", columns)})
                OUTPUT INSERTED.BusinessVerticalDetailID
                VALUES ({string.Join(", ", values)})";

            var businessVerticalDetailID = await connection.ExecuteScalarAsync<long>(insertSql, parameters, transaction);

            if (businessVerticalDetailID <= 0)
            {
                return "fail";
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving business vertical");
            return "fail";
        }
    }

    public async Task<string> UpdateBusinessVerticalAsync(UpdateBusinessVerticalRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            var validColumns = await GetTableColumnsAsync(connection, transaction, "BusinessVerticalDetails");

            var setClause = new StringBuilder();
            setClause.Append("ModifiedDate = GETDATE(), ");
            setClause.Append($"UserID = {userId}, ");
            setClause.Append($"CompanyID = {companyId}, ");
            setClause.Append($"ModifiedBy = {userId}");

            var parameters = new DynamicParameters();
            int paramIndex = 0;
            foreach (var kvp in request.BusinessVerticalDetailsData[0])
            {
                if (validColumns.Contains(kvp.Key))
                {
                    setClause.Append($", {kvp.Key} = @param{paramIndex}");
                    parameters.Add($"@param{paramIndex}", ConvertJsonElement(kvp.Value));
                    paramIndex++;
                }
            }

            parameters.Add("@CompanyID", companyId);
            parameters.Add("@BusinessVerticalDetailID", request.BusinessVerticalDetailID);

            var updateSql = $@"
                UPDATE BusinessVerticalDetails
                SET {setClause}
                WHERE CompanyID = @CompanyID
                AND BusinessVerticalDetailID = @BusinessVerticalDetailID";

            await connection.ExecuteAsync(updateSql, parameters, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating business vertical");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> DeleteBusinessVerticalAsync(string detailID)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            var sql = @"
                UPDATE BusinessVerticalDetails
                SET DeletedBy = @UserID,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE CompanyID = @CompanyID
                AND BusinessVerticalDetailID = @DetailID";

            await connection.ExecuteAsync(sql, new
            {
                UserID = userId,
                CompanyID = companyId,
                DetailID = detailID
            });

            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business vertical");
            return $"Error: {ex.Message}";
        }
    }

    // ==================== Embargo Management ====================

    public async Task<string> PlaceEmbargoAsync(PlaceEmbargoRequest request)
    {
        using var connection = GetConnection();
        var userId = _currentUserService.GetUserId() ?? 0;
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        try
        {
            var jsonMainData = JsonConvert.SerializeObject(request.ObjMainData);
            var jsonArrData = JsonConvert.SerializeObject(request.Arrdata);

            var sql = "EXEC ProcessLedgerEmbargo @CompanyID, @UserID, @JsonMainData, @JsonArrData";

            await connection.ExecuteAsync(sql, new
            {
                CompanyID = companyId,
                UserID = userId,
                JsonMainData = jsonMainData,
                JsonArrData = jsonArrData
            });

            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing embargo");
            return "fail";
        }
    }

    public async Task<object> GetEmbargoDetailsAsync(string ledgerID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT NULLIF(ED.EmbargoStatus,'') as EmbargoStatus,
                   NULLIF(ED.EmbargoReason,'') as EmbargoReason,
                   ED.Remark as Remark,
                   NULLIF(LM.LedgerName,'') as LedgerName,
                   NULLIF(SP.LedgerName,'') as SalesPersonName,
                   NULLIF(CBVM.BusinessVerticalName,'') as BusinessVerticalName,
                   NULLIF(UM.UserName,'') as UserName
            FROM EmbargoDetails as ED
            INNER JOIN LedgerMaster as LM
                ON LM.LedgerID = ED.LedgerID
                AND ISNULL(LM.IsDeletedTransaction, 0) <> 1
            INNER JOIN ClientBusinessVerticalMaster as CBVM
                ON CBVM.BusinessVerticalID = ED.BusinessVerticalID
                AND ISNULL(CBVM.IsDeletedTransaction, 0) <> 1
            INNER JOIN LedgerMaster as SP
                ON SP.LedgerID = ED.SalesPersonLedgerID
                AND ISNULL(SP.IsDeletedTransaction, 0) <> 1
                AND SP.DepartmentID = -50
            INNER JOIN UserMaster as UM
                ON UM.UserID = ED.UserID
                AND ISNULL(UM.IsDeletedUser, 0) <> 1
            WHERE ED.CompanyID = @CompanyID
            AND ED.LedgerID = @LedgerID
            AND ISNULL(ED.IsDeletedTransaction, 0) <> 1
            ORDER BY ED.EmbargoID DESC";

        var result = await connection.QueryAsync<dynamic>(sql, new
        {
            CompanyID = companyId,
            LedgerID = ledgerID
        });

        return result.ToList();
    }

    public async Task<object> GetActiveEmbargosAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT ED.LedgerID,
                   ED.EmbargoID,
                   NULLIF(ED.EmbargoReason, '') AS EmbargoReason,
                   REPLACE(CONVERT(NVARCHAR(30), ED.CreatedDate, 106), ' ', '-') AS EmbargoDate,
                   NULLIF(LM.LedgerName, '') AS LedgerName,
                   ISNULL(LM.MaxCreditLimit, 0) AS MaxCreditLimit,
                   ISNULL(LM.MaxCreditPeriod, 0) AS MaxCreditPeriod,
                   ISNULL(LM.FixedLimit, 0) AS FixedLimit,
                   NULLIF(UM.UserName, '') AS UserName,
                   ISNULL(BV.BusinessVerticalNames, '') AS BusinessVerticalNames
            FROM EmbargoDetails ED
            INNER JOIN LedgerMaster LM
                ON LM.LedgerID = ED.LedgerID
                AND LM.Status = 'InActive'
                AND ISNULL(LM.IsDeletedTransaction, 0) <> 1
            INNER JOIN UserMaster UM
                ON UM.UserID = ED.CreatedBy
                AND ISNULL(UM.isDeletedUser, 0) <> 1
            LEFT JOIN (
                SELECT BVD.LedgerID,
                       STRING_AGG(CBVD.BusinessVerticalName, ', ') AS BusinessVerticalNames
                FROM BusinessVerticalDetails BVD
                INNER JOIN ClientBusinessVerticalMaster CBVD
                    ON CBVD.BusinessVerticalID = BVD.BusinessVerticalID
                WHERE ISNULL(BVD.IsDeletedTransaction, 0) <> 1
                GROUP BY BVD.LedgerID
            ) AS BV
                ON BV.LedgerID = ED.LedgerID
            WHERE ED.CompanyID = @CompanyID
            AND ED.EmbargoID = (
                SELECT MAX(EmbargoID)
                FROM EmbargoDetails
                WHERE LedgerID = ED.LedgerID
            )";

        var result = await connection.QueryAsync<dynamic>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    public async Task<string> SaveEmbargoDetailsAsync(SaveEmbargoDetailsRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            var validColumns = await GetTableColumnsAsync(connection, transaction, "EmbargoDetails");

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
            foreach (var kvp in request.EmbargoDetailsData[0])
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
                INSERT INTO EmbargoDetails ({string.Join(", ", columns)})
                OUTPUT INSERTED.EmbargoID
                VALUES ({string.Join(", ", values)})";

            var embargoID = await connection.ExecuteScalarAsync<long>(insertSql, parameters, transaction);

            if (embargoID <= 0)
            {
                return "fail";
            }

            // Update LedgerMaster status
            var updateStatusSql = @"
                UPDATE LedgerMaster
                SET Status = @Status
                WHERE CompanyID = @CompanyID
                AND LedgerID = @LedgerID";

            await connection.ExecuteAsync(updateStatusSql, new
            {
                Status = request.txtStatus,
                CompanyID = companyId,
                request.LedgerID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving embargo details");
            return "fail";
        }
    }

    // ==================== Utilities ====================

    public async Task<int> GetSessionTimeoutAsync()
    {
        // Read from configuration
        var timeout = (int.TryParse(_configuration["Session:Timeout"], out var timeoutValue) ? timeoutValue : 20);
        return timeout * 1000 * 60; // Convert to milliseconds
    }

    public async Task<string> GetLedgerGroupNameIDAsync(string masterID)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT LedgerGroupNameID
            FROM LedgerGroupMaster
            WHERE LedgerGroupID = @MasterID";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { MasterID = masterID });
        return result ?? "";
    }

    public async Task<List<SupplierDto>> GetSuppliersAsync(string groupNameID)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT ISNULL(LedgerID, 0) AS LedgerID,
                   NULLIF(LedgerName,'') AS LedgerName
            FROM LedgerMaster
            WHERE CompanyID = @CompanyID
            AND ISNULL(IsDeletedTransaction, 0) <> 1
            AND LedgerGroupID IN (
                SELECT DISTINCT LedgerGroupID
                FROM LedgerGroupMaster
                WHERE LedgerGroupNameID = @GroupNameID
            )
            ORDER BY NULLIF(LedgerName,'')";

        var result = await connection.QueryAsync<SupplierDto>(sql, new
        {
            CompanyID = companyId,
            GroupNameID = groupNameID
        });

        return result.ToList();
    }

    // ==================== Helper Methods ====================

    private async Task<bool> CheckAuthorizationAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        long userId,
        long companyId,
        string ledgerGroupID,
        string permission)
    {
        var sql = $@"
            SELECT {permission}
            FROM UserSubModuleAuthentication
            WHERE UserID = @UserID
            AND LedgerGroupID = @LedgerGroupID
            AND CompanyID = @CompanyID";

        var result = await connection.QueryFirstOrDefaultAsync<bool?>(
            sql,
            new { UserID = userId, LedgerGroupID = ledgerGroupID, CompanyID = companyId },
            transaction);

        return result ?? false;
    }

    private async Task<bool> CheckDuplicateLedgerAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string ledgerGroupID,
        Dictionary<string, object>[] detailData,
        long companyId)
    {
        try
        {
            // Get SaveAsString from LedgerGroupMaster
            var saveAsStringSql = @"
                SELECT NULLIF(SaveAsString,'') as SaveAsString
                FROM LedgerGroupMaster
                WHERE LedgerGroupID = @LedgerGroupID
                AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var saveAsString = await connection.QueryFirstOrDefaultAsync<string>(
                saveAsStringSql,
                new { LedgerGroupID = ledgerGroupID },
                transaction);

            if (string.IsNullOrEmpty(saveAsString))
            {
                return false;
            }

            // Build filter based on SaveAsString fields
            var filterConditions = new List<string>();
            foreach (var row in detailData)
            {
                var fieldName = row["FieldName"]?.ToString();
                var fieldValue = row["FieldValue"]?.ToString();

                if (!string.IsNullOrEmpty(fieldName) && saveAsString.Contains(fieldName))
                {
                    if (filterConditions.Count == 0)
                    {
                        filterConditions.Add($"And ISNULL(IsDeletedTransaction,0)<>1 And LedgerGroupID In(Select Distinct LedgerGroupID From LedgerMasterDetails Where FieldName=''{fieldName}'' And FieldValue=''{fieldValue}'')");
                    }
                    else
                    {
                        filterConditions.Add($" And LedgerGroupID In(Select Distinct LedgerGroupID From LedgerMasterDetails Where FieldName=''{fieldName}'' And FieldValue=''{fieldValue}'')");
                    }
                }
            }

            if (filterConditions.Count == 0)
            {
                return false;
            }

            var colValue = string.Join("", filterConditions);

            // Execute GetFilteredLedgerMasterData
            var checkDuplicateSql = $"Exec GetFilteredLedgerMasterData 'LedgerMasterDetails',{companyId},{ledgerGroupID},'{colValue}'";

            var duplicates = await connection.QueryAsync<dynamic>(checkDuplicateSql, transaction: transaction);

            return duplicates.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking duplicate ledger");
            return true; // Return true to prevent save on error
        }
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

    private async Task<HashSet<string>> GetTableColumnsAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string tableName)
    {
        var sql = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName";

        var columns = await connection.QueryAsync<string>(sql, new { TableName = tableName }, transaction);
        return new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase);
    }
}
