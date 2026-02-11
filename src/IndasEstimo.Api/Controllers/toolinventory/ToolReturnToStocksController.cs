using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;

namespace IndasEstimo.Api.Controllers.ToolInventory;

[Authorize]
[ApiController]
[Route("api/tool-inventory/tool-return-to-stocks")]
public class ToolReturnToStocksController : ControllerBase
{
    private readonly IToolReturnToStockService _service;
    private readonly ILogger<ToolReturnToStocksController> _logger;

    public ToolReturnToStocksController(
        IToolReturnToStockService service,
        ILogger<ToolReturnToStocksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    /// <summary>
    /// Save new tool return to stock transaction
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SaveToolReturnToStock([FromBody] SaveToolReturnToStockRequest request)
    {
        var result = await _service.SaveToolReturnToStockAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete tool return to stock transaction (soft delete)
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteToolReturnToStock([FromBody] long transactionId)
    {
        var result = await _service.DeleteToolReturnToStockAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get available tools for return (issued tools with available stock)
    /// </summary>
    [HttpGet("available-for-return")]
    public async Task<IActionResult> GetAvailableForReturn()
    {
        var result = await _service.GetAvailableForReturnAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tool return to stock list
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetReturnToStockList()
    {
        var result = await _service.GetReturnToStockListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    /// <summary>
    /// Generate next return to stock voucher number
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
}
