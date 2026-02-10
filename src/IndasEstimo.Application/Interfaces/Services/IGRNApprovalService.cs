using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IGRNApprovalService
{
    /// <summary>
    /// Get GRN list for approval/unapproval
    /// RadioValue: "Pending Receipt Note" or "Approved Receipt Note"
    /// </summary>
    Task<Result<List<GRNListDto>>> GetGRNListAsync(string radioValue, string fromDate, string toDate);

    /// <summary>
    /// Get batch detail for a specific GRN transaction
    /// Shows item-wise details with approval quantities
    /// </summary>
    Task<Result<List<GRNBatchDetailDto>>> GetGRNBatchDetailAsync(long transactionId, string radioValue);

    /// <summary>
    /// Approve or unapprove GRN items
    /// Updates IsVoucherItemApproved, ApprovedQuantity, RejectedQuantity, QCApprovalNO, etc.
    /// </summary>
    Task<Result<string>> ApproveGRNAsync(ApproveGRNRequest request);

    /// <summary>
    /// Check if GRN can be approved/unapproved
    /// Returns "Exist" if already processed, empty string if can be modified
    /// </summary>
    Task<Result<string>> CheckPermissionAsync(long transactionId);
}
