using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class ProductionUnitMasterService : IProductionUnitMasterService
{
    private readonly IProductionUnitMasterRepository _repository;
    private readonly ILogger<ProductionUnitMasterService> _logger;

    public ProductionUnitMasterService(
        IProductionUnitMasterRepository repository,
        ILogger<ProductionUnitMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ProductionUnitListDto>>> GetProductionUnitListAsync()
    {
        try
        {
            var result = await _repository.GetProductionUnitListAsync();
            return Result<List<ProductionUnitListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production unit list");
            return Result<List<ProductionUnitListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetProductionUnitNoAsync()
    {
        try
        {
            var result = await _repository.GetProductionUnitNoAsync();
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production unit number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CountryDropdownDto>>> GetCountryAsync()
    {
        try
        {
            var result = await _repository.GetCountryAsync();
            return Result<List<CountryDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country list");
            return Result<List<CountryDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<StateDropdownDto>>> GetStateAsync()
    {
        try
        {
            var result = await _repository.GetStateAsync();
            return Result<List<StateDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting state list");
            return Result<List<StateDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CityDropdownDto>>> GetCityAsync()
    {
        try
        {
            var result = await _repository.GetCityAsync();
            return Result<List<CityDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting city list");
            return Result<List<CityDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CompanyDropdownDto>>> GetCompanyNameAsync()
    {
        try
        {
            var result = await _repository.GetCompanyNameAsync();
            return Result<List<CompanyDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company list");
            return Result<List<CompanyDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BranchDropdownDto>>> GetBranchAsync()
    {
        try
        {
            var result = await _repository.GetBranchAsync();
            return Result<List<BranchDropdownDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch list");
            return Result<List<BranchDropdownDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveProductionUnitAsync(SaveProductionUnitRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProductionUnitName))
                return Result<string>.Failure("Production Unit Name is required");

            var result = await _repository.SaveProductionUnitAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving production unit");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateProductionUnitAsync(UpdateProductionUnitRequest request)
    {
        try
        {
            if (request.ProductionUnitID <= 0)
                return Result<string>.Failure("Production Unit ID is required");

            if (string.IsNullOrWhiteSpace(request.ProductionUnitName))
                return Result<string>.Failure("Production Unit Name is required");

            var result = await _repository.UpdateProductionUnitAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production unit {ID}", request.ProductionUnitID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteProductionUnitAsync(long productionUnitId)
    {
        try
        {
            var result = await _repository.DeleteProductionUnitAsync(productionUnitId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting production unit {ID}", productionUnitId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
