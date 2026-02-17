using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Repositories.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Infrastructure.Services.Inventory;

public class ReturnToStockService : IReturnToStockService
{
    private readonly IReturnToStockRepository _repository;
    private readonly ILogger<ReturnToStockService> _logger;

    public ReturnToStockService(
        IReturnToStockRepository repository,
        ILogger<ReturnToStockService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> GetReturnNoAsync(string prefix)
    {
        try
        {
            var data = await _repository.GetReturnNoAsync(prefix);
            return Result<string>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating return number for prefix {Prefix}", prefix);
            return Result<string>.Failure($"Error: {ex.Message}");
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
            _logger.LogError(ex, "Error getting bins list for warehouse {WarehouseName}", warehouseName);
            return Result<List<BinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<BinDto>>> GetDestinationBinsListAsync(string warehouseName)
    {
        try
        {
            var data = await _repository.GetDestinationBinsListAsync(warehouseName);
            return Result<List<BinDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting destination bins list for warehouse {WarehouseName}", warehouseName);
            return Result<List<BinDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MachineDto>>> GetMachinesByDepartmentAsync(int departmentId)
    {
        try
        {
            var data = await _repository.GetMachinesByDepartmentAsync(departmentId);
            return Result<List<MachineDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machines for department {DepartmentId}", departmentId);
            return Result<List<MachineDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ReturnListDto>>> GetReturnListAsync(string fromDate, string toDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromDate))
            {
                var today = DateTime.Today;
                fromDate = new DateTime(today.Year, today.Month, 1).ToString("dd-MM-yyyy");
            }

            if (string.IsNullOrWhiteSpace(toDate))
                toDate = DateTime.Today.ToString("dd-MM-yyyy");

            var data = await _repository.GetReturnListAsync(fromDate, toDate);
            return Result<List<ReturnListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting return list from {FromDate} to {ToDate}", fromDate, toDate);
            return Result<List<ReturnListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ReturnDetailDto>>> GetReturnDetailsAsync(long transactionId)
    {
        try
        {
            var data = await _repository.GetReturnDetailsAsync(transactionId);
            return Result<List<ReturnDetailDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting return details for transaction {TransactionId}", transactionId);
            return Result<List<ReturnDetailDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<FloorStockDto>>> GetAvailableFloorStockAsync(string issueType)
    {
        try
        {
            var validTypes = new[] { "AllIssueVouchers", "JobIssueVouchers", "NonJobIssueVouchers" };
            if (!validTypes.Contains(issueType))
                issueType = "AllIssueVouchers";

            var data = await _repository.GetAvailableFloorStockAsync(issueType);
            return Result<List<FloorStockDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available floor stock for issue type {IssueType}", issueType);
            return Result<List<FloorStockDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ReturnOperationResponseDto>> SaveReturnDataAsync(SaveReturnDataRequest request)
    {
        try
        {
            // Validate request
            if (request.MainData == null || !request.DetailData.Any())
                return Result<ReturnOperationResponseDto>.Failure("Invalid request: Main data and detail data are required");

            var result = await _repository.SaveReturnDataAsync(request);

            if (result.Success)
                return Result<ReturnOperationResponseDto>.Success(result);
            else
                return Result<ReturnOperationResponseDto>.Failure(result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving return to stock data");
            return Result<ReturnOperationResponseDto>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateReturnAsync(UpdateReturnDataRequest request)
    {
        try
        {
            // Validate request
            if (request.TransactionID <= 0)
                return Result<bool>.Failure("Invalid transaction ID");

            if (request.MainData == null || !request.DetailData.Any())
                return Result<bool>.Failure("Invalid request: Main data and detail data are required");

            await _repository.UpdateReturnAsync(request);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating return to stock for transaction {TransactionId}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteReturnAsync(DeleteReturnRequest request)
    {
        try
        {
            // Validate request
            if (request.TransactionID <= 0)
                return Result<bool>.Failure("Invalid transaction ID");

            await _repository.DeleteReturnAsync(request.TransactionID);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete return transaction {TransactionId}", request.TransactionID);
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting return to stock for transaction {TransactionId}", request.TransactionID);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}
