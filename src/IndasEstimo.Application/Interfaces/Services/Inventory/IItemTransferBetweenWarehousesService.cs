using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IItemTransferBetweenWarehousesService
{
    Task<Result<List<TransferListDto>>> GetTransferListAsync(string fromDate, string toDate);
    Task<Result<List<WarehouseStockDto>>> GetWarehouseStockAsync(long warehouseId);
    Task<Result<List<BinDto>>> GetDestinationBinsAsync(string warehouseName, long sourceBinId);
    Task<Result<string>> GetTransferVoucherNoAsync(string prefix);
    Task<Result<TransferOperationResponseDto>> SaveTransferAsync(SaveTransferRequest request);
    Task<Result<bool>> UpdateTransferAsync(UpdateTransferRequest request);
    Task<Result<bool>> DeleteTransferAsync(DeleteTransferRequest request);
}
