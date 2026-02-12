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

    // Layout
    public int UpsAcross { get; set; } // Gbl_UPS_L
    public int UpsAround { get; set; } // Gbl_UPS_H (per Cylinder)
    public int TotalUps { get; set; } // UpsAcross * UpsAround
    public double CylinderCircumference { get; set; } // Crucial for Flexo
    public int CylinderTeeth { get; set; } 

    // Quantities
    public double TotalPaperWeightKg { get; set; }
    public double CalculatedSheets { get; set; } // For Sheet conversion if needed
    public double TotalQuantity { get; set; } // Final Output Qty

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

    // Metadata
    public string MachineName { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty; // Cylinder used
    public List<string> Warnings { get; set; } = new(); // Any plan errors
}
