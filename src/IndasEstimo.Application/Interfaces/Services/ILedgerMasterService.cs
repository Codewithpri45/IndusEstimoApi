using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Masters;
using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.Interfaces.Services;

/// <summary>
/// Ledger Master service - handles dynamic ledger management across ledger groups
/// Preserves all functionality from legacy ASP.NET Web API 2 controller
/// </summary>
public interface ILedgerMasterService
{
    // ==================== Core CRUD Operations ====================

    /// <summary>
    /// Get list of ledger groups (masters) user has permission to view
    /// Old: LedgerMaster_MasterList
    /// </summary>
    Task<Result<List<LedgerMasterListDto>>> GetMasterListAsync();

    /// <summary>
    /// Get grid data for specific master using dynamic query from LedgerGroupMaster.SelectQuery
    /// Returns dynamic data structure based on database query
    /// Old: LedgerMaster_MasterGrid
    /// </summary>
    Task<Result<object>> GetMasterGridAsync(string masterID);

    /// <summary>
    /// Get grid column hide configuration for specific master
    /// Includes TabName, ConcernPerson, EmployeeMachineAllocation
    /// Old: LedgerMaster_GridColumnHide
    /// </summary>
    Task<Result<List<LedgerGridColumnHideDto>>> GetGridColumnHideAsync(string masterID);

    /// <summary>
    /// Get grid column names for specific master
    /// Old: LedgerMaster_GridColumn
    /// </summary>
    Task<Result<List<LedgerGridColumnDto>>> GetGridColumnAsync(string masterID);

    /// <summary>
    /// Get field metadata for specific ledger group
    /// Returns field definitions from LedgerGroupFieldMaster
    /// Old: LedgerMaster_MasterField
    /// </summary>
    Task<Result<List<LedgerMasterFieldDto>>> GetMasterFieldsAsync(string masterID);

    /// <summary>
    /// Get loaded data for editing ledger
    /// Executes stored procedure to retrieve master and detail data
    /// Old: LedgerMaster_LoadedData
    /// </summary>
    Task<Result<object>> GetLoadedDataAsync(string masterID, string ledgerID);

    /// <summary>
    /// Get drill-down data for specific tab
    /// Uses dynamic query from DrillDown table
    /// Old: LedgerMaster_DrillDown
    /// </summary>
    Task<Result<object>> GetDrillDownAsync(string masterID, string tabID, string ledgerID);

    /// <summary>
    /// Save new ledger with dynamic field data
    /// Uses Dictionary structure to handle dynamic fields from LedgerGroupFieldMaster
    /// Old: LedgerMaster_SaveLedger
    /// </summary>
    Task<Result<string>> SaveLedgerAsync(SaveLedgerRequest request);

    /// <summary>
    /// Update existing ledger with dynamic field data
    /// Old: LedgerMaster_UpdateLedger
    /// </summary>
    Task<Result<string>> UpdateLedgerAsync(UpdateLedgerRequest request);

    /// <summary>
    /// Delete ledger (soft delete - sets IsDeletedTransaction = 1)
    /// Old: LedgerMaster_DeleteLedger
    /// </summary>
    Task<Result<string>> DeleteLedgerAsync(string ledgerID, string ledgerGroupID);

    /// <summary>
    /// Check if ledger can be deleted (not referenced in transactions)
    /// Returns "Exist" if ledger is in use, empty string if can be deleted
    /// Old: LedgerMaster_CheckPermission
    /// </summary>
    Task<Result<string>> CheckPermissionAsync(string ledgerID);

    /// <summary>
    /// Load dynamic dropdown data for multiple fields
    /// Executes queries from LedgerGroupFieldMaster.SelectboxQueryDB
    /// Returns dataset with table names like "tbl_{FieldName}"
    /// Old: LedgerMaster_SelectBoxLoad
    /// </summary>
    Task<Result<object>> SelectBoxLoadAsync(JArray jsonData);

    // ==================== Ledger Operations ====================

    /// <summary>
    /// Convert ledger to consignee type
    /// Old: LedgerMaster_ConvertLedgerToConsignee
    /// </summary>
    Task<Result<string>> ConvertLedgerToConsigneeAsync(int ledgerID);

    /// <summary>
    /// Get ledgers by group ID
    /// Old: LedgerMaster_GetLedgersByGroup
    /// </summary>
    Task<Result<object>> GetLedgersByGroupAsync(string groupID);

    // ==================== Concern Person Management ====================

    /// <summary>
    /// Get all concern persons for current company
    /// Old: LedgerMaster_ConcernPerson
    /// </summary>
    Task<Result<List<ConcernPersonDto>>> GetConcernPersonsAsync();

    /// <summary>
    /// Save concern person details for a ledger
    /// Supports both insert and update operations
    /// Old: LedgerMaster_SaveConcernPerson
    /// </summary>
    Task<Result<string>> SaveConcernPersonAsync(SaveConcernPersonRequest request);

    /// <summary>
    /// Delete all concern persons for a ledger
    /// Old: LedgerMaster_DeleteAllConcernPerson
    /// </summary>
    Task<Result<string>> DeleteAllConcernPersonsAsync(string ledgerID);

    /// <summary>
    /// Delete specific concern person
    /// Old: LedgerMaster_DeleteConcernPerson
    /// </summary>
    Task<Result<string>> DeleteConcernPersonAsync(string concernPersonID, string ledgerID);

    // ==================== Employee Machine Allocation ====================

