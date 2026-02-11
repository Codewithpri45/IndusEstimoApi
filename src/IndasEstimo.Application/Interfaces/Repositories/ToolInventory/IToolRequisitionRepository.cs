using IndasEstimo.Domain.Entities.ToolInventory;
namespace IndasEstimo.Application.Interfaces.Repositories.ToolInventory;

public interface IToolRequisitionRepository
{
    // ==================== CRUD Operations ====================
    /// <summary>
    /// Saves tool requisition with main + detail records and updates indent linkage
    /// </summary>
    Task<long> SaveToolRequisitionAsync(
        ToolRequisition main,
        List<ToolRequisitionDetail> details
    );

    /// <summary>
    /// Updates indent detail records to link them to the requisition
    /// </summary>
    Task UpdateIndentLinkageAsync(long requisitionTransactionId, List<Application.DTOs.ToolInventory.ToolRequisitionIndentUpdateDto> indentUpdates);

    /// <summary>
    /// Updates an existing tool requisition (delete-reinsert details pattern)
    /// </summary>
    Task<long> UpdateToolRequisitionAsync(
        long transactionId,
        ToolRequisition main,
        List<ToolRequisitionDetail> details
    );

    /// <summary>
    /// Clears indent linkage before re-linking on update
    /// </summary>
    Task ClearIndentLinkageAsync(long requisitionTransactionId);

    /// <summary>
    /// Soft deletes a tool requisition
    /// </summary>
    Task<bool> DeleteToolRequisitionAsync(long transactionId);

    /// <summary>
    /// Checks if requisition items are approved (IsvoucherToolApproved != 0)
    /// </summary>
    Task<bool> IsToolRequisitionApprovedAsync(long transactionId);

    /// <summary>
    /// Generates next voucher number for tool requisition
    /// </summary>
    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextVoucherNoAsync(string prefix);

    /// <summary>
    /// Gets the voucher number for a transaction
    /// </summary>
    Task<string?> GetVoucherNoAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    /// <summary>
    /// Get indent list - tools from indents (VoucherID=-8) + unindented tools
    /// </summary>
    Task<List<Application.DTOs.ToolInventory.ToolIndentListDto>> GetIndentListAsync();

    /// <summary>
    /// Get created requisition list (VoucherID=-115) with date filters
    /// </summary>
    Task<List<Application.DTOs.ToolInventory.ToolRequisitionListDto>> GetToolRequisitionListAsync(
        string fromDate,
        string toDate
    );

    /// <summary>
    /// Get complete requisition data for editing
    /// </summary>
    Task<List<Application.DTOs.ToolInventory.ToolRequisitionDataDto>> GetToolRequisitionDataAsync(long transactionId);

    // ==================== Helper/Lookup Operations ====================
    /// <summary>
    /// Get last transaction date for VoucherID=-115
    /// </summary>
    Task<string> GetLastTransactionDateAsync();

    /// <summary>
    /// Get all active tools from ToolMaster
    /// </summary>
    Task<List<Application.DTOs.ToolInventory.ToolMasterItemDto>> GetToolMasterListAsync();

    /// <summary>
    /// Check if any detail items are approved
    /// </summary>
    Task<bool> CheckPermissionAsync(long transactionId);
}
