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

public class ToolPurchaseOrderService : IToolPurchaseOrderService
{
    private readonly IToolPurchaseOrderRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<ToolPurchaseOrderService> _logger;

    public ToolPurchaseOrderService(
        IToolPurchaseOrderRepository repository,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ToolPurchaseOrderService> logger)
    {
        _repository = repository;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SaveToolPurchaseOrderResponse>> SaveToolPurchaseOrderAsync(SaveToolPurchaseOrderRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SaveToolPurchaseOrderResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextPONumberAsync(request.Prefix);
            var amountInWords = ConvertAmountToWords(request.TxtNetAmt, request.CurrencyCode);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, amountInWords, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);
            var taxEntities = MapToTaxEntities(request.JsonObjectsRecordTax, userContext);
            var overheadEntities = MapToOverheadEntities(request.JsonObjectsRecordOverHead, userContext);
            var requisitionEntities = MapToRequisitionEntities(request.JsonObjectsRecordRequisition, userContext);

            var transactionId = await _repository.SaveToolPurchaseOrderAsync(mainEntity, detailEntities, taxEntities, overheadEntities, requisitionEntities);

            var response = new SaveToolPurchaseOrderResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SaveToolPurchaseOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving tool purchase order");
            return Result<SaveToolPurchaseOrderResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SaveToolPurchaseOrderResponse>> UpdateToolPurchaseOrderAsync(UpdateToolPurchaseOrderRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canUpdate = await _dbOperations.ValidateProductionUnitAsync("Update");
            if (canUpdate != "Authorize")
            {
                return Result<SaveToolPurchaseOrderResponse>.Failure(canUpdate);
            }

            if (await _repository.IsToolPurchaseOrderApprovedAsync(request.TransactionID))
            {
                return Result<SaveToolPurchaseOrderResponse>.Failure("RequisitionApproved");
            }

            var voucherNo = await _repository.GetVoucherNoAsync(request.TransactionID) ?? "0";
            var amountInWords = ConvertAmountToWords(request.TxtNetAmt, request.CurrencyCode);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], "", 0, voucherNo, amountInWords, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);
            var taxEntities = MapToTaxEntities(request.JsonObjectsRecordTax, userContext);
            var overheadEntities = MapToOverheadEntities(request.JsonObjectsRecordOverHead, userContext);
            var requisitionEntities = MapToRequisitionEntities(request.JsonObjectsRecordRequisition, userContext);

            await _repository.UpdateToolPurchaseOrderAsync(request.TransactionID, mainEntity, detailEntities, taxEntities, overheadEntities, requisitionEntities);

