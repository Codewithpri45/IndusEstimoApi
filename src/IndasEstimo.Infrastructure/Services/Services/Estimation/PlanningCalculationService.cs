using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Planning and Calculation operations
/// </summary>
public class PlanningCalculationService : IPlanningCalculationService
{
    private readonly IPlanningCalculationRepository _repository;
    private readonly ILogger<PlanningCalculationService> _logger;

    public PlanningCalculationService(
        IPlanningCalculationRepository repository,
        ILogger<PlanningCalculationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<CalculateOperationResponse>> CalculateOperationAsync(CalculateOperationRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<CalculateOperationResponse>.Failure("Request is required");
            }

            if (request.ProcessID <= 0)
            {
                return Result<CalculateOperationResponse>.Failure("Valid process ID is required");
            }

            if (request.Quantity <= 0)
            {
                return Result<CalculateOperationResponse>.Failure("Valid quantity is required");
            }

            var data = await _repository.CalculateOperationAsync(request);
            return Result<CalculateOperationResponse>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating operation for process: {ProcessID}", request?.ProcessID);
            return Result<CalculateOperationResponse>.Failure("Failed to calculate operation");
        }
    }

    public async Task<Result<List<ChargeTypeDto>>> GetChargeTypesAsync()
    {
        try
        {
            var data = await _repository.GetChargeTypesAsync();
            return Result<List<ChargeTypeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting charge types");
            return Result<List<ChargeTypeDto>>.Failure("Failed to get charge types");
        }
    }

    public async Task<Result<MaterialFormulaDto>> GetMaterialFormulaAsync(long itemSubGroupId, long plantId)
    {
        try
        {
            if (itemSubGroupId <= 0)
            {
                return Result<MaterialFormulaDto>.Failure("Valid item sub-group ID is required");
            }

            if (plantId <= 0)
            {
                return Result<MaterialFormulaDto>.Failure("Valid plant ID is required");
            }

            var data = await _repository.GetMaterialFormulaAsync(itemSubGroupId, plantId);
            
            if (data == null)
            {
                return Result<MaterialFormulaDto>.Failure("Formula not found for the specified item sub-group and plant");
            }

            return Result<MaterialFormulaDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material formula for item sub-group: {ItemSubGroupID}, plant: {PlantID}", 
                itemSubGroupId, plantId);
            return Result<MaterialFormulaDto>.Failure("Failed to get material formula");
        }
    }

    public async Task<Result<WastageSlabDto>> GetWastageSlabAsync(decimal actualSheets, string wastageType)
    {
        try
        {
            if (actualSheets <= 0)
            {
                return Result<WastageSlabDto>.Failure("Valid sheet count is required");
            }

            if (string.IsNullOrWhiteSpace(wastageType))
            {
                return Result<WastageSlabDto>.Failure("Wastage type is required");
            }

            var data = await _repository.GetWastageSlabAsync(actualSheets, wastageType);
            
            if (data == null)
            {
                return Result<WastageSlabDto>.Failure("No wastage slab found for the specified criteria");
            }

            return Result<WastageSlabDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wastage slab for sheets: {Sheets}, type: {Type}", 
                actualSheets, wastageType);
            return Result<WastageSlabDto>.Failure("Failed to get wastage slab");
        }
    }

    public async Task<Result<List<WastageTypeDto>>> GetAllWastageTypesAsync()
    {
        try
        {
            var data = await _repository.GetAllWastageTypesAsync();
            return Result<List<WastageTypeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wastage types");
            return Result<List<WastageTypeDto>>.Failure($"Failed to get wastage types: {ex.Message}");
        }
    }

    public async Task<Result<KeylineCoordinatesDto>> GetKeylineCoordinatesAsync(string contentType, string? grain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<KeylineCoordinatesDto>.Failure("Content type is required");
            }

            var data = await _repository.GetKeylineCoordinatesAsync(contentType, grain);
            
            if (data == null)
            {
                return Result<KeylineCoordinatesDto>.Failure("Keyline coordinates not found");
            }

            return Result<KeylineCoordinatesDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting keyline coordinates for content: {ContentType}, grain: {Grain}", 
                contentType, grain);
            return Result<KeylineCoordinatesDto>.Failure("Failed to get keyline coordinates");
        }
    }

    public async Task<Result<CorrugationPlanResponse>> CalculateCorrugationAsync(CorrugationPlanRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<CorrugationPlanResponse>.Failure("Request is required");
            }

            if (string.IsNullOrWhiteSpace(request.FluteType))
            {
                return Result<CorrugationPlanResponse>.Failure("Flute type is required");
            }

            if (request.GSM <= 0)
            {
                return Result<CorrugationPlanResponse>.Failure("Valid GSM is required");
            }

            var data = await _repository.CalculateCorrugationAsync(request);
            return Result<CorrugationPlanResponse>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating corrugation for flute: {FluteType}", request?.FluteType);
            return Result<CorrugationPlanResponse>.Failure("Failed to calculate corrugation");
        }
    }
}
