using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Estimation;
using IndasEstimo.Application.Interfaces.Repositories.Estimation;
using IndasEstimo.Application.Interfaces.Services.Estimation;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Estimation;

/// <summary>
/// Service implementation for Machine and Process operations
/// </summary>
public class MachineProcessService : IMachineProcessService
{
    private readonly IMachineProcessRepository _repository;
    private readonly ILogger<MachineProcessService> _logger;

    public MachineProcessService(
        IMachineProcessRepository repository,
        ILogger<MachineProcessService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<MachineGridDto>>> GetMachineGridAsync(string contentDomainType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentDomainType))
            {
                return Result<List<MachineGridDto>>.Failure("Content domain type is required");
            }

            var data = await _repository.GetMachineGridAsync(contentDomainType);
            return Result<List<MachineGridDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine grid for domain: {DomainType}", contentDomainType);
            return Result<List<MachineGridDto>>.Failure("Failed to get machine grid");
        }
    }

    public async Task<Result<List<MachineDto>>> GetAllMachinesAsync()
    {
        try
        {
            var data = await _repository.GetAllMachinesAsync();
            return Result<List<MachineDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all machines");
            return Result<List<MachineDto>>.Failure("Failed to get machines");
        }
    }

    public async Task<Result<List<OperationDto>>> GetDefaultOperationsAsync(string domainType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domainType))
            {
                return Result<List<OperationDto>>.Failure("Domain type is required");
            }

            var data = await _repository.GetDefaultOperationsAsync(domainType);
            return Result<List<OperationDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations for domain: {DomainType}", domainType);
            return Result<List<OperationDto>>.Failure("Failed to get operations");
        }
    }

    public async Task<Result<List<OperationSlabDto>>> GetOperationSlabsAsync(long processId)
    {
        try
        {
            if (processId <= 0)
            {
                return Result<List<OperationSlabDto>>.Failure("Invalid process ID");
            }

            var data = await _repository.GetOperationSlabsAsync(processId);
            return Result<List<OperationSlabDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operation slabs for process: {ProcessID}", processId);
            return Result<List<OperationSlabDto>>.Failure("Failed to get operation slabs");
        }
    }

    public async Task<Result<List<MachineItemDto>>> GetMachineItemsAsync(long machineId)
    {
        try
        {
            if (machineId <= 0)
            {
                return Result<List<MachineItemDto>>.Failure("Invalid machine ID");
            }

            var data = await _repository.GetMachineItemsAsync(machineId);
            return Result<List<MachineItemDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items for machine: {MachineID}", machineId);
            return Result<List<MachineItemDto>>.Failure("Failed to get machine items");
        }
    }
}
