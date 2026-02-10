using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories;

public interface IGRNApprovalRepository
{
    Task<List<GRNListDto>> GetGRNListAsync(string radioValue, string fromDate, string toDate);
    Task<List<GRNBatchDetailDto>> GetGRNBatchDetailAsync(long transactionId, string radioValue);
    Task<(bool Success, string Message)> ApproveGRNAsync(ApproveGRNRequest request);
    Task<string> CheckPermissionAsync(long transactionId);
}
