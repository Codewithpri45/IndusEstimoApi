namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Response DTOs ====================

/// <summary>
/// Process list item for main grid
/// </summary>
public class ProcessListDto
{
    public int ProcessID { get; set; }
    public string ProcessName { get; set; } = "";
    public string DisplayProcessName { get; set; } = "";
    public string TypeofCharges { get; set; } = "";
    public string SizeToBeConsidered { get; set; } = "";
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public bool IsDisplay { get; set; }
    public bool ToolRequired { get; set; }
    public string DepartmentName { get; set; } = "";
    public string StartUnit { get; set; } = "";
    public string EndUnit { get; set; } = "";
    public string UnitConversion { get; set; } = "";
    public string PrePress { get; set; } = "";
    public string ProcessModuleType { get; set; } = "";
    public bool IsOnlineProcess { get; set; }
    public string ProcessCategory { get; set; } = "";
}

/// <summary>
/// Full process detail for editing
/// </summary>
public class ProcessDetailDto
{
    public int ProcessID { get; set; }
    public string ProcessName { get; set; } = "";
    public string DisplayProcessName { get; set; } = "";
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
    public string TypeofCharges { get; set; } = "";
    public string ChargeApplyOnSheets { get; set; } = "";
    public string SizeToBeConsidered { get; set; } = "";
    public string PrePress { get; set; } = "";
    public string StartUnit { get; set; } = "";
    public string EndUnit { get; set; } = "";
    public string UnitConversion { get; set; } = "";
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public bool IsDisplay { get; set; }
    public bool IsEditToBeProduceQty { get; set; }
    public decimal Rate { get; set; }
    public string ProcessProductionType { get; set; } = "";
    public string ProcessPurpose { get; set; } = "";
    public bool IsOnlineProcess { get; set; }
    public string ProcessModuleType { get; set; } = "";
    public decimal MinimumQuantityToBeCharged { get; set; }
    public decimal ProcessFlatWastageValue { get; set; }
    public decimal ProcessWastagePercentage { get; set; }
    public string ProcessCategory { get; set; } = "";
    public string PerHourCostingParameter { get; set; } = "";
    public bool ToolRequired { get; set; }
}

/// <summary>
/// Machine allocation for a process
/// </summary>
public class ProcessMachineAllocationDto
{
    public int MachineID { get; set; }
    public string MachineName { get; set; } = "";
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
    public decimal MachineSpeed { get; set; }
    public decimal MakeReadyTime { get; set; }
    public decimal JobChangeOverTime { get; set; }
    public bool IsDefaultMachine { get; set; }
}

