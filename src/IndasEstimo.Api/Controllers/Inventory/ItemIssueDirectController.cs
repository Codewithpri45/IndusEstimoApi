using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/item-issues")]
[Authorize]
public class ItemIssueDirectController : ControllerBase
{
    private readonly IItemIssueDirectService _itemIssueDirectService;
    private readonly ILogger<ItemIssueDirectController> _logger;

    public ItemIssueDirectController(
        IItemIssueDirectService itemIssueDirectService,
        ILogger<ItemIssueDirectController> logger)
    {
        _itemIssueDirectService = itemIssueDirectService;
        _logger = logger;
    }

    // ==================== CRUD Operations ====================

    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveItemIssueDirectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveItemIssueDirect([FromBody] SaveItemIssueDirectRequest request)
    {
        var result = await _itemIssueDirectService.SaveItemIssueDirectAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(SaveItemIssueDirectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateItemIssueDirect([FromBody] UpdateItemIssueDirectRequest request)
    {
        var result = await _itemIssueDirectService.UpdateItemIssueDirectAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteItemIssueDirect([FromBody] DeleteItemIssueDirectRequest request)
    {
        var result = await _itemIssueDirectService.DeleteItemIssueDirectAsync(request.TransactionId, request.JobContID);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Retrieve Operations ====================

    [HttpGet("retrive-data/{transactionId}")]
    [ProducesResponseType(typeof(List<ItemIssueDirectDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetItemIssueDirect(long transactionId)
    {
        var result = await _itemIssueDirectService.GetItemIssueDirectAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("fill-grid")]
    [ProducesResponseType(typeof(List<ItemIssueDirectListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetItemIssuesDirectList([FromBody] GetItemIssuesDirectListRequest request)
    {
        var result = await _itemIssueDirectService.GetItemIssuesDirectListAsync(request);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("header/{transactionId}")]
    [ProducesResponseType(typeof(ItemIssueDirectDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetItemIssueDirectHeader(long transactionId)
    {
        var result = await _itemIssueDirectService.GetItemIssueDirectHeaderAsync(transactionId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Picklist Operations ====================

    [HttpGet("picklist/job-allocated")]
    [ProducesResponseType(typeof(List<DirectPicklistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobAllocatedPicklist()
    {
        var result = await _itemIssueDirectService.GetJobAllocatedPicklistAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("picklist/all")]
    [ProducesResponseType(typeof(List<DirectPicklistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllPicklistByStockType([FromQuery] string stockType = "Job Consumables")
    {
        var result = await _itemIssueDirectService.GetAllPicklistByStockTypeAsync(stockType);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Stock Operations ====================

    [HttpGet("stock-batch/{itemId}")]
    [ProducesResponseType(typeof(List<StockBatchDirectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockBatchWise(long itemId, [FromQuery] long? jobBookingJobCardContentsID = null)
    {
        var result = await _itemIssueDirectService.GetStockBatchWiseAsync(itemId, jobBookingJobCardContentsID);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    // ==================== Helper/Lookup Operations ====================

    [HttpGet("voucher-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNextIssueNumber([FromQuery] string prefix = "ISS")
    {
        var result = await _itemIssueDirectService.GetNextIssueNumberAsync(prefix);
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("slip-no")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNextSlipNumber()
    {
        var result = await _itemIssueDirectService.GetNextSlipNumberAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("job-cards")]
    [ProducesResponseType(typeof(List<JobCardDirectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobCardFilterList()
    {
        var result = await _itemIssueDirectService.GetJobCardFilterListAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("departments")]
    [ProducesResponseType(typeof(List<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDepartments()
    {
        var result = await _itemIssueDirectService.GetDepartmentsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("machines/{departmentId}")]
    [ProducesResponseType(typeof(List<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMachinesByDepartment(long departmentId)
    {
        var result = await _itemIssueDirectService.GetMachinesByDepartmentAsync(departmentId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("processes/{jobCardContentsId}")]
    [ProducesResponseType(typeof(List<ProcessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProcessListJobWise(long jobCardContentsId)
    {
        var result = await _itemIssueDirectService.GetProcessListJobWiseAsync(jobCardContentsId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(List<ItemIssueWarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouses()
    {
        var result = await _itemIssueDirectService.GetWarehousesAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("bins")]
    [ProducesResponseType(typeof(List<ItemIssueBinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBins([FromQuery] string warehouseName)
    {
        var result = await _itemIssueDirectService.GetBinsAsync(warehouseName);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("last-transaction-date")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastTransactionDate()
    {
        var result = await _itemIssueDirectService.GetLastTransactionDateAsync();
        if (!result.IsSuccess) return Ok("fail");
        return Ok(result.Data);
    }

    [HttpGet("check-authority")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckUserAuthority()
    {
        var result = await _itemIssueDirectService.CheckUserAuthorityAsync();
        if (!result.IsSuccess) return Ok(false);
        return Ok(result.Data);
    }
}

/// <summary>
/// Request DTO for delete operation
/// </summary>
public class DeleteItemIssueDirectRequest
{
    public long TransactionId { get; set; }
    public long? JobContID { get; set; }
}
