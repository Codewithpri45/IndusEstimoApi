using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IPurchaseGRNRepository
{
    // Lookup/Master Data
    Task<List<PurchaseSupplierDto>> GetPurchaseSuppliersListAsync();
    Task<List<PendingPurchaseOrderDto>> GetPendingOrdersListAsync();
    Task<List<ReceiptNoteListDto>> GetReceiptNoteListAsync(string fromDate, string toDate);
    Task<List<ReceiptVoucherBatchDetailDto>> GetReceiptVoucherBatchDetailAsync(long transactionId);
    Task<List<ReceiverDto>> GetReceiverListAsync();
    Task<List<WarehouseDto>> GetWarehouseListAsync();
    Task<List<BinDto>> GetBinsListAsync(string warehouseName);
    Task<List<GatePassDto>> GetGatePassAsync(long ledgerId);
    Task<List<GRNItemDto>> GetGrnItemListAsync(long transactionId);
    Task<UserAuthorityDto?> GetUserAuthorityAsync();

    // Query/Validation Methods
    Task<PreviousReceivedQuantityDto?> GetPreviousReceivedQuantityAsync(long purchaseTransactionId, long itemId, long grnTransactionId);
    Task<string> ValidateSupplierBatchReceiptDataAsync(int voucherID, List<SupplierBatchItem> items);
    Task<string> CheckPermissionAsync(long transactionId);
    Task<string> GetLastTransactionDateAsync();
    Task<string> GetNextVoucherNoAsync(string prefix);

    // CRUD Operations
    Task<(bool Success, string VoucherNo, long TransactionID, string Message)> SaveReceiptDataAsync(
        SaveReceiptDataRequest request);

    Task<(bool Success, string Message)> UpdateReceiptDataAsync(
        UpdateReceiptDataRequest request);

    Task<(bool Success, string Message)> DeleteGRNAsync(
        DeleteGRNRequest request);
}
