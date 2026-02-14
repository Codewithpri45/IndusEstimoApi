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
