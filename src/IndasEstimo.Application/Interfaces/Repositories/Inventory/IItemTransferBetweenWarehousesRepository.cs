using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;

public interface IItemTransferBetweenWarehousesRepository
{
    Task<List<TransferListDto>> GetTransferListAsync(string fromDate, string toDate);
    Task<List<WarehouseStockDto>> GetWarehouseStockAsync(long warehouseId);
    Task<List<BinDto>> GetDestinationBinsAsync(string warehouseName, long sourceBinId);
    Task<string> GetTransferVoucherNoAsync(string prefix);
    Task<TransferOperationResponseDto> SaveTransferAsync(SaveTransferRequest request);
    Task UpdateTransferAsync(UpdateTransferRequest request);
    Task DeleteTransferAsync(long transactionId);
}
