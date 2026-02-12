
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories.Masters;

public interface IProductHSNMasterRepository
{
    Task<IEnumerable<ProductHSNDetailDto>> GetProductHSNsAsync();
    Task<long> CreateProductHSNAsync(CreateProductHSNDto hsn);
    Task<bool> UpdateProductHSNAsync(UpdateProductHSNDto hsn);
    Task<bool> DeleteProductHSNAsync(long id);
    Task<ProductHSNDetailDto> GetProductHSNByIdAsync(long id);
    Task<bool> IsHSNUsedAsync(long id);
}
