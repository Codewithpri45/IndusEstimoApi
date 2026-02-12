
using System.ComponentModel.DataAnnotations;

namespace IndasEstimo.Application.DTOs.Masters;

public class CreateMachineDto
{
    [Required]
    [MaxLength(100)]
    public string MachineName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string MachineType { get; set; } = string.Empty;
    public int Colors { get; set; }
    public decimal MaxLength { get; set; }
    public decimal MaxWidth { get; set; }
    public decimal MinLength { get; set; }
    public decimal MinWidth { get; set; }
    public decimal PerHourRate { get; set; }
    public string CurrentStatus { get; set; } = "ACTIVE";
    
    // Child Tables
    public List<MachineSlabDto> Slabs { get; set; } = new();
    public List<MachineCoatingRateDto> CoatingRates { get; set; } = new();
    public List<long> AllocatedSubGroupIds { get; set; } = new();
}

public class UpdateMachineDto : CreateMachineDto
{
    [Required]
    public long MachineID { get; set; }
}

public class MachineDetailDto : UpdateMachineDto
{
    public bool IsActive { get; set; }
}

public class MachineSlabDto
{
    public long? SlabID { get; set; }
    public decimal RunningMeterRangeFrom { get; set; }
    public decimal RunningMeterRangeTo { get; set; }
    public decimal Rate { get; set; }
    public decimal Wastage { get; set; }
    public decimal PlateCharges { get; set; }
    public decimal MinCharges { get; set; }
    public string PaperGroup { get; set; } = string.Empty;
}

public class MachineCoatingRateDto
{
    public long? CoatingRateID { get; set; }
    public string CoatingName { get; set; } = string.Empty;
    public decimal SheetRangeFrom { get; set; }
    public decimal SheetRangeTo { get; set; }
    public string RateType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
