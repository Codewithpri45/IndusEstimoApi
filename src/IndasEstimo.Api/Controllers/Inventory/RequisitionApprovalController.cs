using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/requisition-approval")]
[Authorize]
public class RequisitionApprovalController : ControllerBase
{
    private readonly IRequisitionApprovalService _service;
    private readonly ILogger<RequisitionApprovalController> _logger;

    public RequisitionApprovalController(
        IRequisitionApprovalService service,
        ILogger<RequisitionApprovalController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get unapproved requisitions - matches UnApprovedRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 0 AND IsCancelled = 0 AND IsAuditApproved = 1
    /// </summary>
    [HttpPost("unapproved")]
    [ProducesResponseType(typeof(List<UnapprovedRequisitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnapprovedRequisitions([FromBody] GetRequisitionListRequest request)
    {
        _logger.LogInformation("Getting unapproved requisitions from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetUnapprovedRequisitionsAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get approved requisitions - matches ApprovedRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 1
    /// </summary>
    [HttpPost("approved")]
    [ProducesResponseType(typeof(List<ApprovedRequisitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApprovedRequisitions([FromBody] GetRequisitionListRequest request)
    {
        _logger.LogInformation("Getting approved requisitions from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetApprovedRequisitionsAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get cancelled requisitions - matches CancelledRequisitions WebMethod
    /// WHERE: IsVoucherItemApproved = 0 AND IsCancelled = 1
    /// </summary>
    [HttpPost("cancelled")]
    [ProducesResponseType(typeof(List<CancelledRequisitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCancelledRequisitions([FromBody] GetRequisitionListRequest request)
    {
        _logger.LogInformation("Getting cancelled requisitions from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetCancelledRequisitionsAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Approve requisitions - matches UpdateData WebMethod with approval action
    /// Sets: IsVoucherItemApproved = 1, VoucherItemApprovedBy = CurrentUser, VoucherItemApprovedDate = Now
    /// </summary>
    [HttpPost("approve")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveRequisitions([FromBody] ApproveRequisitionRequest request)
    {
        _logger.LogInformation("Approving {Count} requisitions", request.RequisitionItems.Count);

        var result = await _service.ApproveRequisitionsAsync(request.RequisitionItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Requisitions approved successfully" });
    }

    /// <summary>
    /// Unapprove requisitions - reverse the approval
    /// Sets: IsVoucherItemApproved = 0, VoucherItemApprovedBy = 0, VoucherItemApprovedDate = NULL
    /// </summary>
    [HttpPost("unapprove")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnapproveRequisitions([FromBody] ApproveRequisitionRequest request)
    {
        _logger.LogInformation("Unapproving {Count} requisitions", request.RequisitionItems.Count);

        var result = await _service.UnapproveRequisitionsAsync(request.RequisitionItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Requisitions unapproved successfully" });
    }

    /// <summary>
    /// Cancel requisitions - matches UpdateData WebMethod with cancel action
    /// Sets: IsCancelled = 1
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelRequisitions([FromBody] CancelRequisitionRequest request)
    {
        _logger.LogInformation("Cancelling {Count} requisitions", request.RequisitionItems.Count);

        var result = await _service.CancelRequisitionsAsync(request.RequisitionItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Requisitions cancelled successfully" });
    }

    /// <summary>
    /// Uncancel requisitions - reverse the cancellation
    /// Sets: IsCancelled = 0
    /// </summary>
    [HttpPost("uncancel")]
    [Authorize(Roles = "Admin,Manager,Approver")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UncancelRequisitions([FromBody] CancelRequisitionRequest request)
    {
        _logger.LogInformation("Uncancelling {Count} requisitions", request.RequisitionItems.Count);

        var result = await _service.UncancelRequisitionsAsync(request.RequisitionItems);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = "Requisitions uncancelled successfully" });
    }
}
