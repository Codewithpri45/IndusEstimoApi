using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class MachineMasterService : IMachineMasterService
{
    private readonly IMachineMasterRepository _repository;
    private readonly ILogger<MachineMasterService> _logger;

    public MachineMasterService(
        IMachineMasterRepository repository,
        ILogger<MachineMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<MachineListDto>>> GetMachineListAsync()
    {
        try
        {
            var data = await _repository.GetMachineListAsync();
            return Result<List<MachineListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine list");
            return Result<List<MachineListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineSlabDto>>> GetMachineSlabsAsync(int machineId)
    {
        try
        {
            var data = await _repository.GetMachineSlabsAsync(machineId);
            return Result<List<MachineSlabDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slabs for machine {MachineID}", machineId);
            return Result<List<MachineSlabDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineOnlineCoatingRateDto>>> GetMachineOnlineCoatingRatesAsync(int machineId)
    {
        try
        {
            var data = await _repository.GetMachineOnlineCoatingRatesAsync(machineId);
            return Result<List<MachineOnlineCoatingRateDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coating rates for machine {MachineID}", machineId);
            return Result<List<MachineOnlineCoatingRateDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineDepartmentDto>>> GetDepartmentsAsync()
    {
        try
        {
            var data = await _repository.GetDepartmentsAsync();
            return Result<List<MachineDepartmentDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return Result<List<MachineDepartmentDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineTypeDto>>> GetMachineTypesAsync()
    {
        try
        {
            var data = await _repository.GetMachineTypesAsync();
            return Result<List<MachineTypeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine types");
            return Result<List<MachineTypeDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineNameDto>>> GetMachineNamesAsync()
    {
        try
        {
            var data = await _repository.GetMachineNamesAsync();
            return Result<List<MachineNameDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine names");
            return Result<List<MachineNameDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineGroupAllocationDto>>> GetGroupGridAsync()
    {
        try
        {
            var data = await _repository.GetGroupGridAsync();
            return Result<List<MachineGroupAllocationDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group grid");
            return Result<List<MachineGroupAllocationDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetGroupAllocationIDsAsync(int machineId)
    {
        try
        {
            var data = await _repository.GetGroupAllocationIDsAsync(machineId);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group allocation IDs for machine {MachineID}", machineId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CoatingNameDto>>> GetCoatingNamesAsync()
    {
        try
        {
            var data = await _repository.GetCoatingNamesAsync();
            return Result<List<CoatingNameDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coating names");
            return Result<List<CoatingNameDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineToolDto>>> GetToolListAsync(int toolGroupId)
    {
        try
        {
            var data = await _repository.GetToolListAsync(toolGroupId);
            return Result<List<MachineToolDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool list for tool group {ToolGroupID}", toolGroupId);
            return Result<List<MachineToolDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetAllocatedToolsAsync(int machineId, int toolGroupId)
    {
        try
        {
            var data = await _repository.GetAllocatedToolsAsync(machineId, toolGroupId);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allocated tools for machine {MachineID}, toolGroup {ToolGroupID}", machineId, toolGroupId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetMachineCodeAsync()
    {
        try
        {
            var data = await _repository.GetMachineCodeAsync();
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine code");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckMachineNameExistsAsync(string machineName)
    {
        try
        {
            var result = await _repository.CheckMachineNameExistsAsync(machineName);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking machine name {MachineName}", machineName);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveMachineAsync(SaveMachineRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.MachineDetail.MachineName))
                return Result<string>.Failure("Machine name is required");

            var result = await _repository.SaveMachineAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving machine");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateMachineAsync(UpdateMachineRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.MachineDetail.MachineName))
                return Result<string>.Failure("Machine name is required");

            var result = await _repository.UpdateMachineAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine {MachineID}", request.MachineID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteMachineAsync(int machineId)
    {
        try
        {
            var result = await _repository.DeleteMachineAsync(machineId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine {MachineID}", machineId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveGroupAllocationAsync(SaveMachineGroupAllocationRequest request)
    {
        try
        {
            var result = await _repository.SaveGroupAllocationAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving group allocation for machine {MachineID}", request.MachineID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteGroupAllocationAsync(int machineId)
    {
        try
        {
            var result = await _repository.DeleteGroupAllocationAsync(machineId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group allocation for machine {MachineID}", machineId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveToolAllocationAsync(SaveMachineToolAllocationRequest request)
    {
        try
        {
            var result = await _repository.SaveToolAllocationAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool allocation for machine {MachineID}", request.MachineID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolAllocationAsync(int machineId, int toolGroupId)
    {
        try
        {
            var result = await _repository.DeleteToolAllocationAsync(machineId, toolGroupId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool allocation for machine {MachineID}, toolGroup {ToolGroupID}", machineId, toolGroupId);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
