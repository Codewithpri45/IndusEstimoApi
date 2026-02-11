using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Repositories.ToolInventory;

/// <summary>
/// Repository interface for Tool Issue data access (VoucherID = -43)
/// </summary>
public interface IToolIssueRepository
{
    // ==================== CRUD Operations ====================

    Task<(long TransactionID, string VoucherNo)> SaveToolIssueAsync(ToolIssue main, List<ToolIssueDetail> details);
    Task<bool> DeleteToolIssueAsync(long transactionId, long parentTransactionId);

    // ==================== Retrieve Operations ====================

    Task<List<ToolIssueVoucherDetailsDto>> GetIssueVoucherDetailsAsync(long transactionId);

    // ==================== Helper/Lookup Operations ====================

    Task<string> GenerateIssueNoAsync(string prefix);
    Task<List<WarehouseDto>> GetWarehouseListAsync();
    Task<List<BinDto>> GetBinsListAsync(string warehouseName);
    Task<List<StockBatchWiseDto>> GetStockBatchWiseAsync(long jobBookingJobCardContentsId);
    Task<List<JobCardDto>> GetJobCardNoAsync();
}
