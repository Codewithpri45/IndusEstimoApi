using Dapper;
using IndasEstimo.Application.DTOs.Menu;
using IndasEstimo.Application.Interfaces.Repositories.Menu;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace IndasEstimo.Infrastructure.Repositories.Menu;

/// <summary>
/// Repository implementation for Menu operations
/// </summary>
public class MenuRepository : IMenuRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MenuRepository> _logger;

    public MenuRepository(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        ILogger<MenuRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private SqlConnection GetConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    /// <summary>
    /// Get parent menu items (grouped by ModuleHeadName)
    /// </summary>
    public async Task<List<ParentMenuDto>> GetParentMenuAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        string query = @"
            SELECT 
                MM.ModuleHeadName,
                (SELECT COUNT(ModuleHeadName) 
                 FROM ModuleMaster 
                 WHERE ModuleHeadName = MM.ModuleHeadName 
                   AND CompanyID = MM.CompanyID 
                   AND ISNULL(IsDeletedTransaction, 0) = 0) AS NumberOfChild,
                (SELECT TOP 1 NULLIF(ModuleName, '') 
                 FROM ModuleMaster 
                 WHERE ModuleHeadName = MM.ModuleHeadName 
                   AND CompanyID = MM.CompanyID 
                   AND ISNULL(IsDeletedTransaction, 0) = 0) AS ModuleName,
                ISNULL(MM.SetGroupIndex, 0) AS SetGroupIndex
            FROM UserModuleAuthentication AS UMA
            INNER JOIN ModuleMaster AS MM 
                ON UMA.ModuleID = MM.ModuleID 
                AND UMA.CompanyID = MM.CompanyID 
                AND ISNULL(MM.IsDeletedTransaction, 0) = 0 
                AND ISNULL(MM.IsLocked, 0) = 0
            WHERE UMA.UserID = @UserID 
              AND UMA.CompanyID = @CompanyID 
              AND ISNULL(UMA.CanView, 0) = 1 
              AND ISNULL(MM.IsDeletedTransaction, 0) = 0
            GROUP BY MM.ModuleHeadName, MM.SetGroupIndex, MM.CompanyID
            ORDER BY SetGroupIndex";

        var results = await connection.QueryAsync<ParentMenuDto>(query, new { UserID = userId, CompanyID = companyId });
        return results.ToList();
    }

    /// <summary>
    /// Get sub-menu items (modules with more than 1 child)
    /// </summary>
    public async Task<List<SubMenuDto>> GetSubMenuAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        string query = @"
            SELECT 
                MM.ModuleHeadName,
                NULLIF(MM.ModuleDisplayName, '') AS ModuleDisplayName,
                ISNULL(MM.SetGroupIndex, 0) AS SetGroupIndex,
                (SELECT COUNT(ModuleHeadName) 
                 FROM ModuleMaster 
                 WHERE ModuleHeadName = MM.ModuleHeadName 
                   AND CompanyID = MM.CompanyID 
                   AND ISNULL(IsDeletedTransaction, 0) = 0) AS NumberOfChild,
                MM.ModuleName,
                MM.ModuleDisplayOrder
            FROM UserModuleAuthentication AS UMA
            INNER JOIN ModuleMaster AS MM 
                ON UMA.ModuleID = MM.ModuleID 
                AND UMA.CompanyID = MM.CompanyID 
                AND ISNULL(MM.IsDeletedTransaction, 0) = 0 
                AND ISNULL(MM.IsLocked, 0) = 0
            WHERE UMA.CompanyID = @CompanyID 
              AND UMA.UserID = @UserID 
              AND ISNULL(UMA.CanView, 0) = 1 
              AND ISNULL(MM.IsDeletedTransaction, 0) = 0
            GROUP BY MM.ModuleHeadName, MM.SetGroupIndex, MM.ModuleName, 
                     MM.ModuleDisplayName, MM.ModuleDisplayOrder, MM.CompanyID
            HAVING (SELECT COUNT(ModuleHeadName) 
                    FROM ModuleMaster 
                    WHERE ModuleHeadName = MM.ModuleHeadName 
                      AND CompanyID = MM.CompanyID 
                      AND ISNULL(IsDeletedTransaction, 0) = 0) > 1
            ORDER BY SetGroupIndex, MM.ModuleDisplayOrder";

        var results = await connection.QueryAsync<SubMenuDto>(query, new { UserID = userId, CompanyID = companyId });
        return results.ToList();
    }

    /// <summary>
    /// Get complete menu with sub-menus (all modules with at least 0 children)
    /// </summary>
    public async Task<List<MenuItemDto>> GetMenuWithSubMenuAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        string query = @"
            SELECT DISTINCT 
                MM.ModuleHeadName,
                NULLIF(MM.ModuleDisplayName, '') AS ModuleDisplayName,
                ISNULL(MM.SetGroupIndex, 0) AS SetGroupIndex,
                (SELECT COUNT(ModuleHeadName) 
                 FROM ModuleMaster 
                 WHERE ModuleHeadName = MM.ModuleHeadName 
                   AND CompanyID = MM.CompanyID 
                   AND ISNULL(IsDeletedTransaction, 0) = 0) AS NumberOfChild,
                MM.ModuleName,
                MM.ModuleDisplayOrder
            FROM UserModuleAuthentication AS UMA
            INNER JOIN ModuleMaster AS MM 
                ON UMA.ModuleID = MM.ModuleID 
                AND UMA.CompanyID = MM.CompanyID 
                AND ISNULL(MM.IsDeletedTransaction, 0) = 0 
                AND ISNULL(MM.IsLocked, 0) = 0
            WHERE UMA.CompanyID = @CompanyID 
              AND UMA.UserID = @UserID 
              AND ISNULL(UMA.CanView, 0) = 1 
              AND ISNULL(MM.IsDeletedTransaction, 0) = 0
            GROUP BY MM.ModuleHeadName, MM.SetGroupIndex, MM.ModuleName, 
                     MM.ModuleDisplayName, MM.ModuleDisplayOrder, MM.CompanyID
            HAVING (SELECT COUNT(ModuleHeadName) 
                    FROM ModuleMaster 
                    WHERE ModuleHeadName = MM.ModuleHeadName 
                      AND CompanyID = MM.CompanyID 
                      AND ISNULL(IsDeletedTransaction, 0) = 0) > 0
            ORDER BY SetGroupIndex, MM.ModuleDisplayOrder";

        var results = await connection.QueryAsync<MenuItemDto>(query, new { UserID = userId, CompanyID = companyId });
        return results.ToList();
    }

    /// <summary>
    /// Get user rights for all accessible modules
    /// </summary>
    public async Task<List<UserRightsDto>> GetUserRightsAsync()
    {
        using var connection = GetConnection();
        var companyId = _currentUserService.GetCompanyId() ?? 0;
        var userId = _currentUserService.GetUserId() ?? 0;

        string query = @"
            SELECT 
                UMA.UserID,
                UMA.ModuleID,
                NULLIF(MM.ModuleName, '') AS ModuleName,
                ISNULL(UMA.CanView, 0) AS CanView,
                ISNULL(UMA.CanSave, 0) AS CanSave,
                ISNULL(UMA.CanEdit, 0) AS CanEdit,
                ISNULL(UMA.CanDelete, 0) AS CanDelete,
                ISNULL(UMA.CanPrint, 0) AS CanPrint,
                ISNULL(UMA.CanExport, 0) AS CanExport
            FROM UserModuleAuthentication AS UMA
            INNER JOIN ModuleMaster AS MM ON UMA.ModuleID = MM.ModuleID
            INNER JOIN UserMaster AS UM ON UM.UserID = UMA.UserID
            WHERE ISNULL(UM.IsDeletedUser, 0) = 0 
              AND UMA.CompanyID = @CompanyID 
              AND UMA.UserID = @UserID 
              AND ISNULL(UMA.CanView, 0) = 1";

        var results = await connection.QueryAsync<UserRightsDto>(query, new { UserID = userId, CompanyID = companyId });
        return results.ToList();
    }
}
