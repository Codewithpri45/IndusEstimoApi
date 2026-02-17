using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class ItemIssueDirectService : IItemIssueDirectService
{
    private readonly IItemIssueDirectRepository _repository;
    private readonly ILogger<ItemIssueDirectService> _logger;

    public ItemIssueDirectService(
        IItemIssueDirectRepository repository,
        ILogger<ItemIssueDirectService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> GetIssueNoAsync(string prefix)
    {
        try
        {
            var data = await _repository.GetIssueNoAsync(prefix);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating issue number for prefix {Prefix}", prefix);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<FloorWarehouseDto>>> GetWarehouseListAsync()
    {
        try
        {
            var data = await _repository.GetWarehouseListAsync();
            return Result<List<FloorWarehouseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting floor warehouse list");
            return Result<List<FloorWarehouseDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<FloorBinDto>>> GetBinsListAsync(string warehouseName)
    {
        try
        {
            var data = await _repository.GetBinsListAsync(warehouseName);
            return Result<List<FloorBinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting floor bins list for warehouse {WarehouseName}", warehouseName);
            return Result<List<FloorBinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<JobCardRenderDto>>> GetJobCardRenderAsync()
    {
        try
        {
            var data = await _repository.GetJobCardRenderAsync();
            return Result<List<JobCardRenderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job card render data");
            return Result<List<JobCardRenderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<JobAllocatedPicklistDto>>> GetJobAllocatedPicklistAsync()
    {
        try
        {
            var data = await _repository.GetJobAllocatedPicklistAsync();
            return Result<List<JobAllocatedPicklistDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job allocated picklist");
            return Result<List<JobAllocatedPicklistDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<AllPicklistDto>>> GetAllPicklistAsync()
    {
        try
        {
            var data = await _repository.GetAllPicklistAsync();
            return Result<List<AllPicklistDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all picklist");
            return Result<List<AllPicklistDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<StockBatchWiseDto>>> GetStockBatchWiseAsync(long itemId, long jobBookingJobCardContentsId)
    {
        try
        {
            var data = await _repository.GetStockBatchWiseAsync(itemId, jobBookingJobCardContentsId);
            return Result<List<StockBatchWiseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock batch wise for item {ItemId}", itemId);
            return Result<List<StockBatchWiseDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<IssueListDto>>> GetIssueListAsync(string fromDate, string toDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromDate))
            {
                var today = DateTime.Today;
                fromDate = new DateTime(today.Year, today.Month, 1).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrWhiteSpace(toDate))
                toDate = DateTime.Today.ToString("dd-MM-yyyy");

            var data = await _repository.GetIssueListAsync(fromDate, toDate);
            return Result<List<IssueListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issue list from {FromDate} to {ToDate}", fromDate, toDate);
            return Result<List<IssueListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<IssueVoucherDetailDto>>> GetIssueVoucherDetailsAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetIssueVoucherDetailsAsync(transactionId);
            return Result<List<IssueVoucherDetailDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issue voucher details for transaction {TransactionId}", transactionId);
            return Result<List<IssueVoucherDetailDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<IssueHeaderDto>>> GetHeaderNameAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetHeaderNameAsync(transactionId);
            return Result<List<IssueHeaderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issue header for transaction {TransactionId}", transactionId);
            return Result<List<IssueHeaderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<IssueSaveResultDto>> SaveIssueDataAsync(SaveIssueDataRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Prefix))
                return Result<IssueSaveResultDto>.Failure("Voucher prefix is required.");

            if (request.DetailData == null || request.DetailData.Count == 0)
                return Result<IssueSaveResultDto>.Failure("At least one issue detail item is required.");

            var result = await _repository.SaveIssueDataAsync(request);
            return Result<IssueSaveResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issue data with prefix {Prefix}", request.Prefix);
            return Result<IssueSaveResultDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateIssueAsync(UpdateIssueDataRequest request)
    {
        try
        {
            if (request.TransactionID <= 0)
                return Result<bool>.Failure("Valid TransactionID is required for update.");

            if (request.DetailData == null || request.DetailData.Count == 0)
                return Result<bool>.Failure("At least one issue detail item is required.");

            await _repository.UpdateIssueAsync(request);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating issue transaction {TransactionId}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteIssueAsync(DeleteIssueRequest request)
    {
        try
        {
            if (request.TransactionID <= 0)
                return Result<bool>.Failure("Valid TransactionID is required for deletion.");

            await _repository.DeleteIssueAsync(request.TransactionID, request.JobBookingJobCardContentsID);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting issue transaction {TransactionId}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
