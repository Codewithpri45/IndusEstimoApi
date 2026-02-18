using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class ProcessMasterService : IProcessMasterService
{
    private readonly IProcessMasterRepository _repository;
    private readonly ILogger<ProcessMasterService> _logger;

    public ProcessMasterService(
        IProcessMasterRepository repository,
        ILogger<ProcessMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<ProcessListDto>>> GetProcessListAsync()
    {
        try
        {
            var result = await _repository.GetProcessListAsync();
            return Result<List<ProcessListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process list");
            return Result<List<ProcessListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessNameDto>>> GetProcessNamesAsync()
    {
        try
        {
            var result = await _repository.GetProcessNamesAsync();
            return Result<List<ProcessNameDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process names");
            return Result<List<ProcessNameDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ProcessLoadedDataDto>> GetProcessByIdAsync(int processId)
    {
        try
        {
            var result = await _repository.GetProcessByIdAsync(processId);
            return Result<ProcessLoadedDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process {ProcessID}", processId);
            return Result<ProcessLoadedDataDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessDepartmentDto>>> GetDepartmentsAsync()
    {
        try
        {
            var result = await _repository.GetDepartmentsAsync();
            return Result<List<ProcessDepartmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return Result<List<ProcessDepartmentDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TypeOfChargesDto>>> GetTypeOfChargesAsync()
    {
        try
        {
            var result = await _repository.GetTypeOfChargesAsync();
            return Result<List<TypeOfChargesDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting type of charges");
            return Result<List<TypeOfChargesDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<UnitDto>>> GetUnitsAsync()
    {
        try
        {
            var result = await _repository.GetUnitsAsync();
            return Result<List<UnitDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units");
            return Result<List<UnitDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessToolGroupDto>>> GetToolGroupListAsync()
    {
        try
        {
            var result = await _repository.GetToolGroupListAsync();
            return Result<List<ProcessToolGroupDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool group list");
            return Result<List<ProcessToolGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetMachineGridAsync()
    {
        try
        {
            var result = await _repository.GetMachineGridAsync();
            return Result<object>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine grid");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetItemGridAsync()
    {
        try
        {
            var result = await _repository.GetItemGridAsync();
            return Result<object>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item grid");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetContentGridAsync()
    {
        try
        {
            var result = await _repository.GetContentGridAsync();
            return Result<object>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content grid");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessSlabDto>>> GetExistingSlabsAsync(int processId)
    {
        try
        {
            var result = await _repository.GetExistingSlabsAsync(processId);
            return Result<List<ProcessSlabDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slabs for process {ProcessID}", processId);
            return Result<List<ProcessSlabDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessMachineAllocationDto>>> GetAllocatedMachinesAsync(int processId)
    {
        try
        {
            var result = await _repository.GetAllocatedMachinesAsync(processId);
            return Result<List<ProcessMachineAllocationDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allocated machines for process {ProcessID}", processId);
            return Result<List<ProcessMachineAllocationDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessMaterialAllocationDto>>> GetAllocatedMaterialsAsync(int processId)
    {
        try
        {
            var result = await _repository.GetAllocatedMaterialsAsync(processId);
            return Result<List<ProcessMaterialAllocationDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allocated materials for process {ProcessID}", processId);
            return Result<List<ProcessMaterialAllocationDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessInspectionParameterDto>>> GetInspectionParametersAsync(int processId)
    {
        try
        {
            var result = await _repository.GetInspectionParametersAsync(processId);
            return Result<List<ProcessInspectionParameterDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inspection parameters for process {ProcessID}", processId);
            return Result<List<ProcessInspectionParameterDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessLineClearanceParameterDto>>> GetLineClearanceParametersAsync(int processId)
    {
        try
        {
            var result = await _repository.GetLineClearanceParametersAsync(processId);
            return Result<List<ProcessLineClearanceParameterDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting line clearance parameters for process {ProcessID}", processId);
            return Result<List<ProcessLineClearanceParameterDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveProcessAsync(SaveProcessRequest request)
    {
        try
        {
            if (request.ProcessDetail == null || string.IsNullOrWhiteSpace(request.ProcessDetail.ProcessName))
            {
                return Result<string>.Failure("Process name is required");
            }

            var result = await _repository.SaveProcessAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving process");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateProcessAsync(UpdateProcessRequest request)
    {
        try
        {
            if (request.ProcessID <= 0)
            {
                return Result<string>.Failure("Process ID is required");
            }

            var result = await _repository.UpdateProcessAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating process {ProcessID}", request.ProcessID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteProcessAsync(int processId)
    {
        try
        {
            var result = await _repository.DeleteProcessAsync(processId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting process {ProcessID}", processId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
