using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services;

public interface IRequisitionApprovalService
{
    Task<Result<List<UnapprovedRequisitionDto>>> GetUnapprovedRequisitionsAsync(string fromDate, string toDate);
    Task<Result<List<ApprovedRequisitionDto>>> GetApprovedRequisitionsAsync(string fromDate, string toDate);
    Task<Result<List<CancelledRequisitionDto>>> GetCancelledRequisitionsAsync(string fromDate, string toDate);
    Task<Result<bool>> ApproveRequisitionsAsync(List<RequisitionApprovalItem> items);
    Task<Result<bool>> CancelRequisitionsAsync(List<RequisitionCancellationItem> items);
    Task<Result<bool>> UnapproveRequisitionsAsync(List<RequisitionApprovalItem> items);
    Task<Result<bool>> UncancelRequisitionsAsync(List<RequisitionCancellationItem> items);
}
