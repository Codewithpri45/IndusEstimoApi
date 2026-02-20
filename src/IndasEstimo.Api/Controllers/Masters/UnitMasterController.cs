using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/unitmaster")]
[Authorize]
public class UnitMasterController : ControllerBase
{
    private readonly IUnitMasterService _service;
    private readonly ILogger<UnitMasterController> _logger;

    public UnitMasterController(
        IUnitMasterService service,
        ILogger<UnitMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all units for main grid.
    /// Old VB method: GetUnit()
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<UnitListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnitList()
    {
        _logger.LogInformation("Getting unit list");

        var result = await _service.GetUnitListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save a new unit. Returns 'Success', 'Exist' (duplicate UnitName), or 'fail'.
    /// Old VB method: SaveUnitData()
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveUnit([FromBody] SaveUnitRequest request)
    {
        _logger.LogInformation("Saving unit {UnitName}", request.UnitName);

        var result = await _service.SaveUnitAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "This Unit Name already exists. Please enter another Unit Name." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update an existing unit. Returns 'Success' or 'fail'.
    /// Old VB method: UpdatUnitData()
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUnit([FromBody] UpdateUnitRequest request)
    {
        _logger.LogInformation("Updating unit {UnitID}", request.UnitID);

        var result = await _service.UpdateUnitAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Soft-delete a unit (sets IsDeletedTransaction=1). Returns 'Success' or 'fail'.
    /// Old VB method: DeleteUnitMasterData()
    /// </summary>
    [HttpPost("delete/{unitId:long}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUnit(long unitId)
    {
        _logger.LogInformation("Deleting unit {UnitID}", unitId);

        var result = await _service.DeleteUnitAsync(unitId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
