using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Services.ToolInventory;

public interface IToolReturnToStockService
{
    // ==================== CRUD Operations ====================
    Task<Result<SaveToolReturnToStockResponse>> SaveToolReturnToStockAsync(SaveToolReturnToStockRequest request);
    Task<Result<string>> DeleteToolReturnToStockAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    Task<Result<List<ToolAvailableForReturnDto>>> GetAvailableForReturnAsync();
    Task<Result<List<ToolReturnToStockListDto>>> GetReturnToStockListAsync();

    // ==================== Helper/Lookup Operations ====================
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);
}
