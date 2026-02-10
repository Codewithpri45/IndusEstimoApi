using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Repositories;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services.Masters;

public class ItemMasterService : IItemMasterService
{
    private readonly IItemMasterRepository _repository;
    private readonly ILogger<ItemMasterService> _logger;

    public ItemMasterService(
        IItemMasterRepository repository,
        ILogger<ItemMasterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<MasterListDto>>> GetMasterListAsync()
    {
        try
        {
            var data = await _repository.GetMasterListAsync();
            return Result<List<MasterListDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master list");
            return Result<List<MasterListDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetMasterGridAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetMasterGridAsync(masterID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master grid for masterID {MasterID}", masterID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GridColumnHideDto>>> GetGridColumnHideAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetGridColumnHideAsync(masterID);
            return Result<List<GridColumnHideDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grid column hide for masterID {MasterID}", masterID);
            return Result<List<GridColumnHideDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GridColumnDto>>> GetGridColumnAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetGridColumnAsync(masterID);
            return Result<List<GridColumnDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grid column for masterID {MasterID}", masterID);
            return Result<List<GridColumnDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveItemAsync(SaveItemRequest request)
    {
        try
        {
            if (request.CostingDataItemMaster == null || request.CostingDataItemMaster.Length == 0)
            {
                return Result<string>.Failure("No item data provided");
            }

            var result = await _repository.SaveItemAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving item");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateItemAsync(UpdateItemRequest request)
    {
        try
        {
            if (request.CostingDataItemMaster == null || request.CostingDataItemMaster.Length == 0)
            {
                return Result<string>.Failure("No item data provided");
            }

            var result = await _repository.UpdateItemAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteItemAsync(string itemID, string itemgroupID)
    {
        try
        {
            var result = await _repository.DeleteItemAsync(itemID, itemgroupID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemID}", itemID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionAsync(string transactionID)
    {
        try
        {
            var result = await _repository.CheckPermissionAsync(transactionID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for transaction {TransactionID}", transactionID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetLoadedDataAsync(string masterID, string itemId)
    {
        try
        {
            var data = await _repository.GetLoadedDataAsync(masterID, itemId);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loaded data for masterID {MasterID}, itemId {ItemId}", masterID, itemId);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> GetDrillDownDataAsync(string masterID, string tabID)
    {
        try
        {
            var data = await _repository.GetDrillDownDataAsync(masterID, tabID);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drill-down data for masterID {MasterID}, tabID {TabID}", masterID, tabID);
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<MasterFieldDto>>> GetMasterFieldsAsync(string masterID)
    {
        try
        {
            var data = await _repository.GetMasterFieldsAsync(masterID);
            return Result<List<MasterFieldDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master fields for masterID {MasterID}", masterID);
            return Result<List<MasterFieldDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> SelectBoxLoadAsync(JArray jsonData)
    {
        try
        {
            if (jsonData == null || jsonData.Count == 0)
            {
                return Result<object>.Failure("No field data provided");
            }

            var data = await _repository.SelectBoxLoadAsync(jsonData);
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading select box data");
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<UnderGroupDto>>> GetUnderGroupAsync()
    {
        try
        {
            var data = await _repository.GetUnderGroupAsync();
            return Result<List<UnderGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting under group");
            return Result<List<UnderGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<GroupDto>>> GetGroupAsync()
    {
        try
        {
            var data = await _repository.GetGroupAsync();
            return Result<List<GroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group");
            return Result<List<GroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> SaveGroupAsync(SaveGroupRequest request)
    {
        try
        {
            if (request.CostingDataGroupMaster == null || request.CostingDataGroupMaster.Length == 0)
            {
                return Result<string>.Failure("No group data provided");
            }

            var result = await _repository.SaveGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving group");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateGroupAsync(UpdateGroupRequest request)
    {
        try
        {
            if (request.CostingDataGroupMaster == null || request.CostingDataGroupMaster.Length == 0)
            {
                return Result<string>.Failure("No group data provided");
            }

            var result = await _repository.UpdateGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteGroupAsync(DeleteGroupRequest request)
    {
        try
        {
            var result = await _repository.DeleteGroupAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<ItemGroupDto>>> GetItemsAsync()
    {
        try
        {
            var data = await _repository.GetItemsAsync();
            return Result<List<ItemGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items");
            return Result<List<ItemGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LedgerGroupDto>>> GetLedgersAsync()
    {
        try
        {
            var data = await _repository.GetLedgersAsync();
            return Result<List<LedgerGroupDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ledgers");
            return Result<List<LedgerGroupDto>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> CheckPermissionForUpdateAsync(string itemID)
    {
        try
        {
            var result = await _repository.CheckPermissionForUpdateAsync(itemID);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for update for itemID {ItemID}", itemID);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UpdateUserItemAsync(UpdateUserItemRequest request)
    {
        try
        {
            if (request.ItemName == null || request.ItemName.Length == 0)
            {
                return Result<string>.Failure("No item data provided");
            }

            var result = await _repository.UpdateUserItemAsync(request);
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user item");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
