using IndasEstimo.Application.DTOs.Estimation;

namespace IndasEstimo.Application.Interfaces.Repositories.Estimation;

/// <summary>
/// Repository interface for Master Data operations
/// </summary>
public interface IMasterDataRepository
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<ClientDto>> GetClientsAsync();
    Task<List<SalesPersonDto>> GetSalesPersonsAsync();
    Task<List<ContentDto>> GetAllContentsAsync();
    Task<List<ContentByCategoryDto>> GetContentsByCategoryAsync(long categoryId);
    Task<CategoryDefaultsDto?> GetCategoryDefaultsAsync(long categoryId);
    Task<List<WindingDirectionDto>> GetWindingDirectionAsync(string contentDomainType);
    Task<List<BookContentDto>> GetBookContentsAsync();
    Task<List<OneTimeChargeDto>> GetOneTimeChargesAsync();
    Task<ContentSizeDto?> GetContentSizeAsync(string contentName);
}
