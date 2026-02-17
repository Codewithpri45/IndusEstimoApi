namespace IndasEstimo.Application.DTOs.Inventory;

/// <summary>
/// Common warehouse lookup DTO
/// Used by: PurchaseGRN
/// Maps to WarehouseMaster.WarehouseName for dropdown/lookup scenarios
/// </summary>
public class WarehouseDto
{
    /// <summary>
    /// Warehouse name
    /// </summary>
    public string Warehouse { get; set; } = string.Empty;
}

/// <summary>
/// Common bin lookup DTO
/// Used by: PurchaseGRN
/// Maps to WarehouseMaster for bin selection within a warehouse
/// </summary>
public class BinDto
{
    /// <summary>
    /// Bin name/identifier
    /// </summary>
    public string Bin { get; set; } = string.Empty;

    /// <summary>
    /// Reference to parent warehouse
    /// </summary>
    public long WarehouseID { get; set; }
}
