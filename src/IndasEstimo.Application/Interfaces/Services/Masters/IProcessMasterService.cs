using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IProcessMasterService
{
    Task<Result<List<ProcessListDto>>> GetProcessListAsync();
    Task<Result<List<ProcessNameDto>>> GetProcessNamesAsync();
    Task<Result<ProcessLoadedDataDto>> GetProcessByIdAsync(int processId);
    Task<Result<List<ProcessDepartmentDto>>> GetDepartmentsAsync();
    Task<Result<List<TypeOfChargesDto>>> GetTypeOfChargesAsync();
    Task<Result<List<UnitDto>>> GetUnitsAsync();
    Task<Result<List<ProcessToolGroupDto>>> GetToolGroupListAsync();
    Task<Result<object>> GetMachineGridAsync();
    Task<Result<object>> GetItemGridAsync();
    Task<Result<object>> GetContentGridAsync();
    Task<Result<List<ProcessSlabDto>>> GetExistingSlabsAsync(int processId);
    Task<Result<List<ProcessMachineAllocationDto>>> GetAllocatedMachinesAsync(int processId);
    Task<Result<List<ProcessMaterialAllocationDto>>> GetAllocatedMaterialsAsync(int processId);
    Task<Result<List<ProcessInspectionParameterDto>>> GetInspectionParametersAsync(int processId);
    Task<Result<List<ProcessLineClearanceParameterDto>>> GetLineClearanceParametersAsync(int processId);
    Task<Result<string>> SaveProcessAsync(SaveProcessRequest request);
    Task<Result<string>> UpdateProcessAsync(UpdateProcessRequest request);
    Task<Result<string>> DeleteProcessAsync(int processId);
}
