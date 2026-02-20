using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class ProductGroupMasterService : IProductGroupMasterService
{
    private readonly IProductGroupMasterRepository _repository;
    private readonly ILogger<ProductGroupMasterService> _logger;

    public ProductGroupMasterService(
        IProductGroupMasterRepository repository,
        ILogger<ProductGroupMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ProductGroupListDto>>> GetProductGroupListAsync()
    {
        try
        {
            var result = await _repository.GetProductGroupListAsync();
            return Result<List<ProductGroupListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product group list");
            return Result<List<ProductGroupListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProductHSNDropdownDto>>> GetHSNDropdownAsync()
    {
        try
        {
            var result = await _repository.GetHSNDropdownAsync();
            return Result<List<ProductHSNDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSN dropdown");
            return Result<List<ProductHSNDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemGroupDropdownDto>>> GetItemGroupsAsync()
    {
        try
        {
            var result = await _repository.GetItemGroupsAsync();
            return Result<List<ItemGroupDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item groups");
            return Result<List<ItemGroupDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TaxTypeDto>>> GetTaxTypeAsync()
    {
        try
        {
            var result = await _repository.GetTaxTypeAsync();
            return Result<List<TaxTypeDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tax type");
            return Result<List<TaxTypeDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(int productHSNId)
    {
        try
        {
            var result = await _repository.CheckPermissionAsync(productHSNId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for product HSN {ProductHSNID}", productHSNId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveProductGroupAsync(SaveProductGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                return Result<string>.Failure("Display Name is required");

            var result = await _repository.SaveProductGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving product group");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateProductGroupAsync(UpdateProductGroupRequest request)
    {
        try
        {
            if (request.ProductHSNID <= 0)
                return Result<string>.Failure("Product HSN ID is required");

            var result = await _repository.UpdateProductGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product group {ProductHSNID}", request.ProductHSNID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteProductGroupAsync(int productHSNId)
    {
        try
        {
            var result = await _repository.DeleteProductGroupAsync(productHSNId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product group {ProductHSNID}", productHSNId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
