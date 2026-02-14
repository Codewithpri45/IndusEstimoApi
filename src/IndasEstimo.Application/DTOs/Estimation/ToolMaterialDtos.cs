namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for Die/Tool search
/// </summary>
public class DieToolDto
{
    public long ToolID { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public decimal SizeL { get; set; }
    public decimal SizeW { get; set; }
    public decimal SizeH { get; set; }
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
    public decimal SizeW { get; set; }
    public decimal SizeH { get; set; }
    public string? ToolType { get; set; }
}

/// <summary>
/// DTO for Reel Master
/// </summary>
public class ReelDto
{
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal GSM { get; set; }
    public decimal BF { get; set; }
    public decimal PhysicalStock { get; set; }
    public string? StockUnit { get; set; }
    public decimal SizeW { get; set; }
    public decimal EstimationRate { get; set; }
}

/// <summary>
/// DTO for Process Materials
/// </summary>
public class ProcessMaterialDto
{
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public long? ItemSubGroupID { get; set; }
    public string? ItemSubGroupName { get; set; }
    public decimal EstimationRate { get; set; }
    public string EstimationUnit { get; set; } = string.Empty;
    public string? StockUnit { get; set; }
}
