using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Infrastructure.Services.ToolInventory;

public class ToolReturnToStockService : IToolReturnToStockService
{
    private readonly IToolReturnToStockRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<ToolReturnToStockService> _logger;

    public ToolReturnToStockService(
        IToolReturnToStockRepository repository,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolReturnToStockService> logger)
    {
        _repository = repository;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SaveToolReturnToStockResponse>> SaveToolReturnToStockAsync(SaveToolReturnToStockRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SaveToolReturnToStockResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextReturnNoAsync(request.Prefix);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);

            var transactionId = await _repository.SaveToolReturnToStockAsync(mainEntity, detailEntities);

            var response = new SaveToolReturnToStockResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SaveToolReturnToStockResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool return to stock");
            return Result<SaveToolReturnToStockResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolReturnToStockAsync(long transactionId)
    {
        try
        {
            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeleteToolReturnToStockAsync(transactionId);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteToolReturnToStockAsync for ID {TransactionID}", transactionId);
            return Result<string>.Failure("fail");
        }
    }

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

    private static ToolReturnToStock MapToMainEntity(ToolReturnToStockMainDto dto, string prefix, long maxVoucherNo, string voucherNo, UserContext context)
    {
        return new ToolReturnToStock
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            VoucherDate = dto.VoucherDate,
            LedgerID = dto.LedgerID,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            Narration = dto.Narration,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private static List<ToolReturnToStockDetail> MapToDetailEntities(List<ToolReturnToStockDetailDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolReturnToStockDetail
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            JobCardFormNo = dto.JobCardFormNo,
            ItemID = dto.ItemID,
            ReceiptQuantity = dto.ReceiptQuantity,
            BatchNo = dto.BatchNo,
            WarehouseID = dto.WarehouseID,
            IssueTransactionID = dto.IssueTransactionID,
            ToolNarration = dto.ToolNarration,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private class UserContext
    {
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string FYear { get; set; } = string.Empty;
        public long ProductionUnitId { get; set; }
    }

    // ==================== Retrieve Operations ====================

    public async Task<Result<List<ToolAvailableForReturnDto>>> GetAvailableForReturnAsync()
    {
        try
        {
            var data = await _repository.GetAvailableForReturnAsync();
            return Result<List<ToolAvailableForReturnDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available tools for return");
            return Result<List<ToolAvailableForReturnDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolReturnToStockListDto>>> GetReturnToStockListAsync()
    {
        try
        {
            var data = await _repository.GetReturnToStockListAsync();
            return Result<List<ToolReturnToStockListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool return to stock list");
            return Result<List<ToolReturnToStockListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextReturnNoAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next tool return to stock voucher number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
