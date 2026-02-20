using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ProductionUnitMasterRepository : IProductionUnitMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductionUnitMasterRepository> _logger;

    public ProductionUnitMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<ProductionUnitMasterRepository> logger)
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
    /// Get all production units for the main grid.
    /// Old VB method: GetProductionUnitMasterShowList()
    /// </summary>
    public async Task<List<ProductionUnitListDto>> GetProductionUnitListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(PU.ProductionUnitID, 0)          AS ProductionUnitID,
                ISNULL(PU.ProductionUnitCode, '')        AS ProductionUnitCode,
                ISNULL(PU.RefProductionUnitCode, '')     AS RefProductionUnitCode,
                ISNULL(PU.ProductionUnitName, '')        AS ProductionUnitName,
                ISNULL(CM.CompanyName, '')               AS CompanyName,
                ISNULL(PU.CompanyID, 0)                  AS CompanyID,
                ISNULL(PU.GSTNo, '')                     AS GSTNo,
                ISNULL(BM.BranchName, '')                AS BranchName,
                ISNULL(PU.BranchID, 0)                   AS BranchID,
                ISNULL(PU.Address, '')                   AS Address,
                ISNULL(PU.City, '')                      AS City,
                ISNULL(PU.State, '')                     AS State,
                ISNULL(PU.Pincode, '')                   AS Pincode,
                ISNULL(PU.Country, '')                   AS Country
            FROM ProductionUnitMaster AS PU
            LEFT JOIN CompanyMaster AS CM
                ON CM.CompanyID = PU.CompanyID
            LEFT JOIN BranchMaster AS BM
                ON BM.BranchID = PU.BranchID
            WHERE ISNULL(PU.IsDeletedTransaction, 0) <> 1
            ORDER BY PU.ProductionUnitID DESC";

        var result = await connection.QueryAsync<ProductionUnitListDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Auto-generate the next production unit code.
    /// Old VB method: GetProductionUnitNo()
    /// </summary>
    public async Task<string> GetProductionUnitNoAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT ISNULL(MAX(MaxProductionUnitNo), 0) + 1
            FROM ProductionUnitMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) <> 1";

        var nextNo = await connection.QueryFirstOrDefaultAsync<long>(sql, new { CompanyID = companyId });
        return nextNo.ToString();
    }

    /// <summary>
    /// Get country list for dropdown.
    /// Old VB method: GetCountry()
    /// </summary>
    public async Task<List<CountryDropdownDto>> GetCountryAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT ISNULL(Country, '') AS Country
            FROM CountryStateMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
              AND NULLIF(Country, '') IS NOT NULL
            ORDER BY Country";

        var result = await connection.QueryAsync<CountryDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get state list for dropdown.
    /// Old VB method: GetState()
    /// </summary>
    public async Task<List<StateDropdownDto>> GetStateAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT ISNULL(State, '') AS State
            FROM CountryStateMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
              AND NULLIF(State, '') IS NOT NULL
            ORDER BY State";

        var result = await connection.QueryAsync<StateDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get city list for dropdown.
    /// Old VB method: GetCity()
    /// </summary>
    public async Task<List<CityDropdownDto>> GetCityAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT ISNULL(City, '') AS City
            FROM CountryStateMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
              AND NULLIF(City, '') IS NOT NULL
            ORDER BY City";

        var result = await connection.QueryAsync<CityDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get company list for dropdown.
    /// Old VB method: GetCompanyName()
    /// </summary>
    public async Task<List<CompanyDropdownDto>> GetCompanyNameAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(CompanyID, 0)    AS CompanyID,
                ISNULL(CompanyName, '') AS CompanyName
            FROM CompanyMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY CompanyName";

        var result = await connection.QueryAsync<CompanyDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get branch list for dropdown.
    /// Old VB method: GetBranch()
    /// </summary>
    public async Task<List<BranchDropdownDto>> GetBranchAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(BranchID, 0)    AS BranchID,
                ISNULL(BranchName, '') AS BranchName
            FROM BranchMaster
            WHERE ISNULL(IsDeletedTransaction, 0) <> 1
            ORDER BY BranchName";

        var result = await connection.QueryAsync<BranchDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Save a new production unit.
    /// Old VB method: SaveProductionUnitMasterData()
    /// </summary>
    public async Task<string> SaveProductionUnitAsync(SaveProductionUnitRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId    = _currentUserService.GetUserId()    ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            // Generate next ProductionUnitID and MaxProductionUnitNo
            var maxIdSql = @"
                SELECT ISNULL(MAX(MaxProductionUnitNo), 0) + 1
                FROM ProductionUnitMaster
                WHERE CompanyID = @CompanyID
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var nextNo = await connection.QueryFirstOrDefaultAsync<long>(
                maxIdSql, new { CompanyID = companyId }, transaction);

            var unitCode = nextNo.ToString();

            var insertSql = @"
                INSERT INTO ProductionUnitMaster
                    (ProductionUnitID, ProductionUnitCode, RefProductionUnitCode,
                     ProductionUnitName, CompanyID, BranchID, GSTNo,
                     Address, City, State, Pincode, Country,
                     MaxProductionUnitNo, UserID, CreatedBy, ModifiedBy,
                     CreatedDate, ModifiedDate, IsDeletedTransaction)
                VALUES
                    (@ProductionUnitID, @ProductionUnitCode, @RefProductionUnitCode,
                     @ProductionUnitName, @CompanyID, @BranchID, @GSTNo,
                     @Address, @City, @State, @Pincode, @Country,
                     @MaxProductionUnitNo, @UserID, @CreatedBy, @ModifiedBy,
                     GETDATE(), GETDATE(), 0)";

            await connection.ExecuteAsync(insertSql, new
            {
                ProductionUnitID      = nextNo,
                ProductionUnitCode    = unitCode,
                request.RefProductionUnitCode,
                request.ProductionUnitName,
                CompanyID             = request.CompanyID > 0 ? request.CompanyID : companyId,
                request.BranchID,
                request.GSTNo,
                request.Address,
                request.City,
                request.State,
                request.Pincode,
                request.Country,
                MaxProductionUnitNo   = nextNo,
                UserID                = userId,
                CreatedBy             = userId,
                ModifiedBy            = userId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving production unit");
            return "fail";
        }
    }

    /// <summary>
    /// Update an existing production unit.
    /// Old VB method: UpdateProductionUnitMasterData()
    /// </summary>
    public async Task<string> UpdateProductionUnitAsync(UpdateProductionUnitRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var updateSql = @"
                UPDATE ProductionUnitMaster
                SET ProductionUnitName    = @ProductionUnitName,
                    RefProductionUnitCode = @RefProductionUnitCode,
                    CompanyID             = @CompanyID,
                    BranchID              = @BranchID,
                    GSTNo                 = @GSTNo,
                    Address               = @Address,
                    City                  = @City,
                    State                 = @State,
                    Pincode               = @Pincode,
                    Country               = @Country,
                    ModifiedBy            = @ModifiedBy,
                    ModifiedDate          = GETDATE()
                WHERE ProductionUnitID = @ProductionUnitID";

            await connection.ExecuteAsync(updateSql, new
            {
                request.ProductionUnitName,
                request.RefProductionUnitCode,
                request.CompanyID,
                request.BranchID,
                request.GSTNo,
                request.Address,
                request.City,
                request.State,
                request.Pincode,
                request.Country,
                ModifiedBy          = userId,
                request.ProductionUnitID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating production unit {ID}", request.ProductionUnitID);
            return "fail";
        }
    }

    /// <summary>
    /// Soft-delete a production unit. Returns 'Exist' if used in transactions.
    /// Old VB method: DeleteProductionUnitMasterData()
    /// </summary>
    public async Task<string> DeleteProductionUnitAsync(long productionUnitId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            // Check if used in any transactions
            var usedSql = @"
                SELECT COUNT(1)
                FROM ItemTransactionMain
                WHERE ProductionUnitID = @ProductionUnitID
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var usedCount = await connection.QueryFirstOrDefaultAsync<int>(
                usedSql, new { ProductionUnitID = productionUnitId }, transaction);

            if (usedCount > 0)
                return "Exist";

            var deleteSql = @"
                UPDATE ProductionUnitMaster
                SET IsDeletedTransaction = 1,
                    DeletedBy            = @DeletedBy,
                    DeletedDate          = GETDATE()
                WHERE ProductionUnitID = @ProductionUnitID";

            await connection.ExecuteAsync(deleteSql, new
            {
                DeletedBy       = userId,
                ProductionUnitID = productionUnitId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting production unit {ID}", productionUnitId);
            return "fail";
        }
    }
}
