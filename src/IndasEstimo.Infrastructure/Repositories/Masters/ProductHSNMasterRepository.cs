
using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ProductHSNMasterRepository : IProductHSNMasterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductHSNMasterRepository> _logger;

    public ProductHSNMasterRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<ProductHSNMasterRepository> logger)
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

    public async Task<IEnumerable<ProductHSNDetailDto>> GetProductHSNsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        var sql = @"
            SELECT PHM.ProductHSNID, PHM.ProductHSNName, PHM.HSNCode, PHM.UnderProductHSNID, PHM.DisplayName, 
                   PHM.TariffNo, PHM.ProductCategory, PHM.GSTTaxPercentage, PHM.IGSTTaxPercentage AS VATTaxPercentage, 
                   PHM.ExciseTaxPercentage, PHM.CGSTTaxPercentage, PHM.SGSTTaxPercentage, PHM.IGSTTaxPercentage, 
                   PHM.ItemGroupID, PHM.IsServiceHSN, PHM.IsExciseApplicable,
                   ISNULL(UM.UserName, '') AS CreatedBy, PHM.CreatedDate
            FROM ProductHSNMaster PHM
            LEFT JOIN UserMaster UM ON UM.UserID = PHM.CreatedBy
            WHERE PHM.CompanyID = @CompanyID AND ISNULL(PHM.IsDeletedTransaction, 0) = 0
            ORDER BY PHM.ProductHSNID DESC";

        return await connection.QueryAsync<ProductHSNDetailDto>(sql, new { CompanyID = companyId });
    }

    public async Task<long> CreateProductHSNAsync(CreateProductHSNDto hsn)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;
        var fYear = _currentUserService.GetFYear();

        var sql = @"
            INSERT INTO ProductHSNMaster (
                ProductHSNName, HSNCode, UnderProductHSNID, DisplayName, TariffNo, ProductCategory,
                GSTTaxPercentage, IGSTTaxPercentage, ExciseTaxPercentage, CGSTTaxPercentage, SGSTTaxPercentage,
                ItemGroupID, IsServiceHSN, IsExciseApplicable,
                CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction, ProductionUnitID
            ) VALUES (
                @ProductHSNName, @HSNCode, @UnderProductHSNID, @DisplayName, @TariffNo, @ProductCategory,
                @GSTTaxPercentage, @VATTaxPercentage, @ExciseTaxPercentage, @CGSTTaxPercentage, @SGSTTaxPercentage,
                @ItemGroupID, @IsServiceHSN, @IsExciseApplicable,
                @CompanyID, @CreatedBy, GETDATE(), @FYear, 0, @ProductionUnitID
            );
            SELECT CAST(SCOPE_IDENTITY() as bigint);";

        // Note: Using VATTaxPercentage mapped to IGSTTaxPercentage based on legacy confusion, assuming legacy code meant VATTaxPercentage/IGSTTaxPercentage mapping
        // The legacy query mapped IGSTTaxPercentage AS VATTaxPercentage. And also had IGSTTaxPercentage column.
        // We will just map DTO properties to DB columns directly.
        // Legacy insert didn't specify column list explicitly in 'InsertDatatableToDatabaseWithouttrans', it relied on DataTable structure. 
        // We assume standard mapping.

        return await connection.ExecuteScalarAsync<long>(sql, new
        {
            hsn.ProductHSNName, hsn.HSNCode, hsn.UnderProductHSNID, hsn.DisplayName, hsn.TariffNo, hsn.ProductCategory,
            hsn.GSTTaxPercentage, hsn.VATTaxPercentage, hsn.ExciseTaxPercentage, hsn.CGSTTaxPercentage, hsn.SGSTTaxPercentage,
            hsn.ItemGroupID, hsn.IsServiceHSN, hsn.IsExciseApplicable,
            CompanyID = companyId, CreatedBy = userId, FYear = fYear, hsn.ProductionUnitID
        });
    }

    public async Task<bool> UpdateProductHSNAsync(UpdateProductHSNDto hsn)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = @"
            UPDATE ProductHSNMaster SET 
                ProductHSNName = @ProductHSNName, HSNCode = @HSNCode, UnderProductHSNID = @UnderProductHSNID, 
                DisplayName = @DisplayName, TariffNo = @TariffNo, ProductCategory = @ProductCategory,
                GSTTaxPercentage = @GSTTaxPercentage, IGSTTaxPercentage = @VATTaxPercentage, 
                ExciseTaxPercentage = @ExciseTaxPercentage, CGSTTaxPercentage = @CGSTTaxPercentage, 
                SGSTTaxPercentage = @SGSTTaxPercentage, ItemGroupID = @ItemGroupID, 
                IsServiceHSN = @IsServiceHSN, IsExciseApplicable = @IsExciseApplicable,
                ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE(), ProductionUnitID = @ProductionUnitID
            WHERE ProductHSNID = @ProductHSNID AND CompanyID = @CompanyID";

        var rows = await connection.ExecuteAsync(sql, new
        {
            hsn.ProductHSNName, hsn.HSNCode, hsn.UnderProductHSNID, hsn.DisplayName, hsn.TariffNo, hsn.ProductCategory,
            hsn.GSTTaxPercentage, hsn.VATTaxPercentage, hsn.ExciseTaxPercentage, hsn.CGSTTaxPercentage, hsn.SGSTTaxPercentage,
            hsn.ItemGroupID, hsn.IsServiceHSN, hsn.IsExciseApplicable,
            ModifiedBy = userId, hsn.ProductionUnitID, hsn.ProductHSNID, CompanyID = companyId
        });
        return rows > 0;
    }

    public async Task<bool> DeleteProductHSNAsync(long id)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = @"
            UPDATE ProductHSNMaster 
            SET IsDeletedTransaction = 1, DeletedBy = @DeletedBy, DeletedDate = GETDATE() 
            WHERE ProductHSNID = @ProductHSNID AND CompanyID = @CompanyID";

        var rows = await connection.ExecuteAsync(sql, new { ProductHSNID = id, DeletedBy = userId, CompanyID = companyId });
        return rows > 0;
    }

    public async Task<ProductHSNDetailDto> GetProductHSNByIdAsync(long id)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"SELECT * FROM ProductHSNMaster WHERE ProductHSNID = @ID AND CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction, 0) = 0";
        return await connection.QueryFirstOrDefaultAsync<ProductHSNDetailDto>(sql, new { ID = id, CompanyID = companyId });
    }

    public async Task<bool> IsHSNUsedAsync(long id)
    {
        using var connection = GetConnection();
        var sql = @"
            SELECT 1 FROM ItemMaster WHERE ProductHSNID = @ID 
            UNION ALL SELECT 1 FROM ToolMaster WHERE ProductHSNID = @ID
            UNION ALL SELECT 1 FROM ItemTransactionDetail WHERE ProductHSNID = @ID
            UNION ALL SELECT 1 FROM JobBooking WHERE ProductHSNID = @ID
            UNION ALL SELECT 1 FROM JobOrderBookingDetails WHERE ProductHSNID = @ID
            UNION ALL SELECT 1 FROM ProductMaster WHERE ProductHSNID = @ID";
            
        var used = await connection.ExecuteScalarAsync<int?>(sql, new { ID = id });
        return used.HasValue;
    }
}
