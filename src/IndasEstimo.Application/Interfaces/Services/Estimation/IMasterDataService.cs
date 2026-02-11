using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Services.Estimation;

/// <summary>
/// Service interface for Master Data operations
/// </summary>
public interface IMasterDataService
{
    Task<Result<List<CategoryDto>>> GetCategoriesAsync();
    Task<Result<List<ClientDto>>> GetClientsAsync();
    Task<Result<List<SalesPersonDto>>> GetSalesPersonsAsync();
    Task<Result<List<ContentDto>>> GetAllContentsAsync();
    Task<Result<List<ContentByCategoryDto>>> GetContentsByCategoryAsync(long categoryId);
    Task<Result<CategoryDefaultsDto>> GetCategoryDefaultsAsync(long categoryId);
    Task<Result<List<WindingDirectionDto>>> GetWindingDirectionAsync(string contentDomainType);
    Task<Result<List<BookContentDto>>>GetBookContentsAsync();
    Task<Result<List<OneTimeChargeDto>>> GetOneTimeChargesAsync();
    Task<Result<ContentSizeDto>> GetContentSizeAsync(string contentName);
}
