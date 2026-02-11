using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Tool and Material operations
/// </summary>
public interface IToolMaterialRepository
{
    Task<List<DieToolDto>> SearchDiesAsync(SearchDiesRequest request);
    Task<List<ReelDto>> GetReelsAsync(decimal reqDeckle, decimal widthPlus, decimal widthMinus);
    Task<List<ProcessMaterialDto>> GetProcessMaterialsAsync(string processIds);
}
