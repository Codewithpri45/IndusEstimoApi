using IndasEstimo.Domain.Entities.ToolInventory;
namespace IndasEstimo.Application.Interfaces.Repositories.ToolInventory;

public interface IToolPurchaseOrderRepository
{
    // ==================== CRUD Operations ====================
    Task<long> SaveToolPurchaseOrderAsync(
        ToolPurchaseOrder main,
        List<ToolPurchaseOrderDetail> details,
        List<ToolPurchaseOrderTax> taxes,
        List<ToolPurchaseOrderOverhead> overheads,
        List<ToolPurchaseOrderRequisition> requisitions
    );

    Task<long> UpdateToolPurchaseOrderAsync(
        long transactionId,
        ToolPurchaseOrder main,
        List<ToolPurchaseOrderDetail> details,
        List<ToolPurchaseOrderTax> taxes,
        List<ToolPurchaseOrderOverhead> overheads,
        List<ToolPurchaseOrderRequisition> requisitions
    );

    Task<bool> DeleteToolPurchaseOrderAsync(long transactionId);

    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPONumberAsync(string prefix);
    Task<string?> GetVoucherNoAsync(long transactionId);
    Task<bool> IsToolPurchaseOrderApprovedAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    Task<List<Application.DTOs.ToolInventory.ToolPurchaseOrderDataDto>> GetToolPurchaseOrderDataAsync(long transactionId);
    Task<List<Application.DTOs.ToolInventory.ToolPOOverheadDataDto>> GetToolPurchaseOrderOverheadAsync(long transactionId);
    Task<List<Application.DTOs.ToolInventory.ToolPOTaxDataDto>> GetToolPurchaseOrderTaxAsync(long transactionId);
    Task<List<Application.DTOs.ToolInventory.ToolPurchaseOrderListDto>> GetToolPurchaseOrderListAsync(
        string fromDate, string toDate, string filterStr, bool detail);
    Task<List<Application.DTOs.ToolInventory.ToolPendingRequisitionDto>> GetPendingRequisitionsAsync();

    // ==================== Helper/Lookup Operations ====================
    Task<string> GetLastTransactionDateAsync();
    Task<List<Application.DTOs.ToolInventory.ToolSupplierDto>> GetSuppliersAsync();
    Task<List<Application.DTOs.ToolInventory.ToolContactPersonDto>> GetContactPersonsAsync(long ledgerId);
    Task<Application.DTOs.ToolInventory.ToolItemRateDto> GetItemRateAsync(long ledgerId, long toolId);
    Task<List<Application.DTOs.ToolInventory.ToolAllottedSupplierDto>> GetAllottedSuppliersAsync(long toolGroupId);
    Task<List<Application.DTOs.ToolInventory.ToolOverflowGridDto>> GetOverflowGridAsync(long toolId, long toolGroupId);
    Task<List<Application.DTOs.ToolInventory.ToolOverheadChargeHeadDto>> GetOverheadChargeHeadsAsync();
    Task<List<Application.DTOs.ToolInventory.ToolTaxChargeLedgerDto>> GetTaxChargeLedgersAsync();
    Task<List<Application.DTOs.ToolInventory.ToolHSNCodeDto>> GetHSNCodesAsync();
    Task<List<Application.DTOs.ToolInventory.ToolCurrencyDto>> GetCurrenciesAsync();
    Task<List<Application.DTOs.ToolInventory.ToolPOApprovalByDto>> GetPOApprovalByAsync();
    Task<bool> CheckPermissionAsync(long transactionId);
}
