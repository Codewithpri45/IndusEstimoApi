using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

/// <summary>
/// Controller for WarehouseMaster operations
/// </summary>
[ApiController]
[Route("api/masters/warehouse")]
[Authorize]
public class WarehouseMasterController : ControllerBase
{
    private readonly IWarehouseMasterService _warehouseService;
    private readonly ILogger<WarehouseMasterController> _logger;

    public WarehouseMasterController(
        IWarehouseMasterService warehouseService,
        ILogger<WarehouseMasterController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Generate new warehouse code
    /// </summary>
    /// <returns>Warehouse code with prefix</returns>
    [HttpGet("generate-code")]
    public async Task<IActionResult> GetWarehouseNo()
    {
        _logger.LogInformation("Generating new warehouse code");

        var result = await _warehouseService.GetWarehouseNoAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of all cities
    /// </summary>
    /// <returns>List of cities</returns>
    [HttpGet("cities")]
    public async Task<IActionResult> GetCityList()
    {
        _logger.LogInformation("Getting city list");

        var result = await _warehouseService.GetCityListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save new warehouse
    /// </summary>
    /// <param name="request">Warehouse data</param>
    /// <returns>Success or failure message</returns>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    public async Task<IActionResult> SaveWarehouse([FromBody] SaveWarehouseRequest request)
    {
        _logger.LogInformation("Saving new warehouse with prefix {Prefix}", request.Prefix);

        var result = await _warehouseService.SaveWarehouseAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update existing warehouse
    /// </summary>
    /// <param name="request">Update data</param>
    /// <returns>Success or failure message</returns>
    [HttpPut("update")]
    [Authorize(Roles = "Admin,Manager,InventoryUser")]
    public async Task<IActionResult> UpdateWarehouse([FromBody] UpdateWarehouseRequest request)
    {
        _logger.LogInformation("Updating warehouse");

        var result = await _warehouseService.UpdateWarehouseAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of all warehouses with production unit filtering
    /// </summary>
    /// <returns>List of warehouses</returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetWarehouseList()
    {
        _logger.LogInformation("Getting warehouse list");

        var result = await _warehouseService.GetWarehouseListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get bin name by warehouse name
    /// </summary>
    /// <param name="warehouseName">Warehouse name</param>
    /// <returns>Warehouse ID and bin name</returns>
    [HttpGet("bin/{warehouseName}")]
    public async Task<IActionResult> GetBinName(string warehouseName)
    {
        _logger.LogInformation("Getting bin name for warehouse {WarehouseName}", warehouseName);

        var result = await _warehouseService.GetBinNameAsync(warehouseName);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete warehouse (soft delete with usage check)
    /// </summary>
    /// <param name="warehouseId">Warehouse ID to delete</param>
    /// <returns>Success, Exist (if in use), or failure message</returns>
    [HttpDelete("{warehouseId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteWarehouse(string warehouseId)
    {
        _logger.LogInformation("Deleting warehouse {WarehouseId}", warehouseId);

        var result = await _warehouseService.DeleteWarehouseAsync(warehouseId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        // Return special response if warehouse is in use
        if (result.Data == "Exist")
        {
            return Conflict(new { message = "Warehouse is being used in transactions and cannot be deleted" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get list of all branches
    /// </summary>
    /// <returns>List of branches</returns>
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranchList()
    {
        _logger.LogInformation("Getting branch list");

        var result = await _warehouseService.GetBranchListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