/// <summary>
/// Material allocation for a process
/// </summary>
public class ProcessMaterialAllocationDto
{
    public int ItemID { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemGroupName { get; set; } = "";
    public string ItemSubGroupName { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string StockUnit { get; set; } = "";
}

/// <summary>
/// Slab rate for a process
/// </summary>
public class ProcessSlabDto
{
    public decimal FromQty { get; set; }
    public decimal ToQty { get; set; }
    public string StartUnit { get; set; } = "";
    public string RateFactor { get; set; } = "";
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public bool IsLocked { get; set; }
}

/// <summary>
/// Tool group for a process
/// </summary>
public class ProcessToolGroupDto
{
    public int ToolGroupID { get; set; }
    public string ToolGroupName { get; set; } = "";
}

/// <summary>
/// Inspection parameter for a process
/// </summary>
public class ProcessInspectionParameterDto
{
    public int ProcessInspectionParameterID { get; set; }
    public string ParameterName { get; set; } = "";
    public string StandardValue { get; set; } = "";
    public string InputFieldType { get; set; } = "";
    public string FieldDataType { get; set; } = "";
    public string DefaultValue { get; set; } = "";
}

/// <summary>
/// Line clearance parameter for a process
/// </summary>
public class ProcessLineClearanceParameterDto
{
    public int LineClearanceParameterID { get; set; }
    public string ParameterName { get; set; } = "";
    public string StandardValue { get; set; } = "";
    public string InputFieldType { get; set; } = "";
    public string FieldDataType { get; set; } = "";
    public string DefaultValue { get; set; } = "";
}

/// <summary>
/// Content allocation for a process
/// </summary>
public class ProcessContentAllocationDto
{
    public int ContentID { get; set; }
    public string ContentName { get; set; } = "";
    public string ContentCaption { get; set; } = "";
}

/// <summary>
/// Combined loaded data for editing a process
/// </summary>
public class ProcessLoadedDataDto
{
    public ProcessDetailDto ProcessDetail { get; set; } = new();
    public List<ProcessMachineAllocationDto> MachineAllocations { get; set; } = new();
    public List<ProcessMaterialAllocationDto> MaterialAllocations { get; set; } = new();
    public List<ProcessSlabDto> Slabs { get; set; } = new();
    public List<ProcessToolGroupDto> ToolGroups { get; set; } = new();
    public List<ProcessInspectionParameterDto> InspectionParameters { get; set; } = new();
    public List<ProcessLineClearanceParameterDto> LineClearanceParameters { get; set; } = new();
    public List<ProcessContentAllocationDto> ContentAllocations { get; set; } = new();
}

/// <summary>
/// Department dropdown item
/// </summary>
public class ProcessDepartmentDto
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = "";
}

/// <summary>
/// Type of charges dropdown item
/// </summary>
public class TypeOfChargesDto
{
    public int TypeOfChargesID { get; set; }
    public string TypeOfChargesName { get; set; } = "";
}

/// <summary>
/// Unit dropdown item
/// </summary>
public class UnitDto
{
    public string UnitName { get; set; } = "";
    public string UnitSymbol { get; set; } = "";
}

/// <summary>
/// Process name item for dropdown
/// </summary>
public class ProcessNameDto
{
    public int ProcessID { get; set; }
    public string ProcessName { get; set; } = "";
}

// ==================== Request DTOs ====================

/// <summary>
/// Request for saving a new process
/// </summary>
public class SaveProcessRequest
{
    public ProcessDetailRecord ProcessDetail { get; set; } = new();
    public List<MachineAllocationRecord> MachineAllocations { get; set; } = new();
    public List<MaterialAllocationRecord> MaterialAllocations { get; set; } = new();
    public List<ContentAllocationRecord> ContentAllocations { get; set; } = new();
    public List<SlabRecord> Slabs { get; set; } = new();
    public List<InspectionParameterRecord> InspectionParameters { get; set; } = new();
    public List<LineClearanceParameterRecord> LineClearanceParameters { get; set; } = new();
    public List<ToolGroupRecord> ToolGroups { get; set; } = new();
}

/// <summary>
/// Request for updating an existing process
/// </summary>
public class UpdateProcessRequest
{
    public int ProcessID { get; set; }
    public ProcessDetailRecord ProcessDetail { get; set; } = new();
    public List<MachineAllocationRecord> MachineAllocations { get; set; } = new();
    public List<MaterialAllocationRecord> MaterialAllocations { get; set; } = new();
    public List<ContentAllocationRecord> ContentAllocations { get; set; } = new();
    public List<SlabRecord> Slabs { get; set; } = new();
    public List<InspectionParameterRecord> InspectionParameters { get; set; } = new();
    public List<LineClearanceParameterRecord> LineClearanceParameters { get; set; } = new();
    public List<ToolGroupRecord> ToolGroups { get; set; } = new();
}

/// <summary>
/// Process detail record for save/update
/// </summary>
public class ProcessDetailRecord
{
    public string ProcessName { get; set; } = "";
    public string DisplayProcessName { get; set; } = "";
    public int DepartmentID { get; set; }
    public string TypeofCharges { get; set; } = "";
    public string ChargeApplyOnSheets { get; set; } = "";
    public string SizeToBeConsidered { get; set; } = "";
    public string PrePress { get; set; } = "";
    public string StartUnit { get; set; } = "";
    public string EndUnit { get; set; } = "";
    public string UnitConversion { get; set; } = "";
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public bool IsDisplay { get; set; }
    public bool IsEditToBeProduceQty { get; set; }
    public decimal Rate { get; set; }
    public string ProcessProductionType { get; set; } = "";
    public string ProcessPurpose { get; set; } = "";
    public bool IsOnlineProcess { get; set; }
    public string ProcessModuleType { get; set; } = "";
    public decimal MinimumQuantityToBeCharged { get; set; }
    public decimal ProcessFlatWastageValue { get; set; }
    public decimal ProcessWastagePercentage { get; set; }
    public string ProcessCategory { get; set; } = "";
    public string PerHourCostingParameter { get; set; } = "";
    public bool ToolRequired { get; set; }
}

/// <summary>
/// Machine allocation record for save/update
/// </summary>
public class MachineAllocationRecord
{
    public int MachineID { get; set; }
    public decimal MachineSpeed { get; set; }
    public decimal MakeReadyTime { get; set; }
    public decimal JobChangeOverTime { get; set; }
    public bool IsDefaultMachine { get; set; }
}

/// <summary>
/// Material allocation record for save/update
/// </summary>
public class MaterialAllocationRecord
{
    public int ItemID { get; set; }
}

/// <summary>
/// Content allocation record for save/update
/// </summary>
public class ContentAllocationRecord
{
    public int ContentID { get; set; }
}

/// <summary>
/// Slab record for save/update
/// </summary>
public class SlabRecord
{
    public decimal FromQty { get; set; }
    public decimal ToQty { get; set; }
    public string StartUnit { get; set; } = "";
    public string RateFactor { get; set; } = "";
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public bool IsLocked { get; set; }
}

/// <summary>
/// Inspection parameter record for save/update
/// </summary>
public class InspectionParameterRecord
{
    public int DepartmentID { get; set; }
    public int SequenceNo { get; set; }
    public string ParameterName { get; set; } = "";
    public string StandardValue { get; set; } = "";
    public string InputFieldType { get; set; } = "";
    public string FieldDataType { get; set; } = "";
    public string DefaultValue { get; set; } = "";
}

/// <summary>
/// Line clearance parameter record for save/update
/// </summary>
public class LineClearanceParameterRecord
{
    public int DepartmentID { get; set; }
    public int SequenceNo { get; set; }
    public string ParameterName { get; set; } = "";
    public string StandardValue { get; set; } = "";
    public string InputFieldType { get; set; } = "";
    public string FieldDataType { get; set; } = "";
    public string DefaultValue { get; set; } = "";
}

/// <summary>
/// Tool group record for save/update
/// </summary>
public class ToolGroupRecord
{
    public int ToolGroupID { get; set; }
}
