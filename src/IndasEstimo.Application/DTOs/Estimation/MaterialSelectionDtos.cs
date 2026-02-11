namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for Quality dropdown
/// </summary>
public class QualityDto
{
    public string Quality { get; set; } = string.Empty;
}

/// <summary>
/// DTO for GSM dropdown
/// </summary>
public class GsmDto
{
    public decimal GSM { get; set; }
}

/// <summary>
/// DTO for Thickness dropdown
/// </summary>
public class ThicknessDto
{
    public decimal Thickness { get; set; }
}

/// <summary>
/// DTO for Mill (Manufacturer) dropdown
/// </summary>
public class MillDto
{
    public string Mill { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Finish dropdown
/// </summary>
public class FinishDto
{
    public string Finish { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Coating options
/// </summary>
public class CoatingDto
{
    public string Headname { get; set; } = string.Empty;
    public string Amount { get; set; } = "0";
}

/// <summary>
/// DTO for BF (Bursting Factor) dropdown
/// </summary>
public class BFDto
{
    public decimal BF { get; set; }
}

/// <summary>
/// DTO for Flute types (Corrugation)
/// </summary>
public class FluteDto
{
    public string FluteType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Layer Items
/// </summary>
public class LayerItemDto
{
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public long? ItemSubGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string? ItemSubGroupName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Quality { get; set; }
    public decimal SizeW { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public decimal GSM { get; set; }
    public string? Manufecturer { get; set; }
    public string? EstimationUnit { get; set; }
    public decimal EstimationRate { get; set; }
    public string? StockUnit { get; set; }
}

/// <summary>
/// DTO for Available Layers search
/// </summary>
public class AvailableLayerDto
{
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public long? ItemSubGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string? ItemSubGroupName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Quality { get; set; }
    public decimal SizeW { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public decimal GSM { get; set; }
    public string? Manufecturer { get; set; }
    public string? EstimationUnit { get; set; }
    public decimal EstimationRate { get; set; }
    public string? StockUnit { get; set; }
}

/// <summary>
/// DTO for Filtered Paper Items (Combined Mill, Finish, GSM)
/// </summary>
public class FilteredPaperDto
{
    public List<MillDto> Mills { get; set; } = new();
    public List<GsmDto> GSMs { get; set; } = new();
    public List<FinishDto> Finishes { get; set; } = new();
}
