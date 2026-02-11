using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
using IndasEstimo.Application.Interfaces.Repositories.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;
using IndasEstimo.Infrastructure.Database.Services;

namespace IndasEstimo.Infrastructure.Services.ToolInventory;

public class ToolRequisitionService : IToolRequisitionService
{
    private readonly IToolRequisitionRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<ToolRequisitionService> _logger;

    public ToolRequisitionService(
        IToolRequisitionRepository repository,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolRequisitionService> logger)
    {
        _repository = repository;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SaveToolRequisitionResponse>> SaveToolRequisitionAsync(SaveToolRequisitionRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SaveToolRequisitionResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextVoucherNoAsync(request.Prefix);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);

            var transactionId = await _repository.SaveToolRequisitionAsync(mainEntity, detailEntities);

            // Update indent details to link them to this requisition
            if (request.JsonObjectsUpdateIndentDetail != null && request.JsonObjectsUpdateIndentDetail.Count > 0)
            {
                await _repository.UpdateIndentLinkageAsync(transactionId, request.JsonObjectsUpdateIndentDetail);
            }

            var response = new SaveToolRequisitionResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SaveToolRequisitionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool requisition");
            return Result<SaveToolRequisitionResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SaveToolRequisitionResponse>> UpdateToolRequisitionAsync(UpdateToolRequisitionRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canUpdate = await _dbOperations.ValidateProductionUnitAsync("Update");
            if (canUpdate != "Authorize")
            {
                return Result<SaveToolRequisitionResponse>.Failure(canUpdate);
            }

            // Check if requisition items are approved
            if (await _repository.IsToolRequisitionApprovedAsync(request.TransactionID))
            {
                return Result<SaveToolRequisitionResponse>.Failure("RequisitionApproved");
            }

            var voucherNo = await _repository.GetVoucherNoAsync(request.TransactionID) ?? "0";
            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], "", 0, voucherNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);

            await _repository.UpdateToolRequisitionAsync(request.TransactionID, mainEntity, detailEntities);

            // Clear old indent linkage and re-link
            await _repository.ClearIndentLinkageAsync(request.TransactionID);
            if (request.JsonObjectsUpdateIndentDetail != null && request.JsonObjectsUpdateIndentDetail.Count > 0)
            {
                await _repository.UpdateIndentLinkageAsync(request.TransactionID, request.JsonObjectsUpdateIndentDetail);
            }

            var response = new SaveToolRequisitionResponse(request.TransactionID, voucherNo, "Success");
            return Result<SaveToolRequisitionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tool requisition {TransactionId}", request.TransactionID);
            return Result<SaveToolRequisitionResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolRequisitionAsync(long transactionId)
    {
        try
        {
            if (await _repository.IsToolRequisitionApprovedAsync(transactionId))
            {
                return Result<string>.Success("RequisitionApproved");
            }

            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeleteToolRequisitionAsync(transactionId);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteToolRequisitionAsync for ID {TransactionID}", transactionId);
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

    private ToolRequisition MapToMainEntity(ToolRequisitionMainDto dto, string prefix, long maxVoucherNo, string voucherNo, UserContext context)
    {
        return new ToolRequisition
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
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

    private List<ToolRequisitionDetail> MapToDetailEntities(List<ToolRequisitionDetailDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolRequisitionDetail
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            ToolGroupID = dto.ToolGroupID,
            RequiredQuantity = dto.RequiredQuantity,
            StockUnit = dto.StockUnit,
            PurchaseUnit = dto.PurchaseUnit,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            ToolNarration = dto.ToolNarration,
            CurrentStockInStockUnit = dto.CurrentStockInStockUnit,
            CurrentStockInPurchaseUnit = dto.CurrentStockInPurchaseUnit,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            JobBookingID = dto.JobBookingID,
            RefJobBookingJobCardContentsID = dto.RefJobBookingJobCardContentsID,
            RefJobCardContentNo = dto.RefJobCardContentNo,
            RequisitionToolID = dto.RequisitionToolID,
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

    public async Task<Result<List<ToolIndentListDto>>> GetIndentListAsync()
    {
        try
        {
            var data = await _repository.GetIndentListAsync();
            return Result<List<ToolIndentListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving indent list");
            return Result<List<ToolIndentListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolRequisitionListDto>>> GetToolRequisitionListAsync(GetToolRequisitionListRequest request)
    {
        try
        {
            var data = await _repository.GetToolRequisitionListAsync(
                request.FromDateValue,
                request.ToDateValue
            );
            return Result<List<ToolRequisitionListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool requisition list");
            return Result<List<ToolRequisitionListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolRequisitionDataDto>>> GetToolRequisitionDataAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetToolRequisitionDataAsync(transactionId);
            return Result<List<ToolRequisitionDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool requisition {TransactionId}", transactionId);
            return Result<List<ToolRequisitionDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextVoucherNoAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next voucher number");
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

    public async Task<Result<List<ToolMasterItemDto>>> GetToolMasterListAsync()
    {
        try
        {
            var data = await _repository.GetToolMasterListAsync();
            return Result<List<ToolMasterItemDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool master list");
            return Result<List<ToolMasterItemDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(long transactionId)
    {
        try
        {
            var isApproved = await _repository.CheckPermissionAsync(transactionId);
            return Result<string>.Success(isApproved ? "Exist" : "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for {TransactionId}", transactionId);
            return Result<string>.Failure("fail");
        }
    }
}
