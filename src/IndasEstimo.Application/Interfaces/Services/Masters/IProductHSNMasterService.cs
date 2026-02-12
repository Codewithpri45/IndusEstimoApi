
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services.Masters;

public interface IProductHSNMasterService
{
    Task<Result<IEnumerable<ProductHSNDetailDto>>> GetProductHSNsAsync();
    Task<Result<long>> CreateProductHSNAsync(CreateProductHSNDto hsn);
    Task<Result<bool>> UpdateProductHSNAsync(UpdateProductHSNDto hsn);
    Task<Result<bool>> DeleteProductHSNAsync(long id);
    Task<Result<ProductHSNDetailDto>> GetProductHSNByIdAsync(long id);
}
