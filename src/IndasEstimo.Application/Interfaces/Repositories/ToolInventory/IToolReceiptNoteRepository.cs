using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Repositories.ToolInventory;

public interface IToolReceiptNoteRepository
{
    // ==================== CRUD Operations ====================
    Task<long> SaveToolReceiptNoteAsync(
        ToolReceiptNote main,
        List<ToolReceiptNoteDetail> details
    );

    Task<long> UpdateToolReceiptNoteAsync(
        long transactionId,
        ToolReceiptNote main,
        List<ToolReceiptNoteDetail> details
    );

    Task<bool> DeleteToolReceiptNoteAsync(long transactionId);

    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextReceiptNoAsync(string prefix);
    Task<string?> GetVoucherNoAsync(long transactionId);
    Task<bool> IsToolReceiptNoteUsedAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    Task<List<ToolReceiptNoteDataDto>> GetToolReceiptNoteDataAsync(long transactionId);
    Task<List<ToolReceiptNoteListDto>> GetToolReceiptNoteListAsync(string fromDate, string toDate);
    Task<List<ToolPendingPurchaseOrderDto>> GetPendingPurchaseOrdersAsync();

    // ==================== Helper/Lookup Operations ====================
    Task<string> GetLastTransactionDateAsync();
    Task<List<ToolReceiverDto>> GetReceiversAsync();
    Task<ToolPreviousReceivedQtyDto> GetPreviousReceivedQuantityAsync(long purchaseTransactionId, long toolId, long grnTransactionId);
    Task<List<ToolWarehouseDto>> GetWarehousesAsync();
    Task<List<ToolBinDto>> GetBinsAsync();
    Task<bool> CheckPermissionAsync(long transactionId);
}
