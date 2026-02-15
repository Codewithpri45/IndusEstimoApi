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
/// Repository implementation for Machine and Process operations
/// </summary>
public class MachineProcessRepository : IMachineProcessRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MachineProcessRepository> _logger;

    public MachineProcessRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MachineProcessRepository> logger)
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

    public async Task<List<MachineGridDto>> GetMachineGridAsync(string contentDomainType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // CRITICAL FIX (Gap #1): LEFT JOIN with MachineToolAllocationMaster to get ALL cylinders per machine
        // Legacy: Api_shiring_serviceController.cs Line 277 (Flexo_Roll_Planning query)
        // This returns MULTIPLE ROWS per machine (one per allocated cylinder)
        string query = @"
            SELECT
                MM.MachineID,
                MM.MachineName,
                MM.MachineType,
                ISNULL(MM.Colors, 0) AS MachineColors,
                ISNULL(MM.MaxLength, 0) AS MaxSheetL,
                CASE WHEN ISNULL(MM.MaxRollWidth, 0) > 0 THEN MM.MaxRollWidth ELSE ISNULL(MM.MaxWidth, 0) END AS MaxSheetW,
                ISNULL(MM.MinLength, 0) AS MinSheetL,
                CASE WHEN ISNULL(MM.MinRollWidth, 0) > 0 THEN MM.MinRollWidth ELSE ISNULL(MM.MinWidth, 0) END AS MinSheetW,
                ISNULL(MM.PerHourCost, 0) AS PerHourRate,
                '' AS PaperGroup,
                ISNULL(MM.MakeReadyTime, 0) AS MakeReadyTime,
                ISNULL(MM.JobChangeOverTime, 0) AS JobChangeOverTime,
                ISNULL(MM.RollChangeTime, 0) AS RollChangeOverTime,
                ISNULL(MM.MachineSpeed, 0) AS Speed,
                ISNULL(MM.AverageRollChangeWastage, 0) AS RollChangeWastage,
                ISNULL(MM.AverageRollLength, 0) AS StandardRollLength,
                ISNULL(MM.MinCircumference, 0) AS MinCircumferenceMM,
                ISNULL(MM.MaxCircumference, 0) AS MaxCircumferenceMM,
                -- Cylinder Details from LEFT JOIN
                ISNULL(TM.ToolID, 0) AS CylinderToolID,
                ISNULL(TM.ToolCode, '') AS CylinderToolCode,
                ISNULL(TM.ToolName, '') AS CylinderToolName,
                ISNULL(TM.CircumferenceMM, 0) AS CylinderCircumferenceMM,
                ISNULL(TM.CircumferenceInch, 0) AS CylinderCircumferenceInch,
                ISNULL(TM.NoOfTeeth, 0) AS CylinderNoOfTeeth,
                ISNULL(TM.SizeW, 0) AS CylinderWidth
            FROM MachineMaster AS MM
            LEFT JOIN MachineToolAllocationMaster AS MTM
                ON MTM.MachineID = MM.MachineID
                AND MTM.CompanyID = MM.CompanyID
                AND MTM.ToolGroupID IN (
                    SELECT ToolGroupID
                    FROM ToolGroupMaster
                    WHERE ToolGroupNameID = -5
                      AND CompanyID = @CompanyID
                )
            LEFT JOIN ToolMaster AS TM
                ON TM.ToolID = MTM.ToolID
                AND TM.CompanyID = MTM.CompanyID
            WHERE MM.CompanyID = @CompanyID
              AND ISNULL(MM.IsDeletedTransaction, 0) = 0
              AND MM.CurrentStatus = 'ACTIVE'
              AND MM.MachineType = @ContentDomainType
            ORDER BY MM.MachineName, TM.ToolID";

        var results = await connection.QueryAsync<MachineGridDto>(query, 
            new { CompanyID = companyId, ContentDomainType = contentDomainType });
        return results.ToList();
    }

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                MachineID,
                MachineName,
                MachineType,
                ISNULL(Colors, 0) AS MachineColors,
                CASE WHEN CurrentStatus = 'ACTIVE' THEN 1 ELSE 0 END AS IsActive
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
              AND CurrentStatus = 'ACTIVE'
            ORDER BY MachineName";

        var results = await connection.QueryAsync<MachineDto>(query, new { CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<OperationDto>> GetDefaultOperationsAsync(string domainType)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                PM.ProcessID,
                PM.ProcessName,
                PM.TypeofCharges,
                ISNULL(PM.Rate, 0) AS Rate,
                ISNULL(PM.MinimumCharges, 0) AS MinimumCharges,
                ISNULL(PM.SetupCharges, 0) AS SetupCharges,
                ISNULL(PM.IsOnlineProcess, 0) AS IsOnlineProcess,
                ISNULL(DM.SequenceNo, 0) AS SequenceNo,
                PM.DepartmentID,
                ISNULL(PM.ProcessWastagePercentage, 0) AS WastagePercentage,
                ISNULL(PM.ProcessFlatWastageValue, 0) AS FlatWastage
            FROM ProcessMaster AS PM
            INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID = PM.DepartmentID
            WHERE PM.CompanyID = @CompanyID
              AND ISNULL(PM.IsDeletedTransaction, 0) = 0
              AND ISNULL(PM.IsBlocked, 0) = 0
              AND PM.ProcessModuleType = @DomainType
            ORDER BY DM.SequenceNo, PM.ProcessName";

        var results = await connection.QueryAsync<OperationDto>(query, 
            new { CompanyID = companyId, DomainType = domainType });
        return results.ToList();
    }

    public async Task<List<OperationSlabDto>> GetOperationSlabsAsync(long processId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                SlabID,
                ProcessID,
                RateFactor,
                ISNULL(FromValue, 0) AS FromValue,
                ISNULL(ToValue, 0) AS ToValue,
                ISNULL(Rate, 0) AS Rate
            FROM ProcessMasterSlabs
            WHERE ProcessID = @ProcessID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY FromValue";

        var results = await connection.QueryAsync<OperationSlabDto>(query, 
            new { ProcessID = processId, CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<MachineItemDto>> GetMachineItemsAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                IM.ItemID,
                IM.ItemCode,
                IM.ItemName,
                IM.ItemGroupID,
                IGM.ItemGroupName,
                ISNULL(IM.EstimationRate, 0) AS EstimationRate,
                IM.EstimationUnit
            FROM MachineMaterialAllocation AS MMA
            INNER JOIN ItemMaster AS IM ON IM.ItemID = MMA.ItemID
            LEFT JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID
            WHERE MMA.MachineID = @MachineID
              AND MMA.CompanyID = @CompanyID
              AND ISNULL(MMA.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.IsDeletedTransaction, 0) = 0
              AND ISNULL(IM.ISItemActive, 1) <> 0
            ORDER BY IM.ItemName";

        var results = await connection.QueryAsync<MachineItemDto>(query, 
            new { MachineID = machineId, CompanyID = companyId });
        return results.ToList();
    }
    
    // Core Flexo Dependency: Fetch Tools (Cylinders) assigned to a Machine
    public async Task<List<DieToolDto>> GetMachineToolsAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                TM.ToolID,
                TM.ToolCode,
                TM.ToolName,
                TM.ToolType,
                ISNULL(TM.SizeL, 0) AS SizeL,
                ISNULL(TM.SizeW, 0) AS SizeW,
                ISNULL(TM.SizeH, 0) AS SizeH,
                ISNULL(TM.CircumferenceInch, 0) AS CircumferenceInch,
                ISNULL(TM.CircumferenceMM, 0) AS CircumferenceMM,
                ISNULL(TM.NoOfTeeth, 0) AS NoOfTeeth,
                ISNULL(TM.ToolRate, 0) AS ToolRate
            FROM MachineToolAllocationMaster AS MTA
            INNER JOIN ToolMaster AS TM ON TM.ToolID = MTA.ToolID
            WHERE MTA.MachineID = @MachineID
              AND MTA.CompanyID = @CompanyID
              AND ISNULL(MTA.IsDeletedTransaction, 0) = 0
              AND ISNULL(TM.IsDeletedTransaction, 0) = 0
            ORDER BY TM.ToolName";

        var results = await connection.QueryAsync<DieToolDto>(query, 
            new { MachineID = machineId, CompanyID = companyId });
        return results.ToList();
    }

    public async Task<List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto>> GetMachineSlabsAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                SlabID,
                RunningMeterRangeFrom,
                RunningMeterRangeTo,
                Rate,
                Wastage,
                PlateCharges,
                MinCharges,
                PaperGroup
            FROM MachineSlabMaster
            WHERE MachineID = @MachineID
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY RunningMeterRangeFrom";

        var results = await connection.QueryAsync<IndasEstimo.Application.DTOs.Masters.MachineSlabDto>(query, 
            new { MachineID = machineId, CompanyID = companyId });
        return results.ToList();
    }
    public async Task<List<CategoryWastageSettingDto>> GetCategoryWastageSettingsAsync(long categoryId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        string query = @"
            SELECT 
                CategoryID, 
                PrintingStyle, 
                ISNULL(NoOfColor, 0) AS NoOfColor, 
                Unit, 
                ISNULL(FlatWastage, 0) AS FlatWastage, 
                ISNULL(WastagePercentage, 0) AS WastagePercentage, 
                CalculationOn 
            FROM CategoryWiseWastageSetting 
            WHERE CategoryID = @CategoryID 
              AND CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0 
            ORDER BY PrintingStyle, NoOfColor";

        var results = await connection.QueryAsync<CategoryWastageSettingDto>(query, 
            new { CategoryID = categoryId, CompanyID = companyId });
        return results.ToList();
    }
}
