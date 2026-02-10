using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Application.Interfaces.Services.Inventory;

public interface IPurchaseRequisitionService
{
    Task<Result<SavePurchaseRequisitionResponse>> SavePurchaseRequisitionAsync(SavePurchaseRequisitionRequest request);
    Task<Result<List<JobCardDto>>> GetJobCardListAsync();
    Task<Result<List<ClientListDto>>> GetClientListAsync();
    Task<Result<string>> CloseIndentsAsync(CloseIndentRequest request);
    Task<Result<string>> CloseRequisitionsAsync(CloseRequisitionRequest request);
    Task<Result<string>> GetNextVoucherNoAsync(string prefix);
    Task<Result<string>> GetLastTransactionDateAsync();
    Task<Result<List<RequisitionDataDto>>> GetRequisitionDataAsync(long transactionId);
    Task<Result<List<ItemLookupDto>>> GetItemLookupListAsync(long? itemGroupId);
    Task<Result<string>> DeletePurchaseRequisitionAsync(long transactionId);
    Task<Result<List<CommentDataDto>>> GetCommentDataAsync(string purchaseTransactionId, string requisitionIds);
    Task<Result<string>> FillGridAsync(string radioValue, string filterString, string fromDateValue, string toDateValue);
}