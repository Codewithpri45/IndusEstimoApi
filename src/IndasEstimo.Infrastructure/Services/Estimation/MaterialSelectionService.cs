using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Material Selection operations
/// Contains business logic and error handling for material dropdowns
/// </summary>
public class MaterialSelectionService : IMaterialSelectionService
{
    private readonly IMaterialSelectionRepository _repository;
    private readonly ILogger<MaterialSelectionService> _logger;

    public MaterialSelectionService(
        IMaterialSelectionRepository repository,
        ILogger<MaterialSelectionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<QualityDto>>> GetQualityAsync(string contentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<List<QualityDto>>.Failure("Content type is required");
            }

            var data = await _repository.GetQualityAsync(contentType);
            return Result<List<QualityDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quality list for content type: {ContentType}", contentType);
            return Result<List<QualityDto>>.Failure("Failed to get quality list");
        }
    }

    public async Task<Result<List<GsmDto>>> GetGsmAsync(string contentType, string? quality = null, decimal thickness = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<List<GsmDto>>.Failure("Content type is required");
            }

            var data = await _repository.GetGsmAsync(contentType, quality, thickness);
            return Result<List<GsmDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GSM list for content type: {ContentType}, quality: {Quality}", contentType, quality);
            return Result<List<GsmDto>>.Failure("Failed to get GSM list");
        }
    }

    public async Task<Result<List<ThicknessDto>>> GetThicknessAsync(string contentType, string? quality = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<List<ThicknessDto>>.Failure("Content type is required");
            }

            var data = await _repository.GetThicknessAsync(contentType, quality);
            return Result<List<ThicknessDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting thickness list for content type: {ContentType}, quality: {Quality}", contentType, quality);
            return Result<List<ThicknessDto>>.Failure("Failed to get thickness list");
        }
    }

    public async Task<Result<List<MillDto>>> GetMillAsync(string contentType, string? quality = null, decimal gsm = 0, decimal thickness = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return Result<List<MillDto>>.Failure("Content type is required");
            }

            var data = await _repository.GetMillAsync(contentType, quality, gsm, thickness);
            return Result<List<MillDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mill list for content type: {ContentType}, quality: {Quality}, gsm: {GSM}", 
                contentType, quality, gsm);
            return Result<List<MillDto>>.Failure("Failed to get mill list");
        }
    }

    public async Task<Result<List<FinishDto>>> GetFinishAsync(string? quality = null, decimal gsm = 0, string? mill = null)
    {
        try
        {
            var data = await _repository.GetFinishAsync(quality, gsm, mill);
            return Result<List<FinishDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting finish list for quality: {Quality}, gsm: {GSM}, mill: {Mill}", 
                quality, gsm, mill);
            return Result<List<FinishDto>>.Failure("Failed to get finish list");
        }
    }

    public async Task<Result<List<CoatingDto>>> GetCoatingAsync()
    {
        try
        {
            var data = await _repository.GetCoatingAsync();
            return Result<List<CoatingDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coating list");
            return Result<List<CoatingDto>>.Failure("Failed to get coating list");
        }
    }

    public async Task<Result<List<BFDto>>> GetBFAsync(string? quality = null, decimal gsm = 0, string? mill = null)
    {
        try
        {
            var data = await _repository.GetBFAsync(quality, gsm, mill);
            return Result<List<BFDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BF list for quality: {Quality}, gsm: {GSM}, mill: {Mill}", 
                quality, gsm, mill);
            return Result<List<BFDto>>.Failure("Failed to get BF list");
        }
    }

    public async Task<Result<List<FluteDto>>> GetFluteAsync()
    {
        try
        {
            var data = await _repository.GetFluteAsync();
            return Result<List<FluteDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flute type list");
            return Result<List<FluteDto>>.Failure("Failed to get flute type list");
        }
    }

    public async Task<Result<List<LayerItemDto>>> GetLayerItemsAsync()
    {
        try
        {
            var data = await _repository.GetLayerItemsAsync();
            return Result<List<LayerItemDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting layer items");
            return Result<List<LayerItemDto>>.Failure("Failed to get layer items");
        }
    }

    public async Task<Result<List<AvailableLayerDto>>> GetAvailableLayersAsync(decimal width, decimal widthTolerance)
    {
        try
        {
            if (width <= 0)
            {
                return Result<List<AvailableLayerDto>>.Failure("Width must be greater than zero");
            }

            if (widthTolerance < 0)
            {
                return Result<List<AvailableLayerDto>>.Failure("Width tolerance cannot be negative");
            }

            var data = await _repository.GetAvailableLayersAsync(width, widthTolerance);
            return Result<List<AvailableLayerDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available layers for width: {Width}, tolerance: {Tolerance}", 
                width, widthTolerance);
            return Result<List<AvailableLayerDto>>.Failure("Failed to get available layers");
        }
    }

    public async Task<Result<FilteredPaperDto>> GetFilteredPaperAsync(string? quality = null, string? gsm = null, string? mill = null)
    {
        try
        {
            var data = await _repository.GetFilteredPaperAsync(quality, gsm, mill);
            return Result<FilteredPaperDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered paper for quality: {Quality}, gsm: {GSM}, mill: {Mill}", 
                quality, gsm, mill);
            return Result<FilteredPaperDto>.Failure("Failed to get filtered paper data");
        }
    }
}
