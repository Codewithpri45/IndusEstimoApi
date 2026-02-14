using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.Inventory;
using IndasEstimo.Domain.Entities.Inventory;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class PurchaseRequisitionService : IPurchaseRequisitionService
{
    private readonly IPurchaseRequisitionRepository _repository;
    private readonly IDbOperationsService _dbOperations;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PurchaseRequisitionService> _logger;

    public PurchaseRequisitionService(
        IPurchaseRequisitionRepository repository,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<PurchaseRequisitionService> logger)
    {
        _repository = repository;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SavePurchaseRequisitionResponse>> SavePurchaseRequisitionAsync(SavePurchaseRequisitionRequest request)
    {
        try
        {
            var userContext = GetUserContext();



            // 2. Validate Production Unit Permission
            string action = "Save";
            var canCrud = await _dbOperations.ValidateProductionUnitAsync(action);
            if (canCrud != "Authorize")
            {
                return Result<SavePurchaseRequisitionResponse>.Failure(canCrud);
            }

            long transactionId = 0;
            string voucherNo = string.Empty;
            int isVoucherItemApproved = 0; // Default: needs approval
            
            // TODO: Implement db.checkDynamicTransactionApprovalRequirement logic 
            // If approval is not required, set isVoucherItemApproved = 1

            // SAVE logic
            try
            {
                var (newVoucherNo, maxVoucherNo) = await _repository.GenerateNextPRNumberAsync(request.Prefix);
                voucherNo = newVoucherNo;

                var mainEntity = MapToMainEntity(request.RecordMain[0], request.Prefix, maxVoucherNo, voucherNo, userContext);
                var detailEntities = MapToDetailEntities(request.RecordDetail, userContext);
                var indentUpdateEntities = MapToIndentUpdateEntities(request.UpdateIndentDetail);

                transactionId = await _repository.SavePurchaseRequisitionAsync(mainEntity, detailEntities, indentUpdateEntities);
                
                if (transactionId <= 0)
                {
                    return Result<SavePurchaseRequisitionResponse>.Failure("Error: Failed to save requisition");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase requisition");
                return Result<SavePurchaseRequisitionResponse>.Failure($"Error: {ex.Message}");
            }

            // 4. Handle Approval Workflow (Only if not already approved)
            if (isVoucherItemApproved == 0 && request.UserApprovalProcess != null && request.UserApprovalProcess.Count > 0)
            {
                try
                {
                    foreach (var approval in request.UserApprovalProcess)
                    {
                        var result = await _repository.CreateApprovalWorkflowAsync(
                            transactionId,
                            0, 
                            approval.ItemID,
                            $"ItemID={approval.ItemID} AND ItemName={approval.ItemName} And PurchaseQty={approval.PurchaseQty}",
                            "Paper Purchase Requisition",
                            62, 
                            voucherNo,
                            approval.ItemName,
                            approval.PurchaseQty
                        );
                        
                        if (result != "Success")
                        {
                            _logger.LogWarning("Approval workflow creation returned: {Result}", result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error creating approval workflow, but requisition was saved successfully");
                    // Don't fail the entire operation if approval workflow fails
                }
            }

            return Result<SavePurchaseRequisitionResponse>.Success(
                new SavePurchaseRequisitionResponse(transactionId, voucherNo, "Success")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SavePurchaseRequisitionAsync");
            return Result<SavePurchaseRequisitionResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<JobCardDto>>> GetJobCardListAsync()
    {
        try
        {
            var productionUnitIds = await _dbOperations.GetProductionUnitIdsAsync();
            var data = await _repository.GetJobCardListAsync(productionUnitIds);
            return Result<List<JobCardDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetJobCardListAsync");
            return Result<List<JobCardDto>>.Failure($"Internal Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ClientListDto>>> GetClientListAsync()
    {
        try
        {
            var data = await _repository.GetClientListAsync();
            return Result<List<ClientListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetClientListAsync");
            return Result<List<ClientListDto>>.Failure($"Internal Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CloseIndentsAsync(CloseIndentRequest request)
    {
        try
        {
            if (request.ItemID == null || !request.ItemID.Any())
            {
                return Result<string>.Failure("Error: No ItemID provided");
            }

            var success = await _repository.CloseIndentsAsync(request.ItemID);
            if (success)
            {
                return Result<string>.Success("Success");
            }
            return Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CloseIndentsAsync");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CloseRequisitionsAsync(CloseRequisitionRequest request)
    {
        try
        {
            if (request.Requisitions == null || !request.Requisitions.Any())
            {
                return Result<string>.Failure("Error: No requisitions provided");
            }

            // VB Logic: If TransactionID = "0" Then Return "Indent can't be closed"
            if (request.Requisitions.Any(r => r.TransactionID == 0))
            {
                return Result<string>.Failure("Indent can't be closed");
            }

            var success = await _repository.CloseRequisitionsAsync(request.Requisitions);
            if (success)
            {
                return Result<string>.Success("Success");
            }
            return Result<string>.Failure("Fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CloseRequisitionsAsync");
            return Result<string>.Failure($"Fail: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextPRNumberAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNextVoucherNoAsync");
            return Result<string>.Failure("fail");
        }
    }

    public async Task<Result<string>> GetLastTransactionDateAsync()
    {
        try
        {
            var date = await _repository.GetLastTransactionDateAsync();
            if (date.HasValue)
            {
                // Format as ISO string for frontend date pickers
                return Result<string>.Success(date.Value.ToString("yyyy-MM-dd"));
            }
            return Result<string>.Success(string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLastTransactionDateAsync");
            return Result<string>.Failure("fail");
        }
    }

    public async Task<Result<List<RequisitionDataDto>>> GetRequisitionDataAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetRequisitionDataAsync(transactionId);
            return Result<List<RequisitionDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRequisitionDataAsync for TransactionID {TransactionID}", transactionId);
            return Result<List<RequisitionDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemLookupDto>>> GetItemLookupListAsync(long? itemGroupId)
    {
        try
        {
            var productionUnitId = await _dbOperations.GetProductionUnitIdsAsync();
            var data = await _repository.GetItemLookupListAsync(itemGroupId, productionUnitId);
            return Result<List<ItemLookupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetItemLookupListAsync");
            return Result<List<ItemLookupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeletePurchaseRequisitionAsync(long transactionId)
    {
        try
        {
            // 1. Validations
            if (await _repository.IsRequisitionUsedAsync(transactionId))
            {
                return Result<string>.Success("TransactionUsed");
            }

            if (await _repository.IsRequisitionApprovedAsync(transactionId))
            {
                return Result<string>.Success("RequisitionApproved");
            }

            // 2. Permission Check
            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                // return Result<string>.Success(canCrud);
            }

            // 3. Perform soft delete (Includes approval cleanup inside transaction)
            var success = await _repository.DeletePurchaseRequisitionAsync(transactionId);
            
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeletePurchaseRequisitionAsync for ID {TransactionID}", transactionId);
            return Result<string>.Failure("fail");
        }
    }

    public async Task<Result<List<CommentDataDto>>> GetCommentDataAsync(string purchaseTransactionId, string requisitionIds)
    {
        try
        {
            var data = await _repository.GetCommentDataAsync(purchaseTransactionId, requisitionIds);
            return Result<List<CommentDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCommentDataAsync");
            return Result<List<CommentDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> FillGridAsync(string radioValue, string filterString, string fromDateValue, string toDateValue)
    {
        try
        {
            var data = await _repository.FillGridAsync(radioValue, filterString, fromDateValue, toDateValue);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FillGridAsync");
            return Result<string>.Failure($"Error: {ex.Message}");
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

    private PurchaseRequisition MapToMainEntity(
        PurchaseRequisitionMainDto dto,
        string prefix,
        long maxVoucherNo,
        string voucherNo,
        UserContext context)
    {
        return new PurchaseRequisition
        {
            VoucherID = (int)dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            LedgerID = dto.LedgerID,
            VoucherDate = dto.VoucherDate,
            TotalQuantity = dto.TotalQuantity,
            Narration = dto.Narration,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private List<PurchaseRequisitionDetail> MapToDetailEntities(
        List<PurchaseRequisitionDetailDto> dtos,
        UserContext context)
    {
        return dtos.Select(dto => new PurchaseRequisitionDetail
        {
            ItemID = dto.ItemID,
            TransID = dto.TransID,
            ItemGroupID = dto.ItemGroupID,
            RequiredNoOfPacks = dto.RequiredNoOfPacks,
            QuantityPerPack = dto.QuantityPerPack,
            RequiredQuantity = dto.RequiredQuantity > 0 ? dto.RequiredQuantity : dto.PurchaseQty,
            StockUnit = !string.IsNullOrEmpty(dto.StockUnit) ? dto.StockUnit : dto.OrderUnit,
            ItemNarration = dto.ItemNarration,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            RefJobBookingJobCardContentsID = dto.RefJobBookingJobCardContentsID,
            RefJobCardContentNo = dto.RefJobCardContentNo,
            CurrentStockInStockUnit = dto.CurrentStockInStockUnit > 0 ? dto.CurrentStockInStockUnit : dto.PhysicalStock,
            CurrentStockInPurchaseUnit = dto.CurrentStockInPurchaseUnit > 0 ? dto.CurrentStockInPurchaseUnit : dto.PhysicalStockInPurchaseUnit,
            IsAuditApproved = dto.IsAuditApproved > 0 ? dto.IsAuditApproved : (dto.AuditApprovalRequired == 0 ? 1 : 0),
            IsVoucherItemApproved = 0, // Default
            VoucherItemApprovedBy = 0,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private List<PurchaseRequisitionIndentUpdate> MapToIndentUpdateEntities(List<UpdateIndentDetailDto> dtos)
    {
        return dtos.Select(dto => new PurchaseRequisitionIndentUpdate
        {
            TransactionID = dto.TransactionID,
            ItemID = dto.ItemID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            RequisitionItemID = dto.RequisitionItemID
        }).ToList();
    }

    private class UserContext
    {
        public long UserId { get; set; }
        public long CompanyId { get; set; }
        public string FYear { get; set; } = string.Empty;
        public long ProductionUnitId { get; set; }
    }
}
