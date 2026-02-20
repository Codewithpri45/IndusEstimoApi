namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Request DTOs ====================

/// <summary>
/// Request for saving new ledger - preserves Dictionary structure for dynamic fields
/// Old: LedgerMaster_SaveLedger
/// </summary>
public class SaveLedgerRequest
{
    public Dictionary<string, object>[] CostingDataLedgerMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] CostingDataLedgerDetailMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string MasterName { get; set; } = "";
    public string ActiveLedger { get; set; } = "";
    public string LedgerGroupID { get; set; } = "";
    public string LedgerRefCode { get; set; } = "";
}

/// <summary>
/// Request for updating existing ledger - preserves Dictionary structure for dynamic fields
/// Old: LedgerMaster_UpdateLedger
/// </summary>
public class UpdateLedgerRequest
{
    public Dictionary<string, object>[] CostingDataLedgerMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] CostingDataLedgerDetailMaster { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string MasterName { get; set; } = "";
    public string LedgerID { get; set; } = "";
    public string UnderGroupID { get; set; } = "";
    public string ActiveLedger { get; set; } = "";
    public string LedgerRefCode { get; set; } = "";
}

/// <summary>
/// Request for saving concern person details
/// Old: LedgerMaster_SaveConcernPerson
/// </summary>
public class SaveConcernPersonRequest
{
    public Dictionary<string, object>[] CostingDataSlab { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] CostingDataSlabUpdate { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string LedgerID { get; set; } = "";
}

/// <summary>
/// Request for saving machine allocation to employee
/// Old: LedgerMaster_SaveMachineAllocation
/// </summary>
public class SaveMachineAllocationRequest
{
    public Dictionary<string, object>[] CostingDataMachinAllocation { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string EmployeeID { get; set; } = "";
    public string GridRow { get; set; } = "";
}

/// <summary>
/// Request for saving group allocation
/// Old: LedgerMaster_SaveGroupAllocation
/// </summary>
public class SaveGroupAllocationRequest
{
    public Dictionary<string, object>[] CostingDataGroupAllocation { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] ObjSparePartAllocation { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string SupplierID { get; set; } = "";
    public string GridRow { get; set; } = "";
}

/// <summary>
/// Request for saving business vertical
/// Old: LedgerMaster_SaveBusinessVertical
/// </summary>
public class SaveBusinessVerticalRequest
{
    public Dictionary<string, object>[] BusinessVerticalDetailsData { get; set; } = Array.Empty<Dictionary<string, object>>();
}

/// <summary>
/// Request for updating business vertical
/// Old: LedgerMaster_UpdateBusinessVertical
/// </summary>
public class UpdateBusinessVerticalRequest
{
    public Dictionary<string, object>[] BusinessVerticalDetailsData { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string BusinessVerticalDetailID { get; set; } = "";
}

/// <summary>
/// Request for placing embargo on ledger
/// Old: LedgerMaster_PlaceEmbargo
/// </summary>
public class PlaceEmbargoRequest
{
    public Dictionary<string, object>[] ObjMainData { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] Arrdata { get; set; } = Array.Empty<Dictionary<string, object>>();
}

/// <summary>
/// Request for saving embargo details
/// Old: LedgerMaster_SaveEmbargoDetails
/// </summary>
public class SaveEmbargoDetailsRequest
{
    public Dictionary<string, object>[] EmbargoDetailsData { get; set; } = Array.Empty<Dictionary<string, object>>();
    public string LedgerID { get; set; } = "";
    public string txtStatus { get; set; } = "";
}

// ==================== Response DTOs ====================

/// <summary>
/// Master list - ledger groups with user permissions
/// Old: LedgerMaster_MasterList
/// </summary>
public class LedgerMasterListDto
{
    public int LedgerGroupID { get; set; }
    public string LedgerGroupName { get; set; } = "";
    public string LedgerGroupNameDisplay { get; set; } = "";
    public string LedgerGroupNameID { get; set; } = "";
}

/// <summary>
/// Grid column configuration for ledger
/// Old: LedgerMaster_GridColumn
/// </summary>
public class LedgerGridColumnDto
{
    public string GridColumnName { get; set; } = "";
}

/// <summary>
/// Grid column hide configuration with tab-specific settings
/// Old: LedgerMaster_GridColumnHide
/// </summary>
public class LedgerGridColumnHideDto
{
    public string GridColumnHide { get; set; } = "";
    public string TabName { get; set; } = "";
    public string ConcernPerson { get; set; } = "";
    public string EmployeeMachineAllocation { get; set; } = "";
}

/// <summary>
/// Master field metadata from LedgerGroupFieldMaster
/// Old: LedgerMaster_MasterField
/// </summary>
public class LedgerMasterFieldDto
{
    public string LedgerGroupFieldID { get; set; } = "";
    public string LedgerGroupID { get; set; } = "";
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
    public bool IsLocked { get; set; }
}

/// <summary>
/// Concern person details for a ledger
/// Old: LedgerMaster_ConcernPerson
/// </summary>
public class ConcernPersonDto
{
    public int ConcernPersonID { get; set; }
    public int LedgerID { get; set; }
    public string Name { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Email { get; set; } = "";
    public string Designation { get; set; } = "";
    public bool IsPrimaryConcernPerson { get; set; }
}

/// <summary>
/// Operator ledger details
/// Old: LedgerMaster_GetOperator
/// </summary>
public class OperatorDto
{
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
}

/// <summary>
/// Employee ledger details
/// Old: LedgerMaster_GetEmployee
/// </summary>
public class EmployeeDto
{
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
}

/// <summary>
/// Item group basic info for ledger operations
/// Old: LedgerMaster_GetItemGroup
/// </summary>
public class LedgerItemGroupDto
{
    public int ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = "";
}

/// <summary>
/// Spare part group information
/// Old: LedgerMaster_GetSpareGroup
/// </summary>
public class SpareGroupDto
{
    public string SparePartGroup { get; set; } = "";
}

/// <summary>
/// Ledger group basic info
/// Old: LedgerMaster_GetLedgerGroup
/// </summary>
public class LedgerGroupMasterDto
{
    public int LedgerGroupID { get; set; }
    public string LedgerGroupNameID { get; set; } = "";
}

/// <summary>
/// Supplier ledger details
/// Old: LedgerMaster_GetSupplier
/// </summary>
public class SupplierDto
{
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
}

/// <summary>
/// Ledger sub-group (under group) information
/// Old: LedgerMaster_UnderGroup
/// </summary>
public class LedgerUnderGroupDto
{
    public int LedgerSubGroupID { get; set; }
    public string LedgerSubGroupDisplayName { get; set; } = "";
}

/// <summary>
/// Ledger group with hierarchy information
/// Old: LedgerMaster_GroupList
/// </summary>
public class LedgerSubGroupDto
{
    public int LedgerSubGroupUniqueID { get; set; }
    public int LedgerSubGroupID { get; set; }
    public string LedgerSubGroupDisplayName { get; set; } = "";
    public int UnderSubGroupID { get; set; }
    public string LedgerSubGroupName { get; set; } = "";
    public int LedgerSubGroupLevel { get; set; }
    public string GroupName { get; set; } = "";
}

/// <summary>
/// Machine allocation details for an employee
/// Old: LedgerMaster_GetMachineAllocation
/// </summary>
public class MachineAllocationDto
{
    public int MachineAllocationID { get; set; }
    public int EmployeeID { get; set; }
    public int MachineID { get; set; }
    public string MachineName { get; set; } = "";
    public DateTime? AllocationDate { get; set; }
    public DateTime? DeallocationDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Group allocation details for supplier
/// Old: LedgerMaster_GetGroupAllocation
/// </summary>
public class GroupAllocationDto
{
    public int GroupAllocationID { get; set; }
    public int SupplierID { get; set; }
    public int ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = "";
    public bool IsActive { get; set; }
}

/// <summary>
/// Business vertical details
/// Old: LedgerMaster_GetBusinessVertical
/// </summary>
public class BusinessVerticalDto
{
    public int BusinessVerticalDetailID { get; set; }
    public string BusinessVerticalName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; }
}

/// <summary>
/// Embargo details for a ledger
/// Old: LedgerMaster_GetEmbargoDetails
/// </summary>
public class EmbargoDetailsDto
{
    public int EmbargoID { get; set; }
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
    public DateTime? EmbargoDate { get; set; }
    public string EmbargoReason { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? ReleaseDate { get; set; }
    public string ReleaseReason { get; set; } = "";
}

/// <summary>
/// Ledger basic information
/// Old: LedgerMaster_GetLedger, LedgerMaster_SearchLedger
/// </summary>
public class LedgerDto
{
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
    public int LedgerGroupID { get; set; }
    public string LedgerGroupName { get; set; } = "";
    public string LedgerRefCode { get; set; } = "";
    public bool IsActive { get; set; }
}

/// <summary>
/// Ledger detail information with all master fields
/// Old: LedgerMaster_GetLedgerDetail
/// </summary>
public class LedgerDetailDto
{
    public int LedgerID { get; set; }
    public string LedgerName { get; set; } = "";
    public int LedgerGroupID { get; set; }
    public string LedgerRefCode { get; set; } = "";
    public Dictionary<string, object> MasterFields { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> DetailFields { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Permission check result for ledger operations
/// Old: LedgerMaster_CheckPermission
/// </summary>
public class LedgerCheckPermissionResult
{
    public bool CanModify { get; set; }
    public string Message { get; set; } = "";
}

