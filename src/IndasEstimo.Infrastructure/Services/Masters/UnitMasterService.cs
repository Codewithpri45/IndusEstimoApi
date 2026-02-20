using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class UnitMasterService : IUnitMasterService
{
    private readonly IUnitMasterRepository _repository;
    private readonly ILogger<UnitMasterService> _logger;

    public UnitMasterService(
        IUnitMasterRepository repository,
        ILogger<UnitMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<UnitListDto>>> GetUnitListAsync()
    {
        try
        {
            var result = await _repository.GetUnitListAsync();
            return Result<List<UnitListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit list");
            return Result<List<UnitListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveUnitAsync(SaveUnitRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UnitName))
                return Result<string>.Failure("Unit Name is required");

            var result = await _repository.SaveUnitAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving unit");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateUnitAsync(UpdateUnitRequest request)
    {
        try
        {
            if (request.UnitID <= 0)
                return Result<string>.Failure("Unit ID is required");

            var result = await _repository.UpdateUnitAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitID}", request.UnitID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteUnitAsync(long unitId)
    {
        try
        {
            var result = await _repository.DeleteUnitAsync(unitId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit {UnitID}", unitId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
