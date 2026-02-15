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
    public string? TypeofCharges { get; set; }
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public bool IsOnlineProcess { get; set; }
    public int SequenceNo { get; set; }
    public long? DepartmentID { get; set; }
    public decimal WastagePercentage { get; set; }
    public decimal FlatWastage { get; set; }
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
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public long? ItemGroupID { get; set; }
    public string? ItemGroupName { get; set; }
    public decimal EstimationRate { get; set; }
    public string? EstimationUnit { get; set; }
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
