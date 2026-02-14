using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Master Data operations
/// </summary>
public class MasterDataService : IMasterDataService
{
    private readonly IMasterDataRepository _repository;
    private readonly ILogger<MasterDataService> _logger;

    public MasterDataService(
        IMasterDataRepository repository,
        ILogger<MasterDataService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var data = await _repository.GetCategoriesAsync();
            return Result<List<CategoryDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Result<List<CategoryDto>>.Failure("Failed to get categories");
        }
    }

    public async Task<Result<List<ClientDto>>> GetClientsAsync()
    {
        try
        {
            var data = await _repository.GetClientsAsync();
            return Result<List<ClientDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clients");
            return Result<List<ClientDto>>.Failure("Failed to get clients");
        }
    }

    public async Task<Result<List<SalesPersonDto>>> GetSalesPersonsAsync()
    {
        try
        {
            var data = await _repository.GetSalesPersonsAsync();
            return Result<List<SalesPersonDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales persons");
            return Result<List<SalesPersonDto>>.Failure("Failed to get sales persons");
        }
    }

    public async Task<Result<List<ContentDto>>> GetAllContentsAsync()
    {
        try
        {
            var data = await _repository.GetAllContentsAsync();
            return Result<List<ContentDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all contents");
            return Result<List<ContentDto>>.Failure("Failed to get contents");
        }
    }

    public async Task<Result<List<ContentByCategoryDto>>> GetContentsByCategoryAsync(long categoryId)
    {
        try
        {
            if (categoryId <= 0)
            {
                return Result<List<ContentByCategoryDto>>.Failure("Invalid category ID");
            }

            var data = await _repository.GetContentsByCategoryAsync(categoryId);
            return Result<List<ContentByCategoryDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contents for category: {CategoryID}", categoryId);
            return Result<List<ContentByCategoryDto>>.Failure("Failed to get contents by category");
        }
    }

    public async Task<Result<CategoryDefaultsDto>> GetCategoryDefaultsAsync(long categoryId)
    {
        try
        {
            if (categoryId <= 0)
            {
                return Result<CategoryDefaultsDto>.Failure("Invalid category ID");
            }

            var data = await _repository.GetCategoryDefaultsAsync(categoryId);
            if (data == null)
            {
                return Result<CategoryDefaultsDto>.Failure("Category not found");
            }

            return Result<CategoryDefaultsDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category defaults for category: {CategoryID}", categoryId);
            return Result<CategoryDefaultsDto>.Failure("Failed to get category defaults");
        }
    }

    public async Task<Result<List<WindingDirectionDto>>> GetWindingDirectionAsync(string contentDomainType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentDomainType))
            {
                return Result<List<WindingDirectionDto>>.Failure("Content domain type is required");
            }

            var data = await _repository.GetWindingDirectionAsync(contentDomainType);
            return Result<List<WindingDirectionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting winding direction for content type: {ContentType}", contentDomainType);
            return Result<List<WindingDirectionDto>>.Failure("Failed to get winding direction");
        }
    }

    public async Task<Result<List<BookContentDto>>> GetBookContentsAsync()
    {
        try
        {
            var data = await _repository.GetBookContentsAsync();
            return Result<List<BookContentDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book contents");
            return Result<List<BookContentDto>>.Failure("Failed to get book contents");
        }
    }

    public async Task<Result<List<OneTimeChargeDto>>> GetOneTimeChargesAsync()
    {
        try
        {
            var data = await _repository.GetOneTimeChargesAsync();
            return Result<List<OneTimeChargeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting one-time charges");
            return Result<List<OneTimeChargeDto>>.Failure("Failed to get one-time charges");
        }
    }

    public async Task<Result<ContentSizeDto>> GetContentSizeAsync(string contentName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentName))
            {
                return Result<ContentSizeDto>.Failure("Content name is required");
            }

            var data = await _repository.GetContentSizeAsync(contentName);
            if (data == null)
            {
                return Result<ContentSizeDto>.Failure("Content not found");
            }

            return Result<ContentSizeDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content size for: {ContentName}", contentName);
            return Result<ContentSizeDto>.Failure("Failed to get content size");
        }
    }
}
