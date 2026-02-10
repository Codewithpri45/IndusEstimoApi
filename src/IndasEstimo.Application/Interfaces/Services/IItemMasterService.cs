using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.Interfaces.Services;

/// <summary>
/// Item Master service - handles dynamic item management across item groups
/// Preserves all functionality from legacy ASP.NET Web API 2 controller
/// </summary>
public interface IItemMasterService
{
    /// <summary>
    /// Get list of item groups (masters) user has permission to view
    /// </summary>
    Task<Result<List<MasterListDto>>> GetMasterListAsync();

    /// <summary>
    /// Get grid data for specific master using dynamic query from ItemGroupMaster.SelectQuery
    /// Returns dynamic data structure based on database query
    /// </summary>
    Task<Result<object>> GetMasterGridAsync(string masterID);

    /// <summary>
    /// Get grid column hide configuration for specific master
    /// Includes TabName, ItemNameFormula, ItemDescriptionFormula
    /// </summary>
    Task<Result<List<GridColumnHideDto>>> GetGridColumnHideAsync(string masterID);

    /// <summary>
    /// Get grid column names for specific master
    /// </summary>
    Task<Result<List<GridColumnDto>>> GetGridColumnAsync(string masterID);

    /// <summary>
    /// Save new item with dynamic field data
    /// Uses Dictionary structure to handle dynamic fields from ItemGroupFieldMaster
    /// </summary>
    Task<Result<string>> SaveItemAsync(SaveItemRequest request);

    /// <summary>
    /// Update existing item with dynamic field data
    /// </summary>
    Task<Result<string>> UpdateItemAsync(UpdateItemRequest request);

    /// <summary>
    /// Delete item (soft delete - sets IsDeletedTransaction = 1)
    /// </summary>
    Task<Result<string>> DeleteItemAsync(string itemID, string itemgroupID);

    /// <summary>
    /// Check if item can be deleted (not referenced in transactions)
    /// Returns "Exist" if item is in use, empty string if can be deleted
    /// </summary>
    Task<Result<string>> CheckPermissionAsync(string transactionID);

    /// <summary>
    /// Get loaded data for editing item
    /// Executes stored procedure SelectedRowMultiUnit
    /// </summary>
    Task<Result<object>> GetLoadedDataAsync(string masterID, string itemId);

    /// <summary>
    /// Get drill-down data for specific tab
    /// Uses dynamic query from DrilDown table
    /// </summary>
    Task<Result<object>> GetDrillDownDataAsync(string masterID, string tabID);

    /// <summary>
    /// Get field metadata for specific item group
    /// Returns field definitions from ItemGroupFieldMaster
    /// </summary>
    Task<Result<List<MasterFieldDto>>> GetMasterFieldsAsync(string masterID);

    /// <summary>
    /// Load dynamic dropdown data for multiple fields
    /// Executes queries from ItemGroupFieldMaster.SelectboxQueryDB
    /// Returns dataset with table names like "tbl_{FieldName}"
    /// </summary>
    Task<Result<object>> SelectBoxLoadAsync(JArray jsonData);

    /// <summary>
    /// Get list of item sub-groups (under groups)
    /// </summary>
    Task<Result<List<UnderGroupDto>>> GetUnderGroupAsync();

    /// <summary>
    /// Get list of item groups with hierarchy information
    /// </summary>
    Task<Result<List<GroupDto>>> GetGroupAsync();

    /// <summary>
    /// Save new item sub-group
    /// </summary>
    Task<Result<string>> SaveGroupAsync(SaveGroupRequest request);

    /// <summary>
    /// Update existing item sub-group
    /// </summary>
    Task<Result<string>> UpdateGroupAsync(UpdateGroupRequest request);

    /// <summary>
    /// Delete item sub-group (soft delete)
    /// </summary>
    Task<Result<string>> DeleteGroupAsync(DeleteGroupRequest request);

    /// <summary>
    /// Get all item groups
    /// </summary>
    Task<Result<List<ItemGroupDto>>> GetItemsAsync();

    /// <summary>
    /// Get all ledger groups for current company
    /// </summary>
    Task<Result<List<LedgerGroupDto>>> GetLedgersAsync();

    /// <summary>
    /// Check if item can be updated (not referenced in critical tables)
    /// Checks JobBookingContent, JobBookingJobCardContents, ProductMasterContents, ItemTransactionDetail
    /// Returns "Exist" if item is in use, "Success" if can be updated
    /// </summary>
    Task<Result<string>> CheckPermissionForUpdateAsync(string itemID);

    /// <summary>
    /// Update user-specific item data (e.g., PurchaseRate, EstimationRate)
    /// </summary>
    Task<Result<string>> UpdateUserItemAsync(UpdateUserItemRequest request);
}
