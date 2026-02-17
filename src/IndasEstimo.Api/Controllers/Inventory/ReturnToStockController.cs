using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/return-to-stock")]
[Authorize]
public class ReturnToStockController : ControllerBase
{
    private readonly IReturnToStockService _service;
    private readonly ILogger<ReturnToStockController> _logger;

    public ReturnToStockController(
        IReturnToStockService service,
        ILogger<ReturnToStockController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Generate next Return To Stock voucher number
    /// Uses VoucherID=-25 on ItemTransactionMain; prefix e.g. "RTS"
    /// </summary>
    [HttpGet("next-return-no")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReturnNo([FromQuery] string prefix = "RTS")
    {
        _logger.LogInformation("Getting next return number for prefix {Prefix}", prefix);

        var result = await _service.GetReturnNoAsync(prefix);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { returnNo = result.Data });
    }

    /// <summary>
    /// Get warehouse list for Return To Stock (non-floor warehouses only)
    /// Returns warehouses where IsFloorWarehouse = 0
    /// </summary>
    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(List<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseList()
    {
        _logger.LogInformation("Getting warehouse list");

        var result = await _service.GetWarehouseListAsync();

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get bins for a warehouse
    /// Returns bins (BinName + WarehouseID) for the specified warehouse
    /// </summary>
    [HttpGet("bins")]
    [ProducesResponseType(typeof(List<BinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBinsList([FromQuery] string warehouseName = "")
    {
        _logger.LogInformation("Getting bins list for warehouse {WarehouseName}", warehouseName);

        var result = await _service.GetBinsListAsync(warehouseName);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get destination bins for a warehouse
    /// Same as /bins endpoint, used for destination bin selection
    /// </summary>
    [HttpGet("destination-bins")]
    [ProducesResponseType(typeof(List<BinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDestinationBinsList([FromQuery] string warehouseName = "")
    {
        _logger.LogInformation("Getting destination bins list for warehouse {WarehouseName}", warehouseName);

        var result = await _service.GetDestinationBinsListAsync(warehouseName);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get machines by department
    /// Returns active machines for the specified department
    /// </summary>
    [HttpGet("machines")]
    [ProducesResponseType(typeof(List<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMachinesByDepartment([FromQuery] int departmentId)
    {
        _logger.LogInformation("Getting machines for department {DepartmentId}", departmentId);

        var result = await _service.GetMachinesByDepartmentAsync(departmentId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get return transactions list by date range
    /// WHERE: VoucherID=-25, filtered by ProductionUnitID, date range
    /// Returns return records with related issue, job card, and batch information
    /// </summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(List<ReturnListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReturnList([FromBody] GetReturnListRequest request)
    {
        _logger.LogInformation("Getting return list from {FromDate} to {ToDate}",
            request.FromDate, request.ToDate);

        var result = await _service.GetReturnListAsync(request.FromDate, request.ToDate);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get full details for a specific Return To Stock transaction (for edit/view)
    /// WHERE: VoucherID=-25, TransactionID=specified
    /// Returns detail records with issue info, job card, batch, warehouse, and GRN data
    /// </summary>
    [HttpGet("details/{transactionId}")]
    [ProducesResponseType(typeof(List<ReturnDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReturnDetails(long transactionId)
    {
        _logger.LogInformation("Getting return details for transaction {TransactionId}", transactionId);

        var result = await _service.GetReturnDetailsAsync(transactionId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get available floor stock items — items issued (VoucherID=-19) with remaining FloorStock > 0
    /// FloorStock = IssueQuantity - SUM(ConsumeQuantity + ReturnQuantity)
    /// issueType: "AllIssueVouchers" | "JobIssueVouchers" | "NonJobIssueVouchers"
    /// </summary>
    [HttpGet("available-floor-stock")]
    [ProducesResponseType(typeof(List<FloorStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableFloorStock([FromQuery] string issueType = "AllIssueVouchers")
    {
        _logger.LogInformation("Getting available floor stock with issueType {IssueType}", issueType);

        var result = await _service.GetAvailableFloorStockAsync(issueType);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new Return To Stock transaction
    /// Creates records in:
    ///   - ItemTransactionMain (VoucherID=-25, prefix="RTS")
    ///   - ItemTransactionDetail (return detail lines)
    ///   - ItemConsumptionMain (VoucherID=-53, reverse consumption)
    ///   - ItemConsumptionDetail (consumption reversal detail)
    /// Executes UPDATE_ITEM_STOCK_VALUES_UNIT_WISE stored procedure after save
    /// </summary>
    [HttpPost("save")]
    [ProducesResponseType(typeof(ReturnOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveReturn([FromBody] SaveReturnDataRequest request)
    {
        _logger.LogInformation("Saving new return to stock with prefix {Prefix}", request.Prefix);

        var result = await _service.SaveReturnDataAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing Return To Stock transaction
    /// Updates:
    ///   - ItemTransactionMain header
    ///   - Deletes + re-inserts ItemTransactionDetail records
    ///   - Updates ItemConsumptionMain
    ///   - Deletes + re-inserts ItemConsumptionDetail records
    /// Executes UPDATE_ITEM_STOCK_VALUES_UNIT_WISE stored procedure after update
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateReturn([FromBody] UpdateReturnDataRequest request)
    {
        _logger.LogInformation("Updating return to stock for transaction {TransactionId}", request.TransactionID);

        var result = await _service.UpdateReturnAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { success = true, message = "Return updated successfully" });
    }

    /// <summary>
    /// Delete (soft) Return To Stock transaction
    /// Validates no subsequent transactions exist for the returned items/batches
    /// Soft deletes:
    ///   - ItemTransactionMain (IsDeletedTransaction=1)
    ///   - ItemTransactionDetail (IsDeletedTransaction=1)
    ///   - ItemConsumptionMain (IsDeletedTransaction=1)
    ///   - ItemConsumptionDetail (IsDeletedTransaction=1)
    /// Executes UPDATE_ITEM_STOCK_VALUES_UNIT_WISE stored procedure after delete
    /// </summary>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteReturn([FromBody] DeleteReturnRequest request)
    {
        _logger.LogInformation("Deleting return to stock for transaction {TransactionId}", request.TransactionID);

        var result = await _service.DeleteReturnAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { success = true, message = "Return deleted successfully" });
    }
}
