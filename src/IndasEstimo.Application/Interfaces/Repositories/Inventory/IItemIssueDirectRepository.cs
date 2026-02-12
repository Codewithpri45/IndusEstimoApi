using IndasEstimo.Domain.Entities.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;

public interface IItemIssueDirectRepository
{
    // ==================== CRUD Operations ====================

    /// <summary>
    /// Saves complete item issue with all related entities in a transaction
    /// Matches SaveIssueData WebMethod
    /// </summary>
    Task<long> SaveItemIssueDirectAsync(
        ItemIssueDirectMain main,
        List<ItemIssueDirectDetail> details,
        List<ItemIssueDirectConsumeMain>? consumeMain,
        List<ItemIssueDirectConsumeDetail>? consumeDetails
    );

    /// <summary>
    /// Updates an existing item issue
    /// Matches UpdateIssue WebMethod
    /// </summary>
    Task<long> UpdateItemIssueDirectAsync(
        long transactionId,
        ItemIssueDirectMain main,
        List<ItemIssueDirectDetail> details,
        List<ItemIssueDirectConsumeMain>? consumeMain,
        List<ItemIssueDirectConsumeDetail>? consumeDetails
    );

    /// <summary>
    /// Soft deletes an item issue
    /// Matches DeleteIssue WebMethod
    /// </summary>
    Task<bool> DeleteItemIssueDirectAsync(long transactionId, long? jobContID);

    // ==================== Voucher Number Generation ====================

    /// <summary>
    /// Generates next issue number
    /// Matches GetIssueNO WebMethod
    /// </summary>
    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextIssueNumberAsync(string prefix);

    /// <summary>
    /// Generates next slip number
    /// </summary>
    Task<(string SlipNo, long MaxSlipNo)> GenerateNextSlipNumberAsync();

    /// <summary>
    /// Gets the voucher number for a transaction
    /// </summary>
    Task<string?> GetVoucherNoAsync(long transactionId);

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get complete item issue data for editing
    /// Matches GetIssueVoucherDetails WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.ItemIssueDirectDataDto>> GetItemIssueDirectDataAsync(long transactionId);

    /// <summary>
    /// Get item issue list/grid with date filters
    /// Matches Showlist WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.ItemIssueDirectListDto>> GetItemIssuesDirectListAsync(
        string fromDate,
        string toDate,
        bool applyDateFilter
    );

    /// <summary>
    /// Get header/main details of an issue
    /// Matches HeaderNAme WebMethod
    /// </summary>
    Task<Application.DTOs.Inventory.ItemIssueDirectDataDto?> GetItemIssueDirectHeaderAsync(long transactionId);

    // ==================== Picklist Operations ====================

    /// <summary>
    /// Get picklist items for "Job Allocated" type
    /// Matches JobAllocatedPicklist/JobAllocated WebMethods
    /// </summary>
    Task<List<Application.DTOs.Inventory.DirectPicklistDto>> GetJobAllocatedPicklistAsync();

    /// <summary>
    /// Get picklist items for "All" type by stock type
    /// Matches AllPicklist/All WebMethods
    /// </summary>
    Task<List<Application.DTOs.Inventory.DirectPicklistDto>> GetAllPicklistByStockTypeAsync(string stockType);

    // ==================== Stock Operations ====================

    /// <summary>
    /// Get stock batch-wise data for an item
    /// Matches GetStockBatchWise WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.StockBatchDirectDto>> GetStockBatchWiseAsync(
        long itemId,
        long? jobBookingJobCardContentsID
    );

    // ==================== Lookup Operations ====================

    /// <summary>
    /// Get job card filter list
    /// Matches JobCardRender WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.JobCardDirectDto>> GetJobCardFilterListAsync();

    /// <summary>
    /// Get departments list
    /// Matches DepartmentName WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.DepartmentDto>> GetDepartmentsAsync();

    /// <summary>
    /// Get machines list by department
    /// </summary>
    Task<List<Application.DTOs.Inventory.MachineDto>> GetMachinesByDepartmentAsync(long departmentId);

    /// <summary>
    /// Get process list by job card
    /// </summary>
    Task<List<Application.DTOs.Inventory.ProcessDto>> GetProcessListJobWiseAsync(long jobCardContentsId);

    /// <summary>
    /// Get warehouses/floors list
    /// Matches GetWarehouseList WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.ItemIssueWarehouseDto>> GetWarehousesAsync();

    /// <summary>
    /// Get bins list for a warehouse
    /// Matches GetBinsList WebMethod
    /// </summary>
    Task<List<Application.DTOs.Inventory.ItemIssueBinDto>> GetBinsAsync(string warehouseName);

    // ==================== Utility Operations ====================

    /// <summary>
    /// Get last transaction date
    /// Matches GetLastTransactionDate WebMethod
    /// </summary>
    Task<string> GetLastTransactionDateAsync();

    /// <summary>
    /// Check user authority
    /// Matches CheckUserAuthority WebMethod
    /// </summary>
    Task<bool> CheckUserAuthorityAsync();

    /// <summary>
    /// Checks if an item issue is used in subsequent transactions
    /// </summary>
    Task<bool> IsItemIssueUsedAsync(long transactionId);

    /// <summary>
    /// Updates stock values after item issue save
    /// </summary>
    Task UpdateStockValuesAsync(long transactionId);
}
