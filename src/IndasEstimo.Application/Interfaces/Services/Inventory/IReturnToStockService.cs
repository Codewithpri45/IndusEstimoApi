using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IReturnToStockService
{
    Task<Result<string>> GetReturnNoAsync(string prefix);
    Task<Result<List<WarehouseDto>>> GetWarehouseListAsync();
    Task<Result<List<BinDto>>> GetBinsListAsync(string warehouseName);
    Task<Result<List<BinDto>>> GetDestinationBinsListAsync(string warehouseName);
    Task<Result<List<MachineDto>>> GetMachinesByDepartmentAsync(int departmentId);
    Task<Result<List<ReturnListDto>>> GetReturnListAsync(string fromDate, string toDate);
    Task<Result<List<ReturnDetailDto>>> GetReturnDetailsAsync(long transactionId);
    Task<Result<List<FloorStockDto>>> GetAvailableFloorStockAsync(string issueType);
    Task<Result<ReturnOperationResponseDto>> SaveReturnDataAsync(SaveReturnDataRequest request);
    Task<Result<bool>> UpdateReturnAsync(UpdateReturnDataRequest request);
    Task<Result<bool>> DeleteReturnAsync(DeleteReturnRequest request);
}
