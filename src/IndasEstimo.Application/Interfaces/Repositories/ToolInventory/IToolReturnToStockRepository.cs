using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Domain.Entities.ToolInventory;

namespace IndasEstimo.Application.Interfaces.Repositories.ToolInventory;

public interface IToolReturnToStockRepository
{
    // ==================== CRUD Operations ====================
    Task<long> SaveToolReturnToStockAsync(
        ToolReturnToStock main,
        List<ToolReturnToStockDetail> details
    );

    Task<bool> DeleteToolReturnToStockAsync(long transactionId);

    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextReturnNoAsync(string prefix);

    // ==================== Retrieve Operations ====================
    Task<List<ToolAvailableForReturnDto>> GetAvailableForReturnAsync();
    Task<List<ToolReturnToStockListDto>> GetReturnToStockListAsync();
}
