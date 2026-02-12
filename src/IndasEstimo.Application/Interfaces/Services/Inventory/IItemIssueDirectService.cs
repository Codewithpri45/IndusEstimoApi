using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IItemIssueDirectService
{
    // ==================== CRUD Operations ====================

    /// <summary>
    /// Save new item issue direct - matches SaveIssueData WebMethod
    /// </summary>
    Task<Result<SaveItemIssueDirectResponse>> SaveItemIssueDirectAsync(SaveItemIssueDirectRequest request);

    /// <summary>
    /// Update existing item issue direct - matches UpdateIssue WebMethod
    /// </summary>
    Task<Result<SaveItemIssueDirectResponse>> UpdateItemIssueDirectAsync(UpdateItemIssueDirectRequest request);

    /// <summary>
    /// Delete item issue direct - matches DeleteIssue WebMethod
    /// </summary>
    Task<Result<string>> DeleteItemIssueDirectAsync(long transactionId, long? jobContID);

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get complete item issue data for editing - matches GetIssueVoucherDetails WebMethod
    /// </summary>
    Task<Result<List<ItemIssueDirectDataDto>>> GetItemIssueDirectAsync(long transactionId);

    /// <summary>
    /// Get item issue list/grid with filters - matches Showlist WebMethod
    /// </summary>
    Task<Result<List<ItemIssueDirectListDto>>> GetItemIssuesDirectListAsync(GetItemIssuesDirectListRequest request);

    /// <summary>
    /// Get header/main details of an issue - matches HeaderNAme WebMethod
    /// </summary>
    Task<Result<ItemIssueDirectDataDto>> GetItemIssueDirectHeaderAsync(long transactionId);

    // ==================== Picklist Operations ====================

    /// <summary>
    /// Get picklist items for "Job Allocated" type - matches JobAllocatedPicklist/JobAllocated WebMethods
    /// </summary>
    Task<Result<List<DirectPicklistDto>>> GetJobAllocatedPicklistAsync();

    /// <summary>
    /// Get picklist items for "All" type by stock type - matches AllPicklist/All WebMethods
    /// </summary>
    Task<Result<List<DirectPicklistDto>>> GetAllPicklistByStockTypeAsync(string stockType);

    // ==================== Stock Operations ====================

    /// <summary>
    /// Get stock batch-wise data for an item - matches GetStockBatchWise WebMethod
    /// </summary>
    Task<Result<List<StockBatchDirectDto>>> GetStockBatchWiseAsync(long itemId, long? jobBookingJobCardContentsID);

    // ==================== Lookup Operations ====================

    /// <summary>
    /// Get next issue number - matches GetIssueNO WebMethod
    /// </summary>
    Task<Result<string>> GetNextIssueNumberAsync(string prefix);

    /// <summary>
    /// Get next slip number
    /// </summary>
    Task<Result<string>> GetNextSlipNumberAsync();

    /// <summary>
    /// Get job card filter list - matches JobCardRender WebMethod
    /// </summary>
    Task<Result<List<JobCardDirectDto>>> GetJobCardFilterListAsync();

    /// <summary>
    /// Get departments list - matches DepartmentName WebMethod
    /// </summary>
    Task<Result<List<DepartmentDto>>> GetDepartmentsAsync();

    /// <summary>
    /// Get machines list by department
    /// </summary>
    Task<Result<List<MachineDto>>> GetMachinesByDepartmentAsync(long departmentId);

    /// <summary>
    /// Get process list by job card
    /// </summary>
    Task<Result<List<ProcessDto>>> GetProcessListJobWiseAsync(long jobCardContentsId);

    /// <summary>
    /// Get warehouses/floors list - matches GetWarehouseList WebMethod
    /// </summary>
    Task<Result<List<ItemIssueWarehouseDto>>> GetWarehousesAsync();

    /// <summary>
    /// Get bins list for a warehouse - matches GetBinsList WebMethod
    /// </summary>
    Task<Result<List<ItemIssueBinDto>>> GetBinsAsync(string warehouseName);

    /// <summary>
    /// Get last transaction date - matches GetLastTransactionDate WebMethod
    /// </summary>
    Task<Result<string>> GetLastTransactionDateAsync();

    /// <summary>
    /// Check user authority - matches CheckUserAuthority WebMethod
    /// </summary>
    Task<Result<bool>> CheckUserAuthorityAsync();
}
