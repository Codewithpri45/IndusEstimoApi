using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Services;

/// <summary>
/// Service interface for WarehouseMaster operations
/// </summary>
public interface IWarehouseMasterService
{
    /// <summary>
    /// Generate new warehouse code with prefix
    /// </summary>
    Task<Result<WarehouseCodeDto>> GetWarehouseNoAsync();

    /// <summary>
    /// Get list of all cities
    /// </summary>
    Task<Result<List<CityDto>>> GetCityListAsync();

    /// <summary>
    /// Save new warehouse record
    /// </summary>
    Task<Result<string>> SaveWarehouseAsync(SaveWarehouseRequest request);

    /// <summary>
    /// Update existing warehouse records
    /// </summary>
    Task<Result<string>> UpdateWarehouseAsync(UpdateWarehouseRequest request);

    /// <summary>
    /// Get list of all warehouses with production unit filtering
    /// </summary>
    Task<Result<List<WarehouseListDto>>> GetWarehouseListAsync();

    /// <summary>
    /// Get bin name by warehouse name
    /// </summary>
    Task<Result<WarehouseBinDto>> GetBinNameAsync(string warehouseName);

    /// <summary>
    /// Check permission and delete warehouse (soft delete)
    /// </summary>
    Task<Result<string>> DeleteWarehouseAsync(string warehouseId);

    /// <summary>
    /// Get list of all branches
    /// </summary>
    Task<Result<List<BranchDto>>> GetBranchListAsync();
}
