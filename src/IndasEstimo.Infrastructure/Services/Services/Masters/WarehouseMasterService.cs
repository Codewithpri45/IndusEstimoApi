using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class WarehouseMasterService : IWarehouseMasterService
{
    private readonly IWarehouseMasterRepository _repository;
    private readonly ILogger<WarehouseMasterService> _logger;

    public WarehouseMasterService(
        IWarehouseMasterRepository repository,
        ILogger<WarehouseMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<WarehouseCodeDto>> GetWarehouseNoAsync()
    {
        try
        {
            var result = await _repository.GetWarehouseNoAsync();
            return Result<WarehouseCodeDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating warehouse number");
            return Result<WarehouseCodeDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CityDto>>> GetCityListAsync()
    {
        try
        {
            var result = await _repository.GetCityListAsync();
            return Result<List<CityDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting city list");
            return Result<List<CityDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveWarehouseAsync(SaveWarehouseRequest request)
    {
        try
        {
            var result = await _repository.SaveWarehouseAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving warehouse");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateWarehouseAsync(UpdateWarehouseRequest request)
    {
        try
        {
            var result = await _repository.UpdateWarehouseAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<WarehouseListDto>>> GetWarehouseListAsync()
    {
        try
        {
            var result = await _repository.GetWarehouseListAsync();
            return Result<List<WarehouseListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse list");
            return Result<List<WarehouseListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<WarehouseBinDto>> GetBinNameAsync(string warehouseName)
    {
        try
        {
            var result = await _repository.GetBinNameAsync(warehouseName);
            if (result == null)
            {
                return Result<WarehouseBinDto>.Failure("Warehouse not found");
            }
            return Result<WarehouseBinDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bin name for warehouse {WarehouseName}", warehouseName);
            return Result<WarehouseBinDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteWarehouseAsync(string warehouseId)
    {
        try
        {
            var result = await _repository.DeleteWarehouseAsync(warehouseId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", warehouseId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BranchDto>>> GetBranchListAsync()
    {
        try
        {
            var result = await _repository.GetBranchListAsync();
            return Result<List<BranchDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch list");
            return Result<List<BranchDto>>.Failure($"Error: {ex.Message}");
        }
    }
}
