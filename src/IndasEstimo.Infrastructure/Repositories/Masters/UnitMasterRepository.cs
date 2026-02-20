using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class UnitMasterRepository : IUnitMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UnitMasterRepository> _logger;

    public UnitMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<UnitMasterRepository> logger)
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
    /// Get all units for main grid.
    /// Old VB method: GetUnit()
    /// </summary>
    public async Task<List<UnitListDto>> GetUnitListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(UnitID, 0) AS UnitID,
                ISNULL(UnitName, '') AS UnitName,
                ISNULL(UnitSymbol, '') AS UnitSymbol,
                ISNULL(Type, '') AS Type,
                ISNULL(ConversionValue, 0) AS ConversionValue,
                ISNULL(DecimalPlace, 0) AS DecimalPlace
            FROM UnitMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY UnitID DESC";

        var result = await connection.QueryAsync<UnitListDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Save a new unit. Checks for duplicate UnitName first.
    /// Old VB method: SaveUnitData()
    /// </summary>
    public async Task<string> SaveUnitAsync(SaveUnitRequest request)
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

            // Check for duplicate UnitName
            var existSql = @"
                SELECT COUNT(1)
                FROM UnitMaster
                WHERE UnitName = @UnitName
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.UnitName },
                transaction);

            if (exists > 0)
                return "Exist";

            var insertSql = @"
                INSERT INTO UnitMaster
                    (UnitName, UnitSymbol, Type, ConversionValue, DecimalPlace,
                     CompanyID, FYear, UserID, CreatedBy, ModifiedBy, ProductionUnitID,
                     CreatedDate, ModifiedDate)
                VALUES
                    (@UnitName, @UnitSymbol, @Type, @ConversionValue, @DecimalPlace,
                     @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, @ProductionUnitID,
                     GETDATE(), GETDATE())";

            await connection.ExecuteAsync(insertSql, new
            {
                request.UnitName,
                request.UnitSymbol,
                request.Type,
                request.ConversionValue,
                request.DecimalPlace,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId,
                CreatedBy = userId,
                ModifiedBy = userId,
                ProductionUnitID = productionUnitId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving unit");
            return "fail";
        }
    }

    /// <summary>
    /// Update an existing unit.
    /// Old VB method: UpdatUnitData()
    /// </summary>
    public async Task<string> UpdateUnitAsync(UpdateUnitRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            var updateSql = @"
                UPDATE UnitMaster
                SET UnitName = @UnitName,
                    UnitSymbol = @UnitSymbol,
                    Type = @Type,
                    ConversionValue = @ConversionValue,
                    DecimalPlace = @DecimalPlace,
                    ModifiedBy = @ModifiedBy,
                    ProductionUnitID = @ProductionUnitID,
                    ModifiedDate = GETDATE()
                WHERE UnitID = @UnitID";

            await connection.ExecuteAsync(updateSql, new
            {
                request.UnitName,
                request.UnitSymbol,
                request.Type,
                request.ConversionValue,
                request.DecimalPlace,
                ModifiedBy = userId,
                ProductionUnitID = productionUnitId,
                request.UnitID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating unit {UnitID}", request.UnitID);
            return "fail";
        }
    }

    /// <summary>
    /// Soft-delete a unit (sets IsDeletedTransaction=1).
    /// Old VB method: DeleteUnitMasterData()
    /// </summary>
    public async Task<string> DeleteUnitAsync(long unitId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            var deleteSql = @"
                UPDATE UnitMaster
                SET IsDeletedTransaction = 1,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ProductionUnitID = @ProductionUnitID
                WHERE UnitID = @UnitID";

            await connection.ExecuteAsync(deleteSql, new
            {
                DeletedBy = userId,
                ProductionUnitID = productionUnitId,
                UnitID = unitId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting unit {UnitID}", unitId);
            return "fail";
        }
    }
}
