using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/purchase-order-approval")]
[Authorize]
public class PurchaseOrderApprovalController : ControllerBase
{
    private readonly IPurchaseOrderApprovalService _service;
    private readonly ILogger<PurchaseOrderApprovalController> _logger;

    public PurchaseOrderApprovalController(
        IPurchaseOrderApprovalService service,
        ILogger<PurchaseOrderApprovalController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get unapproved purchase orders - matches UnApprovedPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=0, IsCancelled=0
    /// </summary>
    [HttpPost("unapproved")]
    [ProducesResponseType(typeof(List<UnapprovedPurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnapprovedPurchaseOrders([FromBody] GetPurchaseOrderApprovalListRequest request)
    {
        _logger.LogInformation("Getting unapproved purchase orders from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetUnapprovedPurchaseOrdersAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get approved purchase orders - matches ApprovedPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=1
    /// </summary>
    [HttpPost("approved")]
    [ProducesResponseType(typeof(List<ApprovedPurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApprovedPurchaseOrders([FromBody] GetPurchaseOrderApprovalListRequest request)
    {
        _logger.LogInformation("Getting approved purchase orders from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetApprovedPurchaseOrdersAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get cancelled purchase orders - matches CancelledPurchaseOrders WebMethod
    /// WHERE: VoucherID=-11, IsVoucherItemApproved=0, IsCancelled=1
    /// </summary>
    [HttpPost("cancelled")]
    [ProducesResponseType(typeof(List<CancelledPurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCancelledPurchaseOrders([FromBody] GetPurchaseOrderApprovalListRequest request)
    {
        _logger.LogInformation("Getting cancelled purchase orders from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetCancelledPurchaseOrdersAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if PO is processed (used in receipts) - matches IsPurchaseOrdersProcessed WebMethod
    /// Used to determine if a PO can be deleted
    /// </summary>
    [HttpGet("is-processed/{transactionId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IsPurchaseOrderProcessed(long transactionId)
    {
        _logger.LogInformation("Checking if purchase order {TransactionId} is processed", transactionId);

        var result = await _service.IsPurchaseOrderProcessedAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Approve purchase orders - matches UpdateData WebMethod with "Approve"
    /// Sets: IsVoucherItemApproved=1, VoucherItemApprovedBy=CurrentUser, VoucherItemApprovedDate=Now, IsCancelled=0
    /// </summary>
    [HttpPost("approve")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApprovePurchaseOrders([FromBody] ApprovePurchaseOrderRequest request)
    {
        _logger.LogInformation("Approving {Count} purchase orders", request.PurchaseOrderItems.Count);

        var result = await _service.ApprovePurchaseOrdersAsync(request.PurchaseOrderItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Purchase orders approved successfully" });
    }

    /// <summary>
    /// Unapprove purchase orders - reverse the approval
    /// Sets: IsVoucherItemApproved=0, VoucherItemApprovedBy=0, VoucherItemApprovedDate=NULL
    /// </summary>
    [HttpPost("unapprove")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnapprovePurchaseOrders([FromBody] ApprovePurchaseOrderRequest request)
    {
        _logger.LogInformation("Unapproving {Count} purchase orders", request.PurchaseOrderItems.Count);

        var result = await _service.UnapprovePurchaseOrdersAsync(request.PurchaseOrderItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Purchase orders unapproved successfully" });
    }

    /// <summary>
    /// Cancel purchase orders - matches UpdateData WebMethod with "Cancel"
    /// Sets: IsCancelled=1
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelPurchaseOrders([FromBody] CancelPurchaseOrderRequest request)
    {
        _logger.LogInformation("Cancelling {Count} purchase orders", request.PurchaseOrderItems.Count);

        var result = await _service.CancelPurchaseOrdersAsync(request.PurchaseOrderItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Purchase orders cancelled successfully" });
    }

    /// <summary>
    /// Uncancel purchase orders - reverse the cancellation
    /// Sets: IsCancelled=0
    /// </summary>
    [HttpPost("uncancel")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UncancelPurchaseOrders([FromBody] CancelPurchaseOrderRequest request)
    {
        _logger.LogInformation("Uncancelling {Count} purchase orders", request.PurchaseOrderItems.Count);

        var result = await _service.UncancelPurchaseOrdersAsync(request.PurchaseOrderItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Purchase orders uncancelled successfully" });
    }
}
