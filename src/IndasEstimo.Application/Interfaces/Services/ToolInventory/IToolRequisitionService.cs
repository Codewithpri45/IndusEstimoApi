using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.ToolInventory;
namespace IndasEstimo.Application.Interfaces.Services.ToolInventory;

public interface IToolRequisitionService
{
    // ==================== CRUD Operations ====================
    Task<Result<SaveToolRequisitionResponse>> SaveToolRequisitionAsync(SaveToolRequisitionRequest request);
    Task<Result<SaveToolRequisitionResponse>> UpdateToolRequisitionAsync(UpdateToolRequisitionRequest request);
    Task<Result<string>> DeleteToolRequisitionAsync(long transactionId);

    // ==================== Retrieve Operations ====================
    /// <summary>
    /// Get indent list (tools available for requisition) - matches FillGrid "Indent List" mode
    /// </summary>
    Task<Result<List<ToolIndentListDto>>> GetIndentListAsync();

    /// <summary>
    /// Get created requisition list with filters - matches FillGrid requisition mode
    /// </summary>
    Task<Result<List<ToolRequisitionListDto>>> GetToolRequisitionListAsync(GetToolRequisitionListRequest request);

    /// <summary>
    /// Get complete requisition data for editing - matches RetriveRequisitionData WebMethod
    /// </summary>
    Task<Result<List<ToolRequisitionDataDto>>> GetToolRequisitionDataAsync(long transactionId);

    // ==================== Helper/Lookup Operations ====================
    /// <summary>
    /// Get next voucher number - matches GetVoucherID WebMethod
    /// </summary>
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);

    /// <summary>
    /// Get last transaction date - matches GetLastTransactionDate WebMethod
    /// </summary>
    Task<Result<string>> GetLastTransactionDateAsync();

    /// <summary>
    /// Get all active tools for selection - matches GetOverFlowGrid WebMethod
    /// </summary>
    Task<Result<List<ToolMasterItemDto>>> GetToolMasterListAsync();

    /// <summary>
    /// Check if requisition items are approved - matches CheckPermission WebMethod
    /// </summary>
    Task<Result<string>> CheckPermissionAsync(long transactionId);
}
