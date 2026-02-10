using IndasEstimo.Domain.Entities.Inventory;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Repositories.Inventory;


public interface IPurchaseRequisitionRepository
{
    /// <summary>
    /// Saves complete purchase requisition with details and indent updates in a transaction
    /// </summary>
    Task<long> SavePurchaseRequisitionAsync(
        PurchaseRequisition main,
        List<PurchaseRequisitionDetail> details,
        List<PurchaseRequisitionIndentUpdate> indentUpdates
    );

    /// <summary>
    /// Generates next PR number
    /// </summary>
    Task<(string VoucherNo, long MaxVoucherNo)> GenerateNextPRNumberAsync(string prefix);

    /// <summary>
    /// Updates stock values after PR save
    /// </summary>
    Task UpdateStockValuesAsync(long transactionId);

    /// <summary>
    /// Creates approval workflow entries
    /// </summary>
    Task<string> CreateApprovalWorkflowAsync(
        long transactionId,
        long transactionDetailId,
        long itemId,
        string itemDescription,
        string displayModuleName,
        long moduleId,
        string voucherNo,
        string itemName,
        decimal purchaseQty
    );

    /// <summary>
    /// Gets list of job card contents filtered by production units
    /// </summary>
    Task<List<JobCardDto>> GetJobCardListAsync(string productionUnitIds);

    /// <summary>
    /// Gets list of clients (Ledgers where LedgerGroupID = 1)
    /// </summary>
    Task<List<ClientListDto>> GetClientListAsync();

    /// <summary>
    /// Closes indents by blocking items in ItemMaster
    /// </summary>
    Task<bool> CloseIndentsAsync(List<long> itemIds);

    /// <summary>
    /// Closes multiple requisitions in ItemTransactionDetail
    /// </summary>
    Task<bool> CloseRequisitionsAsync(List<RequisitionItemDto> requisitions);

    /// <summary>
    /// Gets the date of the last successful transaction
    /// </summary>
    Task<DateTime?> GetLastTransactionDateAsync();

    /// <summary>
    /// Retrieves detailed requisition data by transaction ID
    /// </summary>
    Task<List<RequisitionDataDto>> GetRequisitionDataAsync(long transactionId);

    /// <summary>
    /// Gets item lookup list for overflow grid
    /// </summary>
    Task<List<ItemLookupDto>> GetItemLookupListAsync(long? itemGroupId, string productionUnitId);

    /// <summary>
    /// Checks if a requisition is used in a Purchase Order
    /// </summary>
    Task<bool> IsRequisitionUsedAsync(long transactionId);

    /// <summary>
    /// Checks if a requisition is already approved
    /// </summary>
    Task<bool> IsRequisitionApprovedAsync(long transactionId);



    
    /// <summary>
    /// Performs soft delete on purchase requisition and its details
    /// </summary>
    Task<bool> DeletePurchaseRequisitionAsync(long transactionId);

    /// <summary>
    /// Gets comments for PO or PR
    /// </summary>
    Task<List<CommentDataDto>> GetCommentDataAsync(string purchaseTransactionId, string requisitionIds);

    /// <summary>
    /// Fills grid data based on radio value (Indent List or Created Requisitions)
    /// </summary>
    Task<string> FillGridAsync(string radioValue, string filterString, string fromDateValue, string toDateValue);
}

