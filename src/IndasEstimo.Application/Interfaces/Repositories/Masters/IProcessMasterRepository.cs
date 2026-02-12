
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories.Masters;

public interface IProcessMasterRepository
{
    Task<IEnumerable<ProcessDetailDto>> GetProcessesAsync();
    Task<long> CreateProcessAsync(CreateProcessDto process);
    Task<bool> UpdateProcessAsync(UpdateProcessDto process);
    Task<bool> DeleteProcessAsync(long processId);
    Task<ProcessDetailDto> GetProcessByIdAsync(long processId);
}
