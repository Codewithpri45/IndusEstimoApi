using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ProductGroupMasterRepository : IProductGroupMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductGroupMasterRepository> _logger;

    public ProductGroupMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<ProductGroupMasterRepository> logger)
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
    /// Get all product HSN groups for main grid.
    /// Old VB method: Showlist()
    /// </summary>
    public async Task<List<ProductGroupListDto>> GetProductGroupListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(PHM.ProductHSNID, 0) AS ProductHSNID,
                ISNULL(PHM.ProductHSNName, '') AS ProductHSNName,
                ISNULL(PHM.HSNCode, '') AS HSNCode,
                ISNULL(PHM.UnderProductHSNID, 0) AS UnderProductHSNID,
                ISNULL(PHM.DisplayName, '') AS DisplayName,
                ISNULL(PHM.TariffNo, '') AS TariffNo,
                ISNULL(PHM.ProductCategory, '') AS ProductCategory,
                ISNULL(PHM.GSTTaxPercentage, 0) AS GSTTaxPercentage,
                ISNULL(PHM.CGSTTaxPercentage, 0) AS CGSTTaxPercentage,
                ISNULL(PHM.SGSTTaxPercentage, 0) AS SGSTTaxPercentage,
                ISNULL(PHM.IGSTTaxPercentage, 0) AS IGSTTaxPercentage,
                ISNULL(PHM.ExciseTaxPercentage, 0) AS ExciseTaxPercentage,
                ISNULL(PHM.ItemGroupID, 0) AS ItemGroupID,
                ISNULL(UM.UserName, '') AS CreatedBy,
                ISNULL(PHM.FYear, '') AS FYear,
                REPLACE(CONVERT(nvarchar(30), PHM.CreatedDate, 106), ' ', '-') AS CreatedDate,
                ISNULL(PHM.IsServiceHSN, 0) AS IsServiceHSN,
                ISNULL(PHM.IsExciseApplicable, 0) AS IsExciseApplicable
            FROM ProductHSNMaster AS PHM
            LEFT JOIN UserMaster AS UM ON PHM.CreatedBy = UM.UserID
            WHERE ISNULL(PHM.IsDeletedTransaction, 0) <> 1
            ORDER BY PHM.ProductHSNID DESC";

        var result = await connection.QueryAsync<ProductGroupListDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get HSN dropdown list for UnderGroup parent selection.
    /// Old VB method: UnderGroup()
    /// </summary>
    public async Task<List<ProductHSNDropdownDto>> GetHSNDropdownAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ISNULL(ProductHSNID, 0) AS ProductHSNID,
                NULLIF(ProductHSNName, '') AS ProductHSNName
            FROM ProductHSNMaster
            WHERE NULLIF(ProductHSNName, '') IS NOT NULL
              AND ProductHSNName IS NOT NULL
            ORDER BY ProductHSNName";

        var result = await connection.QueryAsync<ProductHSNDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Get item groups for dropdown.
    /// Old VB method: SelItemGroupName()
    /// </summary>
    public async Task<List<ItemGroupDropdownDto>> GetItemGroupsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(ItemGroupID, 0) AS ItemGroupID,
                ISNULL(ItemGroupName, '') AS ItemGroupName
            FROM ItemGroupMaster
            ORDER BY ItemGroupID";

        var result = await connection.QueryAsync<ItemGroupDropdownDto>(sql);
        return result.ToList();
    }

    /// <summary>
    /// Check company tax type (VAT applicable).
    /// Old VB method: CheckTaxType()
    /// </summary>
    public async Task<List<TaxTypeDto>> GetTaxTypeAsync()
    {
        using var connection = GetConnection();

        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT DISTINCT
                ISNULL(IsVatApplicable, 0) AS IsVatApplicable
            FROM CompanyMaster
            WHERE CompanyID = @CompanyID";

        var result = await connection.QueryAsync<TaxTypeDto>(sql, new { CompanyID = companyId });
        return result.ToList();
    }

    /// <summary>
    /// Check if HSN is in use before delete.
    /// Old VB method: CheckPermission(ProductHSNID)
    /// </summary>
    public async Task<string> CheckPermissionAsync(int productHSNId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT A.ProductHSNID
            FROM (
                SELECT ProductHSNID FROM ItemMaster WHERE ProductHSNID = @ProductHSNID
                UNION ALL
                SELECT ProductHSNID FROM ToolMaster WHERE ProductHSNID = @ProductHSNID
                UNION ALL
                SELECT ProductHSNID FROM ItemTransactionDetail WHERE ProductHSNID = @ProductHSNID
                UNION ALL
                SELECT ProductHSNID FROM JobBooking WHERE ProductHSNID = @ProductHSNID
                UNION ALL
                SELECT ProductHSNID FROM JobOrderBookingDetails WHERE ProductHSNID = @ProductHSNID
                UNION ALL
                SELECT ProductHSNID FROM ProductMaster WHERE ProductHSNID = @ProductHSNID
            ) AS A";

        var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { ProductHSNID = productHSNId });
        return result.HasValue ? "Exist" : "";
    }

    /// <summary>
    /// Save new product HSN group.
    /// Old VB method: SavePGHMData()
    /// </summary>
    public async Task<string> SaveProductGroupAsync(SaveProductGroupRequest request)
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

            // Check for duplicate DisplayName
            var existSql = @"
                SELECT COUNT(1)
                FROM ProductHSNMaster
                WHERE DisplayName = @DisplayName
                  AND ISNULL(IsDeletedTransaction, 0) <> 1";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.DisplayName },
                transaction);

            if (exists > 0)
                return "Exist";

            var insertSql = @"
                INSERT INTO ProductHSNMaster
                    (ProductHSNName, HSNCode, UnderProductHSNID, DisplayName, TariffNo, ProductCategory,
                     GSTTaxPercentage, CGSTTaxPercentage, SGSTTaxPercentage, IGSTTaxPercentage,
                     ExciseTaxPercentage, ItemGroupID, IsServiceHSN, IsExciseApplicable,
                     CompanyID, FYear, UserID, CreatedBy, ModifiedBy, ProductionUnitID,
                     CreatedDate, ModifiedDate)
                VALUES
                    (@ProductHSNName, @HSNCode, @UnderProductHSNID, @DisplayName, @TariffNo, @ProductCategory,
                     @GSTTaxPercentage, @CGSTTaxPercentage, @SGSTTaxPercentage, @IGSTTaxPercentage,
                     @ExciseTaxPercentage, @ItemGroupID, @IsServiceHSN, @IsExciseApplicable,
                     @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, @ProductionUnitID,
                     GETDATE(), GETDATE())";

            await connection.ExecuteAsync(insertSql, new
            {
                request.ProductHSNName,
                request.HSNCode,
                request.UnderProductHSNID,
                request.DisplayName,
                request.TariffNo,
                request.ProductCategory,
                request.GSTTaxPercentage,
                request.CGSTTaxPercentage,
                request.SGSTTaxPercentage,
                request.IGSTTaxPercentage,
                request.ExciseTaxPercentage,
                request.ItemGroupID,
                request.IsServiceHSN,
                request.IsExciseApplicable,
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
            _logger.LogError(ex, "Error saving product group");
            return "fail";
        }
    }

    /// <summary>
    /// Update existing product HSN group.
    /// Old VB method: UpdatePGHM()
    /// </summary>
    public async Task<string> UpdateProductGroupAsync(UpdateProductGroupRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var productionUnitId = _currentUserService.GetProductionUnitId() ?? 0;

            var updateSql = @"
                UPDATE ProductHSNMaster
                SET ProductHSNName = @ProductHSNName,
                    HSNCode = @HSNCode,
                    UnderProductHSNID = @UnderProductHSNID,
                    DisplayName = @DisplayName,
                    TariffNo = @TariffNo,
                    ProductCategory = @ProductCategory,
                    GSTTaxPercentage = @GSTTaxPercentage,
                    CGSTTaxPercentage = @CGSTTaxPercentage,
                    SGSTTaxPercentage = @SGSTTaxPercentage,
                    IGSTTaxPercentage = @IGSTTaxPercentage,
                    ExciseTaxPercentage = @ExciseTaxPercentage,
                    ItemGroupID = @ItemGroupID,
                    IsServiceHSN = @IsServiceHSN,
                    IsExciseApplicable = @IsExciseApplicable,
                    UserID = @UserID,
                    ModifiedBy = @ModifiedBy,
                    ProductionUnitID = @ProductionUnitID,
                    ModifiedDate = GETDATE()
                WHERE ProductHSNID = @ProductHSNID";

            await connection.ExecuteAsync(updateSql, new
            {
                request.ProductHSNName,
                request.HSNCode,
                request.UnderProductHSNID,
                request.DisplayName,
                request.TariffNo,
                request.ProductCategory,
                request.GSTTaxPercentage,
                request.CGSTTaxPercentage,
                request.SGSTTaxPercentage,
                request.IGSTTaxPercentage,
                request.ExciseTaxPercentage,
                request.ItemGroupID,
                request.IsServiceHSN,
                request.IsExciseApplicable,
                UserID = userId,
                ModifiedBy = userId,
                ProductionUnitID = productionUnitId,
                request.ProductHSNID
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating product group {ProductHSNID}", request.ProductHSNID);
            return "fail";
        }
    }

    /// <summary>
    /// Soft-delete a product HSN group.
    /// Old VB method: DeletePGHM()
    /// </summary>
    public async Task<string> DeleteProductGroupAsync(int productHSNId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var deleteSql = @"
                UPDATE ProductHSNMaster
                SET IsDeletedTransaction = 1,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE()
                WHERE ProductHSNID = @ProductHSNID";

            await connection.ExecuteAsync(deleteSql, new
            {
                DeletedBy = userId,
                ProductHSNID = productHSNId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting product group {ProductHSNID}", productHSNId);
            return "fail";
        }
    }
}
