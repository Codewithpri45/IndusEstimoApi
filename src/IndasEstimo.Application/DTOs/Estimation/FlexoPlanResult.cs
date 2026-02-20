namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// Detailed result of a Flexo Estimation Plan calculation.
/// Corresponds to TblPlanning rows in legacy code.
/// </summary>
public class FlexoPlanResult
{
    public long PaperId { get; set; }
    public string PaperName { get; set; }
    public double PaperWidth { get; set; } // RollWidth
    public double PaperLength { get; set; } // Running Meter
    public double CutSizeW { get; set; } // Effective Width Used
    public double CutSizeH { get; set; } // Effective Repeat Length

    // Paper Breakdown (Gap #10)
    public decimal PaperFaceGSM { get; set; }
    public decimal PaperReleaseGSM { get; set; }
    public decimal PaperAdhesiveGSM { get; set; }
    public string PaperMill { get; set; } = string.Empty; // Manufacturer
    public string PaperQuality { get; set; } = string.Empty;
    public string GrainDirection { get; set; } = string.Empty; // Gap #9: "With Grain" or "Across Grain"

    // Layout
    public int UpsAcross { get; set; } // Gbl_UPS_H - Across roll width (uses JobSizeH dimension)
    public int UpsAround { get; set; } // Gbl_UPS_L - Around cylinder (uses JobSizeL dimension)
    public int TotalUps { get; set; } // UpsAcross * UpsAround
    public double CylinderCircumference { get; set; } // Crucial for Flexo
    public int CylinderTeeth { get; set; }

    // Gap & Wastage Strip (Gap #8)
    public double AcrossGap { get; set; } // Gap between labels across
    public double AroundGap { get; set; } // Gap between labels around
    public double WastageStrip { get; set; } // Width wastage in strip 

    // Quantities
    public double TotalPaperWeightKg { get; set; }
    public double CalculatedSheets { get; set; } // For Sheet conversion if needed
    public double TotalQuantity { get; set; } // Final Output Qty

    // Running Meter & Square Meter Calculations (Gap #7 - Legacy Lines 16445-16448)
    public double RequiredRunningMeter { get; set; }
    public double TotalRunningMeter { get; set; }
    public double RequiredSquareMeter { get; set; }
    public double TotalSquareMeter { get; set; }
    public double WastageSquareMeter { get; set; }
    public double ScrapSquareMeter { get; set; }

    // Wastage Breakdown
    public double MakeReadyWastageMeters { get; set; }
    public double ProcessWastagePercent { get; set; }
    public double ProcessWastageMeters { get; set; }
    public double RollChangeWastageMeters { get; set; }
    public double TotalWastageKg { get; set; }

    // Cost Breakdown (Per 1000 Units & Total)
    public double PaperCostTotal { get; set; }
    public double PaperCostPer1000 { get; set; }
    
    public double MachineRunCostTotal { get; set; }
    public double MachineRunCostPer1000 { get; set; }
    public double TotalExecutionTime { get; set; } // Total Time in Minutes
    
    public double PlateCostTotal { get; set; }
    public double PlateCostPer1000 { get; set; }

    public double MaterialCostTotal { get; set; } // Inks, Chemicals, Layers
    public double MaterialCostPer1000 { get; set; }

    public double ConversionCostTotal { get; set; } // Labor + Overhead
    public double ConversionCostPer1000 { get; set; }

    public double WastageCostTotal { get; set; }
    public double WastageCostPer1000 { get; set; }

    // Final Pricing
    public double ProfitMargin { get; set; } // Percentage
    public double ProfitAmount { get; set; }
    
    public double TotalCost { get; set; }
    public double UnitPrice1000 { get; set; } // Cost per 1000 units
    public double UnitPrice { get; set; } // Cost per single unit

    // Printing Impressions (Gap #11)
    public double PrintingImpressions { get; set; } // Total impressions
    public double ImpressionsToBeCharged { get; set; } // Billable impressions

    // Metadata (Gap #14)
    public string MachineName { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty; // Cylinder used
    public string PlanType { get; set; } = string.Empty; // Sheet/Roll based
    public string PrintingStyle { get; set; } = string.Empty; // Single Side/Both Side
    public string OutputFormat { get; set; } = string.Empty; // Roll/Sheet
    public int FrontColors { get; set; }
    public int BackColors { get; set; }
    public List<string> Warnings { get; set; } = new(); // Any plan errors
}
