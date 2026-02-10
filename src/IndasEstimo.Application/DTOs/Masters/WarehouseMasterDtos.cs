namespace IndasEstimo.Application.DTOs.Masters;

/// <summary>
/// DTO for warehouse list display
/// </summary>
public class WarehouseListDto
{
    public int WarehouseID { get; set; }
    public string WarehouseCode { get; set; } = "";
    public string WarehouseName { get; set; } = "";
    public string? RefWarehouseCode { get; set; }
    public bool IsFloorWarehouse { get; set; }
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string ModifiedDate { get; set; } = "";

    // Related entities
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = "";
    public int? BranchID { get; set; }
    public string BranchName { get; set; } = "";
}

/// <summary>
/// DTO for bin name lookup
/// </summary>
public class WarehouseBinDto
{
    public int WarehouseID { get; set; }
    public string BinName { get; set; } = "";
}

/// <summary>
/// DTO for city list
/// </summary>
public class CityDto
{
    public string City { get; set; } = "";
}

/// <summary>
/// DTO for branch list
/// </summary>
public class BranchDto
{
    public int BranchID { get; set; }
    public string BranchName { get; set; } = "";
    public string? BranchCode { get; set; }
    public int CompanyID { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for saving new warehouse
/// </summary>
public class SaveWarehouseRequest
{
    public string Prefix { get; set; } = "WH";
    public Dictionary<string, object>[] SaveRecords { get; set; } = Array.Empty<Dictionary<string, object>>();
}

/// <summary>
/// Request DTO for updating warehouse
/// </summary>
public class UpdateWarehouseRequest
{
    public Dictionary<string, object>[] SaveRecords { get; set; } = Array.Empty<Dictionary<string, object>>();
    public Dictionary<string, object>[] UpdateRecords { get; set; } = Array.Empty<Dictionary<string, object>>();
}

/// <summary>
/// Response DTO for warehouse code generation
/// </summary>
public class WarehouseCodeDto
{
    public string WarehouseCode { get; set; } = "";
    public long MaxWarehouseCode { get; set; }
    public string Prefix { get; set; } = "";
}
