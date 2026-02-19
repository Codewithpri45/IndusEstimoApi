using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

/// <summary>
/// Machine Master service - handles machine management including slabs, coating rates, group/tool allocations
/// Migrated from legacy WebService_MachineMaster.vb
/// </summary>
public interface IMachineMasterService
{
    /// <summary>
    /// Get all machines with full detail for the grid
    /// </summary>
    Task<Result<List<MachineListDto>>> GetMachineListAsync();

    /// <summary>
    /// Get slab (rate tier) data for a specific machine
    /// </summary>
    Task<Result<List<MachineSlabDto>>> GetMachineSlabsAsync(int machineId);

    /// <summary>
    /// Get online coating rates for a specific machine
    /// </summary>
    Task<Result<List<MachineOnlineCoatingRateDto>>> GetMachineOnlineCoatingRatesAsync(int machineId);

    /// <summary>
    /// Get department dropdown for machine form
    /// </summary>
    Task<Result<List<MachineDepartmentDto>>> GetDepartmentsAsync();

    /// <summary>
    /// Get machine type dropdown values
    /// </summary>
    Task<Result<List<MachineTypeDto>>> GetMachineTypesAsync();

    /// <summary>
    /// Get machine name list for dropdowns
    /// </summary>
    Task<Result<List<MachineNameDto>>> GetMachineNamesAsync();

    /// <summary>
    /// Get item sub-groups for group allocation grid
    /// </summary>
    Task<Result<List<MachineGroupAllocationDto>>> GetGroupGridAsync();

    /// <summary>
    /// Get existing group allocation IDs for a machine (comma-separated string)
    /// </summary>
    Task<Result<string>> GetGroupAllocationIDsAsync(int machineId);

    /// <summary>
    /// Get distinct coating names for dropdown
    /// </summary>
    Task<Result<List<CoatingNameDto>>> GetCoatingNamesAsync();

    /// <summary>
    /// Get tools for a specific tool group for tool allocation
    /// </summary>
    Task<Result<List<MachineToolDto>>> GetToolListAsync(int toolGroupId);

    /// <summary>
    /// Get existing allocated tool ID string for a machine and tool group
    /// </summary>
    Task<Result<string>> GetAllocatedToolsAsync(int machineId, int toolGroupId);

    /// <summary>
    /// Generate next machine code with MM prefix
    /// </summary>
    Task<Result<string>> GetMachineCodeAsync();

    /// <summary>
    /// Check if machine name already exists
    /// Returns "Exist" if duplicate, empty string otherwise
    /// </summary>
    Task<Result<string>> CheckMachineNameExistsAsync(string machineName);

    /// <summary>
    /// Save new machine with slabs and coating rates
    /// Returns "Success", "Exist" (duplicate name), or "fail"
    /// </summary>
    Task<Result<string>> SaveMachineAsync(SaveMachineRequest request);

    /// <summary>
    /// Update existing machine with slabs and coating rates (replace strategy)
    /// Returns "Success" or "fail"
    /// </summary>
    Task<Result<string>> UpdateMachineAsync(UpdateMachineRequest request);

    /// <summary>
    /// Soft-delete a machine and its slabs (sets IsDeletedTransaction = 1)
    /// Checks JobBookingContents usage before deleting
    /// Returns "Success", "Further Used, Can't Delete Machine..!", or "fail"
    /// </summary>
    Task<Result<string>> DeleteMachineAsync(int machineId);

    /// <summary>
    /// Save item sub-group allocations for a machine (replace strategy)
    /// </summary>
    Task<Result<string>> SaveGroupAllocationAsync(SaveMachineGroupAllocationRequest request);

    /// <summary>
    /// Remove all group allocations for a machine (soft delete)
    /// </summary>
    Task<Result<string>> DeleteGroupAllocationAsync(int machineId);

    /// <summary>
    /// Save tool allocations for a machine and tool group (replace strategy)
    /// </summary>
    Task<Result<string>> SaveToolAllocationAsync(SaveMachineToolAllocationRequest request);

    /// <summary>
    /// Remove tool allocations for a machine and tool group (soft delete)
    /// </summary>
    Task<Result<string>> DeleteToolAllocationAsync(int machineId, int toolGroupId);
}
