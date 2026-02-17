using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IItemIssueDirectService
{
    Task<Result<string>> GetIssueNoAsync(string prefix);
    Task<Result<List<FloorWarehouseDto>>> GetWarehouseListAsync();
    Task<Result<List<FloorBinDto>>> GetBinsListAsync(string warehouseName);
    Task<Result<List<JobCardRenderDto>>> GetJobCardRenderAsync();
    Task<Result<List<JobAllocatedPicklistDto>>> GetJobAllocatedPicklistAsync();
    Task<Result<List<AllPicklistDto>>> GetAllPicklistAsync();
    Task<Result<List<StockBatchWiseDto>>> GetStockBatchWiseAsync(long itemId, long jobBookingJobCardContentsId);
    Task<Result<List<IssueListDto>>> GetIssueListAsync(string fromDate, string toDate);
    Task<Result<List<IssueVoucherDetailDto>>> GetIssueVoucherDetailsAsync(long transactionId);
    Task<Result<List<IssueHeaderDto>>> GetHeaderNameAsync(long transactionId);
    Task<Result<IssueSaveResultDto>> SaveIssueDataAsync(SaveIssueDataRequest request);
    Task<Result<bool>> UpdateIssueAsync(UpdateIssueDataRequest request);
    Task<Result<bool>> DeleteIssueAsync(DeleteIssueRequest request);
}
