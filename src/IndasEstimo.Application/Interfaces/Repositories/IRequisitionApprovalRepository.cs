using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IRequisitionApprovalRepository
{
    Task<List<UnapprovedRequisitionDto>> GetUnapprovedRequisitionsAsync(string fromDate, string toDate);
    Task<List<ApprovedRequisitionDto>> GetApprovedRequisitionsAsync(string fromDate, string toDate);
    Task<List<CancelledRequisitionDto>> GetCancelledRequisitionsAsync(string fromDate, string toDate);
    Task<bool> ApproveRequisitionsAsync(List<RequisitionApprovalItem> items);
    Task<bool> CancelRequisitionsAsync(List<RequisitionCancellationItem> items);
    Task<bool> UnapproveRequisitionsAsync(List<RequisitionApprovalItem> items);
    Task<bool> UncancelRequisitionsAsync(List<RequisitionCancellationItem> items);
}
