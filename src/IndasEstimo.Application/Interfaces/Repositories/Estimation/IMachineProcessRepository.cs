using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Machine and Process operations
/// </summary>
public interface IMachineProcessRepository
{
    Task<List<MachineGridDto>> GetMachineGridAsync(string contentDomainType);
    Task<List<MachineDto>> GetAllMachinesAsync();
    Task<List<OperationDto>> GetDefaultOperationsAsync(string domainType);
    Task<List<OperationSlabDto>> GetOperationSlabsAsync(long processId);
    Task<List<MachineItemDto>> GetMachineItemsAsync(long machineId);
    Task<List<DieToolDto>> GetMachineToolsAsync(long machineId);
    Task<List<IndasEstimo.Application.DTOs.Masters.MachineSlabDto>> GetMachineSlabsAsync(long machineId);
    Task<List<CategoryWastageSettingDto>> GetCategoryWastageSettingsAsync(long categoryId);
}
