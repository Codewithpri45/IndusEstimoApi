namespace IndasEstimo.Application.DTOs.Menu;

/// <summary>
/// DTO for menu items with hierarchy information
/// </summary>
public class MenuItemDto
{
    public string ModuleHeadName { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public int SetGroupIndex { get; set; }
    public int NumberOfChild { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int ModuleDisplayOrder { get; set; }
}

/// <summary>
/// DTO for main menu items (parent only)
/// </summary>
public class ParentMenuDto
{
    public string ModuleHeadName { get; set; } = string.Empty;
    public int NumberOfChild { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int SetGroupIndex { get; set; }
}

/// <summary>
/// DTO for sub-menu items only
/// </summary>
public class SubMenuDto
{
    public string ModuleHeadName { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public int SetGroupIndex { get; set; }
    public int NumberOfChild { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int ModuleDisplayOrder { get; set; }
}

/// <summary>
/// DTO for user rights/permissions
/// </summary>
public class UserRightsDto
{
    public long UserID { get; set; }
    public long ModuleID { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanSave { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPrint { get; set; }
    public bool CanExport { get; set; }
}
