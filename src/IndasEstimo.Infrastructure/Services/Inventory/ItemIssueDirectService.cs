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

public class ItemIssueDirectService : IItemIssueDirectService
{
    private readonly IItemIssueDirectRepository _repository;
    private readonly IMasterDbService _masterDbService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDbOperationsService _dbOperations;
    private readonly ILogger<ItemIssueDirectService> _logger;

    public ItemIssueDirectService(
        IItemIssueDirectRepository repository,
        IMasterDbService masterDbService,
        IDbOperationsService dbOperations,
        ICurrentUserService currentUserService,
        ILogger<ItemIssueDirectService> logger)
    {
        _repository = repository;
        _masterDbService = masterDbService;
        _dbOperations = dbOperations;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    public async Task<Result<SaveItemIssueDirectResponse>> SaveItemIssueDirectAsync(SaveItemIssueDirectRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canSave = await _dbOperations.ValidateProductionUnitAsync("Save");
            if (canSave != "Authorize")
            {
                return Result<SaveItemIssueDirectResponse>.Failure(canSave);
            }

            var (voucherNo, maxVoucherNo) = await _repository.GenerateNextIssueNumberAsync(request.Prefix);
            var (slipNo, maxSlipNo) = await _repository.GenerateNextSlipNumberAsync();

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, maxVoucherNo, voucherNo, slipNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);
            var consumeMainEntities = MapToConsumeMainEntities(request.ObjectsConsumeMain, userContext);
            var consumeDetailEntities = MapToConsumeDetailEntities(request.ObjectsConsumeDetails, userContext);

            var transactionId = await _repository.SaveItemIssueDirectAsync(mainEntity, detailEntities, consumeMainEntities, consumeDetailEntities);

            // Update stock values after save
            await _repository.UpdateStockValuesAsync(transactionId);

            var response = new SaveItemIssueDirectResponse(transactionId, voucherNo, $"Success,TransactionID: {transactionId}");
            return Result<SaveItemIssueDirectResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving item issue direct");
            return Result<SaveItemIssueDirectResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SaveItemIssueDirectResponse>> UpdateItemIssueDirectAsync(UpdateItemIssueDirectRequest request)
    {
        try
        {
            var userContext = GetUserContext();
            var canUpdate = await _dbOperations.ValidateProductionUnitAsync("Update");
            if (canUpdate != "Authorize")
            {
                return Result<SaveItemIssueDirectResponse>.Failure(canUpdate);
            }

            if (await _repository.IsItemIssueUsedAsync(request.TransactionID))
            {
                return Result<SaveItemIssueDirectResponse>.Failure("TransactionUsed");
            }

            var voucherNo = await _repository.GetVoucherNoAsync(request.TransactionID) ?? "0";

            var mainEntity = MapToMainEntity(request.JsonObjectsRecordMain[0], request.Prefix, 0, voucherNo, request.JsonObjectsRecordMain[0].SlipNo, userContext);
            var detailEntities = MapToDetailEntities(request.JsonObjectsRecordDetail, userContext);
            var consumeMainEntities = MapToConsumeMainEntities(request.ObjectsConsumeMain, userContext);
            var consumeDetailEntities = MapToConsumeDetailEntities(request.ObjectsConsumeDetails, userContext);

            await _repository.UpdateItemIssueDirectAsync(request.TransactionID, mainEntity, detailEntities, consumeMainEntities, consumeDetailEntities);

            // Update stock values after update
            await _repository.UpdateStockValuesAsync(request.TransactionID);

            var response = new SaveItemIssueDirectResponse(request.TransactionID, voucherNo, "Success");
            return Result<SaveItemIssueDirectResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item issue direct {TransactionId}", request.TransactionID);
            return Result<SaveItemIssueDirectResponse>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteItemIssueDirectAsync(long transactionId, long? jobContID)
    {
        try
        {
            if (await _repository.IsItemIssueUsedAsync(transactionId))
            {
                return Result<string>.Success("TransactionUsed");
            }

            var canCrud = await _dbOperations.ValidateProductionUnitAsync("Delete");
            if (canCrud != "Authorize")
            {
                return Result<string>.Failure(canCrud);
            }

            var success = await _repository.DeleteItemIssueDirectAsync(transactionId, jobContID);
            return success ? Result<string>.Success("Success") : Result<string>.Failure("fail");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteItemIssueDirectAsync for ID {TransactionID}", transactionId);
            return Result<string>.Failure("fail");
        }
    }

    // ==================== Retrieve Operations ====================

    public async Task<Result<List<ItemIssueDirectDataDto>>> GetItemIssueDirectAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetItemIssueDirectDataAsync(transactionId);
            return Result<List<ItemIssueDirectDataDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item issue direct {TransactionId}", transactionId);
            return Result<List<ItemIssueDirectDataDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemIssueDirectListDto>>> GetItemIssuesDirectListAsync(GetItemIssuesDirectListRequest request)
    {
        try
        {
            var data = await _repository.GetItemIssuesDirectListAsync(
                request.FromDate,
                request.ToDate,
                request.ApplyDateFilter
            );
            return Result<List<ItemIssueDirectListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item issues direct list");
            return Result<List<ItemIssueDirectListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ItemIssueDirectDataDto>> GetItemIssueDirectHeaderAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetItemIssueDirectHeaderAsync(transactionId);
            if (data == null)
            {
                return Result<ItemIssueDirectDataDto>.Failure("Header not found");
            }
            return Result<ItemIssueDirectDataDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item issue direct header {TransactionId}", transactionId);
            return Result<ItemIssueDirectDataDto>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Picklist Operations ====================

    public async Task<Result<List<DirectPicklistDto>>> GetJobAllocatedPicklistAsync()
    {
        try
        {
            var data = await _repository.GetJobAllocatedPicklistAsync();
            return Result<List<DirectPicklistDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job allocated picklist");
            return Result<List<DirectPicklistDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<DirectPicklistDto>>> GetAllPicklistByStockTypeAsync(string stockType)
    {
        try
        {
            var data = await _repository.GetAllPicklistByStockTypeAsync(stockType);
            return Result<List<DirectPicklistDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all picklist for stock type {StockType}", stockType);
            return Result<List<DirectPicklistDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Stock Operations ====================

    public async Task<Result<List<StockBatchDirectDto>>> GetStockBatchWiseAsync(long itemId, long? jobBookingJobCardContentsID)
    {
        try
        {
            var data = await _repository.GetStockBatchWiseAsync(itemId, jobBookingJobCardContentsID);
            return Result<List<StockBatchDirectDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock batch-wise for item {ItemId}", itemId);
            return Result<List<StockBatchDirectDto>>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Lookup Operations ====================

    public async Task<Result<string>> GetNextIssueNumberAsync(string prefix)
    {
        try
        {
            var (voucherNo, _) = await _repository.GenerateNextIssueNumberAsync(prefix);
            return Result<string>.Success(voucherNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next issue number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetNextSlipNumberAsync()
    {
        try
        {
            var (slipNo, _) = await _repository.GenerateNextSlipNumberAsync();
            return Result<string>.Success(slipNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next slip number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<JobCardDirectDto>>> GetJobCardFilterListAsync()
    {
        try
        {
            var data = await _repository.GetJobCardFilterListAsync();
            return Result<List<JobCardDirectDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job card filter list");
            return Result<List<JobCardDirectDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentDto>>> GetDepartmentsAsync()
    {
        try
        {
            var data = await _repository.GetDepartmentsAsync();
            return Result<List<DepartmentDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return Result<List<DepartmentDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineDto>>> GetMachinesByDepartmentAsync(long departmentId)
    {
        try
        {
            var data = await _repository.GetMachinesByDepartmentAsync(departmentId);
            return Result<List<MachineDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines for department {DepartmentId}", departmentId);
            return Result<List<MachineDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ProcessDto>>> GetProcessListJobWiseAsync(long jobCardContentsId)
    {
        try
        {
            var data = await _repository.GetProcessListJobWiseAsync(jobCardContentsId);
            return Result<List<ProcessDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving process list for job card {JobCardContentsId}", jobCardContentsId);
            return Result<List<ProcessDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemIssueWarehouseDto>>> GetWarehousesAsync()
    {
        try
        {
            var data = await _repository.GetWarehousesAsync();
            return Result<List<ItemIssueWarehouseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses");
            return Result<List<ItemIssueWarehouseDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemIssueBinDto>>> GetBinsAsync(string warehouseName)
    {
        try
        {
            var data = await _repository.GetBinsAsync(warehouseName);
            return Result<List<ItemIssueBinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bins for warehouse {WarehouseName}", warehouseName);
            return Result<List<ItemIssueBinDto>>.Failure($"Error: {ex.Message}");
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

    public async Task<Result<bool>> CheckUserAuthorityAsync()
    {
        try
        {
            var hasAuthority = await _repository.CheckUserAuthorityAsync();
            return Result<bool>.Success(hasAuthority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user authority");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    // ==================== Private Helper Methods ====================

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

    private ItemIssueDirectMain MapToMainEntity(ItemIssueDirectMainDto dto, string prefix, long maxVoucherNo, string voucherNo, string slipNo, UserContext context)
    {
        return new ItemIssueDirectMain
        {
            VoucherID = dto.VoucherID,
            VoucherPrefix = prefix,
            MaxVoucherNo = maxVoucherNo,
            VoucherNo = voucherNo,
            VoucherDate = dto.VoucherDate,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            LedgerID = dto.LedgerID,
            DepartmentID = dto.DepartmentID,
            MachineID = dto.MachineID,
            ProcessID = dto.ProcessID,
            FloorGodownID = dto.FloorGodownID,
            BinID = dto.BinID,
            SlipNo = slipNo,
            SlipDate = dto.SlipDate,
            TotalIssueQuantity = dto.TotalIssueQuantity,
            IssuedQuantity = dto.IssuedQuantity,
            Narration = dto.Narration,
            JobCardNo = dto.JobCardNo,
            BookingNo = dto.BookingNo,
            RequiredSheets = dto.RequiredSheets,
            AllocatedSheets = dto.AllocatedSheets,
            RequiredQuantity = dto.RequiredQuantity,
            JobName = dto.JobName,
            ContentName = dto.ContentName,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        };
    }

    private List<ItemIssueDirectDetail> MapToDetailEntities(List<ItemIssueDirectDetailDto> dtos, UserContext context)
    {
        return dtos.Select(dto => new ItemIssueDirectDetail
        {
            ItemID = dto.ItemID,
            ItemGroupID = dto.ItemGroupID,
            ItemSubGroupID = dto.ItemSubGroupID,
            JobBookingID = dto.JobBookingID,
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            PicklistReleaseTransactionID = dto.PicklistReleaseTransactionID,
            PicklistTransactionID = dto.PicklistTransactionID,
            DepartmentID = dto.DepartmentID,
            MachineID = dto.MachineID,
            ProcessID = dto.ProcessID,
            FloorGodownID = dto.FloorGodownID,
            BinID = dto.BinID,
            BatchID = dto.BatchID,
            BatchNo = dto.BatchNo,
            SupplierBatchNo = dto.SupplierBatchNo,
            GRNNo = dto.GRNNo,
            GRNDate = dto.GRNDate,
            IssueQuantity = dto.IssueQuantity,
            ReleaseQuantity = dto.ReleaseQuantity,
            PendingQuantity = dto.PendingQuantity,
            PhysicalStock = dto.PhysicalStock,
            AllocatedStock = dto.AllocatedStock,
            FreeStock = dto.FreeStock,
            BatchStock = dto.BatchStock,
            AgeingDays = dto.AgeingDays,
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            ItemGroupName = dto.ItemGroupName,
            ItemSubGroupName = dto.ItemSubGroupName,
            StockUnit = dto.StockUnit,
            PicklistNo = dto.PicklistNo,
            ReleaseNo = dto.ReleaseNo,
            JobCardNo = dto.JobCardNo,
            JobName = dto.JobName,
            ContentName = dto.ContentName,
            ProcessName = dto.ProcessName,
            MachineName = dto.MachineName,
            DepartmentName = dto.DepartmentName,
            Warehouse = dto.Warehouse,
            Bin = dto.Bin,
            WtPerPacking = dto.WtPerPacking,
            UnitPerPacking = dto.UnitPerPacking,
            ConversionFactor = dto.ConversionFactor,
            UnitDecimalPlace = dto.UnitDecimalPlace,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private List<ItemIssueDirectConsumeMain>? MapToConsumeMainEntities(List<ItemIssueDirectConsumeMainDto>? dtos, UserContext context)
    {
        if (dtos == null || !dtos.Any())
            return null;

        return dtos.Select(dto => new ItemIssueDirectConsumeMain
        {
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            ConsumeQuantity = dto.ConsumeQuantity,
            CompanyID = context.CompanyId,
            FYear = context.FYear,
            ProductionUnitID = context.ProductionUnitId,
            UserID = context.UserId,
            CreatedBy = context.UserId,
            ModifiedBy = context.UserId
        }).ToList();
    }

    private List<ItemIssueDirectConsumeDetail>? MapToConsumeDetailEntities(List<ItemIssueDirectConsumeDetailDto>? dtos, UserContext context)
    {
        if (dtos == null || !dtos.Any())
            return null;

        return dtos.Select(dto => new ItemIssueDirectConsumeDetail
        {
            JobBookingJobCardContentsID = dto.JobBookingJobCardContentsID,
            ItemID = dto.ItemID,
            ConsumeQuantity = dto.ConsumeQuantity,
            StockUnit = dto.StockUnit,
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
}
