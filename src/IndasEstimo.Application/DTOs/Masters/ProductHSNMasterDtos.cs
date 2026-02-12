
using System.ComponentModel.DataAnnotations;

namespace IndasEstimo.Application.DTOs.Masters;

public class CreateProductHSNDto
{
    [Required]
    [MaxLength(200)]
    public string ProductHSNName { get; set; } = string.Empty;
    public string HSNCode { get; set; } = string.Empty;
    public long? UnderProductHSNID { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TariffNo { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public decimal GSTTaxPercentage { get; set; }
    public decimal VATTaxPercentage { get; set; }
    public decimal ExciseTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
    public long? ItemGroupID { get; set; }
    public bool IsServiceHSN { get; set; }
    public bool IsExciseApplicable { get; set; }
    public long? ProductionUnitID { get; set; }
}

public class UpdateProductHSNDto : CreateProductHSNDto
{
    [Required]
    public long ProductHSNID { get; set; }
}

public class ProductHSNDetailDto : UpdateProductHSNDto
{
    // Additional fields for display if needed
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
