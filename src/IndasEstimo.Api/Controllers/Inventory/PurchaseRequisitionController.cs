using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.Interfaces.Services.Inventory;
using IndasEstimo.Application.DTOs.Inventory;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/purchase-requisition")]
[Authorize]
public class PurchaseRequisitionController : ControllerBase
{
    private readonly IPurchaseRequisitionService _service;

    public PurchaseRequisitionController(IPurchaseRequisitionService service)
    {
        _service = service;
    }

    [HttpPost("saveREQ")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SavePurchaseRequisition([FromBody] SavePurchaseRequisitionRequest request)
    {
        var result = await _service.SavePurchaseRequisitionAsync(request);
        
        if (!result.IsSuccess)
        {
            return Ok(new { response = result.ErrorMessage ?? "fail" });
        }
        
        return Ok(new 
        { 
            response = result.Data.Message, 
            TransactionID = result.Data.TransactionID.ToString(),
            VoucherNo = result.Data.VoucherNo
        });
    }





    [HttpGet("job-card-list")]
    [ProducesResponseType(typeof(List<JobCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobCardList()
    {
        var result = await _service.GetJobCardListAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("client-list")]
    [ProducesResponseType(typeof(List<ClientListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetClientList()
    {
        var result = await _service.GetClientListAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("close-indent")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CloseIndent([FromBody] CloseIndentRequest request)
    {
        var result = await _service.CloseIndentsAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("close-requisitions")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CloseRequisitions([FromBody] CloseRequisitionRequest request)
    {
        var result = await _service.CloseRequisitionsAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("voucher-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoucherNo([FromQuery] string prefix = "PREQ")
    {
        var result = await _service.GetNextVoucherNoAsync(prefix);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _service.GetLastTransactionDateAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("retrive-data/{transactionId}")]
    [ProducesResponseType(typeof(List<RequisitionDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RetriveRequisitionData(long transactionId)
    {
        var result = await _service.GetRequisitionDataAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("item-lookup")]
    [ProducesResponseType(typeof(List<ItemLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetItemLookup([FromQuery] long? itemGroupId)
    {
        var result = await _service.GetItemLookupListAsync(itemGroupId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpDelete("deleteREQ/{transactionId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePurchaseRequisition(long transactionId)
    {
        var result = await _service.DeletePurchaseRequisitionAsync(transactionId);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpPost("get-comment-data")]
    [ProducesResponseType(typeof(List<CommentDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCommentData([FromBody] GetCommentDataRequest request)
    {
        var result = await _service.GetCommentDataAsync(request.PurchaseTransactionID, request.RequisitionIDs);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("fill-grid")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FillGrid([FromBody] FillGridRequest request)
    {
        var result = await _service.FillGridAsync(
            request.RadioValue, 
            request.FilterString, 
            request.FromDateValue, 
            request.ToDateValue);
        
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}