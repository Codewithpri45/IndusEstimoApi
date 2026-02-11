using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;

namespace IndasEstimo.Api.Controllers.ToolInventory;

[Authorize]
[ApiController]
[Route("api/tool-inventory/tool-issues")]
public class ToolIssuesController : ControllerBase
{
    private readonly IToolIssueService _service;
    private readonly ILogger<ToolIssuesController> _logger;

    public ToolIssuesController(
        IToolIssueService service,
        ILogger<ToolIssuesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    /// <summary>
    /// Save new tool issue
    /// VB: SaveToolIssue
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SaveToolIssue([FromBody] SaveToolIssueRequest request)
    {
        var result = await _service.SaveToolIssueAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete tool issue (soft delete)
    /// VB: DeleteToolIssue
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteToolIssue([FromBody] DeleteToolIssueRequest request)
    {
        var result = await _service.DeleteToolIssueAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    /// <summary>
    /// Get tool issue voucher details by transaction ID
    /// VB: GetIssueVoucherDetails
    /// </summary>
    [HttpGet("voucher-details/{transactionId}")]
    public async Task<IActionResult> GetIssueVoucherDetails(long transactionId)
    {
        var result = await _service.GetIssueVoucherDetailsAsync(transactionId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    /// <summary>
    /// Generate next issue voucher number
    /// VB: GetIssueNO
    /// </summary>
    [HttpGet("issue-no")]
    public async Task<IActionResult> GetIssueNo([FromQuery] string prefix = "TI")
    {
        var result = await _service.GetIssueNoAsync(prefix);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of warehouses
    /// VB: GetWarehouseList
    /// </summary>
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouseList()
    {
        var result = await _service.GetWarehouseListAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of bins/floor warehouses by warehouse name
    /// VB: GetBinsList
    /// </summary>
    [HttpGet("bins")]
    public async Task<IActionResult> GetBinsList([FromQuery] string warehouseName)
    {
        var result = await _service.GetBinsListAsync(warehouseName);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get batch-wise stock for a job card
    /// VB: GetStockBatchWise
    /// </summary>
    [HttpGet("stock-batch-wise/{jobBookingJobCardContentsId}")]
    public async Task<IActionResult> GetStockBatchWise(long jobBookingJobCardContentsId)
    {
        var result = await _service.GetStockBatchWiseAsync(jobBookingJobCardContentsId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of job cards
    /// VB: GetJobCardNo
    /// </summary>
    [HttpGet("job-cards")]
    public async Task<IActionResult> GetJobCardNo()
    {
        var result = await _service.GetJobCardNoAsync();

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}
