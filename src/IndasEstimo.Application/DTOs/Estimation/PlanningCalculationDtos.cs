namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// Request for calculating operation cost
/// </summary>
public class CalculateOperationRequest
{
    public long ProcessID { get; set; }
    public decimal Quantity { get; set; }
    public int Ups { get; set; }
    public int NoOfPass { get; set; }
    public int NoOfColors { get; set; }
    public decimal SizeL { get; set; }
    public decimal SizeW { get; set; }
    public string? RateFactor { get; set; }
}

/// <summary>
/// Response for operation calculation
/// </summary>
public class CalculateOperationResponse
{
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal MinimumCharges { get; set; }
    public string TypeOfCharges { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Charge Types
/// </summary>
public class ChargeTypeDto
{
    public string ChargeType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Material Formula Setting
/// </summary>
public class MaterialFormulaDto
{
    public long SettingID { get; set; }
    public long ItemSubGroupID { get; set; }
    public string? ItemSubGroupName { get; set; }
    public string CostingFormula { get; set; } = string.Empty;
    public string? FormulaParameters { get; set; }
    public bool ApplyGSTOnEstimationRate { get; set; }
}

/// <summary>
/// DTO for Wastage Slab
/// </summary>
public class WastageSlabDto
{
    public long SlabID { get; set; }
    public string WastageType { get; set; } = string.Empty;
    public decimal FromValue { get; set; }
    public decimal ToValue { get; set; }
    public decimal WastagePercentage { get; set; }
}

/// <summary>
/// DTO for Wastage Type
/// </summary>
public class WastageTypeDto
{
    public string WastageType { get; set; } = string.Empty;
}

/// <summary>
/// Request for Keyline Coordinates
/// </summary>
public class KeylineRequest
{
    public string ContentType { get; set; } = string.Empty;
    public string? Grain { get; set; }
}

/// <summary>
/// DTO for Keyline Coordinates
/// </summary>
public class KeylineCoordinatesDto
{
    public string ContentType { get; set; } = string.Empty;
    public string? Grain { get; set; }
    public decimal TrimmingLeft { get; set; }
    public decimal TrimmingRight { get; set; }
    public decimal TrimmingTop { get; set; }
    public decimal TrimmingBottom { get; set; }
    public decimal StripingLeft { get; set; }
    public decimal StripingRight { get; set; }
    public decimal StripingTop { get; set; }
    public decimal StripingBottom { get; set; }
}

/// <summary>
/// Request for Corrugation Plan calculation
/// </summary>
public class CorrugationPlanRequest
{
    public string Quality { get; set; } = string.Empty;
    public decimal GSM { get; set; }
    public string Mill { get; set; } = string.Empty;
    public decimal Thickness { get; set; }
    public decimal Width { get; set; }
    public string FluteType { get; set; } = string.Empty;
}

/// <summary>
/// Response for Corrugation Plan
/// </summary>
public class CorrugationPlanResponse
{
    public decimal TakeUpFactor { get; set; }
    public decimal BurstingFactor { get; set; }
    public decimal BurstingStrength { get; set; }
    public decimal CalculatedGSM { get; set; }
}
