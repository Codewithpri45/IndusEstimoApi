using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Request DTOs ====================

/// <summary>
/// Request for saving new item - preserves Dictionary structure for dynamic fields
/// </summary>
public class SaveItemRequest
{
    public Dictionary<string, object>[] CostingDataItemMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string MasterName { get; set; } = "";
    public string ItemGroupID { get; set; } = "";
    public string ActiveItem { get; set; } = "";
    public string StockRefCode { get; set; } = "";
}

/// <summary>
/// Request for updating existing item - preserves Dictionary structure for dynamic fields
/// </summary>
public class UpdateItemRequest
{
    public Dictionary<string, object>[] CostingDataItemMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] CostingDataItemDetailMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string MasterName { get; set; } = "";
    public string ItemID { get; set; } = "";
    public string UnderGroupID { get; set; } = "";
    public string ActiveItem { get; set; } = "";
    public string StockRefCode { get; set; } = "";
}

/// <summary>
/// Request for saving item sub-group
/// </summary>
public class SaveGroupRequest
{
    public Dictionary<string, object>[] CostingDataGroupMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string GroupName { get; set; } = "";
    public string UnderGroupID { get; set; } = "";
}

/// <summary>
/// Request for updating item sub-group
/// </summary>
public class UpdateGroupRequest
{
    public Dictionary<string, object>[] CostingDataGroupMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string ItemSubGroupUniqueID { get; set; } = "";
    public string ItemSubGroupLevel { get; set; } = "";
    public string GroupName { get; set; } = "";
}

/// <summary>
/// Request for deleting item sub-group
/// </summary>
public class DeleteGroupRequest
{
    public string ItemSubGroupUniqueID { get; set; } = "";
}

/// <summary>
/// Request for updating user-specific item data
/// </summary>
public class UpdateUserItemRequest
{
    public Dictionary<string, object>[] ItemName { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string ItemID { get; set; } = "";
    public string StockRefCode { get; set; } = "";
}

// ==================== Response DTOs ====================

/// <summary>
/// Master list - item groups with user permissions
/// </summary>
public class MasterListDto
{
    public int ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = "";
    public string GridColumnName { get; set; } = "";
    public string GridColumnHide { get; set; } = "";
}

/// <summary>
/// Grid column configuration
/// </summary>
public class GridColumnDto
{
    public string GridColumnName { get; set; } = "";
}

/// <summary>
/// Grid column hide configuration with formulas
/// </summary>
public class GridColumnHideDto
{
    public string GridColumnHide { get; set; } = "";
    public string TabName { get; set; } = "";
    public string ItemNameFormula { get; set; } = "";
    public string ItemDescriptionFormula { get; set; } = "";
}

/// <summary>
/// Master field metadata from ItemGroupFieldMaster
/// </summary>
public class MasterFieldDto
{
    public string ItemGroupFieldID { get; set; } = "";
    public string ItemGroupID { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string FieldDataType { get; set; } = "";
    public string FieldDescription { get; set; } = "";
    public string IsDisplay { get; set; } = "";
    public string IsCalculated { get; set; } = "";
    public string FieldFormula { get; set; } = "";
    public string FieldTabIndex { get; set; } = "";
    public string FieldDrawSequence { get; set; } = "";
    public string FieldDefaultValue { get; set; } = "";
    public string CompanyID { get; set; } = "";
    public string UserID { get; set; } = "";
    public string ModifiedDate { get; set; } = "";
    public string FYear { get; set; } = "";
    public string IsActive { get; set; } = "";
    public string IsDeleted { get; set; } = "";
    public string FieldDisplayName { get; set; } = "";
    public string FieldType { get; set; } = "";
    public string SelectBoxQueryDB { get; set; } = "";
    public string SelectBoxDefault { get; set; } = "";
    public string ControllValidation { get; set; } = "";
    public string FieldFormulaString { get; set; } = "";
    public string IsRequiredFieldValidator { get; set; } = "";
    public string UnitMeasurement { get; set; } = "";
    public bool IsLocked { get; set; }
    public decimal MinimumValue { get; set; }
    public decimal MaximumValue { get; set; }
}

/// <summary>
/// Item sub-group (under group)
/// </summary>
public class UnderGroupDto
{
    public int ItemSubGroupID { get; set; }
    public string ItemSubGroupDisplayName { get; set; } = "";
}

/// <summary>
/// Item group with hierarchy information
/// </summary>
public class GroupDto
{
    public int ItemSubGroupUniqueID { get; set; }
    public int ItemSubGroupID { get; set; }
    public string ItemSubGroupDisplayName { get; set; } = "";
    public int UnderSubGroupID { get; set; }
    public string ItemSubGroupName { get; set; } = "";
    public int ItemSubGroupLevel { get; set; }
    public string GroupName { get; set; } = "";
}

/// <summary>
/// Item group basic info
/// </summary>
public class ItemGroupDto
{
    public int ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = "";
}

/// <summary>
/// Ledger group basic info
/// </summary>
public class LedgerGroupDto
{
    public int LedgerGroupID { get; set; }
    public string LedgerGroupName { get; set; } = "";
}

/// <summary>
/// Permission check result
/// </summary>
public class CheckPermissionResult
{
    public bool CanModify { get; set; }
    public string Message { get; set; } = "";
}