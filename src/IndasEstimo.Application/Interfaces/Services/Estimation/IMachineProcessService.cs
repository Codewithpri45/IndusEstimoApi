using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Machine and Process operations
/// </summary>
public interface IMachineProcessService
{
    Task<Result<List<MachineGridDto>>> GetMachineGridAsync(string contentDomainType);
    Task<Result<List<MachineDto>>> GetAllMachinesAsync();
    Task<Result<List<OperationDto>>> GetDefaultOperationsAsync(string domainType);
    Task<Result<List<OperationSlabDto>>> GetOperationSlabsAsync(long processId);
    Task<Result<List<MachineItemDto>>> GetMachineItemsAsync(long machineId);
}
