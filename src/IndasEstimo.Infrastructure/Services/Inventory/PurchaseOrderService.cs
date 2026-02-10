using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Application.Interfaces.Services.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Domain.Entities.Inventory;
using IndasEstimo.Infrastructure.Database.Services;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IMasterDbService _masterDbService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(
        IPurchaseOrderRepository repository,
        IMasterDbService masterDbService,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<PurchaseOrderService> logger)
    {
        _repository = repository;
        _masterDbService = masterDbService;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<SavePurchaseOrderResponse>> SavePurchaseOrderAsync(SavePurchaseOrderRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SavePurchaseOrderResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextPONumberAsync(request.Prefix);
            var amountInWords = await ConvertAmountToWordsAsync(request.TxtNetAmt, request.CurrencyCode);
            var (isApprovalRequired, isVoucherItemApproved, approvalByUserId, moduleId, displayModuleName) = await CheckApprovalRequirementAsync(userContext.CompanyId);

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, amountInWords, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, isVoucherItemApproved, approvalByUserId, userContext);
            var taxEntities = MapToTaxEntities(request.JsonObjectsRecordTax, userContext);
            var scheduleEntities = MapToScheduleEntities(request.JsonObjectsRecordSchedule, userContext);
            var overheadEntities = MapToOverheadEntities(request.JsonObjectsRecordOverHead, userContext);
            var requisitionEntities = MapToRequisitionEntities(request.JsonObjectsRecordRequisition, userContext);

            var transactionId = await _repository.SavePurchaseOrderAsync(mainEntity, detailEntities, taxEntities, scheduleEntities, overheadEntities, requisitionEntities);

            if (isApprovalRequired && isVoucherItemApproved == 0 && request.UserApprovalProcessArray != null)
            {
                await CreateApprovalWorkflowAsync(transactionId, request.JsonObjectsRecordDetail, request.UserApprovalProcessArray, voucherNo, displayModuleName, moduleId, userContext);
            }

            var response = new SavePurchaseOrderResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SavePurchaseOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving purchase order");
            return Result<SavePurchaseOrderResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SavePurchaseOrderResponse>> UpdatePurchaseOrderAsync(UpdatePurchaseOrderRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canUpdate = await _dbOperations.ValidateProductionUnitAsync("Update");
            if (canUpdate != "Authorize")
            {
                return Result<SavePurchaseOrderResponse>.Failure(canUpdate);
            }

            if (await _repository.IsPurchaseOrderUsedAsync(request.TransactionID))
            {
                return Result<SavePurchaseOrderResponse>.Failure("TransactionUsed");
            }

            var (isApprovalRequired, isVoucherItemApproved, approvalByUserId, moduleId, displayModuleName) = await CheckApprovalRequirementAsync(userContext.CompanyId);
            if (isApprovalRequired)
            {
                if (await _repository.IsPurchaseOrderApprovedAsync(request.TransactionID))
                {
                    return Result<SavePurchaseOrderResponse>.Failure("RequisitionApproved");
                }
            }

            var voucherNo = await _repository.GetVoucherNoAsync(request.TransactionID) ?? "0";
            var amountInWords = await ConvertAmountToWordsAsync(request.TxtNetAmt, request.CurrencyCode);
            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, 0, voucherNo, amountInWords, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, isVoucherItemApproved, approvalByUserId, userContext);
            var taxEntities = MapToTaxEntities(request.JsonObjectsRecordTax, userContext);
            var scheduleEntities = MapToScheduleEntities(request.JsonObjectsRecordSchedule, userContext);
            var overheadEntities = MapToOverheadEntities(request.JsonObjectsRecordOverHead, userContext);
            var requisitionEntities = MapToRequisitionEntities(request.JsonObjectsRecordRequisition, userContext);

            await _repository.UpdatePurchaseOrderAsync(request.TransactionID, mainEntity, detailEntities, taxEntities, scheduleEntities, overheadEntities, requisitionEntities);

            if (isApprovalRequired && isVoucherItemApproved == 0 && request.UserApprovalProcessArray != null)
            {
                await CreateApprovalWorkflowAsync(request.TransactionID, request.JsonObjectsRecordDetail, request.UserApprovalProcessArray, voucherNo, displayModuleName, moduleId, userContext);
            }

            var response = new SavePurchaseOrderResponse(request.TransactionID, voucherNo, "Success");
            return Result<SavePurchaseOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase order {TransactionId}", request.TransactionID);
            return Result<SavePurchaseOrderResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeletePurchaseOrderAsync(long transactionId)
    {
        try
        {
            if (await _repository.IsPurchaseOrderUsedAsync(transactionId))
            {
                return Result<string>.Success("TransactionUsed");
            }

            if (await _repository.IsPurchaseOrderApprovedAsync(transactionId))
            {
                return Result<string>.Success("RequisitionApproved");
            }

            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeletePurchaseOrderAsync(transactionId);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeletePurchaseOrderAsync for ID {TransactionID}", transactionId);
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

    private async Task<string> ConvertAmountToWordsAsync(string amount, string currencyCode)
    {
        if (currencyCode == "INR" || string.IsNullOrEmpty(currencyCode))
        {
            return $"{amount} Rupees Only";
        }
        return $"{amount} {currencyCode} Only";
    }

    private async Task<(bool IsApprovalRequired, int IsVoucherItemApproved, long ApprovalByUserId, long ModuleId, string DisplayModuleName)> CheckApprovalRequirementAsync(long companyId)
    {
        return await _repository.CheckApprovalRequirementAsync(companyId, "PurchaseOrder.aspx");
    }

    private async Task CreateApprovalWorkflowAsync(
        long transactionId,
        List<PurchaseOrderDetailDto> details,
        List<UserApprovalDto> approvals,
        string voucherNo,
        string displayModuleName,
        long moduleId,
        UserContext userContext)
    {
        foreach (var approval in approvals)
        {
            var detail = details.FirstOrDefault(d => d.ItemID == approval.ItemID);
            if (detail == null) continue;

            var itemDescription = $"LedgerID = {approval.LedgerID} And LedgerName= {approval.LedgerName} " +
                $"And ItemName= {approval.ItemName} And ItemCode= {approval.ItemCode} " +
                $"And ExpectedDeliveryDate= {approval.ExpectedDeliveryDate:yyyy-MM-dd} " +
                $"And PurchaseQty= {approval.PurchaseQty} And ItemRate = {approval.ItemRate} " +
                $"And ItemID= {approval.ItemID} And ItemAmount = {approval.ItemAmount}";

            var result = await _repository.CreateApprovalWorkflowAsync(
                transactionId,
                0,
                approval.ItemID,
                itemDescription,
                displayModuleName,
                moduleId,
                voucherNo,
                approval.LedgerID,
                approval.ItemName,
                approval.PurchaseQty,
                approval.ItemRate,
                approval.ItemAmount
            );

            if (result != "Success")
            {
                throw new Exception(result);
            }
        }
    }

    private PurchaseOrder MapToMainEntity(PurchaseOrderMainDto dto, string prefix, long maxVoucherNo, string voucherNo, string amountInWords, UserContext context)
    {
        return new PurchaseOrder
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
            TotalCGSTTaxAmount = dto.TotalCGSTTaxAmount,
            TotalSGSTTaxAmount = dto.TotalSGSTTaxAmount,
            TotalIGSTTaxAmount = dto.TotalIGSTTaxAmount,
            TotalTaxAmount = dto.TotalTaxAmount,
            NetAmount = dto.NetAmount,
            TotalOverheadAmount = dto.TotalOverheadAmount,
            PurchaseDivision = dto.PurchaseDivision,
            PurchaseReferenceRemark = dto.PurchaseReferenceRemark,
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

    private List<PurchaseOrderDetail> MapToDetailEntities(List<PurchaseOrderDetailDto> dtos, int isVoucherItemApproved, long approvalByUserId, UserContext context)
    {
        return dtos.Select(dto => new PurchaseOrderDetail
        {
            ItemID = dto.ItemID,
            TransID = dto.TransID,
            ItemGroupID = dto.ItemGroupID,
            RequiredQuantity = dto.RequiredQuantity,
            RequiredNoOfPacks = dto.RequiredNoOfPacks,
            QuantityPerPack = dto.QuantityPerPack,
            PurchaseOrderQuantity = dto.PurchaseOrderQuantity,
            ChallanWeight = dto.ChallanWeight,
            PurchaseUnit = dto.PurchaseUnit,
            StockUnit = dto.StockUnit,
            ItemDescription = dto.ItemDescription,
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
            ItemNarration = dto.ItemNarration,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            RefJobBookingJobCardContentsID = dto.RefJobBookingJobCardContentsID,
            RefJobCardContentNo = dto.RefJobCardContentNo,
            ClientID = dto.ClientID,
            Remark = dto.Remark,
            ProductHSNID = dto.ProductHSNID,
            IsVoucherItemApproved = isVoucherItemApproved,
            VoucherItemApprovedBy = approvalByUserId,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private List<PurchaseOrderTax> MapToTaxEntities(List<PurchaseOrderTaxDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new PurchaseOrderTax
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

    private List<PurchaseOrderSchedule> MapToScheduleEntities(List<PurchaseOrderScheduleDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new PurchaseOrderSchedule
        {
            TransID = dto.TransID,
            ItemID = dto.ItemID,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            ScheduleDeliveryDate = dto.ScheduleDeliveryDate,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private List<PurchaseOrderOverhead> MapToOverheadEntities(List<PurchaseOrderOverheadDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new PurchaseOrderOverhead
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

    private List<PurchaseOrderRequisition> MapToRequisitionEntities(List<PurchaseOrderRequisitionDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new PurchaseOrderRequisition
        {
            TransID = dto.TransID,
            ItemID = dto.ItemID,
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

    public async Task<Result<List<PurchaseOrderDataDto>>> GetPurchaseOrderAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetPurchaseOrderDataAsync(transactionId);
            return Result<List<PurchaseOrderDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase order {TransactionId}", transactionId);
            return Result<List<PurchaseOrderDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<PurchaseOrderListDto>>> GetPurchaseOrderListAsync(GetPurchaseOrderListRequest request)
    {
        try
        {
            var data = await _repository.GetPurchaseOrderListAsync(
                request.FromDateValue,
                request.ToDateValue,
                request.FilterStr,
                request.Detail
            );
            return Result<List<PurchaseOrderListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase order list");
            return Result<List<PurchaseOrderListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<PendingRequisitionDto>>> GetPendingRequisitionsAsync()
    {
        try
        {
            var data = await _repository.GetPendingRequisitionsAsync();
            return Result<List<PendingRequisitionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending requisitions");
            return Result<List<PendingRequisitionDto>>.Failure($"Error: {ex.Message}");
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

    public async Task<Result<List<SupplierDto>>> GetSuppliersAsync()
    {
        try
        {
            var data = await _repository.GetSuppliersAsync();
            return Result<List<SupplierDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return Result<List<SupplierDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ContactPersonDto>>> GetContactPersonsAsync(long ledgerId)
    {
        try
        {
            var data = await _repository.GetContactPersonsAsync(ledgerId);
            return Result<List<ContactPersonDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact persons for ledger {LedgerId}", ledgerId);
            return Result<List<ContactPersonDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<DeliveryAddressDto>>> GetDeliveryAddressesAsync()
    {
        try
        {
            var data = await _repository.GetDeliveryAddressesAsync();
            return Result<List<DeliveryAddressDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery addresses");
            return Result<List<DeliveryAddressDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<OverheadChargeHeadDto>>> GetOverheadChargeHeadsAsync()
    {
        try
        {
            var data = await _repository.GetOverheadChargeHeadsAsync();
            return Result<List<OverheadChargeHeadDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overhead charge heads");
            return Result<List<OverheadChargeHeadDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TaxChargeLedgerDto>>> GetTaxChargeLedgersAsync()
    {
        try
        {
            var data = await _repository.GetTaxChargeLedgersAsync();
            return Result<List<TaxChargeLedgerDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax charge ledgers");
            return Result<List<TaxChargeLedgerDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CurrencyDto>>> GetCurrenciesAsync()
    {
        try
        {
            var data = await _repository.GetCurrenciesAsync();
            return Result<List<CurrencyDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currencies");
            return Result<List<CurrencyDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<HSNCodeDto>>> GetHSNCodesAsync()
    {
        try
        {
            var data = await _repository.GetHSNCodesAsync();
            return Result<List<HSNCodeDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HSN codes");
            return Result<List<HSNCodeDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<AttachmentFileDto>>> GetAttachmentsAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetAttachmentsAsync(transactionId);
            return Result<List<AttachmentFileDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for transaction {TransactionId}", transactionId);
            return Result<List<AttachmentFileDto>>.Failure($"Error: {ex.Message}");
        }
    }
}