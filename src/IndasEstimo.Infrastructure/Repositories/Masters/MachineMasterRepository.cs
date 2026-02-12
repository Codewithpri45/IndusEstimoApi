
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

public class MachineMasterRepository : IMachineMasterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MachineMasterRepository> _logger;

    public MachineMasterRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MachineMasterRepository> logger)
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

    public async Task<string> GetMachineCodeAsync()
    {
        // Placeholder logical call, assuming GeneratePrefixedNo logic or similar
        return "MM-" + DateTime.Now.Ticks.ToString();
    }

    public async Task<long> CreateMachineAsync(CreateMachineDto machine)
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var companyId = _currentUserService.GetCompanyId() ?? 0;
            var userId = _currentUserService.GetUserId() ?? 0;
            var fYear = _currentUserService.GetFYear();

            // 1. Insert MachineMaster
            var sqlMachine = @"
                INSERT INTO MachineMaster (
                    MachineName, MachineType, Colors, MaxLength, MaxWidth, MinLength, MinWidth, PerHourRate, CurrentStatus,
                    CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction, MachineCode
                ) VALUES (
                    @MachineName, @MachineType, @Colors, @MaxLength, @MaxWidth, @MinLength, @MinWidth, @PerHourRate, @CurrentStatus,
                    @CompanyID, @CreatedBy, GETDATE(), @FYear, 0, 'MM-' + CAST(NEWID() AS NVARCHAR(50))
                );
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            var machineId = await connection.ExecuteScalarAsync<long>(sqlMachine, new
            {
                machine.MachineName,
                machine.MachineType,
                machine.Colors,
                machine.MaxLength,
                machine.MaxWidth,
                machine.MinLength,
                machine.MinWidth,
                machine.PerHourRate,
                machine.CurrentStatus,
                CompanyID = companyId,
                CreatedBy = userId,
                FYear = fYear
            }, transaction);

            // 2. Insert Slabs
            if (machine.Slabs != null && machine.Slabs.Any())
            {
                var sqlSlab = @"
                    INSERT INTO MachineSlabMaster (
                        MachineID, RunningMeterRangeFrom, RunningMeterRangeTo, Rate, Wastage, PlateCharges, MinCharges, PaperGroup,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @MachineID, @RunningMeterRangeFrom, @RunningMeterRangeTo, @Rate, @Wastage, @PlateCharges, @MinCharges, @PaperGroup,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";

                foreach (var slab in machine.Slabs)
                {
                    await connection.ExecuteAsync(sqlSlab, new
                    {
                        MachineID = machineId,
                        slab.RunningMeterRangeFrom,
                        slab.RunningMeterRangeTo,
                        slab.Rate,
                        slab.Wastage,
                        slab.PlateCharges,
                        slab.MinCharges,
                        slab.PaperGroup,
                        CompanyID = companyId,
                        CreatedBy = userId,
                        FYear = fYear
                    }, transaction);
                }
            }

            // 3. Insert Coating Rates
            if (machine.CoatingRates != null && machine.CoatingRates.Any())
            {
                var sqlCoating = @"
                    INSERT INTO MachineOnlineCoatingRates (
                        MachineID, CoatingName, SheetRangeFrom, SheetRangeTo, RateType, Rate,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @MachineID, @CoatingName, @SheetRangeFrom, @SheetRangeTo, @RateType, @Rate,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";

                foreach (var coating in machine.CoatingRates)
                {
                    await connection.ExecuteAsync(sqlCoating, new
                    {
                        MachineID = machineId,
                        coating.CoatingName,
                        coating.SheetRangeFrom,
                        coating.SheetRangeTo,
                        coating.RateType,
                        coating.Rate,
                        CompanyID = companyId,
                        CreatedBy = userId,
                        FYear = fYear
                    }, transaction);
                }
            }

            // 4. Insert Allocations (Comma separated string or separate table rows?)
            // Legacy uses 'GroupAllocationIDs' string column in MachineItemSubGroupAllocationMaster
            if (machine.AllocatedSubGroupIds != null && machine.AllocatedSubGroupIds.Any())
            {
                var allocationString = string.Join(",", machine.AllocatedSubGroupIds);
                var sqlAllocation = @"
                    INSERT INTO MachineItemSubGroupAllocationMaster (
                        MachineID, GroupAllocationIDs,
                        CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                    ) VALUES (
                        @MachineID, @GroupAllocationIDs,
                        @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                    )";

                await connection.ExecuteAsync(sqlAllocation, new
                {
                    MachineID = machineId,
                    GroupAllocationIDs = allocationString,
                    CompanyID = companyId,
                    CreatedBy = userId,
                    FYear = fYear
                }, transaction);
            }

            transaction.Commit();
            return machineId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateMachineAsync(UpdateMachineDto machine)
    {
       using var connection = GetConnection();
       connection.Open();
       using var transaction = connection.BeginTransaction();
       
       try 
       {
           var companyId = _currentUserService.GetCompanyId() ?? 0;
           var userId = _currentUserService.GetUserId() ?? 0;
           var fYear = _currentUserService.GetFYear();

           // Update MachineMaster
           var sqlUpdate = @"
                UPDATE MachineMaster SET 
                    MachineName = @MachineName, MachineType = @MachineType, Colors = @Colors, 
                    MaxLength = @MaxLength, MaxWidth = @MaxWidth, MinLength = @MinLength, MinWidth = @MinWidth, 
                    PerHourRate = @PerHourRate, CurrentStatus = @CurrentStatus,
                    ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                WHERE MachineID = @MachineID AND CompanyID = @CompanyID";

           var rows = await connection.ExecuteAsync(sqlUpdate, new {
               machine.MachineName, machine.MachineType, machine.Colors, 
               machine.MaxLength, machine.MaxWidth, machine.MinLength, machine.MinWidth,
               machine.PerHourRate, machine.CurrentStatus, 
               ModifiedBy = userId, machine.MachineID, CompanyID = companyId
           }, transaction);

           if (rows == 0)
           {
               transaction.Rollback();
               return false;
           }

           // 1. Update Slabs (Delete & Insert)
           await connection.ExecuteAsync("DELETE FROM MachineSlabMaster WHERE MachineID = @MachineID AND CompanyID = @CompanyID", new { machine.MachineID, CompanyID = companyId }, transaction);
           if (machine.Slabs != null && machine.Slabs.Any())
           {
               var sqlSlab = @"
                   INSERT INTO MachineSlabMaster (
                       MachineID, RunningMeterRangeFrom, RunningMeterRangeTo, Rate, Wastage, PlateCharges, MinCharges, PaperGroup,
                       CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                   ) VALUES (
                       @MachineID, @RunningMeterRangeFrom, @RunningMeterRangeTo, @Rate, @Wastage, @PlateCharges, @MinCharges, @PaperGroup,
                       @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                   )";
               foreach (var slab in machine.Slabs)
               {
                   await connection.ExecuteAsync(sqlSlab, new { 
                       machine.MachineID, slab.RunningMeterRangeFrom, slab.RunningMeterRangeTo, slab.Rate, slab.Wastage, 
                       slab.PlateCharges, slab.MinCharges, slab.PaperGroup,
                       CompanyID = companyId, CreatedBy = userId, FYear = fYear 
                   }, transaction);
               }
           }

           // 2. Update Coating Rates
           await connection.ExecuteAsync("DELETE FROM MachineOnlineCoatingRates WHERE MachineID = @MachineID AND CompanyID = @CompanyID", new { machine.MachineID, CompanyID = companyId }, transaction);
           if (machine.CoatingRates != null && machine.CoatingRates.Any())
           {
               var sqlCoating = @"
                   INSERT INTO MachineOnlineCoatingRates (
                       MachineID, CoatingName, SheetRangeFrom, SheetRangeTo, RateType, Rate,
                       CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                   ) VALUES (
                       @MachineID, @CoatingName, @SheetRangeFrom, @SheetRangeTo, @RateType, @Rate,
                       @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                   )";
               foreach (var coating in machine.CoatingRates)
               {
                   await connection.ExecuteAsync(sqlCoating, new {
                       machine.MachineID, coating.CoatingName, coating.SheetRangeFrom, coating.SheetRangeTo, coating.RateType, coating.Rate,
                       CompanyID = companyId, CreatedBy = userId, FYear = fYear
                   }, transaction);
               }
           }

           // 3. Update Allocations
           await connection.ExecuteAsync("DELETE FROM MachineItemSubGroupAllocationMaster WHERE MachineID = @MachineID AND CompanyID = @CompanyID", new { machine.MachineID, CompanyID = companyId }, transaction);
           if (machine.AllocatedSubGroupIds != null && machine.AllocatedSubGroupIds.Any())
           {
               var allocationString = string.Join(",", machine.AllocatedSubGroupIds);
               var sqlAllocation = @"
                   INSERT INTO MachineItemSubGroupAllocationMaster (
                       MachineID, GroupAllocationIDs,
                       CompanyID, CreatedBy, CreatedDate, FYear, IsDeletedTransaction
                   ) VALUES (
                       @MachineID, @GroupAllocationIDs,
                       @CompanyID, @CreatedBy, GETDATE(), @FYear, 0
                   )";

               await connection.ExecuteAsync(sqlAllocation, new {
                   machine.MachineID, GroupAllocationIDs = allocationString,
                   CompanyID = companyId, CreatedBy = userId, FYear = fYear
               }, transaction);
           }

           transaction.Commit();
           return true; 
       }
       catch
       {
           transaction.Rollback();
           throw;
       }
    }

    public async Task<bool> DeleteMachineAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        var sql = @"
            UPDATE MachineMaster 
            SET IsDeletedTransaction = 1, DeletedBy = @DeletedBy, DeletedDate = GETDATE() 
            WHERE MachineID = @MachineID AND CompanyID = @CompanyID";

        var rows = await connection.ExecuteAsync(sql, new { MachineID = machineId, DeletedBy = userId, CompanyID = companyId });
        return rows > 0;
    }

    public async Task<MachineDetailDto> GetMachineByIdAsync(long machineId)
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;

        var sql = @"
            SELECT * FROM MachineMaster 
            WHERE MachineID = @MachineID AND CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction, 0) = 0";

        return await connection.QueryFirstOrDefaultAsync<MachineDetailDto>(sql, new { MachineID = machineId, CompanyID = companyId });
    }

    public async Task<IEnumerable<CreateMachineDto>> GetMachinesAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        
        var sql = @"
            SELECT MachineName, MachineType, Colors, MaxLength, MaxWidth, MinLength, MinWidth, PerHourRate, CurrentStatus
            FROM MachineMaster 
            WHERE CompanyID = @CompanyID AND ISNULL(IsDeletedTransaction, 0) = 0";

        return await connection.QueryAsync<CreateMachineDto>(sql, new { CompanyID = companyId });
    }
}
