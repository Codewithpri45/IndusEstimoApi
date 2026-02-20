namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Response DTOs ====================

/// <summary>
/// Unit list item for main grid.
/// Old VB method: GetUnit()
/// </summary>
public class UnitListDto
{
    public long UnitID { get; set; }
    public string UnitName { get; set; } = "";
    public string UnitSymbol { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal ConversionValue { get; set; }
    public int DecimalPlace { get; set; }
}

// ==================== Request DTOs ====================

/// <summary>
/// Request for saving a new unit.
/// Old VB method: SaveUnitData()
/// </summary>
public class SaveUnitRequest
{
    public string UnitName { get; set; } = "";
    public string UnitSymbol { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal ConversionValue { get; set; }
    public int DecimalPlace { get; set; }
}

/// <summary>
/// Request for updating an existing unit.
/// Old VB method: UpdatUnitData()
/// </summary>
public class UpdateUnitRequest
{
    public long UnitID { get; set; }
    public string UnitName { get; set; } = "";
    public string UnitSymbol { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal ConversionValue { get; set; }
    public int DecimalPlace { get; set; }
}
