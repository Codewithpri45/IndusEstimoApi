using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<List<ProductDto>>> GetAllProductsAsync();
    Task<Result<ProductDto>> GetProductByIdAsync(int productId);
    Task<Result<ProductDto>> GetProductByCodeAsync(string productCode);
    Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<Result<ProductDto>> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task<Result> DeleteProductAsync(int productId);
}
