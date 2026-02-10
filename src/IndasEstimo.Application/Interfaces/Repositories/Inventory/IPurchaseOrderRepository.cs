using IndasEstimo.Domain.Entities.Inventory;
namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;

public interface IPurchaseOrderRepository
{
    /// <summary>
    /// Saves complete purchase order with all related entities in a transaction
    /// </summary>
    Task<long> SavePurchaseOrderAsync(
        PurchaseOrder main,
        List<PurchaseOrderDetail> details,
        List<PurchaseOrderTax> taxes,
        List<PurchaseOrderSchedule> schedules,
        List<PurchaseOrderOverhead> overheads,
        List<PurchaseOrderRequisition> requisitions
    );
    /// <summary>
    /// Generates next PO number
    /// </summary>
    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPONumberAsync(string prefix);
    /// <summary>
    /// Gets the voucher number for a transaction
    /// </summary>
    Task<string?> GetVoucherNoAsync(long transactionId);
    /// <summary>
    /// Updates stock values after PO save
    /// </summary>
    Task UpdateStockValuesAsync(long transactionId);
    /// <summary>
    /// Creates approval workflow entries
    /// </summary>
    Task<string> CreateApprovalWorkflowAsync(
        long transactionId,
        long transactionDetailId,
        long itemId,
        string itemDescription,
        string displayModuleName,
        long moduleId,
        string voucherNo,
        long ledgerId,
        string itemName,
        decimal purchaseQty,
        decimal itemRate,
        decimal itemAmount
    );
    /// <summary>
    /// Updates an existing purchase order
    /// </summary>
    Task<long> UpdatePurchaseOrderAsync(
        long transactionId,
        PurchaseOrder main,
        List<PurchaseOrderDetail> details,
        List<PurchaseOrderTax> taxes,
        List<PurchaseOrderSchedule> schedules,
        List<PurchaseOrderOverhead> overheads,
        List<PurchaseOrderRequisition> requisitions
    );
    /// <summary>
    /// Checks if a purchase order is used in subsequent transactions
    /// </summary>
    Task<bool> IsPurchaseOrderUsedAsync(long transactionId);
    /// <summary>
    /// Checks if a purchase order is approved
    /// </summary>
    Task<bool> IsPurchaseOrderApprovedAsync(long transactionId);

    /// <summary>
    /// Soft deletes a purchase order and cleans up approvals
    /// </summary>
    Task<bool> DeletePurchaseOrderAsync(long transactionId);

    /// <summary>
    /// Checks if approval is required for a module based on company configuration
    /// </summary>
    Task<(bool IsApprovalRequired, int IsVoucherItemApproved, long ApprovalByUserId, long ModuleId, string DisplayModuleName)>
        CheckApprovalRequirementAsync(long companyId, string formName);

    // ==================== Retrieve Operations ====================
    /// <summary>
    /// Get complete PO data for editing - matches RetrivePoCreateGrid WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.PurchaseOrderDataDto>> GetPurchaseOrderDataAsync(long transactionId);

    /// <summary>
    /// Get PO list/grid with filters - matches ProcessFillGrid WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.PurchaseOrderListDto>> GetPurchaseOrderListAsync(
        string fromDate,
        string toDate,
        string filterStr,
        bool detail
    );

    /// <summary>
    /// Get pending requisitions for PO creation - matches FillGrid WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.PendingRequisitionDto>> GetPendingRequisitionsAsync();

    // ==================== Helper/Lookup Operations ====================
    /// <summary>
    /// Get last transaction date - matches GetLastTransactionDate WebMethod
    /// </summary>
    Task<string> GetLastTransactionDateAsync();

    /// <summary>
    /// Get supplier list - matches Supplier WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.SupplierDto>> GetSuppliersAsync();

    /// <summary>
    /// Get contact persons for a supplier - matches GetContactPerson WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.ContactPersonDto>> GetContactPersonsAsync(long ledgerId);

    /// <summary>
    /// Get delivery addresses - matches SelectAddressGetData WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.DeliveryAddressDto>> GetDeliveryAddressesAsync();

    /// <summary>
    /// Get overhead charge heads - matches HeadFun WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.OverheadChargeHeadDto>> GetOverheadChargeHeadsAsync();

    /// <summary>
    /// Get tax charge ledgers - matches CHLname WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.TaxChargeLedgerDto>> GetTaxChargeLedgersAsync();

    /// <summary>
    /// Get currency list - matches GetCurrencyList WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.CurrencyDto>> GetCurrenciesAsync();

    /// <summary>
    /// Get HSN codes - matches GetAllHSN WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.HSNCodeDto>> GetHSNCodesAsync();

    /// <summary>
    /// Get file attachments for a transaction - matches GetFiledata WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.AttachmentFileDto>> GetAttachmentsAsync(long transactionId);
}