    /// <summary>
    /// Get all operators (from Operator ledger group)
    /// Old: LedgerMaster_GetOperator
    /// </summary>
    Task<Result<List<OperatorDto>>> GetOperatorsAsync();

    /// <summary>
    /// Get employees by group ID (from Employee ledger group)
    /// Old: LedgerMaster_GetEmployee
    /// </summary>
    Task<Result<List<EmployeeDto>>> GetEmployeesAsync(string groupID);

    /// <summary>
    /// Save machine allocation for employee
    /// Old: LedgerMaster_SaveMachineAllocation
    /// </summary>
    Task<Result<string>> SaveMachineAllocationAsync(SaveMachineAllocationRequest request);

    /// <summary>
    /// Get machine allocation details for employee
    /// Old: LedgerMaster_GetMachineAllocation
    /// </summary>
    Task<Result<string>> GetMachineAllocationAsync(string employeeID);

    /// <summary>
    /// Delete machine allocation for employee
    /// Old: LedgerMaster_DeleteMachineAllocation
    /// </summary>
    Task<Result<string>> DeleteMachineAllocationAsync(string ledgerID);

    // ==================== Supplier Group Allocation ====================

    /// <summary>
    /// Get all item groups for allocation
    /// Old: LedgerMaster_GetItemGroup
    /// </summary>
    Task<Result<List<LedgerItemGroupDto>>> GetItemGroupsAsync();

    /// <summary>
    /// Get all spare part groups for allocation
    /// Old: LedgerMaster_GetSpareGroup
    /// </summary>
    Task<Result<List<SpareGroupDto>>> GetSpareGroupsAsync();

    /// <summary>
    /// Get group allocation details for supplier
    /// Old: LedgerMaster_GetGroupAllocation
    /// </summary>
    Task<Result<string>> GetGroupAllocationAsync(string supplierID);

    /// <summary>
    /// Get spare part allocation details for supplier
    /// Old: LedgerMaster_GetSpareAllocation
    /// </summary>
    Task<Result<string>> GetSpareAllocationAsync(string supplierID);

    /// <summary>
    /// Save group allocation for supplier
    /// Includes both item groups and spare part groups
    /// Old: LedgerMaster_SaveGroupAllocation
    /// </summary>
    Task<Result<string>> SaveGroupAllocationAsync(SaveGroupAllocationRequest request);

    /// <summary>
    /// Delete group allocation for supplier
    /// Old: LedgerMaster_DeleteGroupAllocation
    /// </summary>
    Task<Result<string>> DeleteGroupAllocationAsync(string ledgerID);

    // ==================== Business Vertical ====================

    /// <summary>
    /// Get business vertical configuration settings
    /// Old: LedgerMaster_GetBusinessVerticalSettings
    /// </summary>
    Task<Result<object>> GetBusinessVerticalSettingsAsync();

    /// <summary>
    /// Get business vertical details for ledger
    /// Old: LedgerMaster_GetBusinessVerticalDetails
    /// </summary>
    Task<Result<object>> GetBusinessVerticalDetailsAsync(string ledgerID, string verticalID);

    /// <summary>
    /// Save new business vertical
    /// Old: LedgerMaster_SaveBusinessVertical
    /// </summary>
    Task<Result<string>> SaveBusinessVerticalAsync(SaveBusinessVerticalRequest request);

    /// <summary>
    /// Update existing business vertical
    /// Old: LedgerMaster_UpdateBusinessVertical
    /// </summary>
    Task<Result<string>> UpdateBusinessVerticalAsync(UpdateBusinessVerticalRequest request);

    /// <summary>
    /// Delete business vertical
    /// Old: LedgerMaster_DeleteBusinessVertical
    /// </summary>
    Task<Result<string>> DeleteBusinessVerticalAsync(string detailID);

    // ==================== Embargo Management ====================

    /// <summary>
    /// Place embargo on ledger(s)
    /// Old: LedgerMaster_PlaceEmbargo
    /// </summary>
    Task<Result<string>> PlaceEmbargoAsync(PlaceEmbargoRequest request);

    /// <summary>
    /// Get embargo details for specific ledger
    /// Old: LedgerMaster_GetEmbargoDetails
    /// </summary>
    Task<Result<object>> GetEmbargoDetailsAsync(string ledgerID);

    /// <summary>
    /// Get all active embargos for current company
    /// Old: LedgerMaster_GetActiveEmbargos
    /// </summary>
    Task<Result<object>> GetActiveEmbargosAsync();

    /// <summary>
    /// Save embargo details (update embargo status/information)
    /// Old: LedgerMaster_SaveEmbargoDetails
    /// </summary>
    Task<Result<string>> SaveEmbargoDetailsAsync(SaveEmbargoDetailsRequest request);

    // ==================== Utilities ====================

    /// <summary>
    /// Get session timeout value from settings
    /// Old: LedgerMaster_GetSessionTimeout
    /// </summary>
    Task<Result<int>> GetSessionTimeoutAsync();

    /// <summary>
    /// Get ledger group name ID for specific master
    /// Returns LedgerGroupNameID from LedgerGroupMaster
    /// Old: LedgerMaster_GetLedgerGroupNameID
    /// </summary>
    Task<Result<string>> GetLedgerGroupNameIDAsync(string masterID);

    /// <summary>
    /// Get suppliers filtered by group name ID
    /// Old: LedgerMaster_GetSupplier
    /// </summary>
    Task<Result<List<SupplierDto>>> GetSuppliersAsync(string groupNameID);
}
