using IndasEstimo.Application.DTOs.Masters;
using IndasEstimo.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Masters;

[ApiController]
[Route("api/masters/materialgroupmaster")]
[Authorize]
public class MaterialGroupMasterController : ControllerBase
{
    private readonly IMaterialGroupMasterService _service;
    private readonly ILogger<MaterialGroupMasterController> _logger;

    public MaterialGroupMasterController(
        IMaterialGroupMasterService service,
        ILogger<MaterialGroupMasterController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all material groups for main grid.
    /// Old VB method: GetGroup()
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<MaterialGroupListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGroupList()
    {
        _logger.LogInformation("Getting material group list");

        var result = await _service.GetGroupListAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get under-group dropdown options.
    /// Old VB method: GetUnderGroup()
    /// </summary>
    [HttpGet("undergroup")]
    [ProducesResponseType(typeof(List<UnderGroupDropdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnderGroup()
    {
        _logger.LogInformation("Getting under group dropdown");

        var result = await _service.GetUnderGroupAsync();
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Save a new material group. Returns 'Success', 'Exist' (duplicate name), or 'fail'.
    /// Old VB method: SaveGroupData()
    /// </summary>
    [HttpPost("save")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveGroup([FromBody] SaveMaterialGroupRequest request)
    {
        _logger.LogInformation("Saving material group {Name}", request.ItemSubGroupName);

        var result = await _service.SaveGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Data == "Exist")
            return Conflict(new { message = "This Group Name already exists. Please enter another Group Name." });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update an existing material group. Returns 'Success' or 'fail'.
    /// Old VB method: UpdatGroupData()
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateMaterialGroupRequest request)
    {
        _logger.LogInformation("Updating material group {ID}", request.ItemSubGroupUniqueID);

        var result = await _service.UpdateGroupAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Soft-delete a material group (sets IsDeletedTransaction=1). Returns 'Success' or 'fail'.
    /// Old VB method: DeleteGroupMasterData()
    /// </summary>
    [HttpPost("delete/{itemSubGroupUniqueId:long}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteGroup(long itemSubGroupUniqueId)
    {
        _logger.LogInformation("Deleting material group {ID}", itemSubGroupUniqueId);

        var result = await _service.DeleteGroupAsync(itemSubGroupUniqueId);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
