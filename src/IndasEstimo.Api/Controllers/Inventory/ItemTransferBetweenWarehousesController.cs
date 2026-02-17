using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndasEstimo.Application.DTOs.Inventory;
using IndasEstimo.Application.Interfaces.Services.Inventory;

namespace IndasEstimo.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/item-transfer")]
[Authorize]
public class ItemTransferBetweenWarehousesController : ControllerBase
{
    private readonly IItemTransferBetweenWarehousesService _service;
    private readonly ILogger<ItemTransferBetweenWarehousesController> _logger;

    public ItemTransferBetweenWarehousesController(
        IItemTransferBetweenWarehousesService service,
        ILogger<ItemTransferBetweenWarehousesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all transfer vouchers filtered by date range
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<TransferListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTransferList(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        _logger.LogInformation("Getting transfer list from {FromDate} to {ToDate}", fromDate, toDate);

        var result = await _service.GetTransferListAsync(fromDate, toDate);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get available stock in a specific warehouse (by WarehouseID)
    /// </summary>
    [HttpGet("warehouse-stock")]
    [ProducesResponseType(typeof(List<WarehouseStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWarehouseStock([FromQuery] long warehouseId)
    {
        _logger.LogInformation("Getting warehouse stock for WarehouseID={WarehouseID}", warehouseId);

        var result = await _service.GetWarehouseStockAsync(warehouseId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get destination bins for a warehouse, excluding the source bin
    /// </summary>
    [HttpGet("destination-bins")]
    [ProducesResponseType(typeof(List<BinDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDestinationBins(
        [FromQuery] string warehouseName,
        [FromQuery] long sourceBinId)
    {
        _logger.LogInformation("Getting destination bins for warehouse {WarehouseName}", warehouseName);

        var result = await _service.GetDestinationBinsAsync(warehouseName, sourceBinId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Generate next transfer voucher number (VoucherID=-22, prefix e.g. "TRN")
    /// </summary>
    [HttpGet("next-voucher-no")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNextVoucherNo([FromQuery] string prefix = "TRN")
    {
        _logger.LogInformation("Getting next voucher number for prefix {Prefix}", prefix);

        var result = await _service.GetTransferVoucherNoAsync(prefix);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { voucherNo = result.Data });
    }

    /// <summary>
    /// Save a new transfer between warehouses voucher
    /// Creates Issue detail (source warehouse) + Receipt detail (destination warehouse)
    /// </summary>
    [HttpPost("save")]
    [ProducesResponseType(typeof(TransferOperationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveTransfer([FromBody] SaveTransferRequest request)
    {
        _logger.LogInformation("Saving new transfer voucher with prefix {Prefix}", request.Prefix);

        var result = await _service.SaveTransferAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update an existing transfer voucher
    /// Deletes old details and re-inserts updated Issue + Receipt details
    /// </summary>
    [HttpPut("update")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTransfer([FromBody] UpdateTransferRequest request)
    {
        _logger.LogInformation("Updating transfer voucher {TransactionID}", request.TransactionID);

        var result = await _service.UpdateTransferAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Transfer updated successfully" });
    }

    /// <summary>
    /// Soft-delete a transfer voucher (sets IsDeletedTransaction=1)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTransfer([FromRoute] long id)
    {
        _logger.LogInformation("Deleting transfer voucher {TransactionID}", id);

        var request = new DeleteTransferRequest { TransactionID = id };
        var result = await _service.DeleteTransferAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Transfer deleted successfully" });
    }
}
