namespace IndasEstimo.Application.DTOs.Masters;

// ── Response DTOs ─────────────────────────────────────────────

/// <summary>
/// Row returned by GetGroup() — main grid.
/// Maps to ItemSubGroupMaster.
/// </summary>
public class MaterialGroupListDto
{
    public long   ItemSubGroupUniqueID      { get; set; }
    public long   ItemSubGroupID            { get; set; }
    public string ItemSubGroupName          { get; set; } = "";
    public string ItemSubGroupDisplayName   { get; set; } = "";
    public int    UnderSubGroupID           { get; set; }
    public int    ItemSubGroupLevel         { get; set; }
    public string GroupName                 { get; set; } = "";
}

/// <summary>
/// Dropdown item returned by GetUnderGroup() — parent group selector.
/// </summary>
public class UnderGroupDropdownDto
{
    public int    ItemSubGroupID            { get; set; }
    public string ItemSubGroupDisplayName   { get; set; } = "";
}

// ── Request DTOs ──────────────────────────────────────────────

/// <summary>
/// Payload for SaveGroupData — create a new material group.
/// Old VB method: SaveGroupData()
/// </summary>
public class SaveMaterialGroupRequest
{
    public string ItemSubGroupName        { get; set; } = "";
    public string ItemSubGroupDisplayName { get; set; } = "";
    public int    UnderSubGroupID         { get; set; }
}

/// <summary>
/// Payload for UpdatGroupData — update an existing material group.
/// Old VB method: UpdatGroupData()
/// </summary>
public class UpdateMaterialGroupRequest
{
    public long   ItemSubGroupUniqueID    { get; set; }
    public int    ItemSubGroupLevel       { get; set; }
    public string ItemSubGroupName        { get; set; } = "";
    public string ItemSubGroupDisplayName { get; set; } = "";
    public int    UnderSubGroupID         { get; set; }
}
