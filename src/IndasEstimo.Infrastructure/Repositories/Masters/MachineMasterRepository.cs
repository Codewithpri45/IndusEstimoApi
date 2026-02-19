using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class MachineMasterRepository : IMachineMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MachineMasterRepository> _logger;

    public MachineMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<MachineMasterRepository> logger)
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

    public async Task<List<MachineListDto>> GetMachineListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                MM.MachineID,
                ISNULL(MM.MachineCode, '') AS MachineCode,
                ISNULL(MM.RefMachineCode, '') AS RefMachineCode,
                ISNULL(MM.MachineName, '') AS MachineName,
                ISNULL(MM.DepartmentID, 0) AS DepartmentID,
                ISNULL(DM.DepartmentName, '') AS DepartmentName,
                ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID,
                ISNULL(PUM.ProductionUnitName, '') AS ProductionUnitName,
                ISNULL(MM.MachineType, '') AS MachineType,
                ISNULL(MM.MinimumSheet, 0) AS MinimumSheet,
                ISNULL(MM.Gripper, 0) AS Gripper,
                ISNULL(MM.MaxLength, 0) AS MaxLength,
                ISNULL(MM.MaxWidth, 0) AS MaxWidth,
                ISNULL(MM.MinLength, 0) AS MinLength,
                ISNULL(MM.MinWidth, 0) AS MinWidth,
                ISNULL(MM.MaxPrintL, 0) AS MaxPrintL,
                ISNULL(MM.MaxPrintW, 0) AS MaxPrintW,
                ISNULL(MM.MinPrintL, 0) AS MinPrintL,
                ISNULL(MM.MinPrintW, 0) AS MinPrintW,
                ISNULL(MM.Colors, 0) AS Colors,
                ISNULL(MM.MakeReadyCharges, 0) AS MakeReadyCharges,
                ISNULL(MM.MakeReadyWastageSheet, 0) AS MakeReadyWastageSheet,
                ISNULL(MM.MakeReadyTime, 0) AS MakeReadyTime,
                ISNULL(MM.MakeReadyPerHourCost, 0) AS MakeReadyPerHourCost,
                ISNULL(MM.ElectricConsumption, 0) AS ElectricConsumption,
                ISNULL(MM.PrintingMargin, 0) AS PrintingMargin,
                ISNULL(MM.WebCutOffSize, 0) AS WebCutOffSize,
                ISNULL(MM.MinReelSize, 0) AS MinReelSize,
                ISNULL(MM.MaxReelSize, 0) AS MaxReelSize,
                ISNULL(MM.MachineSpeed, 0) AS MachineSpeed,
                ISNULL(MM.LabourCharges, 0) AS LabourCharges,
                ISNULL(MM.WebCutOffSizeMin, 0) AS WebCutOffSizeMin,
                ISNULL(MM.ChargesType, '') AS ChargesType,
                ISNULL(MM.RoundofImpressionsWith, '') AS RoundofImpressionsWith,
                ISNULL(MM.IsPerfectaMachine, 0) AS IsPerfectaMachine,
                ISNULL(MM.IsVariableCutOff, 0) AS IsVariableCutOff,
                ISNULL(MM.IsSpecialMachine, 0) AS IsSpecialMachine,
                ISNULL(MM.IsPlanningMachine, 0) AS IsPlanningMachine,
                ISNULL(MM.BasicPrintingCharges, 0) AS BasicPrintingCharges,
                ISNULL(MM.JobChangeOverTime, 0) AS JobChangeOverTime,
                ISNULL(MM.PlateLength, 0) AS PlateLength,
                ISNULL(MM.PlateWidth, 0) AS PlateWidth,
                ISNULL(MM.OtherCharges, 0) AS OtherCharges,
                ISNULL(MM.WastageType, '') AS WastageType,
                ISNULL(MM.WastageCalculationOn, '') AS WastageCalculationOn,
                ISNULL(MM.PerHourCost, 0) AS PerHourCost,
                ISNULL(MM.ElectricConsumptionUnitPerMinute, '') AS ElectricConsumptionUnitPerMinute,
                ISNULL(MM.MinRollWidth, 0) AS MinRollWidth,
                ISNULL(MM.MaxRollWidth, 0) AS MaxRollWidth,
                ISNULL(MM.MinCircumference, 0) AS MinCircumference,
                ISNULL(MM.MaxCircumference, 0) AS MaxCircumference,
                ISNULL(MM.MakeReadyWastageRunningMeter, 0) AS MakeReadyWastageRunningMeter,
                ISNULL(MM.AvgBreakDownTime, 0) AS AvgBreakDownTime,
                ISNULL(MM.AvgBreakDownRunningMeters, 0) AS AvgBreakDownRunningMeters,
                ISNULL(MM.MachineWidth, 0) AS MachineWidth,
                ISNULL(MM.AverageRollChangeWastage, 0) AS AverageRollChangeWastage,
                ISNULL(MM.AverageRollLength, 0) AS AverageRollLength,
                ISNULL(MM.RollChangeTime, 0) AS RollChangeTime,
                MM.BranchID,
                ISNULL(BM.BranchName, '') AS BranchName,
                ISNULL(MM.Speedunit, 'IMPRESSION') AS SpeedUnit,
                ISNULL(MM.PlateCharges, 0) AS PlateCharges,
                ISNULL(MM.PlateChargesType, '') AS PlateChargesType,
                ISNULL(MM.PerHourCostingParameter, '') AS PerHourCostingParameter,
                ISNULL(NULLIF(MM.MakeReadyTimeMode, ''), 'Flat') AS MakeReadyTimeMode,
                ISNULL(C.CompanyName, '') AS CompanyName,
                ISNULL(C.CompanyID, 0) AS CompanyID
            FROM MachineMaster AS MM
            INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID = MM.DepartmentID
            LEFT JOIN BranchMaster AS BM ON BM.BranchID = MM.BranchID
            INNER JOIN ProductionUnitMaster AS PUM ON PUM.ProductionUnitID = MM.ProductionUnitID
            INNER JOIN CompanyMaster AS C ON C.CompanyID = PUM.CompanyID
            WHERE ISNULL(MM.IsDeletedTransaction, 0) = 0
            ORDER BY MachineName";

        var result = await connection.QueryAsync<MachineListDto>(sql);
        return result.ToList();
    }

    public async Task<List<MachineSlabDto>> GetMachineSlabsAsync(int machineId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ISNULL(RunningMeterRangeFrom, 0) AS RunningMeterRangeFrom,
                ISNULL(RunningMeterRangeTo, 0) AS RunningMeterRangeTo,
                ISNULL(ProcessWastagepercentage, 0) AS ProcessWastagepercentage,
                ISNULL(SheetRangeFrom, 0) AS SheetRangeFrom,
                ISNULL(SheetRangeTo, 0) AS SheetRangeTo,
                ISNULL(MachineSpeed, 0) AS MachineSpeed,
                ISNULL(Rate, 0) AS Rate,
                ISNULL(PlateCharges, 0) AS PlateCharges,
                ISNULL(PSPlateCharges, 0) AS PSPlateCharges,
                ISNULL(CTCPPlateCharges, 0) AS CTCPPlateCharges,
                ISNULL(Wastage, 0) AS Wastage,
                ISNULL(SpecialColorFrontCharges, 0) AS SpecialColorFrontCharges,
                ISNULL(SpecialColorBackCharges, 0) AS SpecialColorBackCharges,
                ISNULL(PaperGroup, '-') AS PaperGroup,
                ISNULL(MaxPlanW, 0) AS SizeW,
                ISNULL(MaxPlanL, 0) AS SizeL,
                ISNULL(MinCharges, 0) AS MinCharges
            FROM MachineSlabMaster
            WHERE MachineID = @MachineID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryAsync<MachineSlabDto>(sql, new { MachineID = machineId });
        return result.ToList();
    }

    public async Task<List<MachineOnlineCoatingRateDto>> GetMachineOnlineCoatingRatesAsync(int machineId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(CoatingName, '') AS CoatingName,
                ISNULL(SheetRangeFrom, 0) AS SheetRangeFrom,
                ISNULL(SheetRangeTo, 0) AS SheetRangeTo,
                ISNULL(RateType, '') AS RateType,
                ISNULL(Rate, 0) AS Rate,
                ISNULL(BasicCoatingCharges, 0) AS BasicCoatingCharges
            FROM MachineOnlineCoatingRates
            WHERE MachineID = @MachineID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY CoatingName, RateType, SheetRangeFrom, SheetRangeTo";

        var result = await connection.QueryAsync<MachineOnlineCoatingRateDto>(sql, new { MachineID = machineId });
        return result.ToList();
    }

    public async Task<List<MachineDepartmentDto>> GetDepartmentsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ISNULL(DepartmentID, 0) AS DepartmentID,
                ISNULL(DepartmentName, '') AS DepartmentName
            FROM DepartmentMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY DepartmentName";

        var result = await connection.QueryAsync<MachineDepartmentDto>(sql);
        return result.ToList();
    }

    public async Task<List<MachineTypeDto>> GetMachineTypesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(MachineTypeName, '') AS MachineTypeName,
                ISNULL(MachineMasterDisplayFieldsName, '') AS MachineMasterDisplayFieldsName
            FROM MachineTypeMaster";

        var result = await connection.QueryAsync<MachineTypeDto>(sql);
        return result.ToList();
    }

    public async Task<List<MachineNameDto>> GetMachineNamesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ISNULL(MM.MachineID, 0) AS MachineID,
                ISNULL(MM.MachineName, '') AS MachineName
            FROM MachineMaster AS MM
            INNER JOIN DepartmentMaster AS DM ON MM.DepartmentID = DM.DepartmentID
            WHERE ISNULL(MM.IsDeletedTransaction, 0) = 0
            ORDER BY MachineName";

        var result = await connection.QueryAsync<MachineNameDto>(sql);
        return result.ToList();
    }

    public async Task<List<MachineGroupAllocationDto>> GetGroupGridAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(ItemSubGroupID, 0) AS ItemSubGroupID,
                ISNULL(ItemSubGroupName, '') AS ItemSubGroupName
            FROM ItemSubGroupMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY ItemSubGroupName";

        var result = await connection.QueryAsync<MachineGroupAllocationDto>(sql);
        return result.ToList();
    }

    public async Task<string> GetGroupAllocationIDsAsync(int machineId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT TOP(1)
                ISNULL(GroupAllocationIDs, '') AS GroupAllocationIDs
            FROM MachineItemSubGroupAllocationMaster
            WHERE MachineID = @MachineID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { MachineID = machineId });
        return result ?? "";
    }

    public async Task<List<CoatingNameDto>> GetCoatingNamesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ISNULL(CoatingName, '') AS CoatingName
            FROM MachineOnlineCoatingRates
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY CoatingName";

        var result = await connection.QueryAsync<CoatingNameDto>(sql);
        return result.ToList();
    }

    public async Task<List<MachineToolDto>> GetToolListAsync(int toolGroupId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ISNULL(ToolID, 0) AS ToolID,
                ISNULL(ToolName, '') AS ToolName,
                ISNULL(SizeW, 0) AS SizeW,
                ISNULL(NoOfTeeth, 0) AS NoOfTeeth,
                ISNULL(CircumferenceMM, 0) AS CircumferenceMM,
                ISNULL(CircumferenceInch, 0) AS CircumferenceInch,
                ISNULL(LPI, 0) AS LPI
            FROM ToolMaster
            WHERE ToolGroupID = @ToolGroupID
              AND ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY ToolName";

        var result = await connection.QueryAsync<MachineToolDto>(sql, new { ToolGroupID = toolGroupId });
        return result.ToList();
    }

    public async Task<string> GetAllocatedToolsAsync(int machineId, int toolGroupId)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT TOP(1)
                ISNULL(ToolAllocatedIDString, '') AS ToolAllocatedIDString
            FROM MachineToolAllocationMaster
            WHERE MachineID = @MachineID
              AND ToolGroupID = @ToolGroupID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { MachineID = machineId, ToolGroupID = toolGroupId });
        return result ?? "";
    }

    public async Task<string> GetMachineCodeAsync()
    {
        using var connection = GetConnection();

        var companyId = _currentUserService.GetCompanyId() ?? 0;

        // Get next machine number
        var sql = @"
            SELECT ISNULL(MAX(MaxMachineNo), 0) + 1
            FROM MachineMaster
            WHERE CompanyID = @CompanyID
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var maxNo = await connection.QueryFirstOrDefaultAsync<long>(sql, new { CompanyID = companyId });
        return $"MM{maxNo:D6}";
    }

    public async Task<string> CheckMachineNameExistsAsync(string machineName)
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT COUNT(1)
            FROM MachineMaster
            WHERE MachineName = @MachineName
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var count = await connection.QueryFirstOrDefaultAsync<int>(sql, new { MachineName = machineName });
        return count > 0 ? "Exist" : "";
    }

    public async Task<string> SaveMachineAsync(SaveMachineRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Duplicate name check
            var existSql = @"
                SELECT COUNT(1)
                FROM MachineMaster
                WHERE MachineName = @MachineName
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.MachineDetail.MachineName },
                transaction);

            if (exists > 0)
                return "Exist";

            // Generate machine code
            var maxNoSql = @"
                SELECT ISNULL(MAX(MaxMachineNo), 0) + 1
                FROM MachineMaster
                WHERE CompanyID = @CompanyID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var maxMachineNo = await connection.QueryFirstOrDefaultAsync<long>(
                maxNoSql, new { CompanyID = companyId }, transaction);
            var machineCode = $"MM{maxMachineNo:D6}";

            // Insert MachineMaster
            var insertSql = @"
                INSERT INTO MachineMaster
                    (MachineName, DepartmentID, MachineType, MinimumSheet, Gripper,
                     MaxLength, MaxWidth, MinLength, MinWidth,
                     MaxPrintL, MaxPrintW, MinPrintL, MinPrintW, Colors,
                     MakeReadyCharges, MakeReadyWastageSheet, MakeReadyTime, MakeReadyPerHourCost,
                     ElectricConsumption, PrintingMargin, WebCutOffSize, MinReelSize, MaxReelSize,
                     MachineSpeed, LabourCharges, WebCutOffSizeMin, ChargesType,
                     RoundofImpressionsWith, IsPerfectaMachine, IsVariableCutOff, IsSpecialMachine,
                     IsPlanningMachine, BasicPrintingCharges, JobChangeOverTime,
                     PlateLength, PlateWidth, OtherCharges, WastageType, WastageCalculationOn,
                     PerHourCost, ElectricConsumptionUnitPerMinute,
                     MinRollWidth, MaxRollWidth, MinCircumference, MaxCircumference,
                     MakeReadyWastageRunningMeter, AvgBreakDownTime, AvgBreakDownRunningMeters,
                     MachineWidth, AverageRollChangeWastage, AverageRollLength, RollChangeTime,
                     BranchID, SpeedUnit, PlateCharges, PlateChargesType,
                     PerHourCostingParameter, MakeReadyTimeMode, ProductionUnitID,
                     MaxMachineNo, MachineCode,
                     CompanyID, FYear, UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
                VALUES
                    (@MachineName, @DepartmentID, @MachineType, @MinimumSheet, @Gripper,
                     @MaxLength, @MaxWidth, @MinLength, @MinWidth,
                     @MaxPrintL, @MaxPrintW, @MinPrintL, @MinPrintW, @Colors,
                     @MakeReadyCharges, @MakeReadyWastageSheet, @MakeReadyTime, @MakeReadyPerHourCost,
                     @ElectricConsumption, @PrintingMargin, @WebCutOffSize, @MinReelSize, @MaxReelSize,
                     @MachineSpeed, @LabourCharges, @WebCutOffSizeMin, @ChargesType,
                     @RoundofImpressionsWith, @IsPerfectaMachine, @IsVariableCutOff, @IsSpecialMachine,
                     @IsPlanningMachine, @BasicPrintingCharges, @JobChangeOverTime,
                     @PlateLength, @PlateWidth, @OtherCharges, @WastageType, @WastageCalculationOn,
                     @PerHourCost, @ElectricConsumptionUnitPerMinute,
                     @MinRollWidth, @MaxRollWidth, @MinCircumference, @MaxCircumference,
                     @MakeReadyWastageRunningMeter, @AvgBreakDownTime, @AvgBreakDownRunningMeters,
                     @MachineWidth, @AverageRollChangeWastage, @AverageRollLength, @RollChangeTime,
                     @BranchID, @SpeedUnit, @PlateCharges, @PlateChargesType,
                     @PerHourCostingParameter, @MakeReadyTimeMode, @ProductionUnitID,
                     @MaxMachineNo, @MachineCode,
                     @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            var machineId = await connection.QueryFirstAsync<int>(insertSql, new
            {
                request.MachineDetail.MachineName,
                request.MachineDetail.DepartmentID,
                request.MachineDetail.MachineType,
                request.MachineDetail.MinimumSheet,
                request.MachineDetail.Gripper,
                request.MachineDetail.MaxLength,
                request.MachineDetail.MaxWidth,
                request.MachineDetail.MinLength,
                request.MachineDetail.MinWidth,
                request.MachineDetail.MaxPrintL,
                request.MachineDetail.MaxPrintW,
                request.MachineDetail.MinPrintL,
                request.MachineDetail.MinPrintW,
                request.MachineDetail.Colors,
                request.MachineDetail.MakeReadyCharges,
                request.MachineDetail.MakeReadyWastageSheet,
                request.MachineDetail.MakeReadyTime,
                request.MachineDetail.MakeReadyPerHourCost,
                request.MachineDetail.ElectricConsumption,
                request.MachineDetail.PrintingMargin,
                request.MachineDetail.WebCutOffSize,
                request.MachineDetail.MinReelSize,
                request.MachineDetail.MaxReelSize,
                request.MachineDetail.MachineSpeed,
                request.MachineDetail.LabourCharges,
                request.MachineDetail.WebCutOffSizeMin,
                request.MachineDetail.ChargesType,
                request.MachineDetail.RoundofImpressionsWith,
                request.MachineDetail.IsPerfectaMachine,
                request.MachineDetail.IsVariableCutOff,
                request.MachineDetail.IsSpecialMachine,
                request.MachineDetail.IsPlanningMachine,
                request.MachineDetail.BasicPrintingCharges,
                request.MachineDetail.JobChangeOverTime,
                request.MachineDetail.PlateLength,
                request.MachineDetail.PlateWidth,
                request.MachineDetail.OtherCharges,
                request.MachineDetail.WastageType,
                request.MachineDetail.WastageCalculationOn,
                request.MachineDetail.PerHourCost,
                request.MachineDetail.ElectricConsumptionUnitPerMinute,
                request.MachineDetail.MinRollWidth,
                request.MachineDetail.MaxRollWidth,
                request.MachineDetail.MinCircumference,
                request.MachineDetail.MaxCircumference,
                request.MachineDetail.MakeReadyWastageRunningMeter,
                request.MachineDetail.AvgBreakDownTime,
                request.MachineDetail.AvgBreakDownRunningMeters,
                request.MachineDetail.MachineWidth,
                request.MachineDetail.AverageRollChangeWastage,
                request.MachineDetail.AverageRollLength,
                request.MachineDetail.RollChangeTime,
                request.MachineDetail.BranchID,
                request.MachineDetail.SpeedUnit,
                request.MachineDetail.PlateCharges,
                request.MachineDetail.PlateChargesType,
                request.MachineDetail.PerHourCostingParameter,
                request.MachineDetail.MakeReadyTimeMode,
                request.MachineDetail.ProductionUnitID,
                MaxMachineNo = maxMachineNo,
                MachineCode = machineCode,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId,
                CreatedBy = userId,
                ModifiedBy = userId
            }, transaction);

            // Insert slabs
            await InsertSlabsAsync(connection, transaction, machineId, request.Slabs, companyId, userId, fYear);

            // Insert coating rates
            await InsertCoatingRatesAsync(connection, transaction, machineId, request.CoatingRates, companyId);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving machine");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateMachineAsync(UpdateMachineRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            var updateSql = @"
                UPDATE MachineMaster
                SET MachineName = @MachineName,
                    DepartmentID = @DepartmentID,
                    MachineType = @MachineType,
                    MinimumSheet = @MinimumSheet,
                    Gripper = @Gripper,
                    MaxLength = @MaxLength,
                    MaxWidth = @MaxWidth,
                    MinLength = @MinLength,
                    MinWidth = @MinWidth,
                    MaxPrintL = @MaxPrintL,
                    MaxPrintW = @MaxPrintW,
                    MinPrintL = @MinPrintL,
                    MinPrintW = @MinPrintW,
                    Colors = @Colors,
                    MakeReadyCharges = @MakeReadyCharges,
                    MakeReadyWastageSheet = @MakeReadyWastageSheet,
                    MakeReadyTime = @MakeReadyTime,
                    MakeReadyPerHourCost = @MakeReadyPerHourCost,
                    ElectricConsumption = @ElectricConsumption,
                    PrintingMargin = @PrintingMargin,
                    WebCutOffSize = @WebCutOffSize,
                    MinReelSize = @MinReelSize,
                    MaxReelSize = @MaxReelSize,
                    MachineSpeed = @MachineSpeed,
                    LabourCharges = @LabourCharges,
                    WebCutOffSizeMin = @WebCutOffSizeMin,
                    ChargesType = @ChargesType,
                    RoundofImpressionsWith = @RoundofImpressionsWith,
                    IsPerfectaMachine = @IsPerfectaMachine,
                    IsVariableCutOff = @IsVariableCutOff,
                    IsSpecialMachine = @IsSpecialMachine,
                    IsPlanningMachine = @IsPlanningMachine,
                    BasicPrintingCharges = @BasicPrintingCharges,
                    JobChangeOverTime = @JobChangeOverTime,
                    PlateLength = @PlateLength,
                    PlateWidth = @PlateWidth,
                    OtherCharges = @OtherCharges,
                    WastageType = @WastageType,
                    WastageCalculationOn = @WastageCalculationOn,
                    PerHourCost = @PerHourCost,
                    ElectricConsumptionUnitPerMinute = @ElectricConsumptionUnitPerMinute,
                    MinRollWidth = @MinRollWidth,
                    MaxRollWidth = @MaxRollWidth,
                    MinCircumference = @MinCircumference,
                    MaxCircumference = @MaxCircumference,
                    MakeReadyWastageRunningMeter = @MakeReadyWastageRunningMeter,
                    AvgBreakDownTime = @AvgBreakDownTime,
                    AvgBreakDownRunningMeters = @AvgBreakDownRunningMeters,
                    MachineWidth = @MachineWidth,
                    AverageRollChangeWastage = @AverageRollChangeWastage,
                    AverageRollLength = @AverageRollLength,
                    RollChangeTime = @RollChangeTime,
                    BranchID = @BranchID,
                    SpeedUnit = @SpeedUnit,
                    PlateCharges = @PlateCharges,
                    PlateChargesType = @PlateChargesType,
                    PerHourCostingParameter = @PerHourCostingParameter,
                    MakeReadyTimeMode = @MakeReadyTimeMode,
                    ProductionUnitID = @ProductionUnitID,
                    UserID = @UserID,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = GETDATE()
                WHERE MachineID = @MachineID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            await connection.ExecuteAsync(updateSql, new
            {
                request.MachineDetail.MachineName,
                request.MachineDetail.DepartmentID,
                request.MachineDetail.MachineType,
                request.MachineDetail.MinimumSheet,
                request.MachineDetail.Gripper,
                request.MachineDetail.MaxLength,
                request.MachineDetail.MaxWidth,
                request.MachineDetail.MinLength,
                request.MachineDetail.MinWidth,
                request.MachineDetail.MaxPrintL,
                request.MachineDetail.MaxPrintW,
                request.MachineDetail.MinPrintL,
                request.MachineDetail.MinPrintW,
                request.MachineDetail.Colors,
                request.MachineDetail.MakeReadyCharges,
                request.MachineDetail.MakeReadyWastageSheet,
                request.MachineDetail.MakeReadyTime,
                request.MachineDetail.MakeReadyPerHourCost,
                request.MachineDetail.ElectricConsumption,
                request.MachineDetail.PrintingMargin,
                request.MachineDetail.WebCutOffSize,
                request.MachineDetail.MinReelSize,
                request.MachineDetail.MaxReelSize,
                request.MachineDetail.MachineSpeed,
                request.MachineDetail.LabourCharges,
                request.MachineDetail.WebCutOffSizeMin,
                request.MachineDetail.ChargesType,
                request.MachineDetail.RoundofImpressionsWith,
                request.MachineDetail.IsPerfectaMachine,
                request.MachineDetail.IsVariableCutOff,
                request.MachineDetail.IsSpecialMachine,
                request.MachineDetail.IsPlanningMachine,
                request.MachineDetail.BasicPrintingCharges,
                request.MachineDetail.JobChangeOverTime,
                request.MachineDetail.PlateLength,
                request.MachineDetail.PlateWidth,
                request.MachineDetail.OtherCharges,
                request.MachineDetail.WastageType,
                request.MachineDetail.WastageCalculationOn,
                request.MachineDetail.PerHourCost,
                request.MachineDetail.ElectricConsumptionUnitPerMinute,
                request.MachineDetail.MinRollWidth,
                request.MachineDetail.MaxRollWidth,
                request.MachineDetail.MinCircumference,
                request.MachineDetail.MaxCircumference,
                request.MachineDetail.MakeReadyWastageRunningMeter,
                request.MachineDetail.AvgBreakDownTime,
                request.MachineDetail.AvgBreakDownRunningMeters,
                request.MachineDetail.MachineWidth,
                request.MachineDetail.AverageRollChangeWastage,
                request.MachineDetail.AverageRollLength,
                request.MachineDetail.RollChangeTime,
                request.MachineDetail.BranchID,
                request.MachineDetail.SpeedUnit,
                request.MachineDetail.PlateCharges,
                request.MachineDetail.PlateChargesType,
                request.MachineDetail.PerHourCostingParameter,
                request.MachineDetail.MakeReadyTimeMode,
                request.MachineDetail.ProductionUnitID,
                UserID = userId,
                ModifiedBy = userId,
                request.MachineID
            }, transaction);

            // Delete and re-insert slabs
            await connection.ExecuteAsync(
                "DELETE FROM MachineSlabMaster WHERE MachineID = @MachineID",
                new { request.MachineID }, transaction);

            await InsertSlabsAsync(connection, transaction, request.MachineID, request.Slabs, companyId, userId, fYear);

            // Delete and re-insert coating rates
            await connection.ExecuteAsync(
                "DELETE FROM MachineOnlineCoatingRates WHERE MachineID = @MachineID",
                new { request.MachineID }, transaction);

            await InsertCoatingRatesAsync(connection, transaction, request.MachineID, request.CoatingRates, companyId);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating machine {MachineID}", request.MachineID);
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> DeleteMachineAsync(int machineId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            // Check if machine is used in job bookings
            var checkSql = @"
                SELECT COUNT(1)
                FROM JobBookingContents
                WHERE MachineID = @MachineID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            try
            {
                var usageCount = await connection.QueryFirstOrDefaultAsync<int>(
                    checkSql, new { MachineID = machineId }, transaction);

                if (usageCount > 0)
                {
                    await transaction.RollbackAsync();
                    return "Further Used, Can't Delete Machine..!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JobBookingContents table may not exist, skipping usage check");
            }

            // Soft-delete MachineMaster
            var deleteMachineSql = @"
                UPDATE MachineMaster
                SET DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE MachineID = @MachineID";

            await connection.ExecuteAsync(deleteMachineSql, new
            {
                DeletedBy = userId,
                MachineID = machineId
            }, transaction);

            // Soft-delete MachineSlabMaster
            var deleteSlabSql = @"
                UPDATE MachineSlabMaster
                SET DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE MachineID = @MachineID";

            await connection.ExecuteAsync(deleteSlabSql, new
            {
                DeletedBy = userId,
                MachineID = machineId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting machine {MachineID}", machineId);
            return "fail";
        }
    }

    public async Task<string> SaveGroupAllocationAsync(SaveMachineGroupAllocationRequest request)
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
            await connection.ExecuteAsync(
                "DELETE FROM MachineItemSubGroupAllocationMaster WHERE MachineID = @MachineID AND ISNULL(IsDeletedTransaction, 0) = 0",
                new { request.MachineID }, transaction);

            // Insert new allocations if any
            if (request.GroupAllocations != null && request.GroupAllocations.Count > 0)
            {
                var insertSql = @"
                    INSERT INTO MachineItemSubGroupAllocationMaster
                        (MachineID, ItemSubGroupID, GroupAllocationIDs,
                         CompanyID, FYear, UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
                    VALUES
                        (@MachineID, @ItemSubGroupID, @GroupAllocationIDString,
                         @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE())";

                foreach (var group in request.GroupAllocations)
                {
                    await connection.ExecuteAsync(insertSql, new
                    {
                        request.MachineID,
                        group.ItemSubGroupID,
                        request.GroupAllocationIDString,
                        CompanyID = companyId,
                        FYear = fYear,
                        UserID = userId,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    }, transaction);
                }
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving group allocation for machine {MachineID}", request.MachineID);
            return "fail";
        }
    }

    public async Task<string> DeleteGroupAllocationAsync(int machineId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var sql = @"
                UPDATE MachineItemSubGroupAllocationMaster
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE MachineID = @MachineID";

            await connection.ExecuteAsync(sql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                MachineID = machineId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting group allocation for machine {MachineID}", machineId);
            return "fail";
        }
    }

    public async Task<string> SaveToolAllocationAsync(SaveMachineToolAllocationRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Delete existing allocations for this machine + tool group
            await connection.ExecuteAsync(
                "DELETE FROM MachineToolAllocationMaster WHERE MachineID = @MachineID AND ToolGroupID = @ToolGroupID AND ISNULL(IsDeletedTransaction, 0) = 0",
                new { request.MachineID, request.ToolGroupID }, transaction);

            // Insert new allocations if any
            if (request.ToolAllocations != null && request.ToolAllocations.Count > 0)
            {
                var insertSql = @"
                    INSERT INTO MachineToolAllocationMaster
                        (MachineID, ToolGroupID, ToolID, ToolAllocatedIDString,
                         CompanyID, FYear, UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
                    VALUES
                        (@MachineID, @ToolGroupID, @ToolID, @ToolAllocatedIDString,
                         @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE())";

                foreach (var tool in request.ToolAllocations)
                {
                    await connection.ExecuteAsync(insertSql, new
                    {
                        request.MachineID,
                        request.ToolGroupID,
                        tool.ToolID,
                        request.ToolAllocatedIDString,
                        CompanyID = companyId,
                        FYear = fYear,
                        UserID = userId,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    }, transaction);
                }
            }

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving tool allocation for machine {MachineID}", request.MachineID);
            return "fail";
        }
    }

    public async Task<string> DeleteToolAllocationAsync(int machineId, int toolGroupId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var sql = @"
                UPDATE MachineToolAllocationMaster
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE MachineID = @MachineID
                  AND ToolGroupID = @ToolGroupID";

            await connection.ExecuteAsync(sql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                MachineID = machineId,
                ToolGroupID = toolGroupId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting tool allocation for machine {MachineID}, toolGroup {ToolGroupID}", machineId, toolGroupId);
            return "fail";
        }
    }

    // ==================== Private Helper Methods ====================

    private async Task InsertSlabsAsync(SqlConnection connection, SqlTransaction transaction,
        int machineId, List<MachineSlabRecord> slabs, int companyId, int userId, string fYear)
    {
        if (slabs == null || slabs.Count == 0) return;

        var sql = @"
            INSERT INTO MachineSlabMaster
                (MachineID, RunningMeterRangeFrom, RunningMeterRangeTo, ProcessWastagepercentage,
                 SheetRangeFrom, SheetRangeTo, MachineSpeed, Rate,
                 PlateCharges, PSPlateCharges, CTCPPlateCharges, Wastage,
                 SpecialColorFrontCharges, SpecialColorBackCharges, PaperGroup,
                 MaxPlanW, MaxPlanL, MinCharges,
                 CompanyID, FYear, UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
            VALUES
                (@MachineID, @RunningMeterRangeFrom, @RunningMeterRangeTo, @ProcessWastagepercentage,
                 @SheetRangeFrom, @SheetRangeTo, @MachineSpeed, @Rate,
                 @PlateCharges, @PSPlateCharges, @CTCPPlateCharges, @Wastage,
                 @SpecialColorFrontCharges, @SpecialColorBackCharges, @PaperGroup,
                 @SizeW, @SizeL, @MinCharges,
                 @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE())";

        foreach (var slab in slabs)
        {
            await connection.ExecuteAsync(sql, new
            {
                MachineID = machineId,
                slab.RunningMeterRangeFrom,
                slab.RunningMeterRangeTo,
                slab.ProcessWastagepercentage,
                slab.SheetRangeFrom,
                slab.SheetRangeTo,
                slab.MachineSpeed,
                slab.Rate,
                slab.PlateCharges,
                slab.PSPlateCharges,
                slab.CTCPPlateCharges,
                slab.Wastage,
                slab.SpecialColorFrontCharges,
                slab.SpecialColorBackCharges,
                slab.PaperGroup,
                slab.SizeW,
                slab.SizeL,
                slab.MinCharges,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId,
                CreatedBy = userId,
                ModifiedBy = userId
            }, transaction);
        }
    }

    private async Task InsertCoatingRatesAsync(SqlConnection connection, SqlTransaction transaction,
        int machineId, List<MachineCoatingRateRecord> coatingRates, int companyId)
    {
        if (coatingRates == null || coatingRates.Count == 0) return;

        var sql = @"
            INSERT INTO MachineOnlineCoatingRates
                (MachineID, CoatingName, SheetRangeFrom, SheetRangeTo, RateType, Rate, BasicCoatingCharges, CompanyID)
            VALUES
                (@MachineID, @CoatingName, @SheetRangeFrom, @SheetRangeTo, @RateType, @Rate, @BasicCoatingCharges, @CompanyID)";

        foreach (var rate in coatingRates)
        {
            await connection.ExecuteAsync(sql, new
            {
                MachineID = machineId,
                rate.CoatingName,
                rate.SheetRangeFrom,
                rate.SheetRangeTo,
                rate.RateType,
                rate.Rate,
                rate.BasicCoatingCharges,
                CompanyID = companyId
            }, transaction);
        }
    }
}
