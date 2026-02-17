using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/item-issue-direct")]
[Authorize]
public class ItemIssueDirectController : ControllerBase
{
    private readonly IItemIssueDirectService _service;
    private readonly ILogger<ItemIssueDirectController> _logger;

    public ItemIssueDirectController(
        IItemIssueDirectService service,
        ILogger<ItemIssueDirectController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Generate next Issue voucher number
    /// Uses VoucherID=-19 on ItemTransactionMain; prefix e.g. "IID"
    /// </summary>
    [HttpGet("next-issue-no")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIssueNo([FromQuery] string prefix = "IID")
    {
        _logger.LogInformation("Getting next issue number for prefix {Prefix}", prefix);

        var result = await _service.GetIssueNoAsync(prefix);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { issueNo = result.Data });
    }

    /// <summary>
    /// Get floor warehouse list for Item Issue Direct.
    /// Each row includes WarehouseID (MIN per warehouse name) so the caller has a valid
    /// FloorWarehouseID even when the warehouse has no bins configured.
    /// Workflow:
    ///   1. Load this list → user picks a warehouse name.
    ///   2. Call /bins?warehouseName=X → if bins returned, user picks a bin;
    ///      store the bin's WarehouseID as FloorWarehouseID in the issue detail.
    ///   3. If /bins returns empty, store this endpoint's WarehouseID as FloorWarehouseID.
    /// </summary>
    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(List<FloorWarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseList()
    {
        _logger.LogInformation("Getting floor warehouse list");

        var result = await _service.GetWarehouseListAsync();

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get bins for a floor warehouse.
    /// Each bin row carries its own WarehouseID (the WarehouseMaster row for that
    /// WarehouseName+BinName combination). Store this as FloorWarehouseID in the issue detail.
    /// Returns empty list when the warehouse has no bins — caller should use /warehouses WarehouseID instead.
    /// </summary>
    [HttpGet("bins")]
    [ProducesResponseType(typeof(List<FloorBinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBinsList([FromQuery] string warehouseName = "")
    {
        _logger.LogInformation("Getting floor bins list for warehouse {WarehouseName}", warehouseName);

        var result = await _service.GetBinsListAsync(warehouseName);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get job cards for selection (calls ItemIssueDirectJobCardRender stored procedure)
    /// Returns active/released job cards with allocation details
    /// </summary>
    [HttpGet("job-cards")]
    [ProducesResponseType(typeof(List<JobCardRenderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobCardRender()
    {
        _logger.LogInformation("Getting job card render data");

        var result = await _service.GetJobCardRenderAsync();

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get Job-Allocated picklist items (Picklist Type: Job Allocated)
    /// Returns pending picklist release items with stock info
    /// WHERE: VoucherID=-17 (Picklist), pending quantity > 0
    /// </summary>
    [HttpGet("job-allocated-picklist")]
    [ProducesResponseType(typeof(List<JobAllocatedPicklistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobAllocatedPicklist()
    {
        _logger.LogInformation("Getting job allocated picklist data");

        var result = await _service.GetJobAllocatedPicklistAsync();

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get All picklist items (Picklist Type: All)
    /// Returns all released items with batch stock info
    /// WHERE: VoucherID=-17, DepartmentID=-50, IsReleased=1
    /// </summary>
    [HttpGet("all-picklist")]
    [ProducesResponseType(typeof(List<AllPicklistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllPicklist()
    {
        _logger.LogInformation("Getting all picklist data");

        var result = await _service.GetAllPicklistAsync();

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get batch-wise stock for a specific item
    /// Used to populate the stock batch grid when an item is selected
    /// </summary>
    [HttpPost("stock-batch-wise")]
    [ProducesResponseType(typeof(List<StockBatchWiseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockBatchWise([FromBody] GetStockBatchWiseRequest request)
    {
        _logger.LogInformation("Getting stock batch wise for item {ItemId}", request.ItemId);

        var result = await _service.GetStockBatchWiseAsync(request.ItemId, request.JobBookingJobCardContentsID);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get issue transactions list by date range
    /// WHERE: VoucherID=-19, filtered by ProductionUnitID, date range
    /// </summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(List<IssueListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIssueList([FromBody] GetIssueListRequest request)
    {
        _logger.LogInformation("Getting issue list from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetIssueListAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get full item-level details for a specific Issue transaction (edit/view)
    /// WHERE: VoucherID=-19, TransactionID=specified
    /// </summary>
    [HttpGet("voucher-details/{transactionId}")]
    [ProducesResponseType(typeof(List<IssueVoucherDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIssueVoucherDetails(long transactionId)
    {
        _logger.LogInformation("Getting issue voucher details for transaction {TransactionId}", transactionId);

        var result = await _service.GetIssueVoucherDetailsAsync(transactionId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get header/summary data for a specific Issue transaction
    /// Used to populate the form header when an existing issue is loaded
    /// </summary>
    [HttpGet("header/{transactionId}")]
    [ProducesResponseType(typeof(List<IssueHeaderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHeaderName(long transactionId)
    {
        _logger.LogInformation("Getting issue header for transaction {TransactionId}", transactionId);

        var result = await _service.GetHeaderNameAsync(transactionId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new Item Issue Direct transaction
    /// Creates: ItemTransactionMain (VoucherID=-19) + ItemTransactionDetail
    ///          + ItemConsumptionMain (VoucherID=-53) + ItemConsumptionDetail
    /// Then calls UPDATE_ITEM_STOCK_VALUES_UNIT_WISE and AUTOCLOSE_PICKLIST_21052022
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveIssueData([FromBody] SaveIssueDataRequest request)
    {
        _logger.LogInformation("Saving Item Issue Direct with prefix {Prefix}", request.Prefix);

        var result = await _service.SaveIssueDataAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            success = true,
            message = "Issue saved successfully",
            issueNo = result.Data.VoucherNo,
            transactionId = result.Data.TransactionID
        });
    }

    /// <summary>
    /// Update existing Item Issue Direct transaction
    /// Updates: ItemTransactionMain, re-inserts ItemTransactionDetail,
    ///          updates ItemConsumptionMain, re-inserts ItemConsumptionDetail
    /// Then calls UPDATE_ITEM_STOCK_VALUES_UNIT_WISE and AUTOCLOSE_PICKLIST_21052022
    /// </summary>
    [HttpPut("update")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateIssue([FromBody] UpdateIssueDataRequest request)
    {
        _logger.LogInformation("Updating Item Issue Direct transaction {TransactionId}", request.TransactionID);

        var result = await _service.UpdateIssueAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { success = true, message = "Issue updated successfully" });
    }

    /// <summary>
    /// Delete (soft-delete) an Item Issue Direct transaction
    /// Marks IsDeletedTransaction=1 on ItemTransactionMain, ItemTransactionDetail,
    /// ItemConsumptionMain, ItemConsumptionDetail
    /// Then calls UPDATE_ITEM_STOCK_VALUES_UNIT_WISE and AUTOCLOSE_PICKLIST_21052022
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteIssue([FromBody] DeleteIssueRequest request)
    {
        _logger.LogInformation("Deleting Item Issue Direct transaction {TransactionId}", request.TransactionID);

        var result = await _service.DeleteIssueAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { success = true, message = "Issue deleted successfully" });
    }
}
