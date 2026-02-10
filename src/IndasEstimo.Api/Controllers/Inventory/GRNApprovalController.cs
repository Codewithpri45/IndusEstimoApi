using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/grn-approval")]
[Authorize]
public class GRNApprovalController : ControllerBase
{
    private readonly IGRNApprovalService _service;
    private readonly ILogger<GRNApprovalController> _logger;

    public GRNApprovalController(
        IGRNApprovalService service,
        ILogger<GRNApprovalController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get GRN list for approval/unapproval
    /// RadioValue: "Pending Receipt Note" or "Approved Receipt Note"
    /// </summary>
    [HttpPost("grn-list")]
    [ProducesResponseType(typeof(List<GRNListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGRNList([FromBody] GetGRNListRequest request)
    {
        _logger.LogInformation("Getting GRN list with RadioValue: {RadioValue}", request.RadioValue);

        var result = await _service.GetGRNListAsync(request.RadioValue, request.FromDate, request.ToDate);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get batch detail for a specific GRN transaction
    /// Shows item-wise details with approval quantities
    /// </summary>
    [HttpPost("batch-detail")]
    [ProducesResponseType(typeof(List<GRNBatchDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBatchDetail([FromBody] GetGRNBatchDetailRequest request)
    {
        _logger.LogInformation(
            "Getting GRN batch detail for TransactionID: {TransactionID}, RadioValue: {RadioValue}",
            request.TransactionID, request.RadioValue);

        var result = await _service.GetGRNBatchDetailAsync(request.TransactionID, request.RadioValue);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Approve or unapprove GRN items
    /// Updates IsVoucherItemApproved, ApprovedQuantity, RejectedQuantity, QCApprovalNO, etc.
    /// </summary>
    [HttpPost("approve")]
    [Authorize(Roles = "Admin,Manager,PurchaseUser,InventoryUser,QCUser")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveGRN([FromBody] ApproveGRNRequest request)
    {
        _logger.LogInformation("Approving GRN {GRNTransactionID}", request.GRNTransactionID);

        var result = await _service.ApproveGRNAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { success = true, message = result.Data });
    }

    /// <summary>
    /// Check if GRN can be approved/unapproved
    /// Returns "Exist" if already processed, empty string if can be modified
    /// </summary>
    [HttpGet("check-permission/{transactionId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckPermission(long transactionId)
    {
        _logger.LogInformation("Checking permission for GRN {TransactionId}", transactionId);

        var result = await _service.CheckPermissionAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { canModify = string.IsNullOrEmpty(result.Data), message = result.Data });
    }
}
