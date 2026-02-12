
using Dapper;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IndasEstimo.Infrastructure.Repositories.Masters;

public class ProcessMasterRepository : IProcessMasterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProcessMasterRepository> _logger;

    public ProcessMasterRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<ProcessMasterRepository> logger)
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

    public async Task<long> CreateProcessAsync(CreateProcessDto process)
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;
            var fYear = _currentUserService.GetFYear();

            // 1. Insert ProcessMaster
            var sqlProcess = @"
                INSERT INTO ProcessMaster (
                    ProcessName, DisplayProcessName, TypeofCharges, SizeToBeConsidered, Rate, MinimumCharges, SetupCharges,
                    StartUnit, EndUnit, IsDisplay, ToolRequired, IsEditToBeProduceQty, ProcessProductionType,
                    ChargeApplyOnSheets, PrePress, UnitConversion, DepartmentID, AllocattedMachineID, AllocatedContentID,
                    ProcessPurpose, IsOnlineProcess, ProcessModuleType, MinimumQuantityToBeCharged, ProcessFlatWastageValue,
                    ProcessWastagePercentage, ProcessCategory, ProductionUnitID,
                    CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                ) VALUES (
                    @ProcessName, @DisplayProcessName, @TypeofCharges, @SizeToBeConsidered, @Rate, @MinimumCharges, @SetupCharges,
                    @StartUnit, @EndUnit, @IsDisplay, @ToolRequired, @IsEditToBeProduceQty, @ProcessProductionType,
                    @ChargeApplyOnSheets, @PrePress, @UnitConversion, @DepartmentID, @AllocattedMachineID, @AllocatedContentID,
                    @ProcessPurpose, @IsOnlineProcess, @ProcessModuleType, @MinimumQuantityToBeCharged, @ProcessFlatWastageValue,
                    @ProcessWastagePercentage, @ProcessCategory, @ProductionUnitID,
                    @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                );
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            var processId = await connection.ExecuteScalarAsync<long>(sqlProcess, new
            {
                process.ProcessName,
                process.DisplayProcessName,
                process.TypeofCharges,
                process.SizeToBeConsidered,
                process.Rate,
                process.MinimumCharges,
                process.SetupCharges,
                process.StartUnit,
                process.EndUnit,
                process.IsDisplay,
                process.ToolRequired,
                process.IsEditToBeProduceQty,
                process.ProcessProductionType,
                process.ChargeApplyOnSheets,
                process.PrePress,
                process.UnitConversion,
                process.DepartmentID,
                process.AllocattedMachineID,
                process.AllocatedContentID,
                process.ProcessPurpose,
                process.IsOnlineProcess,
                process.ProcessModuleType,
                process.MinimumQuantityToBeCharged,
                process.ProcessFlatWastageValue,
                process.ProcessWastagePercentage,
                process.ProcessCategory,
                process.ProductionUnitID,
                CompanyID = companyId,
                CreatedBy = userId,
                FYear = fYear
            }, transaction);

            // 2. Insert Tool Allocation
            if (process.Tools != null)
            {
                var sqlTool = @"
                    INSERT INTO ProcessToolGroupAllocationMaster (
                        ProcessID, ToolGroupID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @ToolGroupID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.Tools)
                {
                    await connection.ExecuteAsync(sqlTool, new { ProcessID = processId, item.ToolGroupID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
                }
            }

            // 3. Insert Machine Allocation
            if (process.Machines != null)
            {
                var sqlMachine = @"
                    INSERT INTO ProcessAllocatedMachineMaster (
                        ProcessID, MachineID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @MachineID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.Machines)
                {
                    await connection.ExecuteAsync(sqlMachine, new { ProcessID = processId, item.MachineID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
                }
            }

            // 4. Insert Material Allocation
            if (process.Materials != null)
            {
                var sqlMaterial = @"
                    INSERT INTO ProcessAllocatedMaterialMaster (
                        ProcessID, ItemSubGroupID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @ItemSubGroupID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.Materials)
                {
                    await connection.ExecuteAsync(sqlMaterial, new { ProcessID = processId, item.ItemSubGroupID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
                }
            }

            // 5. Insert Slabs
            if (process.Slabs != null)
            {
                var sqlSlab = @"
                    INSERT INTO ProcessMasterSlabs (
                        ProcessID, FromQty, ToQty, StartUnit, RateFactor, Rate, MinimumCharges, IsLocked,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @FromQty, @ToQty, @StartUnit, @RateFactor, @Rate, @MinimumCharges, @IsLocked,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.Slabs)
                {
                    await connection.ExecuteAsync(sqlSlab, new { 
                        ProcessID = processId, item.FromQty, item.ToQty, item.StartUnit, item.RateFactor, 
                        item.Rate, item.MinimumCharges, item.IsLocked,
                        CompanyID = companyId, CreatedBy = userId, FYear = fYear 
                    }, transaction);
                }
            }

            // 6. Insert Inspection Params
            if (process.InspectionParams != null)
            {
                var sqlInsp = @"
                    INSERT INTO ProcessInspectionParameterMaster (
                        ProcessID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue, DepartmentID,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue, @DepartmentID,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.InspectionParams)
                {
                    await connection.ExecuteAsync(sqlInsp, new { 
                        ProcessID = processId, item.SequenceNo, item.ParameterName, item.StandardValue, 
                        item.InputFieldType, item.FieldDataType, item.DefaultValue, item.DepartmentID,
                        CompanyID = companyId, CreatedBy = userId, FYear = fYear 
                    }, transaction);
                }
            }

            // 7. Insert Line Clearance Params
            if (process.LineClearanceParams != null)
            {
                var sqlLine = @"
                    INSERT INTO LineClearanceParameterMaster (
                        ProcessID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue, DepartmentID,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @ProcessID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue, @DepartmentID,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";
                foreach (var item in process.LineClearanceParams)
                {
                    await connection.ExecuteAsync(sqlLine, new { 
                        ProcessID = processId, item.SequenceNo, item.ParameterName, item.StandardValue, 
                        item.InputFieldType, item.FieldDataType, item.DefaultValue, item.DepartmentID,
                        CompanyID = companyId, CreatedBy = userId, FYear = fYear 
                    }, transaction);
                }
            }

            transaction.Commit();
            return processId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateProcessAsync(UpdateProcessDto process)
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;
            var fYear = _currentUserService.GetFYear();

            var sqlUpdate = @"
                UPDATE ProcessMaster SET
                    ProcessName = @ProcessName, DisplayProcessName = @DisplayProcessName, TypeofCharges = @TypeofCharges, 
                    SizeToBeConsidered = @SizeToBeConsidered, Rate = @Rate, MinimumCharges = @MinimumCharges, SetupCharges = @SetupCharges,
                    StartUnit = @StartUnit, EndUnit = @EndUnit, IsDisplay = @IsDisplay, ToolRequired = @ToolRequired, 
                    IsEditToBeProduceQty = @IsEditToBeProduceQty, ProcessProductionType = @ProcessProductionType,
                    ChargeApplyOnSheets = @ChargeApplyOnSheets, PrePress = @PrePress, UnitConversion = @UnitConversion, 
                    DepartmentID = @DepartmentID, AllocattedMachineID = @AllocattedMachineID, AllocatedContentID = @AllocatedContentID,
                    ProcessPurpose = @ProcessPurpose, IsOnlineProcess = @IsOnlineProcess, ProcessModuleType = @ProcessModuleType, 
                    MinimumQuantityToBeCharged = @MinimumQuantityToBeCharged, ProcessFlatWastageValue = @ProcessFlatWastageValue,
                    ProcessWastagePercentage = @ProcessWastagePercentage, ProcessCategory = @ProcessCategory, ProductionUnitID = @ProductionUnitID,
                    ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID";

            var rows = await connection.ExecuteAsync(sqlUpdate, new
            {
                process.ProcessName, process.DisplayProcessName, process.TypeofCharges, process.SizeToBeConsidered,
                process.Rate, process.MinimumCharges, process.SetupCharges, process.StartUnit, process.EndUnit,
                process.IsDisplay, process.ToolRequired, process.IsEditToBeProduceQty, process.ProcessProductionType,
                process.ChargeApplyOnSheets, process.PrePress, process.UnitConversion, process.DepartmentID,
                process.AllocattedMachineID, process.AllocatedContentID, process.ProcessPurpose, process.IsOnlineProcess,
                process.ProcessModuleType, process.MinimumQuantityToBeCharged, process.ProcessFlatWastageValue,
                process.ProcessWastagePercentage, process.ProcessCategory, process.ProductionUnitID,
                ModifiedBy = userId, process.ProcessID, CompanyID = companyId
            }, transaction);

            if (rows == 0)
            {
                transaction.Rollback();
                return false;
            }

            var deleteParams = new { ProcessID = process.ProcessID, CompanyID = companyId };
            await connection.ExecuteAsync("DELETE FROM ProcessToolGroupAllocationMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);
            await connection.ExecuteAsync("DELETE FROM ProcessAllocatedMachineMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);
            await connection.ExecuteAsync("DELETE FROM ProcessAllocatedMaterialMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);
            await connection.ExecuteAsync("DELETE FROM ProcessMasterSlabs WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);
            await connection.ExecuteAsync("DELETE FROM ProcessInspectionParameterMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);
            await connection.ExecuteAsync("DELETE FROM LineClearanceParameterMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID", deleteParams, transaction);

            if (process.Tools != null)
            {
                var sqlTool = @"
                    INSERT INTO ProcessToolGroupAllocationMaster (ProcessID, ToolGroupID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @ToolGroupID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.Tools) await connection.ExecuteAsync(sqlTool, new { ProcessID = process.ProcessID, item.ToolGroupID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            if (process.Machines != null)
            {
                var sqlMachine = @"
                    INSERT INTO ProcessAllocatedMachineMaster (ProcessID, MachineID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @MachineID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.Machines) await connection.ExecuteAsync(sqlMachine, new { ProcessID = process.ProcessID, item.MachineID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            if (process.Materials != null)
            {
                var sqlMaterial = @"
                    INSERT INTO ProcessAllocatedMaterialMaster (ProcessID, ItemSubGroupID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @ItemSubGroupID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.Materials) await connection.ExecuteAsync(sqlMaterial, new { ProcessID = process.ProcessID, item.ItemSubGroupID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            if (process.Slabs != null)
            {
                var sqlSlab = @"
                    INSERT INTO ProcessMasterSlabs (ProcessID, FromQty, ToQty, StartUnit, RateFactor, Rate, MinimumCharges, IsLocked, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @FromQty, @ToQty, @StartUnit, @RateFactor, @Rate, @MinimumCharges, @IsLocked, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.Slabs) await connection.ExecuteAsync(sqlSlab, new { ProcessID = process.ProcessID, item.FromQty, item.ToQty, item.StartUnit, item.RateFactor, item.Rate, item.MinimumCharges, item.IsLocked, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            if (process.InspectionParams != null)
            {
                var sqlInsp = @"
                    INSERT INTO ProcessInspectionParameterMaster (ProcessID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue, DepartmentID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue, @DepartmentID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.InspectionParams) await connection.ExecuteAsync(sqlInsp, new { ProcessID = process.ProcessID, item.SequenceNo, item.ParameterName, item.StandardValue, item.InputFieldType, item.FieldDataType, item.DefaultValue, item.DepartmentID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            if (process.LineClearanceParams != null)
            {
                var sqlLine = @"
                    INSERT INTO LineClearanceParameterMaster (ProcessID, SequenceNo, ParameterName, StandardValue, InputFieldType, FieldDataType, DefaultValue, DepartmentID, CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction) 
                    VALUES (@ProcessID, @SequenceNo, @ParameterName, @StandardValue, @InputFieldType, @FieldDataType, @DefaultValue, @DepartmentID, @CompanyID, @CreatedBy, GETDATE(), @FYear, 0)";
                foreach (var item in process.LineClearanceParams) await connection.ExecuteAsync(sqlLine, new { ProcessID = process.ProcessID, item.SequenceNo, item.ParameterName, item.StandardValue, item.InputFieldType, item.FieldDataType, item.DefaultValue, item.DepartmentID, CompanyID = companyId, CreatedBy = userId, FYear = fYear }, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Process");
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteProcessAsync(long processId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = "UPDATE ProcessMaster SET IsDeletedTransaction = 1, DeletedBy = @DeletedBy, DeletedDate = GETDATE() WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID";
        var rows = await connection.ExecuteAsync(sql, new { ProcessID = processId, DeletedBy = userId, CompanyID = companyId });
        return rows > 0;
    }

    public async Task<ProcessDetailDto> GetProcessByIdAsync(long processId)
    {
         using var connection = GetConnection();
         var companyId = _currentUserService.GetCompanyId() ?? 0;
         return await connection.QueryFirstOrDefaultAsync<ProcessDetailDto>("SELECT * FROM ProcessMaster WHERE ProcessID = @ProcessID AND CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction, 0) = 0", new { ProcessID = processId, CompanyID = companyId });
    }

    public async Task<IEnumerable<ProcessDetailDto>> GetProcessesAsync()
    {
         using var connection = GetConnection();
         var companyId = _currentUserService.GetCompanyId() ?? 0;
         return await connection.QueryAsync<ProcessDetailDto>("SELECT * FROM ProcessMaster WHERE CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction, 0) = 0 ORDER BY ProcessName", new { CompanyID = companyId });
    }
}
