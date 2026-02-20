namespace IndasEstimo.Application.DTOs.Estimation;

/// <summary>
/// DTO for Machine Grid
/// </summary>
public class MachineGridDto
{
    public long MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public int MachineColors { get; set; }
    public decimal? MaxSheetL { get; set; }
    public decimal? MaxSheetW { get; set; }
    public decimal? MinSheetL { get; set; }
    public decimal? MinSheetW { get; set; }
    public decimal PerHourRate { get; set; }
    public string? PaperGroup { get; set; }
    public decimal MakeReadyTime { get; set; } // Legacy: 19
    public decimal JobChangeOverTime { get; set; } // Legacy: 20
    public decimal RollChangeOverTime { get; set; }
    public decimal Speed { get; set; } // Legacy: 24
    public decimal RollChangeWastage { get; set; } // Legacy: 34
    public decimal StandardRollLength { get; set; } // Legacy: 35
    public decimal MakeReadyWastageRunningMeter { get; set; } // Legacy: 30

    // Cylinder/Tool Details (Gap #2 - From LEFT JOIN with MachineToolAllocationMaster)
    // Legacy columns: 38-45 (ToolID, ToolCode, Manufecturer, CylinderWidth, NoOfTeeth, CircumferenceMM, etc.)
    public long CylinderToolID { get; set; } // Legacy column 38
    public string CylinderToolCode { get; set; } = string.Empty; // Legacy column 39
    public string CylinderToolName { get; set; } = string.Empty; // Tool Name
    public decimal CylinderCircumferenceMM { get; set; } // Legacy column 42
    public decimal CylinderCircumferenceInch { get; set; } // Legacy column 43
    public int CylinderNoOfTeeth { get; set; } // Legacy column 41
    public decimal CylinderWidth { get; set; } // Legacy column 40
    public decimal MinCircumferenceMM { get; set; } // Legacy column 28
    public decimal MaxCircumferenceMM { get; set; } // Legacy column 29
}

/// <summary>
/// DTO for Machine List
/// </summary>
public class MachineDto
{
    public long MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string MachineType { get; set; } = string.Empty;
    public int MachineColors { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for Process/Operation
/// </summary>
public class OperationDto
{
    public long ProcessID { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? PrePress { get; set; }
    public string? TypeofCharges { get; set; }
    public string? SizeToBeConsidered { get; set; }
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public string? IsDisplay { get; set; }
    public bool IsOnlineProcess { get; set; }
    public string? ChargeApplyOnSheets { get; set; }
    public string? DisplayProcessName { get; set; }
    public decimal Amount { get; set; }
    public string? RateFactor { get; set; }
    public string? AddRow { get; set; }
    public decimal MakeReadyTime { get; set; }
    public long MachineID { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal JobChangeOverTime { get; set; }
    public decimal MakeReadyPerHourCost { get; set; }
    public decimal MachinePerHourCost { get; set; }
    public int ToolRequired { get; set; }
    public string ProcessProductionType { get; set; } = "None";
    public int PaperConsumptionRequired { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
    public int SequenceNo { get; set; }
    public decimal FlatWastage { get; set; }
    public decimal WastagePercentage { get; set; }
    public string ProcessModuleType { get; set; } = "Universal";
    public decimal RollChangeOverTime { get; set; }
    public decimal ExecutionTime { get; set; }
    public decimal TotalExecutionTime { get; set; }
    public decimal MakeReadyMachineCost { get; set; }
    public decimal ExecutionCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal MaterialCost { get; set; }
    public int Pieces { get; set; } = 1;
    public int NoOfStitch { get; set; } = 1;
    public int NoOfLoops { get; set; } = 1;
    public int NoOfColors { get; set; } = 1;
    public int PagesPerSection { get; set; } = 1;
    public decimal NoOfForms { get; set; }
    public int PlateRequired { get; set; }
    public int NoOfFolds { get; set; } = 1;
    public string? PerHourCostingParameter { get; set; }
    public decimal MinimumQuantityToBeCharged { get; set; }
    public decimal PerHourCalculationQuantity { get; set; }
    public decimal AdditionalWeight { get; set; }
}

/// <summary>
/// DTO for Operation Slabs
/// </summary>
public class OperationSlabDto
{
    public long SlabID { get; set; }
    public long ProcessID { get; set; }
    public string? RateFactor { get; set; }
    public decimal FromValue { get; set; }
    public decimal ToValue { get; set; }
    public decimal Rate { get; set; }
}

/// <summary>
/// DTO for Machine Allocated Items
/// </summary>
public class MachineItemDto
{
    public long MachineID { get; set; }
    public long ItemGroupID { get; set; }
    public int? ItemGroupNameID { get; set; }
    public long? ItemSubGroupID { get; set; }
    public string? ItemGroupName { get; set; }
    public string? ItemSubGroupName { get; set; }
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal SizeL { get; set; }
    public decimal SizeW { get; set; }
    public decimal SizeH { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public decimal GSM { get; set; }
    public decimal Caliper { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public string? StockUnit { get; set; }
    public string? EstimationUnit { get; set; }
    public decimal PhysicalStock { get; set; }
    public decimal EstimationRate { get; set; }
    public string? PurchaseUnit { get; set; }
    public decimal PurchaseRate { get; set; }
}

/// <summary>
/// DTO for Category Wise Wastage Config
/// </summary>
public class CategoryWastageSettingDto
{
    public long CategoryID { get; set; }
    public string PrintingStyle { get; set; } = string.Empty; // 'Single Side', 'Both Side'
    public int NoOfColor { get; set; }
    public string Unit { get; set; } = string.Empty; // 'Sheets', 'Meter'
    public decimal FlatWastage { get; set; }
    public decimal WastagePercentage { get; set; }
    public string CalculationOn { get; set; } = string.Empty;
}
