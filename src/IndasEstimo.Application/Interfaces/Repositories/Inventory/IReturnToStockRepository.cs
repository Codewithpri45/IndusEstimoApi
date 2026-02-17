using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;

public interface IReturnToStockRepository
{
    Task<string> GetReturnNoAsync(string prefix);
    Task<List<WarehouseDto>> GetWarehouseListAsync();
    Task<List<BinDto>> GetBinsListAsync(string warehouseName);
    Task<List<BinDto>> GetDestinationBinsListAsync(string warehouseName);
    Task<List<MachineDto>> GetMachinesByDepartmentAsync(int departmentId);
    Task<List<ReturnListDto>> GetReturnListAsync(string fromDate, string toDate);
    Task<List<ReturnDetailDto>> GetReturnDetailsAsync(long transactionId);
    Task<List<FloorStockDto>> GetAvailableFloorStockAsync(string issueType);
    Task<ReturnOperationResponseDto> SaveReturnDataAsync(SaveReturnDataRequest request);
    Task UpdateReturnAsync(UpdateReturnDataRequest request);
    Task DeleteReturnAsync(long transactionId);
}
