using Dapper;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Estimation;

/// <summary>
/// Repository implementation for Master Data operations
/// </summary>
public class MasterDataRepository : IMasterDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MasterDataRepository> _logger;

    public MasterDataRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MasterDataRepository> logger)
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

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        // First check if user has segment allocation
        string segmentCheckQuery = @"
            SELECT DISTINCT SegmentID 
            FROM UserSegmentAllocation 
            WHERE UserID = @UserID 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
              AND CompanyID = @CompanyID";

        var segments = await connection.QueryAsync<long>(segmentCheckQuery, new { UserID = userId, CompanyID = companyId });

        string query;
        if (segments.Any())
        {
            query = @"
                SELECT DISTINCT 
                    CM.CategoryID, 
                    SM.SegmentID, 
                    NULLIF(REPLACE(CM.CategoryName, '""', ''), '') AS CategoryName, 
                    SM.SegmentName 
                FROM CategoryMaster AS CM 
                INNER JOIN SegmentMaster AS SM ON SM.SegmentID = CM.SegmentID 
                WHERE CM.CompanyID = @CompanyID 
                  AND ISNULL(CM.IsDeletedTransaction, 0) <> 1 
                  AND CM.SegmentID IN (
                      SELECT DISTINCT SegmentID 
                      FROM UserSegmentAllocation 
                      WHERE UserID = @UserID 
                        AND ISNULL(IsDeletedTransaction, 0) = 0 
                        AND CompanyID = @CompanyID
                  ) 
                ORDER BY CategoryName";
        }
        else
        {
            query = @"
                SELECT DISTINCT 
                    CM.CategoryID, 
                    SM.SegmentID, 
                    NULLIF(REPLACE(CM.CategoryName, '""', ''), '') AS CategoryName, 
                    SM.SegmentName 
                FROM CategoryMaster AS CM 
                INNER JOIN SegmentMaster AS SM ON SM.SegmentID = CM.SegmentID 
                WHERE CM.CompanyID = @CompanyID 
                  AND ISNULL(CM.IsDeletedTransaction, 0) <> 1 
                ORDER BY CategoryName";
        }

        var results = await connection.QueryAsync<CategoryDto>(query, new { CompanyID = companyId, UserID = userId });
        return results.ToList();
    }

    public async Task<List<ClientDto>> GetClientsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT ISNULL(NULLIF(REPLACE(LM.LedgerName, '""', ''), ''), '') AS LedgerName,
                   LM.LedgerID,
                   ISNULL(LM.CreditDays, 0) AS CreditDays
            FROM LedgerMaster AS LM
            INNER JOIN LedgerGroupMaster AS LGM ON LGM.LedgerGroupID = LM.LedgerGroupID
            WHERE LGM.LedgerGroupNameID = 24
              AND ISNULL(LM.IsDeletedTransaction, 0) <> 1
              AND LM.CompanyID = @CompanyID
              AND ISNULL(LM.LedgerName, '') <> ''
            ORDER BY LM.LedgerName";

        var results = await connection.QueryAsync<ClientDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<SalesPersonDto>> GetSalesPersonsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT DISTINCT
                LM.LedgerID AS EmployeeID,
                NULLIF(REPLACE(LM.LedgerName, '""', ''), '') AS EmployeeName
            FROM LedgerMaster AS LM
            INNER JOIN LedgerGroupMaster AS LG ON LG.LedgerGroupID = LM.LedgerGroupID
                AND LG.CompanyID = LM.CompanyID
            WHERE LG.LedgerGroupNameID = 27
              AND LM.DepartmentID = -50
              AND ISNULL(LM.IsDeletedTransaction, 0) <> 1
            ORDER BY EmployeeName";

        var results = await connection.QueryAsync<SalesPersonDto>(query);
        return results.ToList();
    }

    public async Task<List<ContentDto>> GetAllContentsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        // Check for segment allocation
        string segmentCheckQuery = @"
            SELECT DISTINCT SM.SegmentID, SM.SegmentName 
            FROM SegmentMaster AS SM 
            INNER JOIN UserSegmentAllocation AS US ON US.SegmentID = SM.SegmentID 
            WHERE ISNULL(SM.IsDeletedTransaction, 0) = 0 
              AND ISNULL(US.IsDeletedTransaction, 0) = 0 
              AND US.UserID = @UserID 
              AND SM.CompanyID = @CompanyID 
            ORDER BY SM.SegmentName";

        var segments = await connection.QueryAsync<dynamic>(segmentCheckQuery, new { UserID = userId, CompanyID = companyId });

        string query;
        if (segments.Any())
        {
            query = @"
                SELECT DISTINCT 
                    C.ContentID,
                    NULLIF(REPLACE(C.ContentName, '""', ''), '') AS ContentName,
                    NULLIF(REPLACE(C.ContentCaption, '""', ''), '') AS ContentCaption,
                    NULLIF(REPLACE(C.ContentOpenHref, '""', ''), '') AS ContentOpenHref,
                    NULLIF(REPLACE(C.ContentClosedHref, '""', ''), '') AS ContentClosedHref,
                    NULLIF(REPLACE(C.ContentSizes, '""', ''), '') AS ContentSizes,
                    NULLIF(REPLACE(C.ContentDomainType, '""', ''), '') AS ContentDomainType,
                    CM.CategoryName,
                    SM.SegmentName
                FROM ContentMaster AS C
                INNER JOIN CategoryContentAllocationMaster AS CCA ON CCA.ContentID = C.ContentID
                    AND ISNULL(CCA.IsDeletedTransaction, 0) = 0
                INNER JOIN CategoryMaster AS CM ON CM.CategoryID = CCA.CategoryID
                    AND ISNULL(CM.IsDeletedTransaction, 0) = 0
                INNER JOIN SegmentMaster AS SM ON SM.SegmentID = CM.SegmentID
                    AND ISNULL(SM.IsDeletedTransaction, 0) = 0
                WHERE ISNULL(C.IsActive, 0) = 1
                  AND C.CompanyID = @CompanyID
                  AND SM.SegmentID IN (
                      SELECT DISTINCT SegmentID 
                      FROM UserSegmentAllocation 
                      WHERE CompanyID = @CompanyID 
                        AND UserID = @UserID 
                        AND ISNULL(IsDeletedTransaction, 0) = 0
                  )
                ORDER BY SegmentName, CategoryName, ContentName";
        }
        else
        {
            query = @"
                SELECT 
                    ContentID,
                    NULLIF(REPLACE(ContentName, '""', ''), '') AS ContentName,
                    NULLIF(REPLACE(ContentCaption, '""', ''), '') AS ContentCaption,
                    NULLIF(REPLACE(ContentOpenHref, '""', ''), '') AS ContentOpenHref,
                    NULLIF(REPLACE(ContentClosedHref, '""', ''), '') AS ContentClosedHref,
                    NULLIF(REPLACE(ContentSizes, '""', ''), '') AS ContentSizes,
                    NULLIF(REPLACE(ContentDomainType, '""', ''), '') AS ContentDomainType
                FROM ContentMaster
                WHERE ISNULL(IsActive, 0) = 1
                  AND CompanyID = @CompanyID
                ORDER BY SequencNo";
        }

        var results = await connection.QueryAsync<ContentDto>(query, new { CompanyID = companyId, UserID = userId });
        return results.ToList();
    }

    public async Task<List<ContentByCategoryDto>> GetContentsByCategoryAsync(long categoryId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT DISTINCT 
                C.ContentID,
                NULLIF(REPLACE(C.ContentName, '""', ''), '') AS ContentName,
                NULLIF(REPLACE(C.ContentDomainType, '""', ''), '') AS ContentDomainType
            FROM ContentMaster AS C
            INNER JOIN CategoryContentAllocationMaster AS CCA ON CCA.ContentID = C.ContentID
            WHERE CCA.CategoryID = @CategoryID
              AND C.CompanyID = @CompanyID
              AND ISNULL(C.IsActive, 0) = 1

            ORDER BY ContentName";

        var results = await connection.QueryAsync<ContentByCategoryDto>(query, new { CategoryID = categoryId, CompanyID = companyId });
        return results.ToList();
    }

    public async Task<CategoryDefaultsDto?> GetCategoryDefaultsAsync(long categoryId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                CategoryID, 
                CategoryName, 
                CategoryID, 
                CategoryName,
                ISNULL(DefaultAcrossGap, 0) AS DefaultAcrossGap,
                ISNULL(DefaultAroundGap, 0) AS DefaultAroundGap,
                ISNULL(DefaultPlateBearer, 0) AS DefaultPlateBearer,
                ISNULL(DefaultSideStrip, 0) AS DefaultSideStrip
            FROM CategoryMaster 
            WHERE CategoryID = @CategoryID 
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<CategoryDefaultsDto>(query, new { CategoryID = categoryId, CompanyID = companyId });
        return result;
    }

    public async Task<List<WindingDirectionDto>> GetWindingDirectionAsync(string contentDomainType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                WindingDirectionID, 
                Direction, 
                Image 
            FROM UnwindingDirectionImage 
            WHERE CompanyID = @CompanyID 
              AND ContentDomainType = @ContentDomainType";

        var results = await connection.QueryAsync<WindingDirectionDto>(query, new { CompanyID = companyId, ContentDomainType = contentDomainType });
        return results.ToList();
    }

    public async Task<List<BookContentDto>> GetBookContentsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                ContentID,
                ContentName,
                ContentsCategory AS ContentCategory
            FROM ContentMaster 
            WHERE ContentsCategory = 'Book' 
              AND CompanyID = @CompanyID 
            ORDER BY ContentName ASC";

        var results = await connection.QueryAsync<BookContentDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<OneTimeChargeDto>> GetOneTimeChargesAsync()
    {
        using var connection = GetConnection();

        string query = @"
            SELECT 
                ParameterValue AS Headname, 
                '0' AS Amount 
            FROM ERPParameterSetting 
            WHERE ParameterType = 'One Time Charges' 
              AND ISNULL(IsDeletedTransaction, 0) = 0 
            ORDER BY ParameterID";

        var results = await connection.QueryAsync<OneTimeChargeDto>(query);
        return results.ToList();
    }

    public async Task<ContentSizeDto?> GetContentSizeAsync(string contentName)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                ContentID,
                NULLIF(REPLACE(ContentName, '""', ''), '') AS ContentName, 
                NULLIF(REPLACE(ContentSizes, '""', ''), '') AS ContentSizes 
            FROM ContentMaster 
            WHERE ISNULL(IsActive, 0) = 1 
              AND ContentName = @ContentName 
              AND CompanyID = @CompanyID 
            ORDER BY SequencNo";

        var result = await connection.QueryFirstOrDefaultAsync<ContentSizeDto>(query, new { ContentName = contentName, CompanyID = companyId });
        return result;
    }
}
