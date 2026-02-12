
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services.Masters;

public interface IMachineMasterService
{
    Task<Result<string>> GenerateMachineCodeAsync();
    Task<Result<IEnumerable<CreateMachineDto>>> GetMachinesAsync();
    Task<Result<long>> CreateMachineAsync(CreateMachineDto machine);
    Task<Result<bool>> UpdateMachineAsync(UpdateMachineDto machine);
    Task<Result<bool>> DeleteMachineAsync(long machineId);
    Task<Result<MachineDetailDto>> GetMachineByIdAsync(long machineId);
}
