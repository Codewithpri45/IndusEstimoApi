using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Services.ToolInventory;

public interface IToolReceiptNoteService
{
    // ==================== CRUD Operations ====================
    Task<Result<SaveToolReceiptNoteResponse>> SaveToolReceiptNoteAsync(SaveToolReceiptNoteRequest request);
    Task<Result<SaveToolReceiptNoteResponse>> UpdateToolReceiptNoteAsync(UpdateToolReceiptNoteRequest request);
    Task<Result<string>> DeleteToolReceiptNoteAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    Task<Result<List<ToolReceiptNoteDataDto>>> GetToolReceiptNoteDataAsync(long transactionId);
    Task<Result<List<ToolReceiptNoteListDto>>> GetToolReceiptNoteListAsync(GetToolReceiptNoteListRequest request);
    Task<Result<List<ToolPendingPurchaseOrderDto>>> GetPendingPurchaseOrdersAsync();

    // ==================== Helper/Lookup Operations ====================
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);
    Task<Result<string>> GetLastTransactionDateAsync();
    Task<Result<List<ToolReceiverDto>>> GetReceiversAsync();
    Task<Result<ToolPreviousReceivedQtyDto>> GetPreviousReceivedQuantityAsync(long purchaseTransactionId, long toolId, long grnTransactionId);
    Task<Result<List<ToolWarehouseDto>>> GetWarehousesAsync();
    Task<Result<List<ToolBinDto>>> GetBinsAsync();
    Task<Result<string>> CheckPermissionAsync(long transactionId);
}
