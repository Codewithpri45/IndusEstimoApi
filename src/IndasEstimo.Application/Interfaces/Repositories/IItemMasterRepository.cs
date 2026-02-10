using IndasEstimo.Application.DTOs.Masters;
using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.Interfaces.Repositories;

/// <summary>
/// Item Master repository - data access for dynamic item management
/// </summary>
public interface IItemMasterRepository
{
    Task<List<MasterListDto>> GetMasterListAsync();
    Task<object> GetMasterGridAsync(string masterID);
    Task<List<GridColumnHideDto>> GetGridColumnHideAsync(string masterID);
    Task<List<GridColumnDto>> GetGridColumnAsync(string masterID);
    Task<string> SaveItemAsync(SaveItemRequest request);
    Task<string> UpdateItemAsync(UpdateItemRequest request);
    Task<string> DeleteItemAsync(string itemID, string itemgroupID);
    Task<string> CheckPermissionAsync(string transactionID);
    Task<object> GetLoadedDataAsync(string masterID, string itemId);
    Task<object> GetDrillDownDataAsync(string masterID, string tabID);
    Task<List<MasterFieldDto>> GetMasterFieldsAsync(string masterID);
    Task<object> SelectBoxLoadAsync(JArray jsonData);
    Task<List<UnderGroupDto>> GetUnderGroupAsync();
    Task<List<GroupDto>> GetGroupAsync();
    Task<string> SaveGroupAsync(SaveGroupRequest request);
    Task<string> UpdateGroupAsync(UpdateGroupRequest request);
    Task<string> DeleteGroupAsync(DeleteGroupRequest request);
    Task<List<ItemGroupDto>> GetItemsAsync();
    Task<List<LedgerGroupDto>> GetLedgersAsync();
    Task<string> CheckPermissionForUpdateAsync(string itemID);
    Task<string> UpdateUserItemAsync(UpdateUserItemRequest request);
}
