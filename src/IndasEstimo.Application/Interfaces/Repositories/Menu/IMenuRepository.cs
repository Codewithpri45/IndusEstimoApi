using IndasEstimo.Application.DTOs.Menu;

namespace IndasEstimo.Application.Interfaces.Repositories.Menu;

/// <summary>
/// Repository interface for Menu operations
/// Handles dynamic menu generation based on user authentication
/// </summary>
public interface IMenuRepository
{
    /// <summary>
    /// Get parent menu items for a user (modules with children grouped)
    /// </summary>
    Task<List<ParentMenuDto>> GetParentMenuAsync();

    /// <summary>
    /// Get sub-menu items for a user (modules with multiple children)
    /// </summary>
    Task<List<SubMenuDto>> GetSubMenuAsync();

    /// <summary>
    /// Get complete menu hierarchy with both parent and sub-menus
    /// </summary>
    Task<List<MenuItemDto>> GetMenuWithSubMenuAsync();

    /// <summary>
    /// Get user rights/permissions for all accessible modules
    /// </summary>
    Task<List<UserRightsDto>> GetUserRightsAsync();
}
