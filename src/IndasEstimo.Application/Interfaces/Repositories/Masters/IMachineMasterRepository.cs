
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories.Masters;

public interface IMachineMasterRepository
{
    Task<string> GetMachineCodeAsync();
    Task<IEnumerable<CreateMachineDto>> GetMachinesAsync(); // Use appropriate DTO
    Task<long> CreateMachineAsync(CreateMachineDto machine);
    Task<bool> UpdateMachineAsync(UpdateMachineDto machine);
    Task<bool> DeleteMachineAsync(long machineId);
    Task<MachineDetailDto> GetMachineByIdAsync(long machineId);
}
