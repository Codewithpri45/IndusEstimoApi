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
                TM.ToolID,
                NULLIF(TM.ToolCode, '') AS ToolCode,
                NULLIF(TM.ToolName, '') AS ToolName,
                NULLIF(TM.ToolDescription, '') AS ToolDescription,
                ISNULL(TM.LedgerName, '-') AS LedgerName,
                ISNULL(TM.SizeL, 0) AS SizeL,
                ISNULL(TM.SizeW, 0) AS SizeW,
                ISNULL(TM.SizeH, 0) AS SizeH,
                ISNULL(TM.UpsL, 0) AS UpsL,
                ISNULL(TM.UpsW, 0) AS UpsW,
                ISNULL(TM.TotalUps, 0) AS TotalUps,
                TM.Manufecturer,
                ISNULL(TM.CircumferenceInch, 0) AS CircumferenceInch,
                ISNULL(TM.CircumferenceMM, 0) AS CircumferenceMM,
                ISNULL(TM.NoOfTeeth, 0) AS NoOfTeeth,
                ISNULL(TM.ToolRate, 0) AS ToolRate
            FROM ToolMaster AS TM
            INNER JOIN ToolGroupMaster AS TG ON TM.ToolGroupID = TG.ToolGroupID
            WHERE TG.ToolGroupNameID IN (-3, -8)
              AND TM.CompanyID = @CompanyID
              AND ISNULL(TM.IsDeletedTransaction, 0) = 0
              AND (TM.SizeL >= (@SizeL - @SizeLTolerance) AND TM.SizeL <= (@SizeL + @SizeLTolerance))
              AND (TM.SizeW >= (@SizeW - @SizeWTolerance) AND TM.SizeW <= (@SizeW + @SizeWTolerance))
              AND (TM.SizeH >= (@SizeH - @SizeHTolerance) AND TM.SizeH <= (@SizeH + @SizeHTolerance))
            ORDER BY TM.ToolCode";

        var results = await connection.QueryAsync<DieToolDto>(query, new
        {
            CompanyID = companyId,
            SizeL = request.SizeL,
            SizeLTolerance = request.SizeLTolerance,
            SizeW = request.SizeW,
            SizeWTolerance = request.SizeWTolerance,
            SizeH = request.SizeH,
            SizeHTolerance = request.SizeHTolerance
        });
        return results.ToList();
    }

    public async Task<List<ReelDto>> GetReelsAsync(decimal reqDeckle, decimal widthPlus, decimal widthMinus, int itemGroupId = -2, string quality = "", double gsm = 0, string mill = "")
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Build quality filter (matching legacy logic from Api_shiring_serviceController.cs line 200)
        var qualityFilter = "";
        if (!string.IsNullOrEmpty(quality))
        {
            qualityFilter += " AND IM.Quality = @Quality";
        }
        if (gsm > 0)
        {
            qualityFilter += " AND IM.GSM = @GSM";
        }
        if (!string.IsNullOrEmpty(mill))
        {
            qualityFilter += " AND IM.Manufecturer = @Mill";
        }

        string query;

        if (widthPlus > 0 || widthMinus > 0)
        {
            var lowerLimit = reqDeckle - widthMinus;
            var upperLimit = reqDeckle + widthPlus;

            query = @"
                SELECT DISTINCT
                    IM.ItemID,
                    IM.ItemCode,
                    IM.ItemName,
                    ISNULL(IM.Quality, '') AS Quality,
                    ISNULL(IM.GSM, 0) AS GSM,
                    ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                    ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                    ISNULL(IM.Thickness, 0) AS Thickness,
                    ISNULL(IM.Density, 0) AS Density,
                    ISNULL(IM.Manufecturer, '') AS Manufecturer,
                    ISNULL(IM.BF, 0) AS BF,
                    ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                    IM.StockUnit,
                    ISNULL(IM.SizeW, 0) AS SizeW,
                    ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                    ISNULL(IM.EstimationUnit, '') AS EstimationUnit,
                    ISNULL(IM.Finish, '') AS Finish,
                    ISNULL(IM.AvgRollLength, 0) AS AvgRollLength,
                    ISNULL(IM.PaperGroup, '') AS PaperGroup,
                    ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                    ISNULL(IM.PurchaseUnit, '') AS PurchaseUnit,
                    ISNULL(IM.IsStandardItem, 0) AS IsStandardItem,
                    CASE WHEN ISNULL(IM.PhysicalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
                FROM ItemMaster AS IM
                INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID
                WHERE IG.ItemGroupNameID = @ItemGroupNameID
                  AND IM.CompanyID = @CompanyID
                  AND IM.SizeW >= @LowerLimit
                  AND IM.SizeW <= @UpperLimit
                  AND ISNULL(IM.IsDeletedTransaction, 0) = 0"
                  + qualityFilter + @"
                ORDER BY IM.ItemCode";

            var results = await connection.QueryAsync<ReelDto>(query, new
            {
                CompanyID = companyId,
                ItemGroupNameID = itemGroupId,
                LowerLimit = lowerLimit,
                UpperLimit = upperLimit,
                Quality = quality,
                GSM = gsm,
                Mill = mill
            });
            return results.ToList();
        }
        else
        {
            query = @"
                SELECT DISTINCT
                    IM.ItemID,
                    IM.ItemCode,
                    IM.ItemName,
                    ISNULL(IM.Quality, '') AS Quality,
                    ISNULL(IM.GSM, 0) AS GSM,
                    ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                    ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                    ISNULL(IM.Thickness, 0) AS Thickness,
                    ISNULL(IM.Density, 0) AS Density,
                    ISNULL(IM.Manufecturer, '') AS Manufecturer,
                    ISNULL(IM.BF, 0) AS BF,
                    ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                    IM.StockUnit,
                    ISNULL(IM.SizeW, 0) AS SizeW,
                    ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                    ISNULL(IM.EstimationUnit, '') AS EstimationUnit,
                    ISNULL(IM.Finish, '') AS Finish,
                    ISNULL(IM.AvgRollLength, 0) AS AvgRollLength,
                    ISNULL(IM.PaperGroup, '') AS PaperGroup,
                    ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                    ISNULL(IM.PurchaseUnit, '') AS PurchaseUnit,
                    ISNULL(IM.IsStandardItem, 0) AS IsStandardItem,
                    CASE WHEN ISNULL(IM.PhysicalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
                FROM ItemMaster AS IM
                INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID
                WHERE IG.ItemGroupNameID = @ItemGroupNameID
                  AND IM.CompanyID = @CompanyID
                  AND IM.SizeW >= @ReqDeckle
                  AND ISNULL(IM.IsDeletedTransaction, 0) = 0"
                  + qualityFilter + @"
                ORDER BY IM.ItemCode";

            var results = await connection.QueryAsync<ReelDto>(query, new
            {
                CompanyID = companyId,
                ItemGroupNameID = itemGroupId,
                ReqDeckle = reqDeckle,
                Quality = quality,
                GSM = gsm,
                Mill = mill
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
                ISNULL(IM.Quality, '') AS Quality,
                ISNULL(IM.GSM, 0) AS GSM,
                ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                ISNULL(IM.Thickness, 0) AS Thickness,
                ISNULL(IM.Density, 0) AS Density,
                ISNULL(IM.Manufecturer, '') AS Manufecturer,
                ISNULL(IM.BF, 0) AS BF,
                ISNULL(IM.PhysicalStock, 0) AS PhysicalStock,
                IM.StockUnit,
                ISNULL(IM.SizeW, 0) AS SizeW,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                ISNULL(IM.EstimationUnit, '') AS EstimationUnit,
                ISNULL(IM.Finish, '') AS Finish,
                ISNULL(IM.AvgRollLength, 0) AS AvgRollLength,
                ISNULL(IM.PaperGroup, '') AS PaperGroup,
                ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                ISNULL(IM.PurchaseUnit, '') AS PurchaseUnit,
                ISNULL(IM.IsStandardItem, 0) AS IsStandardItem,
                CASE WHEN ISNULL(IM.PhysicalStock, 0) > 0 THEN 1 ELSE 0 END AS IsAvailable
            FROM ItemMaster AS IM
            INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID
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

        // Legacy query with CTE for first allocated machine per process
        string query = $@"
            WITH RankedMachines AS (
                SELECT ProcessID, MachineID, 
                       ROW_NUMBER() OVER (PARTITION BY ProcessID ORDER BY MachineID) AS rn 
                FROM ProcessAllocatedMachineMaster
            )
            SELECT 
                PM.ProcessID,
                PM.ProcessModuleType AS DomainType,
                PM.ProcessName,
                RM.MachineID,
                MM.MachineName,
                IM.ItemID,
                IGM.ItemGroupID,
                ISG.ItemSubGroupID,
                IGM.ItemGroupNameID,
                IGM.ItemGroupName,
                ISG.ItemSubGroupName,
                IM.ItemName,
                ISNULL(IM.SizeL, 0) AS SizeL,
                ISNULL(IM.SizeW, 0) AS SizeW,
                ISNULL(IM.SizeH, 0) AS SizeH,
                ISNULL(IM.Thickness, 0) AS Thickness,
                ISNULL(IM.Density, 0) AS Density,
                ISNULL(IM.GSM, 0) AS GSM,
                ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM,
                ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM,
                IM.StockUnit,
                IM.PurchaseUnit,
                IM.EstimationUnit,
                ISNULL(IM.PurchaseRate, 0) AS PurchaseRate,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate
            FROM ProcessMaster AS PM
            INNER JOIN ProcessAllocatedMaterialMaster AS PAM ON PAM.ProcessID = PM.ProcessID
            INNER JOIN RankedMachines AS RM ON RM.ProcessID = PAM.ProcessID AND RM.rn = 1
            INNER JOIN MachineMaster AS MM ON MM.MachineID = RM.MachineID
            INNER JOIN ItemMaster AS IM ON IM.ItemID = PAM.ItemID
            INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            INNER JOIN ItemSubGroupMaster AS ISG ON ISG.ItemSubGroupID = IM.ItemSubGroupID
            WHERE PM.ProcessID IN ({processIds})";

        var results = await connection.QueryAsync<ProcessMaterialDto>(query);
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
