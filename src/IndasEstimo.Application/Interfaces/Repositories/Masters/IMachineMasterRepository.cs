using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

/// <summary>
/// Machine Master repository - data access for machine management
/// </summary>
public interface IMachineMasterRepository
{
    Task<List<MachineListDto>> GetMachineListAsync();
    Task<List<MachineSlabDto>> GetMachineSlabsAsync(int machineId);
    Task<List<MachineOnlineCoatingRateDto>> GetMachineOnlineCoatingRatesAsync(int machineId);
    Task<List<MachineDepartmentDto>> GetDepartmentsAsync();
    Task<List<MachineTypeDto>> GetMachineTypesAsync();
    Task<List<MachineNameDto>> GetMachineNamesAsync();
    Task<List<MachineGroupAllocationDto>> GetGroupGridAsync();
    Task<string> GetGroupAllocationIDsAsync(int machineId);
    Task<List<CoatingNameDto>> GetCoatingNamesAsync();
    Task<List<MachineToolDto>> GetToolListAsync(int toolGroupId);
    Task<string> GetAllocatedToolsAsync(int machineId, int toolGroupId);
    Task<string> GetMachineCodeAsync();
    Task<string> CheckMachineNameExistsAsync(string machineName);
    Task<string> SaveMachineAsync(SaveMachineRequest request);
    Task<string> UpdateMachineAsync(UpdateMachineRequest request);
    Task<string> DeleteMachineAsync(int machineId);
    Task<string> SaveGroupAllocationAsync(SaveMachineGroupAllocationRequest request);
    Task<string> DeleteGroupAllocationAsync(int machineId);
    Task<string> SaveToolAllocationAsync(SaveMachineToolAllocationRequest request);
    Task<string> DeleteToolAllocationAsync(int machineId, int toolGroupId);
}
