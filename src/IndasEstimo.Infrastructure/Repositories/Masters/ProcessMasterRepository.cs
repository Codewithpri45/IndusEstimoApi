using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ProcessMasterRepository : IProcessMasterRepository
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProcessMasterRepository> _logger;

    public ProcessMasterRepository(
        ITenantProvider tenantProvider,
        IDbConnectionFactory connectionFactory,
        ICurrentUserService currentUserService,
        ILogger<ProcessMasterRepository> logger)
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

    public async Task<List<ProcessListDto>> GetProcessListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                P.ProcessID,
                ISNULL(P.DisplayProcessName, '') AS DisplayProcessName,
                ISNULL(P.ProcessName, '') AS ProcessName,
                ISNULL(P.TypeofCharges, '') AS TypeofCharges,
                ISNULL(P.SizeToBeConsidered, '') AS SizeToBeConsidered,
                ISNULL(P.MinimumCharges, 0) AS MinimumCharges,
                ISNULL(P.SetupCharges, 0) AS SetupCharges,
                ISNULL(P.IsDisplay, 0) AS IsDisplay,
                ISNULL(P.ToolRequired, 0) AS ToolRequired,
                ISNULL(D.DepartmentName, '') AS DepartmentName,
                ISNULL(P.StartUnit, '') AS StartUnit,
                ISNULL(P.EndUnit, '') AS EndUnit,
                ISNULL(P.UnitConversion, '') AS UnitConversion,
                ISNULL(P.PrePress, '') AS PrePress,
                ISNULL(P.ProcessModuleType, '') AS ProcessModuleType,
                ISNULL(P.IsOnlineProcess, 0) AS IsOnlineProcess,
                ISNULL(P.ProcessCategory, '') AS ProcessCategory
            FROM ProcessMaster P
            LEFT JOIN DepartmentMaster D ON P.DepartmentID = D.DepartmentID
            WHERE ISNULL(P.IsDeletedTransaction, 0) = 0
            ORDER BY P.ProcessName";

        var result = await connection.QueryAsync<ProcessListDto>(sql);
        return result.ToList();
    }

    public async Task<List<ProcessNameDto>> GetProcessNamesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT ProcessID, ISNULL(ProcessName, '') AS ProcessName
            FROM ProcessMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY ProcessName";

        var result = await connection.QueryAsync<ProcessNameDto>(sql);
        return result.ToList();
    }

    public async Task<ProcessLoadedDataDto> GetProcessByIdAsync(int processId)
    {
        using var connection = GetConnection();

        var loadedData = new ProcessLoadedDataDto();

        // Get process detail
        var detailSql = @"
            SELECT
                P.ProcessID,
                ISNULL(P.ProcessName, '') AS ProcessName,
                ISNULL(P.DisplayProcessName, '') AS DisplayProcessName,
                ISNULL(P.DepartmentID, 0) AS DepartmentID,
                ISNULL(D.DepartmentName, '') AS DepartmentName,
                ISNULL(P.TypeofCharges, '') AS TypeofCharges,
                ISNULL(P.ChargeApplyOnSheets, '') AS ChargeApplyOnSheets,
                ISNULL(P.SizeToBeConsidered, '') AS SizeToBeConsidered,
                ISNULL(P.PrePress, '') AS PrePress,
                ISNULL(P.StartUnit, '') AS StartUnit,
                ISNULL(P.EndUnit, '') AS EndUnit,
                ISNULL(P.UnitConversion, '') AS UnitConversion,
                ISNULL(P.MinimumCharges, 0) AS MinimumCharges,
                ISNULL(P.SetupCharges, 0) AS SetupCharges,
                ISNULL(P.IsDisplay, 0) AS IsDisplay,
                ISNULL(P.IsEditToBeProduceQty, 0) AS IsEditToBeProduceQty,
                ISNULL(P.Rate, 0) AS Rate,
                ISNULL(P.ProcessProductionType, '') AS ProcessProductionType,
                ISNULL(P.ProcessPurpose, '') AS ProcessPurpose,
                ISNULL(P.IsOnlineProcess, 0) AS IsOnlineProcess,
                ISNULL(P.ProcessModuleType, '') AS ProcessModuleType,
                ISNULL(P.MinimumQuantityToBeCharged, 0) AS MinimumQuantityToBeCharged,
                ISNULL(P.ProcessFlatWastageValue, 0) AS ProcessFlatWastageValue,
                ISNULL(P.ProcessWastagePercentage, 0) AS ProcessWastagePercentage,
                ISNULL(P.ProcessCategory, '') AS ProcessCategory,
                ISNULL(P.PerHourCostingParameter, '') AS PerHourCostingParameter,
                ISNULL(P.ToolRequired, 0) AS ToolRequired
            FROM ProcessMaster P
            LEFT JOIN DepartmentMaster D ON P.DepartmentID = D.DepartmentID
            WHERE P.ProcessID = @ProcessID
              AND ISNULL(P.IsDeletedTransaction, 0) = 0";

        var detail = await connection.QueryFirstOrDefaultAsync<ProcessDetailDto>(detailSql, new { ProcessID = processId });
        if (detail != null)
            loadedData.ProcessDetail = detail;

        // Get allocated machines from AllocattedMachineID (comma-separated) in ProcessMaster
        try
        {
            var allocIdSql = @"SELECT ISNULL(AllocattedMachineID, '') FROM ProcessMaster WHERE ProcessID = @ProcessID AND ISNULL(IsDeletedTransaction, 0) = 0";
            var allocIds = await connection.QueryFirstOrDefaultAsync<string>(allocIdSql, new { ProcessID = processId });
            if (!string.IsNullOrWhiteSpace(allocIds))
            {
                var machineIds = allocIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim()).Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList();
                if (machineIds.Count > 0)
                {
                    var machinesSql = @"
                        SELECT M.MachineID, ISNULL(M.MachineName, '') AS MachineName,
                            ISNULL(M.DepartmentID, 0) AS DepartmentID, ISNULL(D.DepartmentName, '') AS DepartmentName,
                            0 AS MachineSpeed, 0 AS MakeReadyTime, 0 AS JobChangeOverTime, 0 AS IsDefaultMachine
                        FROM MachineMaster M
                        LEFT JOIN DepartmentMaster D ON M.DepartmentID = D.DepartmentID
                        WHERE M.MachineID IN @MachineIds";
                    loadedData.MachineAllocations = (await connection.QueryAsync<ProcessMachineAllocationDto>(machinesSql, new { MachineIds = machineIds })).ToList();
                }
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Error loading allocated machines"); }

        // Get allocated materials
        try
        {
            var materialsSql = @"
                SELECT
                    MAL.ItemID,
                    ISNULL(IM.ItemCode, '') AS ItemCode,
                    ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                    ISNULL(ISG.ItemSubGroupDisplayName, '') AS ItemSubGroupName,
                    ISNULL(IM.ItemName, '') AS ItemName,
                    ISNULL(IM.StockUnit, '') AS StockUnit
                FROM ProcessAllocatedMaterialMaster MAL
                INNER JOIN ItemMaster IM ON MAL.ItemID = IM.ItemID
                LEFT JOIN ItemGroupMaster IG ON IM.ItemGroupID = IG.ItemGroupID
                LEFT JOIN ItemSubGroupMaster ISG ON IM.ItemSubGroupID = ISG.ItemSubGroupUniqueID
                WHERE MAL.ProcessID = @ProcessID";
            loadedData.MaterialAllocations = (await connection.QueryAsync<ProcessMaterialAllocationDto>(materialsSql, new { ProcessID = processId })).ToList();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ProcessAllocatedMaterialMaster table may not exist"); }

        // Get slabs
        try
        {
            loadedData.Slabs = await GetExistingSlabsAsync(processId);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ProcessMasterSlabs table may not exist"); }

        // Get tool groups
        try
        {
            var toolGroupsSql = @"
                SELECT
                    TGA.ToolGroupID,
                    ISNULL(TG.ToolGroupName, '') AS ToolGroupName
                FROM ProcessToolGroupAllocationMaster TGA
                INNER JOIN ToolGroupMaster TG ON TGA.ToolGroupID = TG.ToolGroupID
                WHERE TGA.ProcessID = @ProcessID";
            loadedData.ToolGroups = (await connection.QueryAsync<ProcessToolGroupDto>(toolGroupsSql, new { ProcessID = processId })).ToList();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ProcessToolGroupAllocationMaster table may not exist"); }

        // Get inspection parameters
        try
        {
            loadedData.InspectionParameters = await GetInspectionParametersAsync(processId);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ProcessInspectionParameter table may not exist"); }

        // Get line clearance parameters
        try
        {
            loadedData.LineClearanceParameters = await GetLineClearanceParametersAsync(processId);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ProcessLineClearanceParameter table may not exist"); }

        // Get content allocations from AllocatedContentID (comma-separated) in ProcessMaster
        try
        {
            var contentIdSql = @"SELECT ISNULL(AllocatedContentID, '') FROM ProcessMaster WHERE ProcessID = @ProcessID AND ISNULL(IsDeletedTransaction, 0) = 0";
            var contentIds = await connection.QueryFirstOrDefaultAsync<string>(contentIdSql, new { ProcessID = processId });
            if (!string.IsNullOrWhiteSpace(contentIds))
            {
                var cIds = contentIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim()).Where(id => int.TryParse(id, out _)).Select(int.Parse).ToList();
                if (cIds.Count > 0)
                {
                    var contentSql = @"
                        SELECT C.ContentID, ISNULL(C.ContentName, '') AS ContentName, ISNULL(C.ContentCaption, '') AS ContentCaption
                        FROM ContentMaster C WHERE C.ContentID IN @ContentIds";
                    loadedData.ContentAllocations = (await connection.QueryAsync<ProcessContentAllocationDto>(contentSql, new { ContentIds = cIds })).ToList();
                }
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Error loading content allocations"); }

        return loadedData;
    }

    public async Task<List<ProcessDepartmentDto>> GetDepartmentsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DepartmentID, ISNULL(DepartmentName, '') AS DepartmentName
            FROM DepartmentMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
            ORDER BY DepartmentName";

        var result = await connection.QueryAsync<ProcessDepartmentDto>(sql);
        return result.ToList();
    }

    public async Task<List<TypeOfChargesDto>> GetTypeOfChargesAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT DISTINCT
                ROW_NUMBER() OVER (ORDER BY TypeofCharges) AS TypeOfChargesID,
                ISNULL(TypeofCharges, '') AS TypeOfChargesName
            FROM ProcessMaster
            WHERE ISNULL(IsDeletedTransaction, 0) = 0
              AND TypeofCharges IS NOT NULL
              AND TypeofCharges <> ''
            ORDER BY TypeOfChargesName";

        var result = await connection.QueryAsync<TypeOfChargesDto>(sql);
        return result.ToList();
    }

    public async Task<List<UnitDto>> GetUnitsAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT ISNULL(UnitName, '') AS UnitName, ISNULL(UnitSymbol, '') AS UnitSymbol
            FROM UnitMaster
            ORDER BY UnitName";

        var result = await connection.QueryAsync<UnitDto>(sql);
        return result.ToList();
    }

    public async Task<List<ProcessToolGroupDto>> GetToolGroupListAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT ToolGroupID, ISNULL(ToolGroupName, '') AS ToolGroupName
            FROM ToolGroupMaster
            ORDER BY ToolGroupName";

        var result = await connection.QueryAsync<ProcessToolGroupDto>(sql);
        return result.ToList();
    }

    public async Task<object> GetMachineGridAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                M.MachineID,
                ISNULL(M.MachineName, '') AS MachineName,
                ISNULL(M.DepartmentID, 0) AS DepartmentID,
                ISNULL(D.DepartmentName, '') AS DepartmentName
            FROM MachineMaster M
            LEFT JOIN DepartmentMaster D ON M.DepartmentID = D.DepartmentID
            ORDER BY M.MachineName";

        var result = await connection.QueryAsync(sql);
        return result.ToList();
    }

    public async Task<object> GetItemGridAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                IM.ItemID,
                ISNULL(IM.ItemCode, '') AS ItemCode,
                ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                ISNULL(ISG.ItemSubGroupDisplayName, '') AS ItemSubGroupName,
                ISNULL(IM.ItemName, '') AS ItemName,
                ISNULL(IM.StockUnit, '') AS StockUnit
            FROM ItemMaster IM
            LEFT JOIN ItemGroupMaster IG ON IM.ItemGroupID = IG.ItemGroupID
            LEFT JOIN ItemSubGroupMaster ISG ON IM.ItemSubGroupID = ISG.ItemSubGroupUniqueID
            WHERE ISNULL(IM.IsDeletedTransaction, 0) = 0
            ORDER BY IM.ItemName";

        var result = await connection.QueryAsync(sql);
        return result.ToList();
    }

    public async Task<object> GetContentGridAsync()
    {
        using var connection = GetConnection();

        var sql = @"
            SELECT
                ContentID,
                ISNULL(ContentName, '') AS ContentName,
                ISNULL(ContentCaption, '') AS ContentCaption
            FROM ContentMaster
            ORDER BY ContentName";

        var result = await connection.QueryAsync(sql);
        return result.ToList();
    }

    public async Task<List<ProcessSlabDto>> GetExistingSlabsAsync(int processId)
    {
        try
        {
            using var connection = GetConnection();

            var sql = @"
                SELECT
                    ISNULL(FromQty, 0) AS FromQty,
                    ISNULL(ToQty, 0) AS ToQty,
                    ISNULL(StartUnit, '') AS StartUnit,
                    ISNULL(RateFactor, '') AS RateFactor,
                    ISNULL(Rate, 0) AS Rate,
                    ISNULL(MinimumCharges, 0) AS MinimumCharges,
                    ISNULL(IsLocked, 0) AS IsLocked
                FROM ProcessMasterSlabs
                WHERE ProcessID = @ProcessID
                ORDER BY FromQty";

            var result = await connection.QueryAsync<ProcessSlabDto>(sql, new { ProcessID = processId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessMasterSlabs table may not exist, returning empty list");
            return new List<ProcessSlabDto>();
        }
    }

    public async Task<List<ProcessMachineAllocationDto>> GetAllocatedMachinesAsync(int processId)
    {
        try
        {
            using var connection = GetConnection();

            // AllocattedMachineID in ProcessMaster stores comma-separated MachineIDs
            var sql = @"
                SELECT ISNULL(AllocattedMachineID, '') AS AllocattedMachineID
                FROM ProcessMaster
                WHERE ProcessID = @ProcessID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var allocatedIds = await connection.QueryFirstOrDefaultAsync<string>(sql, new { ProcessID = processId });
            if (string.IsNullOrWhiteSpace(allocatedIds))
                return new List<ProcessMachineAllocationDto>();

            var machineIds = allocatedIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            if (machineIds.Count == 0)
                return new List<ProcessMachineAllocationDto>();

            var machineSql = @"
                SELECT
                    M.MachineID,
                    ISNULL(M.MachineName, '') AS MachineName,
                    ISNULL(M.DepartmentID, 0) AS DepartmentID,
                    ISNULL(D.DepartmentName, '') AS DepartmentName,
                    0 AS MachineSpeed,
                    0 AS MakeReadyTime,
                    0 AS JobChangeOverTime,
                    0 AS IsDefaultMachine
                FROM MachineMaster M
                LEFT JOIN DepartmentMaster D ON M.DepartmentID = D.DepartmentID
                WHERE M.MachineID IN @MachineIds";

            var result = await connection.QueryAsync<ProcessMachineAllocationDto>(machineSql, new { MachineIds = machineIds });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting allocated machines for process, returning empty list");
            return new List<ProcessMachineAllocationDto>();
        }
    }

    public async Task<List<ProcessMaterialAllocationDto>> GetAllocatedMaterialsAsync(int processId)
    {
        try
        {
            using var connection = GetConnection();

            var sql = @"
                SELECT
                    MAL.ItemID,
                    ISNULL(IM.ItemCode, '') AS ItemCode,
                    ISNULL(IG.ItemGroupName, '') AS ItemGroupName,
                    ISNULL(ISG.ItemSubGroupDisplayName, '') AS ItemSubGroupName,
                    ISNULL(IM.ItemName, '') AS ItemName,
                    ISNULL(IM.StockUnit, '') AS StockUnit
                FROM ProcessAllocatedMaterialMaster MAL
                INNER JOIN ItemMaster IM ON MAL.ItemID = IM.ItemID
                LEFT JOIN ItemGroupMaster IG ON IM.ItemGroupID = IG.ItemGroupID
                LEFT JOIN ItemSubGroupMaster ISG ON IM.ItemSubGroupID = ISG.ItemSubGroupUniqueID
                WHERE MAL.ProcessID = @ProcessID";

            var result = await connection.QueryAsync<ProcessMaterialAllocationDto>(sql, new { ProcessID = processId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessAllocatedMaterialMaster table may not exist, returning empty list");
            return new List<ProcessMaterialAllocationDto>();
        }
    }

    public async Task<List<ProcessInspectionParameterDto>> GetInspectionParametersAsync(int processId)
    {
        try
        {
            using var connection = GetConnection();

            var sql = @"
                SELECT
                    ISNULL(ProcessInspectionParameterID, 0) AS ProcessInspectionParameterID,
                    ISNULL(ParameterName, '') AS ParameterName,
                    ISNULL(StandardValue, '') AS StandardValue,
                    ISNULL(InputFieldType, '') AS InputFieldType,
                    ISNULL(FieldDataType, '') AS FieldDataType,
                    ISNULL(DefaultValue, '') AS DefaultValue
                FROM ProcessInspectionParameterMaster
                WHERE ProcessID = @ProcessID
                ORDER BY SequenceNo";

            var result = await connection.QueryAsync<ProcessInspectionParameterDto>(sql, new { ProcessID = processId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessInspectionParameter table may not exist, returning empty list");
            return new List<ProcessInspectionParameterDto>();
        }
    }

    public async Task<List<ProcessLineClearanceParameterDto>> GetLineClearanceParametersAsync(int processId)
    {
        try
        {
            using var connection = GetConnection();

            var sql = @"
                SELECT
                    ISNULL(LineClearanceParameterID, 0) AS LineClearanceParameterID,
                    ISNULL(ParameterName, '') AS ParameterName,
                    ISNULL(StandardValue, '') AS StandardValue,
                    ISNULL(InputFieldType, '') AS InputFieldType,
                    ISNULL(FieldDataType, '') AS FieldDataType,
                    ISNULL(DefaultValue, '') AS DefaultValue
                FROM ProcessLineClearanceParameters
                WHERE ProcessID = @ProcessID
                ORDER BY SequenceNo";

            var result = await connection.QueryAsync<ProcessLineClearanceParameterDto>(sql, new { ProcessID = processId });
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessLineClearanceParameter table may not exist, returning empty list");
            return new List<ProcessLineClearanceParameterDto>();
        }
    }

    public async Task<string> SaveProcessAsync(SaveProcessRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var fYear = _currentUserService.GetFYear() ?? "";

            // Check for duplicate ProcessName
            var existSql = @"
                SELECT COUNT(1)
                FROM ProcessMaster
                WHERE ProcessName = @ProcessName
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            var exists = await connection.QueryFirstOrDefaultAsync<int>(
                existSql,
                new { request.ProcessDetail.ProcessName },
                transaction);

            if (exists > 0)
                return "Exist";

            // Insert ProcessMaster
            var insertSql = @"
                INSERT INTO ProcessMaster
                    (ProcessName, DisplayProcessName, DepartmentID, TypeofCharges, ChargeApplyOnSheets,
                     SizeToBeConsidered, PrePress, StartUnit, EndUnit, UnitConversion,
                     MinimumCharges, SetupCharges, IsDisplay, IsEditToBeProduceQty, Rate,
                     ProcessProductionType, ProcessPurpose, IsOnlineProcess, ProcessModuleType,
                     MinimumQuantityToBeCharged, ProcessFlatWastageValue, ProcessWastagePercentage,
                     ProcessCategory, PerHourCostingParameter, ToolRequired,
                     CompanyID, FYear, UserID, CreatedBy, ModifiedBy, CreatedDate, ModifiedDate)
                VALUES
                    (@ProcessName, @DisplayProcessName, @DepartmentID, @TypeofCharges, @ChargeApplyOnSheets,
                     @SizeToBeConsidered, @PrePress, @StartUnit, @EndUnit, @UnitConversion,
                     @MinimumCharges, @SetupCharges, @IsDisplay, @IsEditToBeProduceQty, @Rate,
                     @ProcessProductionType, @ProcessPurpose, @IsOnlineProcess, @ProcessModuleType,
                     @MinimumQuantityToBeCharged, @ProcessFlatWastageValue, @ProcessWastagePercentage,
                     @ProcessCategory, @PerHourCostingParameter, @ToolRequired,
                     @CompanyID, @FYear, @UserID, @CreatedBy, @ModifiedBy, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            var processId = await connection.QueryFirstAsync<int>(insertSql, new
            {
                request.ProcessDetail.ProcessName,
                request.ProcessDetail.DisplayProcessName,
                request.ProcessDetail.DepartmentID,
                request.ProcessDetail.TypeofCharges,
                request.ProcessDetail.ChargeApplyOnSheets,
                request.ProcessDetail.SizeToBeConsidered,
                request.ProcessDetail.PrePress,
                request.ProcessDetail.StartUnit,
                request.ProcessDetail.EndUnit,
                request.ProcessDetail.UnitConversion,
                request.ProcessDetail.MinimumCharges,
                request.ProcessDetail.SetupCharges,
                request.ProcessDetail.IsDisplay,
                request.ProcessDetail.IsEditToBeProduceQty,
                request.ProcessDetail.Rate,
                request.ProcessDetail.ProcessProductionType,
                request.ProcessDetail.ProcessPurpose,
                request.ProcessDetail.IsOnlineProcess,
                request.ProcessDetail.ProcessModuleType,
                request.ProcessDetail.MinimumQuantityToBeCharged,
                request.ProcessDetail.ProcessFlatWastageValue,
                request.ProcessDetail.ProcessWastagePercentage,
                request.ProcessDetail.ProcessCategory,
                request.ProcessDetail.PerHourCostingParameter,
                request.ProcessDetail.ToolRequired,
                CompanyID = companyId,
                FYear = fYear,
                UserID = userId,
                CreatedBy = userId,
                ModifiedBy = userId
            }, transaction);

            // Insert Machine Allocations
            await InsertMachineAllocationsAsync(connection, transaction, processId, request.MachineAllocations, companyId, userId);

            // Insert Material Allocations
            await InsertMaterialAllocationsAsync(connection, transaction, processId, request.MaterialAllocations, companyId, userId);

            // Insert Content Allocations
            await InsertContentAllocationsAsync(connection, transaction, processId, request.ContentAllocations, companyId, userId);

            // Insert Slabs
            await InsertSlabsAsync(connection, transaction, processId, request.Slabs, companyId, userId);

            // Insert Inspection Parameters
            await InsertInspectionParametersAsync(connection, transaction, processId, request.InspectionParameters, companyId, userId);

            // Insert Line Clearance Parameters
            await InsertLineClearanceParametersAsync(connection, transaction, processId, request.LineClearanceParameters, companyId, userId);

            // Insert Tool Groups
            await InsertToolGroupsAsync(connection, transaction, processId, request.ToolGroups, companyId, userId);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving process");
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> UpdateProcessAsync(UpdateProcessRequest request)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;
            var companyId = _currentUserService.GetCompanyId() ?? 0;

            // Update ProcessMaster
            var updateSql = @"
                UPDATE ProcessMaster
                SET ProcessName = @ProcessName,
                    DisplayProcessName = @DisplayProcessName,
                    DepartmentID = @DepartmentID,
                    TypeofCharges = @TypeofCharges,
                    ChargeApplyOnSheets = @ChargeApplyOnSheets,
                    SizeToBeConsidered = @SizeToBeConsidered,
                    PrePress = @PrePress,
                    StartUnit = @StartUnit,
                    EndUnit = @EndUnit,
                    UnitConversion = @UnitConversion,
                    MinimumCharges = @MinimumCharges,
                    SetupCharges = @SetupCharges,
                    IsDisplay = @IsDisplay,
                    IsEditToBeProduceQty = @IsEditToBeProduceQty,
                    Rate = @Rate,
                    ProcessProductionType = @ProcessProductionType,
                    ProcessPurpose = @ProcessPurpose,
                    IsOnlineProcess = @IsOnlineProcess,
                    ProcessModuleType = @ProcessModuleType,
                    MinimumQuantityToBeCharged = @MinimumQuantityToBeCharged,
                    ProcessFlatWastageValue = @ProcessFlatWastageValue,
                    ProcessWastagePercentage = @ProcessWastagePercentage,
                    ProcessCategory = @ProcessCategory,
                    PerHourCostingParameter = @PerHourCostingParameter,
                    ToolRequired = @ToolRequired,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = GETDATE()
                WHERE ProcessID = @ProcessID
                  AND ISNULL(IsDeletedTransaction, 0) = 0";

            await connection.ExecuteAsync(updateSql, new
            {
                request.ProcessDetail.ProcessName,
                request.ProcessDetail.DisplayProcessName,
                request.ProcessDetail.DepartmentID,
                request.ProcessDetail.TypeofCharges,
                request.ProcessDetail.ChargeApplyOnSheets,
                request.ProcessDetail.SizeToBeConsidered,
                request.ProcessDetail.PrePress,
                request.ProcessDetail.StartUnit,
                request.ProcessDetail.EndUnit,
                request.ProcessDetail.UnitConversion,
                request.ProcessDetail.MinimumCharges,
                request.ProcessDetail.SetupCharges,
                request.ProcessDetail.IsDisplay,
                request.ProcessDetail.IsEditToBeProduceQty,
                request.ProcessDetail.Rate,
                request.ProcessDetail.ProcessProductionType,
                request.ProcessDetail.ProcessPurpose,
                request.ProcessDetail.IsOnlineProcess,
                request.ProcessDetail.ProcessModuleType,
                request.ProcessDetail.MinimumQuantityToBeCharged,
                request.ProcessDetail.ProcessFlatWastageValue,
                request.ProcessDetail.ProcessWastagePercentage,
                request.ProcessDetail.ProcessCategory,
                request.ProcessDetail.PerHourCostingParameter,
                request.ProcessDetail.ToolRequired,
                ModifiedBy = userId,
                request.ProcessID
            }, transaction);

            // Delete existing allocations and re-insert (replace strategy)
            await DeleteChildRecordsAsync(connection, transaction, request.ProcessID);

            // Re-insert all child records
            await InsertMachineAllocationsAsync(connection, transaction, request.ProcessID, request.MachineAllocations, companyId, userId);
            await InsertMaterialAllocationsAsync(connection, transaction, request.ProcessID, request.MaterialAllocations, companyId, userId);
            await InsertContentAllocationsAsync(connection, transaction, request.ProcessID, request.ContentAllocations, companyId, userId);
            await InsertSlabsAsync(connection, transaction, request.ProcessID, request.Slabs, companyId, userId);
            await InsertInspectionParametersAsync(connection, transaction, request.ProcessID, request.InspectionParameters, companyId, userId);
            await InsertLineClearanceParametersAsync(connection, transaction, request.ProcessID, request.LineClearanceParameters, companyId, userId);
            await InsertToolGroupsAsync(connection, transaction, request.ProcessID, request.ToolGroups, companyId, userId);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating process {ProcessID}", request.ProcessID);
            return $"fail {ex.Message}";
        }
    }

    public async Task<string> DeleteProcessAsync(int processId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var userId = _currentUserService.GetUserId() ?? 0;

            var deleteSql = @"
                UPDATE ProcessMaster
                SET ModifiedBy = @ModifiedBy,
                    DeletedBy = @DeletedBy,
                    DeletedDate = GETDATE(),
                    ModifiedDate = GETDATE(),
                    IsDeletedTransaction = 1
                WHERE ProcessID = @ProcessID";

            await connection.ExecuteAsync(deleteSql, new
            {
                ModifiedBy = userId,
                DeletedBy = userId,
                ProcessID = processId
            }, transaction);

            await transaction.CommitAsync();
            return "Success";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting process {ProcessID}", processId);
            return "fail";
        }
    }

    // ==================== Private Helper Methods ====================

    private async Task DeleteChildRecordsAsync(SqlConnection connection, SqlTransaction transaction, int processId)
    {
        var deleteSqls = new[]
        {
            "DELETE FROM ProcessAllocatedMachineMaster WHERE ProcessID = @ProcessID",
            "DELETE FROM ProcessAllocatedMaterialMaster WHERE ProcessID = @ProcessID",
            // Content allocation is stored as AllocatedContentID in ProcessMaster, no separate table
            "DELETE FROM ProcessMasterSlabs WHERE ProcessID = @ProcessID",
            "DELETE FROM ProcessInspectionParameterMaster WHERE ProcessID = @ProcessID",
            "DELETE FROM ProcessLineClearanceParameters WHERE ProcessID = @ProcessID",
            "DELETE FROM ProcessToolGroupAllocationMaster WHERE ProcessID = @ProcessID"
        };

        foreach (var sql in deleteSqls)
        {
            try
            {
                await connection.ExecuteAsync(sql, new { ProcessID = processId }, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Child table may not exist for delete: {Sql}", sql);
            }
        }
    }

    private async Task InsertMachineAllocationsAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<MachineAllocationRecord> machines, int companyId, int userId)
    {
        if (machines == null || machines.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessAllocatedMachineMaster
                    (ProcessID, MachineID, MachineSpeed, MakeReadyTime, JobChangeOverTime, IsDefaultMachine,
                     CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @MachineID, @MachineSpeed, @MakeReadyTime, @JobChangeOverTime, @IsDefaultMachine,
                     @CompanyID, @CreatedBy, GETDATE())";

            foreach (var machine in machines)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    machine.MachineID,
                    machine.MachineSpeed,
                    machine.MakeReadyTime,
                    machine.JobChangeOverTime,
                    machine.IsDefaultMachine,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessMachineAllocation table may not exist, skipping insert");
        }
    }

    private async Task InsertMaterialAllocationsAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<MaterialAllocationRecord> materials, int companyId, int userId)
    {
        if (materials == null || materials.Count == 0) return;

        var sql = @"
            INSERT INTO ProcessAllocatedMaterialMaster
                (ProcessID, ItemID, CompanyID, CreatedBy, CreatedDate)
            VALUES
                (@ProcessID, @ItemID, @CompanyID, @CreatedBy, GETDATE())";

        foreach (var material in materials)
        {
            await connection.ExecuteAsync(sql, new
            {
                ProcessID = processId,
                material.ItemID,
                CompanyID = companyId,
                CreatedBy = userId
            }, transaction);
        }
    }

    private async Task InsertContentAllocationsAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<ContentAllocationRecord> contents, int companyId, int userId)
    {
        if (contents == null || contents.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessContentAllocation
                    (ProcessID, ContentID, CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @ContentID, @CompanyID, @CreatedBy, GETDATE())";

            foreach (var content in contents)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    content.ContentID,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessContentAllocation table may not exist, skipping insert");
        }
    }

    private async Task InsertSlabsAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<SlabRecord> slabs, int companyId, int userId)
    {
        if (slabs == null || slabs.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessMasterSlabs
                    (ProcessID, FromQty, ToQty, StartUnit, RateFactor, Rate, MinimumCharges, IsLocked,
                     CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @FromQty, @ToQty, @StartUnit, @RateFactor, @Rate, @MinimumCharges, @IsLocked,
                     @CompanyID, @CreatedBy, GETDATE())";

            foreach (var slab in slabs)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    slab.FromQty,
                    slab.ToQty,
                    slab.StartUnit,
                    slab.RateFactor,
                    slab.Rate,
                    slab.MinimumCharges,
                    slab.IsLocked,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessMasterSlabs table may not exist, skipping insert");
        }
    }

    private async Task InsertInspectionParametersAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<InspectionParameterRecord> parameters, int companyId, int userId)
    {
        if (parameters == null || parameters.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessInspectionParameterMaster
                    (ProcessID, DepartmentID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue,
                     CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @DepartmentID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue,
                     @CompanyID, @CreatedBy, GETDATE())";

            foreach (var param in parameters)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    param.DepartmentID,
                    param.SequenceNo,
                    param.ParameterName,
                    param.StandardValue,
                    param.InputFieldType,
                    param.FieldDataType,
                    param.DefaultValue,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessInspectionParameter table may not exist, skipping insert");
        }
    }

    private async Task InsertLineClearanceParametersAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<LineClearanceParameterRecord> parameters, int companyId, int userId)
    {
        if (parameters == null || parameters.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessLineClearanceParameters
                    (ProcessID, DepartmentID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue,
                     CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @DepartmentID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue,
                     @CompanyID, @CreatedBy, GETDATE())";

            foreach (var param in parameters)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    param.DepartmentID,
                    param.SequenceNo,
                    param.ParameterName,
                    param.StandardValue,
                    param.InputFieldType,
                    param.FieldDataType,
                    param.DefaultValue,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessLineClearanceParameter table may not exist, skipping insert");
        }
    }

    private async Task InsertToolGroupsAsync(SqlConnection connection, SqlTransaction transaction,
        int processId, List<ToolGroupRecord> toolGroups, int companyId, int userId)
    {
        if (toolGroups == null || toolGroups.Count == 0) return;

        try
        {
            var sql = @"
                INSERT INTO ProcessToolGroupAllocationMaster
                    (ProcessID, ToolGroupID, CompanyID, CreatedBy, CreatedDate)
                VALUES
                    (@ProcessID, @ToolGroupID, @CompanyID, @CreatedBy, GETDATE())";

            foreach (var toolGroup in toolGroups)
            {
                await connection.ExecuteAsync(sql, new
                {
                    ProcessID = processId,
                    toolGroup.ToolGroupID,
                    CompanyID = companyId,
                    CreatedBy = userId
                }, transaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProcessToolGroupAllocationMaster table may not exist, skipping insert");
        }
    }
}
