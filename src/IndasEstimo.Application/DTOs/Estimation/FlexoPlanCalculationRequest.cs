using System.ComponentModel.DataAnnotations;

namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// Detailed DTO for calculating a Flexo Plan.
/// Equivalent to the legacy ShirinJobMasterRequest but strongly typed.
/// </summary>
public class FlexoPlanCalculationRequest
{
    // Job Dimensions
    [Required]
    public double JobSizeL { get; set; }
    [Required]
    public double JobSizeW { get; set; }
    public double JobSizeH { get; set; } // Often unused for flat labels but kept for boxes

    // Layout Constraints
    public int UpsAcross { get; set; } // Gbl_UPS_H (across roll width, uses JobSizeH)
    public int UpsAround { get; set; } // Gbl_UPS_L (around cylinder, uses JobSizeL)
    public double GapAcross { get; set; } = 3.0; // Gbl_Standard_AC_Gap (default 3mm)
    public double GapAround { get; set; } = 1.25; // Gbl_Standard_AR_Gap (default 1.25mm)
    public double Bleed { get; set; } // Gbl_Bleed

    // Material Selection
    [Required]
    public long PaperId { get; set; } // Gbl_Paper_ID
    public string PaperQuality { get; set; } = string.Empty; // Gbl_Paper_Quality (for filtering when PaperId = 0)
    public double PaperGSM { get; set; } // Gbl_Paper_GSM (for filtering when PaperId = 0)
    public string PaperMill { get; set; } = string.Empty; // Gbl_Paper_Mill/Manufecturer (for filtering when PaperId = 0)
    public double PaperRate { get; set; } // Rate per KG/SQM/RM
    public string PaperRateType { get; set; } = "KG"; // Estimation_Paper_Rate_Type: KG, SQM, RM (Running Meter)
    public string PaperUnit { get; set; } = "KG"; // Gbl_Estimation_Unit

    // Machine Selection (OPTIONAL - If 0, plans will be generated for ALL machines with cylinders)
    public int MachineId { get; set; } // Gbl_Machine_ID (0 = All machines)
    public long? CylinderId { get; set; } // Gbl_Cylinder_Tool_ID (Crucial for Flexo)
    
    // Process Details
    public int FrontColors { get; set; } // Gbl_Front_Color
    public int BackColors { get; set; } // Gbl_Back_Color
    public int SpecialFrontColors { get; set; }
    public int SpecialBackColors { get; set; }
    public string CoatingType { get; set; } = "None"; // GblOnlineCoating

    // Roll Finishing Specs
    public long WindingDirectionId { get; set; } // Gbl_windingdirection
    public float CoreInnerDia { get; set; } // Gbl_CoreInnerDia
    public float CoreOuterDia { get; set; } // Gbl_CoreOuterDia
    public int LabelsPerRoll { get; set; } // Gbl_LabelPerRoll
    public string LabelType { get; set; } = string.Empty; // Gbl_labeltype (Wrap Around, Shrink Sleeve)
    public string FinishedFormat { get; set; } = "Roll"; // Gbl_finishedformat (Roll/Sheet)

    // Quantity
    [Required]
    public int Quantity { get; set; } // Gbl_Order_Quantity

    // Additional Costs
    public double PlateRate { get; set; }
    public double MakeReadyRate { get; set; }
    public double CoatingRate { get; set; }
    public List<OperationCostDto> AdditionalOperations { get; set; } = new();

    // Advanced Wastage & Planning Options
    public bool ShadeCardRequired { get; set; } // GblShadeCardCreationRequired
    public string Orientation { get; set; } = "Roll"; // PrePlannedSheetLabel or standard
    public string WastageType { get; set; } = "Machine Default"; // Machine Default, Percentage, Flat
    public double FlatWastageValue { get; set; } // Percentage or Flat Meters
    public long CategoryId { get; set; } // Category for wastage calculation

    // Grain Direction & Planning Options (NEWLY ADDED - Feb 16, 2026)
    public string GrainDirection { get; set; } = "Both"; // With Grain, Across Grain, Both (PlanPrintingGrain)
    public bool PlanInAvailableStock { get; set; } // ChkPlanInAvailableStock
    public bool PlanInSpecialSizePaper { get; set; } // Check_Plan_In_Special_Size
    public bool PlanInStandardSizePaper { get; set; } // Check_Plan_In_Standard_Size
    public bool PaperSuppliedByClient { get; set; } // CheckPaperByClient
    public bool BackToBackPastingRequired { get; set; } // ChkBackToBackPastingRequired

    // Plate & Layout Details (NEWLY ADDED - Feb 16, 2026)
    public double PlateBearer { get; set; } // PlanPlateBearer (Gbl_Plate_Bearer) - User can change during planning
    public double MakeReadyWastage { get; set; } // PlanMakeReadyWastage (in meters)
}

public class OperationCostDto
{
    public long ProcessId { get; set; }
    public double Rate { get; set; }
    public string RateType { get; set; } = "PerUnit"; // Legacy: TypeofCharges (e.g. PerUnit, PerHour, Fixed, Reference, etc.)
    public double MinimumCharges { get; set; }
    public double SetupCharges { get; set; }
    public double WastagePercent { get; set; } // ProcessWastagePercentage
    public double FlatWastage { get; set; } // ProcessFlatWastageValue
}
