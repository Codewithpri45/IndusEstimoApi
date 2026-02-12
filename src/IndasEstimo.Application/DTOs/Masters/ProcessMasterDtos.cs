
using System.ComponentModel.DataAnnotations;

namespace IndasEstimo.Application.DTOs.Masters;

public class CreateProcessDto
{
    [Required]
    [MaxLength(200)]
    public string ProcessName { get; set; } = string.Empty;
    public string DisplayProcessName { get; set; } = string.Empty;
    public string TypeofCharges { get; set; } = string.Empty;
    public string SizeToBeConsidered { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public decimal SetupCharges { get; set; }
    public string StartUnit { get; set; } = string.Empty;
    public string EndUnit { get; set; } = string.Empty;
    public bool IsDisplay { get; set; }
    public bool ToolRequired { get; set; }
    public bool IsEditToBeProduceQty { get; set; }
    public string ProcessProductionType { get; set; } = "None";
    public string ChargeApplyOnSheets { get; set; } = string.Empty;
    public string PrePress { get; set; } = string.Empty;
    public string UnitConversion { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
    public string AllocattedMachineID { get; set; } = string.Empty; // Comma separated IDs or just single ID? Legacy updates column AllocattedMachineID in ProcessMaster
    public string AllocatedContentID { get; set; } = string.Empty; // Comma separated IDs
    public string ProcessPurpose { get; set; } = string.Empty;
    public bool IsOnlineProcess { get; set; }
    public string ProcessModuleType { get; set; } = string.Empty;
    public decimal MinimumQuantityToBeCharged { get; set; }
    public decimal ProcessFlatWastageValue { get; set; }
    public decimal ProcessWastagePercentage { get; set; }
    public string ProcessCategory { get; set; } = string.Empty;
    public long? ProductionUnitID { get; set; }

    // Child Lists
    public List<ProcessToolAllocationDto> Tools { get; set; } = new();
    public List<ProcessMachineAllocationDto> Machines { get; set; } = new();
    public List<ProcessMaterialAllocationDto> Materials { get; set; } = new();
    public List<ProcessSlabDto> Slabs { get; set; } = new();
    public List<ProcessInspectionParamDto> InspectionParams { get; set; } = new();
    public List<ProcessLineClearanceParamDto> LineClearanceParams { get; set; } = new();
}

public class UpdateProcessDto : CreateProcessDto
{
    [Required]
    public long ProcessID { get; set; }
}

public class ProcessDetailDto : UpdateProcessDto
{
}

public class ProcessToolAllocationDto
{
    public long ToolGroupID { get; set; }
}

public class ProcessMachineAllocationDto
{
    public long MachineID { get; set; }
}

public class ProcessMaterialAllocationDto
{
    public long ItemSubGroupID { get; set; }
}

public class ProcessSlabDto
{
    public decimal FromQty { get; set; }
    public decimal ToQty { get; set; }
    public string StartUnit { get; set; } = string.Empty;
    public string RateFactor { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal MinimumCharges { get; set; }
    public bool IsLocked { get; set; }
}

public class ProcessInspectionParamDto
{
    public int SequenceNo { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string StandardValue { get; set; } = string.Empty;
    public string InputFieldType { get; set; } = string.Empty; // e.g. Text, Number
    public string FieldDataType { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
}

public class ProcessLineClearanceParamDto
{
    public int SequenceNo { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string StandardValue { get; set; } = string.Empty;
    public string InputFieldType { get; set; } = string.Empty;
    public string FieldDataType { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
}
