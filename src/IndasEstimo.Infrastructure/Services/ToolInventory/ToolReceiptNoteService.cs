using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Infrastructure.Services.ToolInventory;

public class ToolReceiptNoteService : IToolReceiptNoteService
{
    private readonly IToolReceiptNoteRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<ToolReceiptNoteService> _logger;

    public ToolReceiptNoteService(
        IToolReceiptNoteRepository repository,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolReceiptNoteService> logger)
    {
        _repository = repository;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SaveToolReceiptNoteResponse>> SaveToolReceiptNoteAsync(SaveToolReceiptNoteRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SaveToolReceiptNoteResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextReceiptNoAsync(request.Prefix);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);

            var transactionId = await _repository.SaveToolReceiptNoteAsync(mainEntity, detailEntities);

            var response = new SaveToolReceiptNoteResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SaveToolReceiptNoteResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool receipt note");
            return Result<SaveToolReceiptNoteResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SaveToolReceiptNoteResponse>> UpdateToolReceiptNoteAsync(UpdateToolReceiptNoteRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canUpdate = await _dbOperations.ValidateProductionUnitAsync("Update");
            if (canUpdate != "Authorize")
            {
                return Result<SaveToolReceiptNoteResponse>.Failure(canUpdate);
            }

            if (await _repository.IsToolReceiptNoteUsedAsync(request.TransactionID))
            {
                return Result<SaveToolReceiptNoteResponse>.Failure("Exist");
            }

            var voucherNo = await _repository.GetVoucherNoAsync(request.TransactionID) ?? "0";

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], "", 0, voucherNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);

            await _repository.UpdateToolReceiptNoteAsync(request.TransactionID, mainEntity, detailEntities);

            var response = new SaveToolReceiptNoteResponse(request.TransactionID, voucherNo, "Success");
            return Result<SaveToolReceiptNoteResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tool receipt note {TransactionId}", request.TransactionID);
            return Result<SaveToolReceiptNoteResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolReceiptNoteAsync(long transactionId)
    {
        try
        {
            if (await _repository.IsToolReceiptNoteUsedAsync(transactionId))
            {
                return Result<string>.Success("Exist");
            }

            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeleteToolReceiptNoteAsync(transactionId);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteToolReceiptNoteAsync for ID {TransactionID}", transactionId);
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

    private static ToolReceiptNote MapToMainEntity(ToolReceiptNoteMainDto dto, string prefix, long maxVoucherNo, string voucherNo, UserContext context)
    {
        return new ToolReceiptNote
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            VoucherDate = dto.VoucherDate,
            LedgerID = dto.LedgerID,
            DeliveryNoteNo = dto.DeliveryNoteNo,
            DeliveryNoteDate = dto.DeliveryNoteDate,
            GateEntryNo = dto.GateEntryNo,
            GateEntryDate = dto.GateEntryDate,
            LRNoVehicleNo = dto.LRNoVehicleNo,
            Transporter = dto.Transporter,
            ReceivedBy = dto.ReceivedBy,
            EWayBillNumber = dto.EWayBillNumber,
            EWayBillDate = dto.EWayBillDate,
            Narration = dto.Narration,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private static List<ToolReceiptNoteDetail> MapToDetailEntities(List<ToolReceiptNoteDetailDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolReceiptNoteDetail
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            ToolGroupID = dto.ToolGroupID,
            ChallanQuantity = dto.ChallanQuantity,
            BatchNo = dto.BatchNo,
            StockUnit = dto.StockUnit,
            ReceiptWtPerPacking = dto.ReceiptWtPerPacking,
            WarehouseID = dto.WarehouseID,
            PurchaseTransactionID = dto.PurchaseTransactionID,
            ParentTransactionID = 0, // Will be set to TransactionID during insert
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

    public async Task<Result<List<ToolReceiptNoteDataDto>>> GetToolReceiptNoteDataAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetToolReceiptNoteDataAsync(transactionId);
            return Result<List<ToolReceiptNoteDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool receipt note {TransactionId}", transactionId);
            return Result<List<ToolReceiptNoteDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolReceiptNoteListDto>>> GetToolReceiptNoteListAsync(GetToolReceiptNoteListRequest request)
    {
        try
        {
            var data = await _repository.GetToolReceiptNoteListAsync(request.FromDateValue, request.ToDateValue);
            return Result<List<ToolReceiptNoteListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool receipt note list");
            return Result<List<ToolReceiptNoteListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPendingPurchaseOrderDto>>> GetPendingPurchaseOrdersAsync()
    {
        try
        {
            var data = await _repository.GetPendingPurchaseOrdersAsync();
            return Result<List<ToolPendingPurchaseOrderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending purchase orders");
            return Result<List<ToolPendingPurchaseOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextReceiptNoAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next tool receipt note voucher number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetLastTransactionDateAsync()
    {
        try
        {
            var date = await _repository.GetLastTransactionDateAsync();
            return Result<string>.Success(date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last transaction date");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolReceiverDto>>> GetReceiversAsync()
    {
        try
        {
            var data = await _repository.GetReceiversAsync();
            return Result<List<ToolReceiverDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receivers");
            return Result<List<ToolReceiverDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ToolPreviousReceivedQtyDto>> GetPreviousReceivedQuantityAsync(long purchaseTransactionId, long toolId, long grnTransactionId)
    {
        try
        {
            var data = await _repository.GetPreviousReceivedQuantityAsync(purchaseTransactionId, toolId, grnTransactionId);
            return Result<ToolPreviousReceivedQtyDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving previous received quantity for PO {PurchaseTransactionId} tool {ToolId}", purchaseTransactionId, toolId);
            return Result<ToolPreviousReceivedQtyDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolWarehouseDto>>> GetWarehousesAsync()
    {
        try
        {
            var data = await _repository.GetWarehousesAsync();
            return Result<List<ToolWarehouseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses");
            return Result<List<ToolWarehouseDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolBinDto>>> GetBinsAsync()
    {
        try
        {
            var data = await _repository.GetBinsAsync();
            return Result<List<ToolBinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bins");
            return Result<List<ToolBinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(long transactionId)
    {
        try
        {
            var hasPermission = await _repository.CheckPermissionAsync(transactionId);
            return Result<string>.Success(hasPermission ? "Exist" : "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for {TransactionId}", transactionId);
            return Result<string>.Failure("fail");
        }
    }
}
