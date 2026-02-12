
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
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

    public async Task<Result<IEnumerable<ProcessDetailDto>>> GetProcessesAsync()
    {
        try
        {
            var list = await _repository.GetProcessesAsync();
            return Result<IEnumerable<ProcessDetailDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processes");
            return Result<IEnumerable<ProcessDetailDto>>.Failure("Failed to get processes");
        }
    }

    public async Task<Result<long>> CreateProcessAsync(CreateProcessDto process)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(process.ProcessName))
                return Result<long>.Failure("Process Name is required");

            var processId = await _repository.CreateProcessAsync(process);
            return Result<long>.Success(processId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating process");
            return Result<long>.Failure("Failed to create process");
        }
    }

    public async Task<Result<bool>> UpdateProcessAsync(UpdateProcessDto process)
    {
        try
        {
            var result = await _repository.UpdateProcessAsync(process);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating process {ProcessID}", process.ProcessID);
            return Result<bool>.Failure("Failed to update process");
        }
    }

    public async Task<Result<bool>> DeleteProcessAsync(long processId)
    {
        try
        {
            var result = await _repository.DeleteProcessAsync(processId);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting process {ProcessID}", processId);
            return Result<bool>.Failure("Failed to delete process");
        }
    }

    public async Task<Result<ProcessDetailDto>> GetProcessByIdAsync(long processId)
    {
        try
        {
            var result = await _repository.GetProcessByIdAsync(processId);
            if (result == null)
                return Result<ProcessDetailDto>.Failure("Process not found");

            return Result<ProcessDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process {ProcessID}", processId);
            return Result<ProcessDetailDto>.Failure("Failed to get process");
        }
    }
}
