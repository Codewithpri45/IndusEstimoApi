namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Response DTOs ====================

/// <summary>
/// Machine list item for main grid
/// </summary>
public class MachineListDto
{
    public int MachineID { get; set; }
    public string MachineCode { get; set; } = "";
    public string RefMachineCode { get; set; } = "";
    public string MachineName { get; set; } = "";
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = "";
    public string MachineType { get; set; } = "";
    public decimal MinimumSheet { get; set; }
    public decimal Gripper { get; set; }
    public decimal MaxLength { get; set; }
    public decimal MaxWidth { get; set; }
    public decimal MinLength { get; set; }
    public decimal MinWidth { get; set; }
    public decimal MaxPrintL { get; set; }
    public decimal MaxPrintW { get; set; }
    public decimal MinPrintL { get; set; }
    public decimal MinPrintW { get; set; }
    public int Colors { get; set; }
    public decimal MakeReadyCharges { get; set; }
    public decimal MakeReadyWastageSheet { get; set; }
    public decimal MakeReadyTime { get; set; }
    public decimal MakeReadyPerHourCost { get; set; }
    public decimal ElectricConsumption { get; set; }
    public decimal PrintingMargin { get; set; }
    public decimal WebCutOffSize { get; set; }
    public decimal MinReelSize { get; set; }
    public decimal MaxReelSize { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal LabourCharges { get; set; }
    public decimal WebCutOffSizeMin { get; set; }
    public string ChargesType { get; set; } = "";
    public int RoundofImpressionsWith { get; set; }
    public bool IsPerfectaMachine { get; set; }
    public bool IsVariableCutOff { get; set; }
    public bool IsSpecialMachine { get; set; }
    public bool IsPlanningMachine { get; set; }
    public decimal BasicPrintingCharges { get; set; }
    public decimal JobChangeOverTime { get; set; }
    public decimal PlateLength { get; set; }
    public decimal PlateWidth { get; set; }
    public decimal OtherCharges { get; set; }
    public string WastageType { get; set; } = "";
    public string WastageCalculationOn { get; set; } = "";
    public decimal PerHourCost { get; set; }
    public string ElectricConsumptionUnitPerMinute { get; set; } = "";
    public decimal MinRollWidth { get; set; }
    public decimal MaxRollWidth { get; set; }
    public decimal MinCircumference { get; set; }
    public decimal MaxCircumference { get; set; }
    public decimal MakeReadyWastageRunningMeter { get; set; }
    public decimal AvgBreakDownTime { get; set; }
    public decimal AvgBreakDownRunningMeters { get; set; }
    public decimal MachineWidth { get; set; }
    public decimal AverageRollChangeWastage { get; set; }
    public decimal AverageRollLength { get; set; }
    public decimal RollChangeTime { get; set; }
    public int? BranchID { get; set; }
    public string BranchName { get; set; } = "";
    public string SpeedUnit { get; set; } = "IMPRESSION";
    public decimal PlateCharges { get; set; }
    public string PlateChargesType { get; set; } = "";
    public string PerHourCostingParameter { get; set; } = "";
    public string MakeReadyTimeMode { get; set; } = "Flat";
    public string CompanyName { get; set; } = "";
    public int CompanyID { get; set; }
}

/// <summary>
/// Machine slab (rate tier) for a machine
/// </summary>
public class MachineSlabDto
{
    public decimal RunningMeterRangeFrom { get; set; }
    public decimal RunningMeterRangeTo { get; set; }
    public decimal ProcessWastagepercentage { get; set; }
    public decimal SheetRangeFrom { get; set; }
    public decimal SheetRangeTo { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal Rate { get; set; }
    public decimal PlateCharges { get; set; }
    public decimal PSPlateCharges { get; set; }
    public decimal CTCPPlateCharges { get; set; }
    public decimal Wastage { get; set; }
    public decimal SpecialColorFrontCharges { get; set; }
    public decimal SpecialColorBackCharges { get; set; }
    public string PaperGroup { get; set; } = "";
    public decimal SizeW { get; set; }
    public decimal SizeL { get; set; }
    public decimal MinCharges { get; set; }
}

/// <summary>
/// Online coating rate for a machine
/// </summary>
public class MachineOnlineCoatingRateDto
{
    public string CoatingName { get; set; } = "";
    public decimal SheetRangeFrom { get; set; }
    public decimal SheetRangeTo { get; set; }
    public string RateType { get; set; } = "";
    public decimal Rate { get; set; }
    public decimal BasicCoatingCharges { get; set; }
}

/// <summary>
/// Machine name dropdown item
/// </summary>
public class MachineNameDto
{
    public int MachineID { get; set; }
    public string MachineName { get; set; } = "";
}

/// <summary>
/// Department dropdown item for machine master
/// </summary>
public class MachineDepartmentDto
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
}

/// <summary>
/// Machine type item
/// </summary>
public class MachineTypeDto
{
    public string MachineTypeName { get; set; } = "";
    public string MachineMasterDisplayFieldsName { get; set; } = "";
}

/// <summary>
/// Item sub-group for group allocation
/// </summary>
public class MachineGroupAllocationDto
{
    public int ItemSubGroupID { get; set; }
    public string ItemSubGroupName { get; set; } = "";
}

/// <summary>
/// Allocated group IDs string
/// </summary>
public class MachineGroupAllocationResultDto
{
    public string GroupAllocationIDs { get; set; } = "";
}

/// <summary>
/// Tool item for tool allocation
/// </summary>
public class MachineToolDto
{
    public int ToolID { get; set; }
    public string ToolName { get; set; } = "";
    public decimal SizeW { get; set; }
    public decimal NoOfTeeth { get; set; }
    public decimal CircumferenceMM { get; set; }
    public decimal CircumferenceInch { get; set; }
    public decimal LPI { get; set; }
}

/// <summary>
/// Allocated tool IDs string
/// </summary>
public class MachineToolAllocationResultDto
{
    public string ToolAllocatedIDString { get; set; } = "";
}

/// <summary>
/// Coating name dropdown item
/// </summary>
public class CoatingNameDto
{
    public string CoatingName { get; set; } = "";
}

/// <summary>
/// Combined loaded data for editing a machine
/// </summary>
public class MachineLoadedDataDto
{
    public MachineListDto? MachineDetail { get; set; }
    public List<MachineSlabDto> Slabs { get; set; } = new();
    public List<MachineOnlineCoatingRateDto> CoatingRates { get; set; } = new();
    public string GroupAllocationIDs { get; set; } = "";
}

// ==================== Request DTOs ====================

/// <summary>
/// Machine detail record for save/update
/// </summary>
public class MachineDetailRecord
{
    public string MachineName { get; set; } = "";
    public int DepartmentID { get; set; }
    public string MachineType { get; set; } = "";
    public decimal MinimumSheet { get; set; }
    public decimal Gripper { get; set; }
    public decimal MaxLength { get; set; }
    public decimal MaxWidth { get; set; }
    public decimal MinLength { get; set; }
    public decimal MinWidth { get; set; }
    public decimal MaxPrintL { get; set; }
    public decimal MaxPrintW { get; set; }
    public decimal MinPrintL { get; set; }
    public decimal MinPrintW { get; set; }
    public int Colors { get; set; }
    public decimal MakeReadyCharges { get; set; }
    public decimal MakeReadyWastageSheet { get; set; }
    public decimal MakeReadyTime { get; set; }
    public decimal MakeReadyPerHourCost { get; set; }
    public decimal ElectricConsumption { get; set; }
    public decimal PrintingMargin { get; set; }
    public decimal WebCutOffSize { get; set; }
    public decimal MinReelSize { get; set; }
    public decimal MaxReelSize { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal LabourCharges { get; set; }
    public decimal WebCutOffSizeMin { get; set; }
    public string ChargesType { get; set; } = "";
    public int RoundofImpressionsWith { get; set; }
    public bool IsPerfectaMachine { get; set; }
    public bool IsVariableCutOff { get; set; }
    public bool IsSpecialMachine { get; set; }
    public bool IsPlanningMachine { get; set; }
    public decimal BasicPrintingCharges { get; set; }
    public decimal JobChangeOverTime { get; set; }
    public decimal PlateLength { get; set; }
    public decimal PlateWidth { get; set; }
    public decimal OtherCharges { get; set; }
    public string WastageType { get; set; } = "";
    public string WastageCalculationOn { get; set; } = "";
    public decimal PerHourCost { get; set; }
    public string ElectricConsumptionUnitPerMinute { get; set; } = "";
    public decimal MinRollWidth { get; set; }
    public decimal MaxRollWidth { get; set; }
    public decimal MinCircumference { get; set; }
    public decimal MaxCircumference { get; set; }
    public decimal MakeReadyWastageRunningMeter { get; set; }
    public decimal AvgBreakDownTime { get; set; }
    public decimal AvgBreakDownRunningMeters { get; set; }
    public decimal MachineWidth { get; set; }
    public decimal AverageRollChangeWastage { get; set; }
    public decimal AverageRollLength { get; set; }
    public decimal RollChangeTime { get; set; }
    public int? BranchID { get; set; }
    public string SpeedUnit { get; set; } = "IMPRESSION";
    public decimal PlateCharges { get; set; }
    public string PlateChargesType { get; set; } = "";
    public string PerHourCostingParameter { get; set; } = "";
    public string MakeReadyTimeMode { get; set; } = "Flat";
    public int ProductionUnitID { get; set; }
}

/// <summary>
/// Slab record for machine save/update
/// </summary>
public class MachineSlabRecord
{
    public decimal RunningMeterRangeFrom { get; set; }
    public decimal RunningMeterRangeTo { get; set; }
    public decimal ProcessWastagepercentage { get; set; }
    public decimal SheetRangeFrom { get; set; }
    public decimal SheetRangeTo { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal Rate { get; set; }
    public decimal PlateCharges { get; set; }
    public decimal PSPlateCharges { get; set; }
    public decimal CTCPPlateCharges { get; set; }
    public decimal Wastage { get; set; }
    public decimal SpecialColorFrontCharges { get; set; }
    public decimal SpecialColorBackCharges { get; set; }
    public string PaperGroup { get; set; } = "";
    public decimal SizeW { get; set; }
    public decimal SizeL { get; set; }
    public decimal MinCharges { get; set; }
}

/// <summary>
/// Online coating rate record for machine save/update
/// </summary>
public class MachineCoatingRateRecord
{
    public string CoatingName { get; set; } = "";
    public decimal SheetRangeFrom { get; set; }
    public decimal SheetRangeTo { get; set; }
    public string RateType { get; set; } = "";
    public decimal Rate { get; set; }
    public decimal BasicCoatingCharges { get; set; }
}

/// <summary>
/// Group allocation record for machine
/// </summary>
public class MachineGroupAllocationRecord
{
    public int ItemSubGroupID { get; set; }
}

/// <summary>
/// Tool allocation record for machine
/// </summary>
public class MachineToolAllocationRecord
{
    public int ToolID { get; set; }
}

/// <summary>
/// Request for saving a new machine
/// </summary>
public class SaveMachineRequest
{
    public MachineDetailRecord MachineDetail { get; set; } = new();
    public List<MachineSlabRecord> Slabs { get; set; } = new();
    public List<MachineCoatingRateRecord> CoatingRates { get; set; } = new();
}

/// <summary>
/// Request for updating an existing machine
/// </summary>
public class UpdateMachineRequest
{
    public int MachineID { get; set; }
    public MachineDetailRecord MachineDetail { get; set; } = new();
    public List<MachineSlabRecord> Slabs { get; set; } = new();
    public List<MachineCoatingRateRecord> CoatingRates { get; set; } = new();
}

/// <summary>
/// Request for saving group allocation for a machine
/// </summary>
public class SaveMachineGroupAllocationRequest
{
    public int MachineID { get; set; }
    public List<MachineGroupAllocationRecord> GroupAllocations { get; set; } = new();
    public string GroupAllocationIDString { get; set; } = "";
}

/// <summary>
/// Request for saving tool allocation for a machine
/// </summary>
public class SaveMachineToolAllocationRequest
{
    public int MachineID { get; set; }
    public int ToolGroupID { get; set; }
    public List<MachineToolAllocationRecord> ToolAllocations { get; set; } = new();
    public string ToolAllocatedIDString { get; set; } = "";
}
