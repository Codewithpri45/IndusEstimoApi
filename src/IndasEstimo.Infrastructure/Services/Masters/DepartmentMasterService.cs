using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class DepartmentMasterService : IDepartmentMasterService
{
    private readonly IDepartmentMasterRepository _repository;
    private readonly ILogger<DepartmentMasterService> _logger;

    public DepartmentMasterService(
        IDepartmentMasterRepository repository,
        ILogger<DepartmentMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<DepartmentListDto>>> GetDepartmentListAsync()
    {
        try
        {
            var result = await _repository.GetDepartmentListAsync();
            return Result<List<DepartmentListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department list");
            return Result<List<DepartmentListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveDepartmentAsync(SaveDepartmentRequest request)
    {
        try
        {
            var result = await _repository.SaveDepartmentAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving department");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateDepartmentAsync(UpdateDepartmentRequest request)
    {
        try
        {
            var result = await _repository.UpdateDepartmentAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteDepartmentAsync(int departmentId)
    {
        try
        {
            var result = await _repository.DeleteDepartmentAsync(departmentId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", departmentId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
