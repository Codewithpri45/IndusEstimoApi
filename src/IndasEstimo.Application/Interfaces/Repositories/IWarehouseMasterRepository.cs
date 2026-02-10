using IndasEstimo.Application.DTOs.Masters;

namespace IndasEstimo.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for WarehouseMaster data access
/// </summary>
public interface IWarehouseMasterRepository
{
    Task<WarehouseCodeDto> GetWarehouseNoAsync();
    Task<List<CityDto>> GetCityListAsync();
    Task<string> SaveWarehouseAsync(SaveWarehouseRequest request);
    Task<string> UpdateWarehouseAsync(UpdateWarehouseRequest request);
    Task<List<WarehouseListDto>> GetWarehouseListAsync();
    Task<WarehouseBinDto?> GetBinNameAsync(string warehouseName);
    Task<string> DeleteWarehouseAsync(string warehouseId);
    Task<List<BranchDto>> GetBranchListAsync();
}
