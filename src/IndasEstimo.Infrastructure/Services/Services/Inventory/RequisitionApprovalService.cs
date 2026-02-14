using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class RequisitionApprovalService : IRequisitionApprovalService
{
    private readonly IRequisitionApprovalRepository _repository;
    private readonly ILogger<RequisitionApprovalService> _logger;

    public RequisitionApprovalService(
        IRequisitionApprovalRepository repository,
        ILogger<RequisitionApprovalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<UnapprovedRequisitionDto>>> GetUnapprovedRequisitionsAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetUnapprovedRequisitionsAsync(fromDate, toDate);
            return Result<List<UnapprovedRequisitionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unapproved requisitions");
            return Result<List<UnapprovedRequisitionDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ApprovedRequisitionDto>>> GetApprovedRequisitionsAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetApprovedRequisitionsAsync(fromDate, toDate);
            return Result<List<ApprovedRequisitionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approved requisitions");
            return Result<List<ApprovedRequisitionDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CancelledRequisitionDto>>> GetCancelledRequisitionsAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetCancelledRequisitionsAsync(fromDate, toDate);
            return Result<List<CancelledRequisitionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cancelled requisitions");
            return Result<List<CancelledRequisitionDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveRequisitionsAsync(List<RequisitionApprovalItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for approval");
            }

            var result = await _repository.ApproveRequisitionsAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving requisitions");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelRequisitionsAsync(List<RequisitionCancellationItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for cancellation");
            }

            var result = await _repository.CancelRequisitionsAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling requisitions");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnapproveRequisitionsAsync(List<RequisitionApprovalItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for unapproval");
            }

            var result = await _repository.UnapproveRequisitionsAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unapproving requisitions");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UncancelRequisitionsAsync(List<RequisitionCancellationItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<bool>.Failure("No items provided for uncancellation");
            }

            var result = await _repository.UncancelRequisitionsAsync(items);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uncancelling requisitions");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
