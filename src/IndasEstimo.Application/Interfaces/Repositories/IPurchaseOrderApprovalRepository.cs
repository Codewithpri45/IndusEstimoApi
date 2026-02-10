using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IPurchaseOrderApprovalRepository
{
    Task<List<UnapprovedPurchaseOrderDto>> GetUnapprovedPurchaseOrdersAsync(string fromDate, string toDate);
    Task<List<ApprovedPurchaseOrderDto>> GetApprovedPurchaseOrdersAsync(string fromDate, string toDate);
    Task<List<CancelledPurchaseOrderDto>> GetCancelledPurchaseOrdersAsync(string fromDate, string toDate);
    Task<bool> IsPurchaseOrderProcessedAsync(long transactionId);
    Task<bool> ApprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items);
    Task<bool> CancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items);
    Task<bool> UnapprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items);
    Task<bool> UncancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items);
}
