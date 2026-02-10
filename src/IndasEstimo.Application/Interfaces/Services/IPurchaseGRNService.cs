using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IPurchaseGRNService
{
    // Lookup/Master Data
    Task<Result<List<PurchaseSupplierDto>>> GetPurchaseSuppliersListAsync();
    Task<Result<List<PendingPurchaseOrderDto>>> GetPendingOrdersListAsync();
    Task<Result<List<ReceiptNoteListDto>>> GetReceiptNoteListAsync(string fromDate, string toDate);
    Task<Result<List<ReceiptVoucherBatchDetailDto>>> GetReceiptVoucherBatchDetailAsync(long transactionId);
    Task<Result<List<ReceiverDto>>> GetReceiverListAsync();
    Task<Result<List<WarehouseDto>>> GetWarehouseListAsync();
    Task<Result<List<BinDto>>> GetBinsListAsync(string warehouseName);
    Task<Result<List<GatePassDto>>> GetGatePassAsync(long ledgerId);
    Task<Result<List<GRNItemDto>>> GetGrnItemListAsync(long transactionId);
    Task<Result<UserAuthorityDto>> GetUserAuthorityAsync();

    // Query/Validation Methods
    Task<Result<PreviousReceivedQuantityDto>> GetPreviousReceivedQuantityAsync(
        long purchaseTransactionId, long itemId, long grnTransactionId);
    Task<Result<string>> ValidateSupplierBatchReceiptDataAsync(int voucherID, List<SupplierBatchItem> items);
    Task<Result<string>> CheckPermissionAsync(long transactionId);
    Task<Result<string>> GetLastTransactionDateAsync();

    // CRUD Operations
    Task<Result<(string VoucherNo, long TransactionID)>> SaveReceiptDataAsync(SaveReceiptDataRequest request);
    Task<Result<bool>> UpdateReceiptDataAsync(UpdateReceiptDataRequest request);
    Task<Result<bool>> DeleteGRNAsync(DeleteGRNRequest request);
}
