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
/// Repository implementation for Tool and Material operations
/// </summary>
public class ToolMaterialRepository : IToolMaterialRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolMaterialRepository> _logger;

    public ToolMaterialRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<ToolMaterialRepository> logger)
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

    public async Task<List<DieToolDto>> SearchDiesAsync(SearchDiesRequest request)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                ToolID,
                ToolCode,
                ToolName,
                ToolType,
                ISNULL(SizeL, 0) AS SizeL,
                ISNULL(SizeW, 0) AS SizeW,
                ISNULL(SizeH, 0) AS SizeH,
                ISNULL(CircumferenceInch, 0) AS CircumferenceInch,
                ISNULL(CircumferenceMM, 0) AS CircumferenceMM,
                ISNULL(NoOfTeeth, 0) AS NoOfTeeth,
                ISNULL(ToolRate, 0) AS ToolRate
            FROM ToolMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND (
                  (SizeL >= @SizeL AND SizeW >= @SizeW)
                  OR (SizeL >= @SizeW AND SizeW >= @SizeL)
              )
              AND (@ToolType IS NULL OR ToolType = @ToolType)
            ORDER BY ToolCode";

        var results = await connection.QueryAsync<DieToolDto>(query, new
        {
            CompanyID = companyId,
            SizeL = request.SizeL,
            SizeW = request.SizeW,
            ToolType = request.ToolType
        });
        return results.ToList();
    }

    public async Task<List<ReelDto>> GetReelsAsync(decimal reqDeckle, decimal widthPlus, decimal widthMinus, int itemGroupId = -2)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query;

        if (widthPlus > 0 || widthMinus > 0)
        {
            var lowerLimit = reqDeckle - widthMinus;
            var upperLimit = reqDeckle + widthPlus;

            query = @"
                SELECT 
                    IM.ItemID,
                    IM.ItemCode,
                    IM.ItemName,
                    ISNULL(IM.GSM, 0) AS GSM,
                    ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                    ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                    ISNULL(IM.BF, 0) AS BF,
                    ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                    IM.StockUnit,
                    ISNULL(IM.SizeW, 0) AS SizeW,
                    ISNULL(IM.EstimationRate, 0) AS EstimationRate
                FROM ItemMaster AS IM
                INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID
                WHERE IG.ItemGroupNameID = @ItemGroupNameID
                  AND IM.CompanyID = @CompanyID
                  AND IM.SizeW >= @LowerLimit
                  AND IM.SizeW <= @UpperLimit
                  AND ISNULL(IM.IsDeletedTransaction, 0) = 0
                ORDER BY IM.ItemCode";

            var results = await connection.QueryAsync<ReelDto>(query, new
            {
                CompanyID = companyId,
                ItemGroupNameID = itemGroupId,
                LowerLimit = lowerLimit,
                UpperLimit = upperLimit
            });
            return results.ToList();
        }
        else
        {
            query = @"
                SELECT 
                    IM.ItemID,
                    IM.ItemCode,
                    IM.ItemName,
                    ISNULL(IM.GSM, 0) AS GSM,
                    ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                    ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                    ISNULL(IM.BF, 0) AS BF,
                    ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                    IM.StockUnit,
                    ISNULL(IM.SizeW, 0) AS SizeW,
                    ISNULL(IM.EstimationRate, 0) AS EstimationRate
                FROM ItemMaster AS IM
                INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID
                WHERE IG.ItemGroupNameID = @ItemGroupNameID
                  AND IM.CompanyID = @CompanyID
                  AND IM.SizeW >= @ReqDeckle
                  AND ISNULL(IM.IsDeletedTransaction, 0) = 0
                ORDER BY IM.ItemCode";

            var results = await connection.QueryAsync<ReelDto>(query, new
            {
                CompanyID = companyId,
                ItemGroupNameID = itemGroupId,
                ReqDeckle = reqDeckle
            });
            return results.ToList();
        }
    }

    public async Task<ReelDto?> GetReelByIdAsync(long itemId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                IM.ItemID,
                IM.ItemCode,
                IM.ItemName,
                ISNULL(IM.GSM, 0) AS GSM,
                ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                ISNULL(IM.BF, 0) AS BF,
                ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                IM.StockUnit,
                ISNULL(IM.SizeW, 0) AS SizeW,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate
            FROM ItemMaster AS IM
            WHERE IM.ItemID = @ItemID
              AND IM.CompanyID = @CompanyID
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<ReelDto>(query, new
        {
            CompanyID = companyId,
            ItemID = itemId
        });
        return result;
    }

    public async Task<List<ProcessMaterialDto>> GetProcessMaterialsAsync(string processIds)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Convert comma-separated IDs to list for IN clause
        string query = $@"
            SELECT DISTINCT
                IM.ItemID,
                IM.ItemCode,
                IM.ItemName,
                IM.ItemGroupID,
                IGM.ItemGroupName,
                IM.ItemSubGroupID,
                ISGM.ItemSubGroupName,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                IM.EstimationUnit,
                IM.StockUnit
            FROM ProcessMaterialAllocation AS PMA
            INNER JOIN ItemMaster AS IM ON IM.ItemID = PMA.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID
            WHERE PMA.ProcessID IN ({processIds})
              AND PMA.CompanyID = @CompanyID
              AND ISNULL(PMA.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.ISItemActive, 1) <> 0
            ORDER BY IM.ItemName";

        var results = await connection.QueryAsync<ProcessMaterialDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }
    public async Task<DieToolDto?> GetToolByIdAsync(long toolId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                ToolID,
                ToolCode,
                ToolName,
                ToolType,
                ISNULL(SizeL, 0) AS SizeL,
                ISNULL(SizeW, 0) AS SizeW,
                ISNULL(SizeH, 0) AS SizeH,
                ISNULL(CircumferenceInch, 0) AS CircumferenceInch,
                ISNULL(CircumferenceMM, 0) AS CircumferenceMM,
                ISNULL(NoOfTeeth, 0) AS NoOfTeeth,
                ISNULL(ToolRate, 0) AS ToolRate
            FROM ToolMaster
            WHERE ToolID = @ToolID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<DieToolDto>(query, new
        {
            CompanyID = companyId,
            ToolID = toolId
        });
        return result;
    }
}
