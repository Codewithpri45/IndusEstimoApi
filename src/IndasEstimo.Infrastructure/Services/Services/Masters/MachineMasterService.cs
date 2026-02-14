
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories.Masters;
using IndasEstimo.Application.Interfaces.Services.Masters;
using Microsoft.Extensions.Logging;

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

    public async Task<Result<string>> GenerateMachineCodeAsync()
    {
        try
        {
            var code = await _repository.GetMachineCodeAsync();
            return Result<string>.Success(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating machine code");
            return Result<string>.Failure("Failed to generate machine code");
        }
    }

    public async Task<Result<IEnumerable<CreateMachineDto>>> GetMachinesAsync()
    {
        try
        {
            var list = await _repository.GetMachinesAsync();
            return Result<IEnumerable<CreateMachineDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machines");
            return Result<IEnumerable<CreateMachineDto>>.Failure("Failed to get machines");
        }
    }

    public async Task<Result<long>> CreateMachineAsync(CreateMachineDto machine)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(machine.MachineName))
                return Result<long>.Failure("Machine Name is required");

            var machineId = await _repository.CreateMachineAsync(machine);
            return Result<long>.Success(machineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating machine");
            return Result<long>.Failure("Failed to create machine");
        }
    }

    public async Task<Result<bool>> UpdateMachineAsync(UpdateMachineDto machine)
    {
        try
        {
            var result = await _repository.UpdateMachineAsync(machine);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating machine {MachineID}", machine.MachineID);
            return Result<bool>.Failure("Failed to update machine");
        }
    }

    public async Task<Result<bool>> DeleteMachineAsync(long machineId)
    {
        try
        {
            var result = await _repository.DeleteMachineAsync(machineId);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine {MachineID}", machineId);
            return Result<bool>.Failure("Failed to delete machine");
        }
    }

    public async Task<Result<MachineDetailDto>> GetMachineByIdAsync(long machineId)
    {
        try
        {
            var result = await _repository.GetMachineByIdAsync(machineId);
            if (result == null)
                return Result<MachineDetailDto>.Failure("Machine not found");

            return Result<MachineDetailDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machine {MachineID}", machineId);
            return Result<MachineDetailDto>.Failure("Failed to get machine");
        }
    }
}
