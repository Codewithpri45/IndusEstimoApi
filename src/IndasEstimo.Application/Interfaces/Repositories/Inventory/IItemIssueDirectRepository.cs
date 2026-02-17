using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;

public interface IItemIssueDirectRepository
{
    Task<string> GetIssueNoAsync(string prefix);
    Task<List<FloorWarehouseDto>> GetWarehouseListAsync();
    Task<List<FloorBinDto>> GetBinsListAsync(string warehouseName);
    Task<List<JobCardRenderDto>> GetJobCardRenderAsync();
    Task<List<JobAllocatedPicklistDto>> GetJobAllocatedPicklistAsync();
    Task<List<AllPicklistDto>> GetAllPicklistAsync();
    Task<List<StockBatchWiseDto>> GetStockBatchWiseAsync(long itemId, long jobBookingJobCardContentsId);
    Task<List<IssueListDto>> GetIssueListAsync(string fromDate, string toDate);
    Task<List<IssueVoucherDetailDto>> GetIssueVoucherDetailsAsync(long transactionId);
    Task<List<IssueHeaderDto>> GetHeaderNameAsync(long transactionId);
    Task<IssueSaveResultDto> SaveIssueDataAsync(SaveIssueDataRequest request);
    Task UpdateIssueAsync(UpdateIssueDataRequest request);
    Task DeleteIssueAsync(long transactionId, long jobBookingJobCardContentsId);
}
