using IndasEstimo.Application.DTOs.Masters;
using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.Interfaces.Repositories;

/// <summary>
/// Ledger Master repository - data access for dynamic ledger management
/// </summary>
public interface ILedgerMasterRepository
{
    // Core CRUD Operations
    Task<List<LedgerMasterListDto>> GetMasterListAsync();
    Task<object> GetMasterGridAsync(string masterID);
    Task<List<LedgerGridColumnHideDto>> GetGridColumnHideAsync(string masterID);
    Task<List<LedgerGridColumnDto>> GetGridColumnAsync(string masterID);
    Task<List<LedgerMasterFieldDto>> GetMasterFieldsAsync(string masterID);
    Task<object> GetLoadedDataAsync(string masterID, string ledgerID);
    Task<object> GetDrillDownAsync(string masterID, string tabID, string ledgerID);
    Task<string> SaveLedgerAsync(SaveLedgerRequest request);
    Task<string> UpdateLedgerAsync(UpdateLedgerRequest request);
    Task<string> DeleteLedgerAsync(string ledgerID, string ledgerGroupID);
    Task<string> CheckPermissionAsync(string ledgerID);
    Task<object> SelectBoxLoadAsync(JArray jsonData);

    // Ledger Operations
    Task<string> ConvertLedgerToConsigneeAsync(int ledgerID);
    Task<object> GetLedgersByGroupAsync(string groupID);

    // Concern Person Management
    Task<List<ConcernPersonDto>> GetConcernPersonsAsync();
    Task<string> SaveConcernPersonAsync(SaveConcernPersonRequest request);
    Task<string> DeleteAllConcernPersonsAsync(string ledgerID);
    Task<string> DeleteConcernPersonAsync(string concernPersonID, string ledgerID);

    // Employee Machine Allocation
    Task<List<OperatorDto>> GetOperatorsAsync();
    Task<List<EmployeeDto>> GetEmployeesAsync(string groupID);
    Task<string> SaveMachineAllocationAsync(SaveMachineAllocationRequest request);
    Task<string> GetMachineAllocationAsync(string employeeID);
    Task<string> DeleteMachineAllocationAsync(string ledgerID);

    // Supplier Group Allocation
    Task<List<LedgerItemGroupDto>> GetItemGroupsAsync();
    Task<List<SpareGroupDto>> GetSpareGroupsAsync();
    Task<string> GetGroupAllocationAsync(string supplierID);
    Task<string> GetSpareAllocationAsync(string supplierID);
    Task<string> SaveGroupAllocationAsync(SaveGroupAllocationRequest request);
    Task<string> DeleteGroupAllocationAsync(string ledgerID);

    // Business Vertical
    Task<object> GetBusinessVerticalSettingsAsync();
    Task<object> GetBusinessVerticalDetailsAsync(string ledgerID, string verticalID);
    Task<string> SaveBusinessVerticalAsync(SaveBusinessVerticalRequest request);
    Task<string> UpdateBusinessVerticalAsync(UpdateBusinessVerticalRequest request);
    Task<string> DeleteBusinessVerticalAsync(string detailID);

    // Embargo Management
    Task<string> PlaceEmbargoAsync(PlaceEmbargoRequest request);
    Task<object> GetEmbargoDetailsAsync(string ledgerID);
    Task<object> GetActiveEmbargosAsync();
    Task<string> SaveEmbargoDetailsAsync(SaveEmbargoDetailsRequest request);

    // Utilities
    Task<int> GetSessionTimeoutAsync();
    Task<string> GetLedgerGroupNameIDAsync(string masterID);
    Task<List<SupplierDto>> GetSuppliersAsync(string groupNameID);
}
