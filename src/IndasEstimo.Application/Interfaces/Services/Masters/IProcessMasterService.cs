
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services.Masters;

public interface IProcessMasterService
{
    Task<Result<IEnumerable<ProcessDetailDto>>> GetProcessesAsync();
    Task<Result<long>> CreateProcessAsync(CreateProcessDto process);
    Task<Result<bool>> UpdateProcessAsync(UpdateProcessDto process);
    Task<Result<bool>> DeleteProcessAsync(long processId);
    Task<Result<ProcessDetailDto>> GetProcessByIdAsync(long processId);
}
