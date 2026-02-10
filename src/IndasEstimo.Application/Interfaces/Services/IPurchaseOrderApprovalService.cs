using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IPurchaseOrderApprovalService
{
    Task<Result<List<UnapprovedPurchaseOrderDto>>> GetUnapprovedPurchaseOrdersAsync(string fromDate, string toDate);
    Task<Result<List<ApprovedPurchaseOrderDto>>> GetApprovedPurchaseOrdersAsync(string fromDate, string toDate);
    Task<Result<List<CancelledPurchaseOrderDto>>> GetCancelledPurchaseOrdersAsync(string fromDate, string toDate);
    Task<Result<bool>> IsPurchaseOrderProcessedAsync(long transactionId);
    Task<Result<bool>> ApprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items);
    Task<Result<bool>> CancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items);
    Task<Result<bool>> UnapprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items);
    Task<Result<bool>> UncancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items);
}
