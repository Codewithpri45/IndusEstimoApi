using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Services.ToolInventory;

/// <summary>
/// Service interface for Tool Issue operations (VoucherID = -43)
/// Handles issuing tools from warehouse for job cards/production
/// </summary>
public interface IToolIssueService
{
    // ==================== CRUD Operations ====================

    /// <summary>
    /// Save new tool issue
    /// </summary>
    Task<Result<ToolIssueResponse>> SaveToolIssueAsync(SaveToolIssueRequest request);

    /// <summary>
    /// Delete tool issue (soft delete)
    /// </summary>
    Task<Result<string>> DeleteToolIssueAsync(DeleteToolIssueRequest request);

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get tool issue voucher details by transaction ID
    /// </summary>
    Task<Result<List<ToolIssueVoucherDetailsDto>>> GetIssueVoucherDetailsAsync(long transactionId);

    // ==================== Helper/Lookup Operations ====================

    /// <summary>
    /// Generate next issue voucher number
    /// </summary>
    Task<Result<string>> GetIssueNoAsync(string prefix);

    /// <summary>
    /// Get list of warehouses
    /// </summary>
    Task<Result<List<WarehouseDto>>> GetWarehouseListAsync();

    /// <summary>
    /// Get list of bins/floor warehouses by warehouse name
    /// </summary>
    Task<Result<List<BinDto>>> GetBinsListAsync(string warehouseName);

    /// <summary>
    /// Get batch-wise stock for a job card
    /// </summary>
    Task<Result<List<StockBatchWiseDto>>> GetStockBatchWiseAsync(long jobBookingJobCardContentsId);

    /// <summary>
    /// Get list of job cards
    /// </summary>
    Task<Result<List<JobCardDto>>> GetJobCardNoAsync();
}
