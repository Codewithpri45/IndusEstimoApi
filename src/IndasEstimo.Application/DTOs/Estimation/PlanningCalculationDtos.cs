using System.Text.Json.Serialization;

namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// Request for calculating operation cost
/// </summary>
public class CalculateOperationRequest
{
    // Primary process ID (used when calculating specific operation cost)
    public long ProcessID { get; set; }
    
    // Comma-separated operation IDs (used when multiple processes selected, or sent as GblOperId from frontend)
    public string? GblOperId { get; set; }
    
    // When true, load default operations for the given category/content (no ProcessID required)
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; } = false;
    
    // Category context for default operations
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    public string? Gbl_Content_Domain_Type { get; set; }

    // Job dimensions
    public decimal? Gbl_Job_L { get; set; }
    public decimal? Gbl_Job_H { get; set; }
    public decimal? Gbl_Job_W { get; set; }
    public decimal? Gbl_Order_Quantity { get; set; }
    
    // Machine info
    public decimal? Make_Ready_Time { get; set; }
    public decimal? Job_Change_Over_Time { get; set; }
    public string? GblOperId_str => GblOperId; // convenience accessor

    // Legacy / standard fields
    public decimal Quantity { get; set; }        // Final_Quantity (sheets/running meters)
    public long QuantityPcs { get; set; }        // Final_Quantity_Pcs (pieces)
    public int Ups { get; set; }                  // Total_Ups
    public int NoOfPass { get; set; }             // NoOfPass
    public int NoOfColors { get; set; }           // Total_Colors
    public decimal SizeL { get; set; }            // Size_L (machine sheet size)
    public decimal SizeW { get; set; }            // Size_W
    public string? RateFactor { get; set; }
    
    // Additional legacy fields for charge type formulas
    public decimal TotalPaperKG { get; set; }     // Total_Paper_KG
    public int Sets { get; set; } = 1;            // Sets (forms/sections)
    public decimal PubSheets { get; set; }        // Pub_Sheets (total sheets after wastage)
    public decimal JobL { get; set; }             // Job_L (job label length)
    public decimal JobH { get; set; }             // Job_H (job label height)
    public decimal JobW { get; set; }             // Job_W (job label width)
    public long TotalPlates { get; set; }         // Total_Plates
    public int PagesPerSection { get; set; } = 1; // PagesPerSection
    public decimal NoOfForms { get; set; }        // NoOfForms
    public long OrderQuantity { get; set; }       // Order_Quantity
    public long JobPages { get; set; }            // Gbl_Job_Pages
    public long JobLeaves { get; set; }           // Gbl_Job_Leaves
    public decimal BookSpine { get; set; }        // Gbl_Book_Spine
    public decimal PaperGSM { get; set; }         // Gbl_Paper_GSM
    public int FrontColors { get; set; }          // Gbl_Front_Color
    public int UpsL { get; set; }                 // Gbl_UPS_L
    public int UpsH { get; set; }                 // Gbl_UPS_H
    public int Stitch { get; set; } = 1;          // Stitch count
    public int Folds { get; set; }                // Folds count
    public string? ContentSizeInputUnit { get; set; } // MM/INCH/CM
}

/// <summary>
/// Response for operation calculation
/// </summary>
public class CalculateOperationResponse
{
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
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