            var response = new SaveToolPurchaseOrderResponse(request.TransactionID, voucherNo, "Success");
            return Result<SaveToolPurchaseOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tool purchase order {TransactionId}", request.TransactionID);
            return Result<SaveToolPurchaseOrderResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteToolPurchaseOrderAsync(long transactionId)
    {
        try
        {
            if (await _repository.IsToolPurchaseOrderApprovedAsync(transactionId))
            {
                return Result<string>.Success("RequisitionApproved");
            }

            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeleteToolPurchaseOrderAsync(transactionId);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteToolPurchaseOrderAsync for ID {TransactionID}", transactionId);
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

    private static string ConvertAmountToWords(string amount, string currencyCode)
    {
        if (currencyCode == "INR" || string.IsNullOrEmpty(currencyCode))
        {
            return $"{amount} Rupees Only";
        }
        return $"{amount} {currencyCode} Only";
    }

    private static ToolPurchaseOrder MapToMainEntity(ToolPurchaseOrderMainDto dto, string prefix, long maxVoucherNo, string voucherNo, string amountInWords, UserContext context)
    {
        return new ToolPurchaseOrder
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            VoucherDate = dto.VoucherDate,
            LedgerID = dto.LedgerID,
            ContactPersonID = dto.ContactPersonID,
            TotalQuantity = dto.TotalQuantity,
            TotalBasicAmount = dto.TotalBasicAmount,
            TotalTaxAmount = dto.TotalTaxAmount,
            TotalOverheadAmount = dto.TotalOverheadAmount,
            NetAmount = dto.NetAmount,
            PurchaseDivision = dto.PurchaseDivision,
            PurchaseReferenceRemark = dto.PurchaseReferenceRemark,
            Narration = dto.Narration,
            DeliveryAddress = dto.DeliveryAddress,
            TermsOfPayment = dto.TermsOfPayment,
            CurrencyCode = dto.CurrencyCode,
            ModeOfTransport = dto.ModeOfTransport,
            DealerID = dto.DealerID,
            VoucherApprovalByEmployeeID = dto.VoucherApprovalByEmployeeID,
            AmountInWords = amountInWords,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private static List<ToolPurchaseOrderDetail> MapToDetailEntities(List<ToolPurchaseOrderDetailDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolPurchaseOrderDetail
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            ToolGroupID = dto.ToolGroupID,
            RequiredQuantity = dto.RequiredQuantity,
            PurchaseOrderQuantity = dto.PurchaseOrderQuantity,
            PurchaseUnit = dto.PurchaseUnit,
            StockUnit = dto.StockUnit,
            PurchaseRate = dto.PurchaseRate,
            PurchaseTolerance = dto.PurchaseTolerance,
            GrossAmount = dto.GrossAmount,
            DiscountPercentage = dto.DiscountPercentage,
            DiscountAmount = dto.DiscountAmount,
            BasicAmount = dto.BasicAmount,
            TaxableAmount = dto.TaxableAmount,
            GSTPercentage = dto.GSTPercentage,
            CGSTPercentage = dto.CGSTPercentage,
            SGSTPercentage = dto.SGSTPercentage,
            IGSTPercentage = dto.IGSTPercentage,
            CGSTAmount = dto.CGSTAmount,
            SGSTAmount = dto.SGSTAmount,
            IGSTAmount = dto.IGSTAmount,
            NetAmount = dto.NetAmount,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            ProductHSNID = dto.ProductHSNID,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private static List<ToolPurchaseOrderTax> MapToTaxEntities(List<ToolPurchaseOrderTaxDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolPurchaseOrderTax
        {
            TransID = dto.TransID,
            LedgerID = dto.LedgerID,
            TaxPercentage = dto.TaxPercentage,
            Amount = dto.Amount,
            TaxInAmount = dto.TaxInAmount,
            IsComulative = dto.IsComulative,
            GSTApplicable = dto.GSTApplicable,
            CalculatedON = dto.CalculatedON,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private static List<ToolPurchaseOrderOverhead> MapToOverheadEntities(List<ToolPurchaseOrderOverheadDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolPurchaseOrderOverhead
        {
            HeadID = dto.HeadID,
            TransID = dto.TransID,
            HeadName = dto.HeadName,
            Quantity = dto.Quantity,
            ChargesType = dto.ChargesType,
            Rate = dto.Rate,
            Amount = dto.Amount,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private static List<ToolPurchaseOrderRequisition> MapToRequisitionEntities(List<ToolPurchaseOrderRequisitionDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ToolPurchaseOrderRequisition
        {
            TransID = dto.TransID,
            ToolID = dto.ToolID,
            RequisitionProcessQuantity = dto.RequisitionProcessQuantity,
            StockUnit = dto.StockUnit,
            RequisitionTransactionID = dto.RequisitionTransactionID,
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

    public async Task<Result<List<ToolPurchaseOrderDataDto>>> GetToolPurchaseOrderAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetToolPurchaseOrderDataAsync(transactionId);
            return Result<List<ToolPurchaseOrderDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool purchase order {TransactionId}", transactionId);
            return Result<List<ToolPurchaseOrderDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPOOverheadDataDto>>> GetToolPurchaseOrderOverheadAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetToolPurchaseOrderOverheadAsync(transactionId);
            return Result<List<ToolPOOverheadDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool PO overhead for {TransactionId}", transactionId);
            return Result<List<ToolPOOverheadDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPOTaxDataDto>>> GetToolPurchaseOrderTaxAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetToolPurchaseOrderTaxAsync(transactionId);
            return Result<List<ToolPOTaxDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool PO tax for {TransactionId}", transactionId);
            return Result<List<ToolPOTaxDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPurchaseOrderListDto>>> GetToolPurchaseOrderListAsync(GetToolPurchaseOrderListRequest request)
    {
        try
        {
            var data = await _repository.GetToolPurchaseOrderListAsync(
                request.FromDateValue,
                request.ToDateValue,
                request.FilterStr,
                request.Detail
            );
            return Result<List<ToolPurchaseOrderListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tool purchase order list");
            return Result<List<ToolPurchaseOrderListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPendingRequisitionDto>>> GetPendingRequisitionsAsync()
    {
        try
        {
            var data = await _repository.GetPendingRequisitionsAsync();
            return Result<List<ToolPendingRequisitionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending tool requisitions");
            return Result<List<ToolPendingRequisitionDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Helper/Lookup Operations ====================

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextPONumberAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next tool PO voucher number");
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

    public async Task<Result<List<ToolSupplierDto>>> GetSuppliersAsync()
    {
        try
        {
            var data = await _repository.GetSuppliersAsync();
            return Result<List<ToolSupplierDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return Result<List<ToolSupplierDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolContactPersonDto>>> GetContactPersonsAsync(long ledgerId)
    {
        try
        {
            var data = await _repository.GetContactPersonsAsync(ledgerId);
            return Result<List<ToolContactPersonDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact persons for ledger {LedgerId}", ledgerId);
            return Result<List<ToolContactPersonDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ToolItemRateDto>> GetItemRateAsync(long ledgerId, long toolId)
    {
        try
        {
            var data = await _repository.GetItemRateAsync(ledgerId, toolId);
            return Result<ToolItemRateDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item rate for ledger {LedgerId} tool {ToolId}", ledgerId, toolId);
            return Result<ToolItemRateDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolAllottedSupplierDto>>> GetAllottedSuppliersAsync(long toolGroupId)
    {
        try
        {
            var data = await _repository.GetAllottedSuppliersAsync(toolGroupId);
            return Result<List<ToolAllottedSupplierDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allotted suppliers for group {ToolGroupId}", toolGroupId);
            return Result<List<ToolAllottedSupplierDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolOverflowGridDto>>> GetOverflowGridAsync(long toolId, long toolGroupId)
    {
        try
        {
            var data = await _repository.GetOverflowGridAsync(toolId, toolGroupId);
            return Result<List<ToolOverflowGridDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overflow grid for tool {ToolId}", toolId);
            return Result<List<ToolOverflowGridDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolOverheadChargeHeadDto>>> GetOverheadChargeHeadsAsync()
    {
        try
        {
            var data = await _repository.GetOverheadChargeHeadsAsync();
            return Result<List<ToolOverheadChargeHeadDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overhead charge heads");
            return Result<List<ToolOverheadChargeHeadDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolTaxChargeLedgerDto>>> GetTaxChargeLedgersAsync()
    {
        try
        {
            var data = await _repository.GetTaxChargeLedgersAsync();
            return Result<List<ToolTaxChargeLedgerDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax charge ledgers");
            return Result<List<ToolTaxChargeLedgerDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolHSNCodeDto>>> GetHSNCodesAsync()
    {
        try
        {
            var data = await _repository.GetHSNCodesAsync();
            return Result<List<ToolHSNCodeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HSN codes");
            return Result<List<ToolHSNCodeDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolCurrencyDto>>> GetCurrenciesAsync()
    {
        try
        {
            var data = await _repository.GetCurrenciesAsync();
            return Result<List<ToolCurrencyDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currencies");
            return Result<List<ToolCurrencyDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ToolPOApprovalByDto>>> GetPOApprovalByAsync()
    {
        try
        {
            var data = await _repository.GetPOApprovalByAsync();
            return Result<List<ToolPOApprovalByDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PO approval by list");
            return Result<List<ToolPOApprovalByDto>>.Failure($"Error: {ex.Message}");
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
