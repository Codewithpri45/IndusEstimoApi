using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class PurchaseGRNService : IPurchaseGRNService
{
    private readonly IPurchaseGRNRepository _repository;
    private readonly ILogger<PurchaseGRNService> _logger;

    public PurchaseGRNService(
        IPurchaseGRNRepository repository,
        ILogger<PurchaseGRNService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<PurchaseSupplierDto>>> GetPurchaseSuppliersListAsync()
    {
        try
        {
            var data = await _repository.GetPurchaseSuppliersListAsync();
            return Result<List<PurchaseSupplierDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase suppliers list");
            return Result<List<PurchaseSupplierDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<PendingPurchaseOrderDto>>> GetPendingOrdersListAsync()
    {
        try
        {
            var data = await _repository.GetPendingOrdersListAsync();
            return Result<List<PendingPurchaseOrderDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending orders list");
            return Result<List<PendingPurchaseOrderDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ReceiptNoteListDto>>> GetReceiptNoteListAsync(string fromDate, string toDate)
    {
        try
        {
            // Default to current month if dates are empty
            if (string.IsNullOrWhiteSpace(fromDate))
            {
                var today = DateTime.Today;
                fromDate = new DateTime(today.Year, today.Month, 1).ToString("dd-MM-yyyy");
            }
            if (string.IsNullOrWhiteSpace(toDate))
                toDate = DateTime.Today.ToString("dd-MM-yyyy");

            var data = await _repository.GetReceiptNoteListAsync(fromDate, toDate);
            return Result<List<ReceiptNoteListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt note list");
            return Result<List<ReceiptNoteListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ReceiptVoucherBatchDetailDto>>> GetReceiptVoucherBatchDetailAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetReceiptVoucherBatchDetailAsync(transactionId);
            return Result<List<ReceiptVoucherBatchDetailDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt voucher batch detail");
            return Result<List<ReceiptVoucherBatchDetailDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PreviousReceivedQuantityDto>> GetPreviousReceivedQuantityAsync(
        long purchaseTransactionId, long itemId, long grnTransactionId)
    {
        try
        {
            var data = await _repository.GetPreviousReceivedQuantityAsync(
                purchaseTransactionId, itemId, grnTransactionId);

            if (data == null)
            {
                return Result<PreviousReceivedQuantityDto>.Failure("Purchase order not found");
            }

            return Result<PreviousReceivedQuantityDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previous received quantity");
            return Result<PreviousReceivedQuantityDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ReceiverDto>>> GetReceiverListAsync()
    {
        try
        {
            var data = await _repository.GetReceiverListAsync();
            return Result<List<ReceiverDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receiver list");
            return Result<List<ReceiverDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<WarehouseDto>>> GetWarehouseListAsync()
    {
        try
        {
            var data = await _repository.GetWarehouseListAsync();
            return Result<List<WarehouseDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse list");
            return Result<List<WarehouseDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BinDto>>> GetBinsListAsync(string warehouseName)
    {
        try
        {
            var data = await _repository.GetBinsListAsync(warehouseName);
            return Result<List<BinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bins list");
            return Result<List<BinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GatePassDto>>> GetGatePassAsync(long ledgerId)
    {
        try
        {
            var data = await _repository.GetGatePassAsync(ledgerId);
            return Result<List<GatePassDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate pass");
            return Result<List<GatePassDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GRNItemDto>>> GetGrnItemListAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetGrnItemListAsync(transactionId);
            return Result<List<GRNItemDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GRN item list");
            return Result<List<GRNItemDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<UserAuthorityDto>> GetUserAuthorityAsync()
    {
        try
        {
            var data = await _repository.GetUserAuthorityAsync();
            if (data == null)
            {
                return Result<UserAuthorityDto>.Failure("User authority not found");
            }
            return Result<UserAuthorityDto>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user authority");
            return Result<UserAuthorityDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ValidateSupplierBatchReceiptDataAsync(int voucherID, List<SupplierBatchItem> items)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                return Result<string>.Success("Success");
            }

            var result = await _repository.ValidateSupplierBatchReceiptDataAsync(voucherID, items);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating supplier batch data");
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

    public async Task<Result<string>> GetLastTransactionDateAsync()
    {
        try
        {
            var result = await _repository.GetLastTransactionDateAsync();
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last transaction date");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetNextVoucherNoAsync(string prefix)
    {
        try
        {
            var result = await _repository.GetNextVoucherNoAsync(prefix);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next voucher number");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<(string VoucherNo, long TransactionID)>> SaveReceiptDataAsync(SaveReceiptDataRequest request)
    {
        try
        {
            if (request.DetailData == null || request.DetailData.Count == 0)
            {
                return Result<(string, long)>.Failure("No items provided for receipt");
            }

            var result = await _repository.SaveReceiptDataAsync(request);

            if (!result.Success)
            {
                return Result<(string, long)>.Failure(result.Message);
            }

            return Result<(string, long)>.Success((result.VoucherNo, result.TransactionID));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving receipt data");
            return Result<(string, long)>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateReceiptDataAsync(UpdateReceiptDataRequest request)
    {
        try
        {
            if (request.DetailData == null || request.DetailData.Count == 0)
            {
                return Result<bool>.Failure("No items provided for update");
            }

            var result = await _repository.UpdateReceiptDataAsync(request);

            if (!result.Success)
            {
                return Result<bool>.Failure(result.Message);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt data");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteGRNAsync(DeleteGRNRequest request)
    {
        try
        {
            var result = await _repository.DeleteGRNAsync(request);

            if (!result.Success)
            {
                return Result<bool>.Failure(result.Message);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting GRN");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
