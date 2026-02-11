using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Planning and Calculation operations
/// </summary>
public interface IPlanningCalculationRepository
{
    Task<CalculateOperationResponse> CalculateOperationAsync(CalculateOperationRequest request);
    Task<List<ChargeTypeDto>> GetChargeTypesAsync();
    Task<MaterialFormulaDto?> GetMaterialFormulaAsync(long itemSubGroupId, long plantId);
    Task<WastageSlabDto?> GetWastageSlabAsync(decimal actualSheets, string wastageType);
    Task<List<WastageTypeDto>> GetAllWastageTypesAsync();
    Task<KeylineCoordinatesDto?> GetKeylineCoordinatesAsync(string contentType, string? grain);
    Task<CorrugationPlanResponse> CalculateCorrugationAsync(CorrugationPlanRequest request);
}
