using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class GRNApprovalService : IGRNApprovalService
{
    private readonly IGRNApprovalRepository _repository;
    private readonly ILogger<GRNApprovalService> _logger;

    public GRNApprovalService(
        IGRNApprovalRepository repository,
        ILogger<GRNApprovalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<GRNListDto>>> GetGRNListAsync(string radioValue, string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetGRNListAsync(radioValue, fromDate, toDate);
            return Result<List<GRNListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GRN list");
            return Result<List<GRNListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GRNBatchDetailDto>>> GetGRNBatchDetailAsync(long transactionId, string radioValue)
    {
        try
        {
            var data = await _repository.GetGRNBatchDetailAsync(transactionId, radioValue);
            return Result<List<GRNBatchDetailDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GRN batch detail");
            return Result<List<GRNBatchDetailDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ApproveGRNAsync(ApproveGRNRequest request)
    {
        try
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return Result<string>.Failure("No items provided for approval");
            }

            var result = await _repository.ApproveGRNAsync(request);

            if (!result.Success)
            {
                return Result<string>.Failure(result.Message);
            }

            return Result<string>.Success(result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving GRN");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(long transactionId)
    {
        try
        {
            var result = await _repository.CheckPermissionAsync(transactionId);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
