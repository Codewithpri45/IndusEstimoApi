using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IProcessMasterRepository
{
    Task<List<ProcessListDto>> GetProcessListAsync();
    Task<List<ProcessNameDto>> GetProcessNamesAsync();
    Task<ProcessLoadedDataDto> GetProcessByIdAsync(int processId);
    Task<List<ProcessDepartmentDto>> GetDepartmentsAsync();
    Task<List<TypeOfChargesDto>> GetTypeOfChargesAsync();
    Task<List<UnitDto>> GetUnitsAsync();
    Task<List<ProcessToolGroupDto>> GetToolGroupListAsync();
    Task<object> GetMachineGridAsync();
    Task<object> GetItemGridAsync();
    Task<object> GetContentGridAsync();
    Task<List<ProcessSlabDto>> GetExistingSlabsAsync(int processId);
    Task<List<ProcessMachineAllocationDto>> GetAllocatedMachinesAsync(int processId);
    Task<List<ProcessMaterialAllocationDto>> GetAllocatedMaterialsAsync(int processId);
    Task<List<ProcessInspectionParameterDto>> GetInspectionParametersAsync(int processId);
    Task<List<ProcessLineClearanceParameterDto>> GetLineClearanceParametersAsync(int processId);
    Task<string> SaveProcessAsync(SaveProcessRequest request);
    Task<string> UpdateProcessAsync(UpdateProcessRequest request);
    Task<string> DeleteProcessAsync(int processId);
}
