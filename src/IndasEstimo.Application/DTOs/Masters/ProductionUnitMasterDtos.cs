namespace IndasEstimo.Application.DTOs.Masters;

// ── Response DTOs ─────────────────────────────────────────────

/// <summary>
/// Row returned by GetProductionUnitMasterShowList() — main grid.
/// Maps to ProductionUnitMaster joined with CompanyMaster and BranchMaster.
/// </summary>
public class ProductionUnitListDto
{
    public long   ProductionUnitID           { get; set; }
    public string ProductionUnitCode         { get; set; } = "";
    public string RefProductionUnitCode      { get; set; } = "";
    public string ProductionUnitName         { get; set; } = "";
    public string CompanyName                { get; set; } = "";
    public long   CompanyID                  { get; set; }
    public string GSTNo                      { get; set; } = "";
    public string BranchName                 { get; set; } = "";
    public int    BranchID                   { get; set; }
    public string Address                    { get; set; } = "";
    public string City                       { get; set; } = "";
    public string State                      { get; set; } = "";
    public string Pincode                    { get; set; } = "";
    public string Country                    { get; set; } = "";
}

/// <summary>
/// Dropdown item for Country selector.
/// Old VB method: GetCountry()
/// </summary>
public class CountryDropdownDto
{
    public string Country { get; set; } = "";
}

/// <summary>
/// Dropdown item for State selector.
/// Old VB method: GetState()
/// </summary>
public class StateDropdownDto
{
    public string State { get; set; } = "";
}

/// <summary>
/// Dropdown item for City selector.
/// Old VB method: GetCity()
/// </summary>
public class CityDropdownDto
{
    public string City { get; set; } = "";
}

/// <summary>
/// Dropdown item for Company selector.
/// Old VB method: GetCompanyName()
/// </summary>
public class CompanyDropdownDto
{
    public long   CompanyID   { get; set; }
    public string CompanyName { get; set; } = "";
}

/// <summary>
/// Dropdown item for Branch selector.
/// Old VB method: GetBranch()
/// </summary>
public class BranchDropdownDto
{
    public int    BranchID   { get; set; }
    public string BranchName { get; set; } = "";
}

// ── Request DTOs ──────────────────────────────────────────────

/// <summary>
/// Payload for SaveProductionUnitMasterData — create a new production unit.
/// Old VB method: SaveProductionUnitMasterData()
/// </summary>
public class SaveProductionUnitRequest
{
    public string ProductionUnitName   { get; set; } = "";
    public string RefProductionUnitCode { get; set; } = "";
    public long   CompanyID            { get; set; }
    public int    BranchID             { get; set; }
    public string GSTNo                { get; set; } = "";
    public string Address              { get; set; } = "";
    public string City                 { get; set; } = "";
    public string State                { get; set; } = "";
    public string Pincode              { get; set; } = "";
    public string Country              { get; set; } = "";
}

/// <summary>
/// Payload for UpdateProductionUnitMasterData — update an existing production unit.
/// Old VB method: UpdateProductionUnitMasterData()
/// </summary>
public class UpdateProductionUnitRequest
{
    public long   ProductionUnitID     { get; set; }
    public string ProductionUnitName   { get; set; } = "";
    public string RefProductionUnitCode { get; set; } = "";
    public long   CompanyID            { get; set; }
    public int    BranchID             { get; set; }
    public string GSTNo                { get; set; } = "";
    public string Address              { get; set; } = "";
    public string City                 { get; set; } = "";
    public string State                { get; set; } = "";
    public string Pincode              { get; set; } = "";
    public string Country              { get; set; } = "";
}
