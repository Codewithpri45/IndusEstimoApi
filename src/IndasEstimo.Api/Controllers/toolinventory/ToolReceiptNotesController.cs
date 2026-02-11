using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;

namespace IndasEstimo.Api.Controllers.ToolInventory;

[Authorize]
[ApiController]
[Route("api/tool-inventory/tool-receipt-notes")]
public class ToolReceiptNotesController : ControllerBase
{
    private readonly IToolReceiptNoteService _service;
    private readonly ILogger<ToolReceiptNotesController> _logger;

    public ToolReceiptNotesController(
        IToolReceiptNoteService service,
        ILogger<ToolReceiptNotesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    /// <summary>
    /// Save new tool receipt note
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SaveToolReceiptNote([FromBody] SaveToolReceiptNoteRequest request)
    {
        var result = await _service.SaveToolReceiptNoteAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing tool receipt note
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateToolReceiptNote([FromBody] UpdateToolReceiptNoteRequest request)
    {
        var result = await _service.UpdateToolReceiptNoteAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete tool receipt note (soft delete)
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteToolReceiptNote([FromBody] long transactionId)
    {
        var result = await _service.DeleteToolReceiptNoteAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get receipt note batch detail for editing
    /// </summary>
    [HttpGet("retrive-data/{transactionId}")]
    public async Task<IActionResult> GetToolReceiptNoteData(long transactionId)
    {
        var result = await _service.GetToolReceiptNoteDataAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get receipt note list with date filter
    /// </summary>
    [HttpPost("fill-grid")]
    public async Task<IActionResult> GetToolReceiptNoteList([FromBody] GetToolReceiptNoteListRequest request)
    {
        var result = await _service.GetToolReceiptNoteListAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get pending purchase orders for receipt
    /// </summary>
    [HttpGet("pending-orders")]
    public async Task<IActionResult> GetPendingPurchaseOrders()
    {
        var result = await _service.GetPendingPurchaseOrdersAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    /// <summary>
    /// Generate next receipt note voucher number
    /// </summary>
    [HttpGet("voucher-no")]
    public async Task<IActionResult> GetNextVoucherNo([FromQuery] string prefix)
    {
        var result = await _service.GetNextVoucherNoAsync(prefix);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get last receipt note transaction date
    /// </summary>
    [HttpGet("last-transaction-date")]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _service.GetLastTransactionDateAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get receiver employee list
    /// </summary>
    [HttpGet("receivers")]
    public async Task<IActionResult> GetReceivers()
    {
        var result = await _service.GetReceiversAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get previous received quantity for a purchase order + tool
    /// </summary>
    [HttpGet("previous-received-quantity")]
    public async Task<IActionResult> GetPreviousReceivedQuantity(
        [FromQuery] long purchaseTransactionId,
        [FromQuery] long toolId,
        [FromQuery] long grnTransactionId)
    {
        var result = await _service.GetPreviousReceivedQuantityAsync(purchaseTransactionId, toolId, grnTransactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get warehouse list
    /// </summary>
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var result = await _service.GetWarehousesAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get bin list
    /// </summary>
    [HttpGet("bins")]
    public async Task<IActionResult> GetBins()
    {
        var result = await _service.GetBinsAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Check if receipt note can be edited/deleted
    /// </summary>
    [HttpGet("check-permission/{transactionId}")]
    public async Task<IActionResult> CheckPermission(long transactionId)
    {
        var result = await _service.CheckPermissionAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}
