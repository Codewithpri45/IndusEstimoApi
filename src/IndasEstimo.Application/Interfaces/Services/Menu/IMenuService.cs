using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Menu;

namespace IndasEstimo.Application.Interfaces.Services.Menu;

/// <summary>
/// Service interface for Menu operations
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Get parent menu items for current user
    /// </summary>
    Task<Result<List<ParentMenuDto>>> GetParentMenuAsync();

    /// <summary>
    /// Get sub-menu items for current user
    /// </summary>
    Task<Result<List<SubMenuDto>>> GetSubMenuAsync();

    /// <summary>
    /// Get complete menu with sub-menus for current user
    /// </summary>
    Task<Result<List<MenuItemDto>>> GetMenuWithSubMenuAsync();

    /// <summary>
    /// Get user rights/permissions for current user
    /// </summary>
    Task<Result<List<UserRightsDto>>> GetUserRightsAsync();
}
