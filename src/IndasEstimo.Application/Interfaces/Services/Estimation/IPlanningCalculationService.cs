using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Planning and Calculation operations
/// </summary>
public interface IPlanningCalculationService
{
    Task<Result<CalculateOperationResponse>> CalculateOperationAsync(CalculateOperationRequest request);
    Task<Result<List<ChargeTypeDto>>> GetChargeTypesAsync();
    Task<Result<MaterialFormulaDto>> GetMaterialFormulaAsync(long itemSubGroupId, long plantId);
    Task<Result<WastageSlabDto>> GetWastageSlabAsync(decimal actualSheets, string wastageType);
    Task<Result<List<WastageTypeDto>>> GetAllWastageTypesAsync();
    Task<Result<KeylineCoordinatesDto>> GetKeylineCoordinatesAsync(string contentType, string? grain);
    Task<Result<CorrugationPlanResponse>> CalculateCorrugationAsync(CorrugationPlanRequest request);
}
