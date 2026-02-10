using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IPurchaseOrderService
{
    // ==================== CRUD Operations ====================
    Task<Result<SavePurchaseOrderResponse>> SavePurchaseOrderAsync(SavePurchaseOrderRequest request);
    Task<Result<SavePurchaseOrderResponse>> UpdatePurchaseOrderAsync(UpdatePurchaseOrderRequest request);
    Task<Result<string>> DeletePurchaseOrderAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    /// <summary>
    /// Get complete PO data for editing - matches RetrivePoCreateGrid WebMethod
    /// </summary>
    Task<Result<List<PurchaseOrderDataDto>>> GetPurchaseOrderAsync(long transactionId);

    /// <summary>
    /// Get PO list/grid with filters - matches ProcessFillGrid WebMethod
    /// </summary>
    Task<Result<List<PurchaseOrderListDto>>> GetPurchaseOrderListAsync(GetPurchaseOrderListRequest request);

    /// <summary>
    /// Get pending requisitions for PO creation - matches FillGrid WebMethod
    /// </summary>
    Task<Result<List<PendingRequisitionDto>>> GetPendingRequisitionsAsync();

    // ==================== Helper/Lookup Operations ====================
    /// <summary>
    /// Get next PO voucher number - matches GetPONO WebMethod
    /// </summary>
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);

    /// <summary>
    /// Get last transaction date - matches GetLastTransactionDate WebMethod
    /// </summary>
    Task<Result<string>> GetLastTransactionDateAsync();

    /// <summary>
    /// Get supplier list - matches Supplier WebMethod
    /// </summary>
    Task<Result<List<SupplierDto>>> GetSuppliersAsync();

    /// <summary>
    /// Get contact persons for a supplier - matches GetContactPerson WebMethod
    /// </summary>
    Task<Result<List<ContactPersonDto>>> GetContactPersonsAsync(long ledgerId);

    /// <summary>
    /// Get delivery addresses - matches SelectAddressGetData WebMethod
    /// </summary>
    Task<Result<List<DeliveryAddressDto>>> GetDeliveryAddressesAsync();

    /// <summary>
    /// Get overhead charge heads - matches HeadFun WebMethod
    /// </summary>
    Task<Result<List<OverheadChargeHeadDto>>> GetOverheadChargeHeadsAsync();

    /// <summary>
    /// Get tax charge ledgers - matches CHLname WebMethod
    /// </summary>
    Task<Result<List<TaxChargeLedgerDto>>> GetTaxChargeLedgersAsync();

    /// <summary>
    /// Get currency list - matches GetCurrencyList WebMethod
    /// </summary>
    Task<Result<List<CurrencyDto>>> GetCurrenciesAsync();

    /// <summary>
    /// Get HSN codes - matches GetAllHSN WebMethod
    /// </summary>
    Task<Result<List<HSNCodeDto>>> GetHSNCodesAsync();

    /// <summary>
    /// Get file attachments for a transaction - matches GetFiledata WebMethod
    /// </summary>
    Task<Result<List<AttachmentFileDto>>> GetAttachmentsAsync(long transactionId);
}