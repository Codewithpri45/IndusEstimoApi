using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.ToolInventory;
using IndasEstimo.Application.Interfaces.Services.ToolInventory;
namespace IndasEstimo.Api.Controllers.ToolInventory;

[ApiController]
[Route("api/tool-inventory/tool-requisitions")]
[Authorize]
public class ToolRequisitionsController : ControllerBase
{
    private readonly IToolRequisitionService _toolRequisitionService;
    private readonly ILogger<ToolRequisitionsController> _logger;
    public ToolRequisitionsController(
        IToolRequisitionService toolRequisitionService,
        ILogger<ToolRequisitionsController> logger)
    {
        _toolRequisitionService = toolRequisitionService;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveToolRequisitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveToolRequisition([FromBody] SaveToolRequisitionRequest request)
    {
        var result = await _toolRequisitionService.SaveToolRequisitionAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveToolRequisitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateToolRequisition([FromBody] UpdateToolRequisitionRequest request)
    {
        var result = await _toolRequisitionService.UpdateToolRequisitionAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteToolRequisition([FromBody] long transactionId)
    {
        var result = await _toolRequisitionService.DeleteToolRequisitionAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    [HttpGet("indent-list")]
    [ProducesResponseType(typeof(List<ToolIndentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIndentList()
    {
        var result = await _toolRequisitionService.GetIndentListAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("fill-grid")]
    [ProducesResponseType(typeof(List<ToolRequisitionListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolRequisitionList([FromBody] GetToolRequisitionListRequest request)
    {
        var result = await _toolRequisitionService.GetToolRequisitionListAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("retrive-data/{transactionId}")]
    [ProducesResponseType(typeof(List<ToolRequisitionDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolRequisitionData(long transactionId)
    {
        var result = await _toolRequisitionService.GetToolRequisitionDataAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    [HttpGet("voucher-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVoucherNo([FromQuery] string prefix = "TR")
    {
        var result = await _toolRequisitionService.GetNextVoucherNoAsync(prefix);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _toolRequisitionService.GetLastTransactionDateAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("tool-master-list")]
    [ProducesResponseType(typeof(List<ToolMasterItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToolMasterList()
    {
        var result = await _toolRequisitionService.GetToolMasterListAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("check-permission/{transactionId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckPermission(long transactionId)
    {
        var result = await _toolRequisitionService.CheckPermissionAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}
