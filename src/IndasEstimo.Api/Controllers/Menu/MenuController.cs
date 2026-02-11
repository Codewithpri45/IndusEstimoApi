using IndasEstimo.Application.DTOs.Menu;
using IndasEstimo.Application.Interfaces.Services.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasEstimo.Api.Controllers.Menu;

/// <summary>
/// Controller for Menu and User Rights management
/// </summary>
[Authorize]
[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuService menuService,
        ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _logger = logger;
    }

    /// <summary>
    /// Get parent menu items for current user
    /// </summary>
    /// <returns>List of parent menu items grouped by ModuleHeadName</returns>
    [HttpGet("parent")]
    [ProducesResponseType(typeof(List<ParentMenuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetParentMenu()
    {
        var result = await _menuService.GetParentMenuAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get sub-menu items for current user (modules with multiple children)
    /// </summary>
    /// <returns>List of sub-menu items</returns>
    [HttpGet("submenu")]
    [ProducesResponseType(typeof(List<SubMenuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubMenu()
    {
        var result = await _menuService.GetSubMenuAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get complete menu hierarchy with both parent and sub-menus
    /// </summary>
    /// <returns>Complete menu structure for the current user</returns>
    [HttpGet("complete")]
    [ProducesResponseType(typeof(List<MenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMenuWithSubMenu()
    {
        var result = await _menuService.GetMenuWithSubMenuAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    /// <summary>
    /// Get user permissions for all accessible modules
    /// </summary>
    /// <returns>User rights/permissions including view, save, edit, delete, print, export</returns>
    [HttpGet("rights")]
    [ProducesResponseType(typeof(List<UserRightsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserRights()
    {
        var result = await _menuService.GetUserRightsAsync();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}
