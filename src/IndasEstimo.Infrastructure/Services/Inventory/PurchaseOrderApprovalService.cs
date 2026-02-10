using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class PurchaseOrderApprovalService : IPurchaseOrderApprovalService
{
    private readonly IPurchaseOrderApprovalRepository _repository;
    private readonly ILogger<PurchaseOrderApprovalService> _logger;

    public PurchaseOrderApprovalService(
        IPurchaseOrderApprovalRepository repository,
        ILogger<PurchaseOrderApprovalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<UnapprovedPurchaseOrderDto>>> GetUnapprovedPurchaseOrdersAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetUnapprovedPurchaseOrdersAsync(fromDate, toDate);
            return Result<List<UnapprovedPurchaseOrderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unapproved purchase orders");
            return Result<List<UnapprovedPurchaseOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ApprovedPurchaseOrderDto>>> GetApprovedPurchaseOrdersAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetApprovedPurchaseOrdersAsync(fromDate, toDate);
            return Result<List<ApprovedPurchaseOrderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approved purchase orders");
            return Result<List<ApprovedPurchaseOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CancelledPurchaseOrderDto>>> GetCancelledPurchaseOrdersAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetCancelledPurchaseOrdersAsync(fromDate, toDate);
            return Result<List<CancelledPurchaseOrderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancelled purchase orders");
            return Result<List<CancelledPurchaseOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsPurchaseOrderProcessedAsync(long transactionId)
    {
        try
        {
            var isProcessed = await _repository.IsPurchaseOrderProcessedAsync(transactionId);
            return Result<bool>.Success(isProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if purchase order is processed");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for approval");
            }

            var result = await _repository.ApprovePurchaseOrdersAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving purchase orders");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for cancellation");
            }

            var result = await _repository.CancelPurchaseOrdersAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase orders");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnapprovePurchaseOrdersAsync(List<PurchaseOrderApprovalItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for unapproval");
            }

            var result = await _repository.UnapprovePurchaseOrdersAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unapproving purchase orders");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UncancelPurchaseOrdersAsync(List<PurchaseOrderCancellationItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for uncancellation");
            }

            var result = await _repository.UncancelPurchaseOrdersAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uncancelling purchase orders");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
