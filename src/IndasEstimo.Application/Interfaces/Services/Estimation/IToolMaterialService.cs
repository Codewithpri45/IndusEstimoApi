using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Tool and Material operations
/// </summary>
public interface IToolMaterialService
{
    Task<Result<List<DieToolDto>>> SearchDiesAsync(SearchDiesRequest request);
    Task<Result<List<ReelDto>>> GetReelsAsync(decimal reqDeckle, decimal widthPlus, decimal widthMinus);
    Task<Result<List<ProcessMaterialDto>>> GetProcessMaterialsAsync(string processIds);
}
