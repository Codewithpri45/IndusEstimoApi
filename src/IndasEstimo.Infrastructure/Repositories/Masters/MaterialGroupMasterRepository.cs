using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class MaterialGroupMasterRepository : IMaterialGroupMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MaterialGroupMasterRepository> _logger;

    public MaterialGroupMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<MaterialGroupMasterRepository> logger)
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

    /// <summary>
    /// Get all material groups for the main grid.
    /// Old VB method: GetGroup()
    /// </summary>
    public async Task<List<MaterialGroupListDto>> GetGroupListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(ISG.ItemSubGroupUniqueID, 0)    AS ItemSubGroupUniqueID,
                ISNULL(ISG.ItemSubGroupID, 0)          AS ItemSubGroupID,
                ISNULL(ISG.ItemSubGroupName, '')       AS ItemSubGroupName,
                ISNULL(ISG.ItemSubGroupDisplayName,'') AS ItemSubGroupDisplayName,
                ISNULL(ISG.UnderSubGroupID, 0)         AS UnderSubGroupID,
                ISNULL(ISG.ItemSubGroupLevel, 0)       AS ItemSubGroupLevel,
                ISNULL(PG.ItemSubGroupDisplayName,'')  AS GroupName
            FROM ItemSubGroupMaster AS ISG
            LEFT JOIN ItemSubGroupMaster AS PG
                ON PG.ItemSubGroupID = ISG.UnderSubGroupID
                AND PG.CompanyID     = ISG.CompanyID
            WHERE ISNULL(ISG.IsDeletedTransaction, 0) <> 1
            ORDER BY ISG.ItemSubGroupID DESC";

        var result = await connection.QueryAsync<MaterialGroupListDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get top-level groups for the Under Group dropdown.
    /// Old VB method: GetUnderGroup()
    /// </summary>
    public async Task<List<UnderGroupDropdownDto>> GetUnderGroupAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(ItemSubGroupID, 0)          AS ItemSubGroupID,
                ISNULL(ItemSubGroupDisplayName,'') AS ItemSubGroupDisplayName
            FROM ItemSubGroupMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY ItemSubGroupDisplayName";

        var result = await connection.QueryAsync<UnderGroupDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Save a new material group. Checks for duplicate ItemSubGroupName first.
    /// Old VB method: SaveGroupData()
    /// </summary>
    public async Task<string> SaveGroupAsync(SaveMaterialGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId           = _currentUserService.GetUserId()          ?? 0;
            var companyId        = _currentUserService.GetCompanyId()       ?? 0;
            var fYear            = _currentUserService.GetFYear()           ?? "";
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            // Check for duplicate
            var existSql = @"
                SELECT COUNT(1)
                FROM ItemSubGroupMaster
                WHERE ItemSubGroupName = @ItemSubGroupName
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.ItemSubGroupName },
                transaction);

            if (exists > 0)
                return "Exist";

            // Determine next ItemSubGroupID within company
            var maxIdSql = @"
                SELECT ISNULL(MAX(ItemSubGroupID), 0) + 1
                FROM ItemSubGroupMaster
                WHERE CompanyID = @CompanyID";

            var nextId = await connection.QueryFirstOrDefaultAsync<long>(
                maxIdSql,
                new { CompanyID = companyId },
                transaction);

            var insertSql = @"
                INSERT INTO ItemSubGroupMaster
                    (ItemSubGroupID, ItemSubGroupName, ItemSubGroupDisplayName,
                     UnderSubGroupID, CompanyID, FYear, UserID, CreatedBy, ModifiedBy,
                     ProductionUnitID, CreatedDate, ModifiedDate, IsDeletedTransaction)
                VALUES
                    (@ItemSubGroupID, @ItemSubGroupName, @ItemSubGroupDisplayName,
                     @UnderSubGroupID, @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy,
                     @ProductionUnitID, GETDATE(), GETDATE(), 0)";

            await connection.ExecuteAsync(insertSql, new
            {
                ItemSubGroupID          = nextId,
                request.ItemSubGroupName,
                request.ItemSubGroupDisplayName,
                request.UnderSubGroupID,
                CompanyID        = companyId,
                FYear            = fYear,
                UserID           = userId,
                CreatedBy        = userId,
                ModifiedBy       = userId,
                ProductionUnitID = productionUnitId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving material group");
            return "fail";
        }
    }

    /// <summary>
    /// Update an existing material group.
    /// Old VB method: UpdatGroupData()
    /// </summary>
    public async Task<string> UpdateGroupAsync(UpdateMaterialGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId           = _currentUserService.GetUserId()          ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            var updateSql = @"
                UPDATE ItemSubGroupMaster
                SET ItemSubGroupName        = @ItemSubGroupName,
                    ItemSubGroupDisplayName = @ItemSubGroupDisplayName,
                    UnderSubGroupID         = @UnderSubGroupID,
                    ModifiedBy              = @ModifiedBy,
                    ProductionUnitID        = @ProductionUnitID,
                    ModifiedDate            = GETDATE()
                WHERE ItemSubGroupUniqueID = @ItemSubGroupUniqueID";

            await connection.ExecuteAsync(updateSql, new
            {
                request.ItemSubGroupName,
                request.ItemSubGroupDisplayName,
                request.UnderSubGroupID,
                ModifiedBy              = userId,
                ProductionUnitID        = productionUnitId,
                request.ItemSubGroupUniqueID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating material group {ID}", request.ItemSubGroupUniqueID);
            return "fail";
        }
    }

    /// <summary>
    /// Soft-delete a material group (sets IsDeletedTransaction=1).
    /// Old VB method: DeleteGroupMasterData()
    /// </summary>
    public async Task<string> DeleteGroupAsync(long itemSubGroupUniqueId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId           = _currentUserService.GetUserId()          ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            var deleteSql = @"
                UPDATE ItemSubGroupMaster
                SET IsDeletedTransaction = 1,
                    DeletedBy            = @DeletedBy,
                    DeletedDate          = GETDATE(),
                    ProductionUnitID     = @ProductionUnitID
                WHERE ItemSubGroupUniqueID = @ItemSubGroupUniqueID";

            await connection.ExecuteAsync(deleteSql, new
            {
                DeletedBy            = userId,
                ProductionUnitID     = productionUnitId,
                ItemSubGroupUniqueID = itemSubGroupUniqueId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting material group {ID}", itemSubGroupUniqueId);
            return "fail";
        }
    }
}
