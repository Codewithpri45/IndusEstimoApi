using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Menu;
using IndasEstimo.Application.Interfaces.Repositories.Menu;
using IndasEstimo.Application.Interfaces.Services.Menu;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Services.Menu;

/// <summary>
/// Service implementation for Menu operations
/// Contains business logic and error handling for menu generation
/// </summary>
public class MenuService : IMenuService
{
    private readonly IMenuRepository _repository;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        IMenuRepository repository,
        ILogger<MenuService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get parent menu items for current user
    /// </summary>
    public async Task<Result<List<ParentMenuDto>>> GetParentMenuAsync()
    {
        try
        {
            var data = await _repository.GetParentMenuAsync();
            return Result<List<ParentMenuDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent menu");
            return Result<List<ParentMenuDto>>.Failure("Failed to get parent menu");
        }
    }

    /// <summary>
    /// Get sub-menu items for current user
    /// </summary>
    public async Task<Result<List<SubMenuDto>>> GetSubMenuAsync()
    {
        try
        {
            var data = await _repository.GetSubMenuAsync();
            return Result<List<SubMenuDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-menu");
            return Result<List<SubMenuDto>>.Failure("Failed to get sub-menu");
        }
    }

    /// <summary>
    /// Get complete menu with sub-menus for current user
    /// </summary>
    public async Task<Result<List<MenuItemDto>>> GetMenuWithSubMenuAsync()
    {
        try
        {
            var data = await _repository.GetMenuWithSubMenuAsync();
            
            if (data == null || data.Count == 0)
            {
                _logger.LogWarning("No menu items found for current user");
                return Result<List<MenuItemDto>>.Success(new List<MenuItemDto>());
            }

            return Result<List<MenuItemDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menu with sub-menu");
            return Result<List<MenuItemDto>>.Failure("Failed to get menu structure");
        }
    }

    /// <summary>
    /// Get user rights/permissions for current user
    /// </summary>
    public async Task<Result<List<UserRightsDto>>> GetUserRightsAsync()
    {
        try
        {
            var data = await _repository.GetUserRightsAsync();
            
            if (data == null || data.Count == 0)
            {
                _logger.LogWarning("No user rights found for current user");
                return Result<List<UserRightsDto>>.Success(new List<UserRightsDto>());
            }

            return Result<List<UserRightsDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user rights");
            return Result<List<UserRightsDto>>.Failure("Failed to get user permissions");
        }
    }
}
