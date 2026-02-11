using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
namespace IndasEstimo.Application.Interfaces.Services.ToolInventory;

public interface IToolPurchaseOrderService
{
    // ==================== CRUD Operations ====================
    Task<Result<SaveToolPurchaseOrderResponse>> SaveToolPurchaseOrderAsync(SaveToolPurchaseOrderRequest request);
    Task<Result<SaveToolPurchaseOrderResponse>> UpdateToolPurchaseOrderAsync(UpdateToolPurchaseOrderRequest request);
    Task<Result<string>> DeleteToolPurchaseOrderAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    Task<Result<List<ToolPurchaseOrderDataDto>>> GetToolPurchaseOrderAsync(long transactionId);
    Task<Result<List<ToolPOOverheadDataDto>>> GetToolPurchaseOrderOverheadAsync(long transactionId);
    Task<Result<List<ToolPOTaxDataDto>>> GetToolPurchaseOrderTaxAsync(long transactionId);
    Task<Result<List<ToolPurchaseOrderListDto>>> GetToolPurchaseOrderListAsync(GetToolPurchaseOrderListRequest request);
    Task<Result<List<ToolPendingRequisitionDto>>> GetPendingRequisitionsAsync();

    // ==================== Helper/Lookup Operations ====================
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);
    Task<Result<string>> GetLastTransactionDateAsync();
    Task<Result<List<ToolSupplierDto>>> GetSuppliersAsync();
    Task<Result<List<ToolContactPersonDto>>> GetContactPersonsAsync(long ledgerId);
    Task<Result<ToolItemRateDto>> GetItemRateAsync(long ledgerId, long toolId);
    Task<Result<List<ToolAllottedSupplierDto>>> GetAllottedSuppliersAsync(long toolGroupId);
    Task<Result<List<ToolOverflowGridDto>>> GetOverflowGridAsync(long toolId, long toolGroupId);
    Task<Result<List<ToolOverheadChargeHeadDto>>> GetOverheadChargeHeadsAsync();
    Task<Result<List<ToolTaxChargeLedgerDto>>> GetTaxChargeLedgersAsync();
    Task<Result<List<ToolHSNCodeDto>>> GetHSNCodesAsync();
    Task<Result<List<ToolCurrencyDto>>> GetCurrenciesAsync();
    Task<Result<List<ToolPOApprovalByDto>>> GetPOApprovalByAsync();
    Task<Result<string>> CheckPermissionAsync(long transactionId);
}
