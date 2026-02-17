using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class ItemTransferBetweenWarehousesService : IItemTransferBetweenWarehousesService
{
    private readonly IItemTransferBetweenWarehousesRepository _repository;
    private readonly ILogger<ItemTransferBetweenWarehousesService> _logger;

    public ItemTransferBetweenWarehousesService(
        IItemTransferBetweenWarehousesRepository repository,
        ILogger<ItemTransferBetweenWarehousesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<TransferListDto>>> GetTransferListAsync(string fromDate, string toDate)
    {
        try
        {
            var data = await _repository.GetTransferListAsync(fromDate, toDate);
            return Result<List<TransferListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer list from {FromDate} to {ToDate}", fromDate, toDate);
            return Result<List<TransferListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<WarehouseStockDto>>> GetWarehouseStockAsync(long warehouseId)
    {
        try
        {
            var data = await _repository.GetWarehouseStockAsync(warehouseId);
            return Result<List<WarehouseStockDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse stock for warehouse {WarehouseID}", warehouseId);
            return Result<List<WarehouseStockDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BinDto>>> GetDestinationBinsAsync(string warehouseName, long sourceBinId)
    {
        try
        {
            var data = await _repository.GetDestinationBinsAsync(warehouseName, sourceBinId);
            return Result<List<BinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting destination bins for warehouse {WarehouseName}", warehouseName);
            return Result<List<BinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetTransferVoucherNoAsync(string prefix)
    {
        try
        {
            var data = await _repository.GetTransferVoucherNoAsync(prefix);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating voucher number for prefix {Prefix}", prefix);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<TransferOperationResponseDto>> SaveTransferAsync(SaveTransferRequest request)
    {
        try
        {
            if (request.IssueDetails == null || request.IssueDetails.Count == 0)
                return Result<TransferOperationResponseDto>.Failure("No issue detail items provided");

            if (request.ReceiptDetails == null || request.ReceiptDetails.Count == 0)
                return Result<TransferOperationResponseDto>.Failure("No receipt detail items provided");

            var data = await _repository.SaveTransferAsync(request);
            return Result<TransferOperationResponseDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving transfer");
            return Result<TransferOperationResponseDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateTransferAsync(UpdateTransferRequest request)
    {
        try
        {
            if (request.IssueDetails == null || request.IssueDetails.Count == 0)
                return Result<bool>.Failure("No issue detail items provided");

            if (request.ReceiptDetails == null || request.ReceiptDetails.Count == 0)
                return Result<bool>.Failure("No receipt detail items provided");

            await _repository.UpdateTransferAsync(request);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transfer {TransactionID}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTransferAsync(DeleteTransferRequest request)
    {
        try
        {
            await _repository.DeleteTransferAsync(request.TransactionID);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transfer {TransactionID}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
