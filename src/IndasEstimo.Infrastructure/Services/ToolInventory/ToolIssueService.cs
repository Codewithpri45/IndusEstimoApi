using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Infrastructure.Services.ToolInventory;

public class ToolIssueService : IToolIssueService
{
    private readonly IToolIssueRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToolIssueService> _logger;

    public ToolIssueService(
        IToolIssueRepository repository,
        ICurrentUserService currentUserService,
        ILogger<ToolIssueService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    public async Task<Result<ToolIssueResponse>> SaveToolIssueAsync(SaveToolIssueRequest request)
    {
        try
        {
            var context = GetUserContext();

            // Map DTOs to domain entities
            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], context, request.Prefix);
            var detailEntities = request.JsonObjectsRecordDetail
                .Select(dto => MapToDetailEntity(dto, context))
                .ToList();

            // Save to database
            var (transactionId, voucherNo) = await _repository.SaveToolIssueAsync(mainEntity, detailEntities);

            return Result<ToolIssueResponse>.Success(new ToolIssueResponse
            {
                TransactionID = transactionId,
                VoucherNo = voucherNo,
                Message = "Success"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool issue");
            return Result<ToolIssueResponse>.Failure($"Error saving tool issue: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolIssueAsync(DeleteToolIssueRequest request)
    {
        try
        {
            var success = await _repository.DeleteToolIssueAsync(request.TransactionID, request.ParentTransactionID);
            return success
                ? Result<string>.Success("Success")
                : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool issue {TransactionID}", request.TransactionID);
            return Result<string>.Failure($"Error deleting tool issue: {ex.Message}");
        }
    }

    // ==================== Retrieve Operations ====================

    public async Task<Result<List<ToolIssueVoucherDetailsDto>>> GetIssueVoucherDetailsAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetIssueVoucherDetailsAsync(transactionId);
            return Result<List<ToolIssueVoucherDetailsDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool issue details {TransactionID}", transactionId);
            return Result<List<ToolIssueVoucherDetailsDto>>.Failure($"Error retrieving details: {ex.Message}");
        }
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<Result<string>> GetIssueNoAsync(string prefix)
    {
        try
        {
            var voucherNo = await _repository.GenerateIssueNoAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating issue number with prefix {Prefix}", prefix);
            return Result<string>.Failure($"Error generating issue number: {ex.Message}");
        }
    }

    public async Task<Result<List<WarehouseDto>>> GetWarehouseListAsync()
    {
        try
        {
            var data = await _repository.GetWarehouseListAsync();
            return Result<List<WarehouseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse list");
            return Result<List<WarehouseDto>>.Failure($"Error retrieving warehouses: {ex.Message}");
        }
    }

    public async Task<Result<List<BinDto>>> GetBinsListAsync(string warehouseName)
    {
        try
        {
            var data = await _repository.GetBinsListAsync(warehouseName);
            return Result<List<BinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bins list for warehouse {WarehouseName}", warehouseName);
            return Result<List<BinDto>>.Failure($"Error retrieving bins: {ex.Message}");
        }
    }

    public async Task<Result<List<StockBatchWiseDto>>> GetStockBatchWiseAsync(long jobBookingJobCardContentsId)
    {
        try
        {
            var data = await _repository.GetStockBatchWiseAsync(jobBookingJobCardContentsId);
            return Result<List<StockBatchWiseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch-wise stock for job card {JobCardId}", jobBookingJobCardContentsId);
            return Result<List<StockBatchWiseDto>>.Failure($"Error retrieving stock: {ex.Message}");
        }
    }

    public async Task<Result<List<JobCardDto>>> GetJobCardNoAsync()
    {
        try
        {
            var data = await _repository.GetJobCardNoAsync();
            return Result<List<JobCardDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job card list");
            return Result<List<JobCardDto>>.Failure($"Error retrieving job cards: {ex.Message}");
        }
    }

    // ==================== Private Mapping Methods ====================

    private UserContext GetUserContext()
    {
        return new UserContext
        {
            UserId = _currentUserService.GetUserId() ?? 0,
            CompanyId = _currentUserService.GetCompanyId() ?? 0,
            FYear = _currentUserService.GetFYear() ?? string.Empty,
            ProductionUnitId = _currentUserService.GetProductionUnitId() ?? 0
        };
    }

    private ToolIssue MapToMainEntity(ToolIssueMainDto dto, UserContext context, string prefix)
    {
        return new ToolIssue
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            VoucherDate = dto.VoucherDate,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            DeliveryNoteNo = dto.DeliveryNoteNo,
            DeliveryNoteDate = dto.DeliveryNoteDate,
            Narration = dto.Narration,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private ToolIssueDetail MapToDetailEntity(ToolIssueDetailDto dto, UserContext context)
    {
        return new ToolIssueDetail
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            ItemID = dto.ItemID,
            IssueQuantity = dto.IssueQuantity,
            BatchNo = dto.BatchNo,
            WarehouseID = dto.WarehouseID,
            FloorWarehouseID = dto.FloorWarehouseID,
            ToolNarration = dto.ToolNarration,
            ParentTransactionID = dto.ParentTransactionID,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            JobCardFormNo = dto.JobCardFormNo,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private class UserContext
    {
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string FYear { get; set; } = string.Empty;
        public long ProductionUnitId { get; set; }
    }
}
