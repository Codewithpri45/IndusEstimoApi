
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class ProductHSNMasterService : IProductHSNMasterService
{
    private readonly IProductHSNMasterRepository _repository;
    private readonly ILogger<ProductHSNMasterService> _logger;

    public ProductHSNMasterService(
        IProductHSNMasterRepository repository,
        ILogger<ProductHSNMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProductHSNDetailDto>>> GetProductHSNsAsync()
    {
        try
        {
            var list = await _repository.GetProductHSNsAsync();
            return Result<IEnumerable<ProductHSNDetailDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSNs");
            return Result<IEnumerable<ProductHSNDetailDto>>.Failure("Failed to get HSN codes");
        }
    }

    public async Task<Result<long>> CreateProductHSNAsync(CreateProductHSNDto hsn)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hsn.ProductHSNName))
                return Result<long>.Failure("HSN Name is required");

            var id = await _repository.CreateProductHSNAsync(hsn);
            return Result<long>.Success(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HSN {HSNName}", hsn.ProductHSNName);
            return Result<long>.Failure("Failed to create HSN code");
        }
    }

    public async Task<Result<bool>> UpdateProductHSNAsync(UpdateProductHSNDto hsn)
    {
        try
        {
            var result = await _repository.UpdateProductHSNAsync(hsn);
            if (!result) return Result<bool>.Failure("HSN not found for update");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating HSN {ID}", hsn.ProductHSNID);
            return Result<bool>.Failure("Failed to update HSN code");
        }
    }

    public async Task<Result<bool>> DeleteProductHSNAsync(long id)
    {
        try
        {
            var isUsed = await _repository.IsHSNUsedAsync(id);
            if (isUsed)
                return Result<bool>.Failure("HSN code is used in other records and cannot be deleted.");

            var result = await _repository.DeleteProductHSNAsync(id);
            if (!result) return Result<bool>.Failure("HSN not found for deletion");
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting HSN {ID}", id);
            return Result<bool>.Failure("Failed to delete HSN code");
        }
    }

    public async Task<Result<ProductHSNDetailDto>> GetProductHSNByIdAsync(long id)
    {
        try
        {
            var hsn = await _repository.GetProductHSNByIdAsync(id);
            if (hsn == null) return Result<ProductHSNDetailDto>.Failure("HSN not found");
            return Result<ProductHSNDetailDto>.Success(hsn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSN {ID}", id);
            return Result<ProductHSNDetailDto>.Failure("Failed to get HSN details");
        }
    }
}
