namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for Die/Tool search
/// </summary>
public class DieToolDto
{
    public long ToolID { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string? ToolType { get; set; }
    public string? ToolDescription { get; set; }
    public string LedgerName { get; set; } = "-";
    public decimal SizeL { get; set; }
    public decimal SizeW { get; set; }
    public decimal SizeH { get; set; }
    public int UpsL { get; set; }
    public int UpsW { get; set; }
    public int TotalUps { get; set; }
    public string? Manufecturer { get; set; }
    public decimal CircumferenceInch { get; set; }
    public decimal CircumferenceMM { get; set; }
    public int NoOfTeeth { get; set; }
    public decimal ToolRate { get; set; }
}

/// <summary>
/// Request for searching dies
/// </summary>
public class SearchDiesRequest
{
    public decimal SizeL { get; set; }
    public decimal SizeLTolerance { get; set; }
    public decimal SizeW { get; set; }
    public decimal SizeWTolerance { get; set; }
    public decimal SizeH { get; set; }
    public decimal SizeHTolerance { get; set; }
}

/// <summary>
/// DTO for Reel Master
/// </summary>
public class ReelDto
{
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;  // Added from legacy
    public decimal GSM { get; set; }  // FaceGSM
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public decimal Thickness { get; set; }  // Added from legacy
    public decimal Density { get; set; }  // Added from legacy
    public string Manufecturer { get; set; } = string.Empty;  // Added from legacy (Mill)
    public decimal BF { get; set; }
    public decimal PhysicalStock { get; set; }
    public string? StockUnit { get; set; }
    public decimal SizeW { get; set; }
    public decimal EstimationRate { get; set; }
    public string EstimationUnit { get; set; } = string.Empty;  // CRITICAL: SQM/KG/RM - Rate Type
    public string Finish { get; set; } = string.Empty;  // Added from legacy
    public decimal AvgRollLength { get; set; } // Legacy: 18 (for Roll Change logic)
    public string PaperGroup { get; set; } = string.Empty;  // Added from legacy
    public string ItemGroupName { get; set; } = string.Empty;  // Added from legacy
    public string PurchaseUnit { get; set; } = string.Empty;  // Added from legacy
    public bool IsStandardItem { get; set; }  // For filtering standard/special size papers
    public bool IsAvailable { get; set; }  // Stock availability flag
    public decimal StockQuantity => PhysicalStock;  // Alias for PhysicalStock (frontend compatibility)
}

/// <summary>
/// DTO for Process Materials
/// </summary>
public class ProcessMaterialDto
{
    public long ProcessID { get; set; }
    public string? DomainType { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public long MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public long? ItemSubGroupID { get; set; }
    public int? ItemGroupNameID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string? ItemSubGroupName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal SizeL { get; set; }
    public decimal SizeW { get; set; }
    public decimal SizeH { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public decimal GSM { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public string? StockUnit { get; set; }
    public string? PurchaseUnit { get; set; }
    public string EstimationUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal EstimationRate { get; set; }
}
