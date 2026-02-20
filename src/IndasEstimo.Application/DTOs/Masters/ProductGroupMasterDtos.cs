namespace IndasEstimo.Application.DTOs.Masters;

// ==================== Response DTOs ====================

/// <summary>
/// Product HSN group list item for main grid
/// Old VB method: Showlist()
/// </summary>
public class ProductGroupListDto
{
    public int ProductHSNID { get; set; }
    public string ProductHSNName { get; set; } = "";
    public string HSNCode { get; set; } = "";
    public int UnderProductHSNID { get; set; }
    public string DisplayName { get; set; } = "";
    public string TariffNo { get; set; } = "";
    public string ProductCategory { get; set; } = "";
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
    public decimal ExciseTaxPercentage { get; set; }
    public int ItemGroupID { get; set; }
    public string CreatedBy { get; set; } = "";
    public string FYear { get; set; } = "";
    public string CreatedDate { get; set; } = "";
    public bool IsServiceHSN { get; set; }
    public bool IsExciseApplicable { get; set; }
}

/// <summary>
/// HSN dropdown item for UnderGroup (parent group) dropdown
/// Old VB method: UnderGroup()
/// </summary>
public class ProductHSNDropdownDto
{
    public int ProductHSNID { get; set; }
    public string ProductHSNName { get; set; } = "";
}

/// <summary>
/// Item group dropdown item
/// Old VB method: SelItemGroupName()
/// </summary>
public class ItemGroupDropdownDto
{
    public int ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = "";
}

/// <summary>
/// Tax type check result
/// Old VB method: CheckTaxType()
/// </summary>
public class TaxTypeDto
{
    public int IsVatApplicable { get; set; }
}

// ==================== Request DTOs ====================

/// <summary>
/// Request for saving a new product group / HSN
/// Old VB method: SavePGHMData()
/// </summary>
public class SaveProductGroupRequest
{
    public string ProductHSNName { get; set; } = "";
    public string HSNCode { get; set; } = "";
    public int UnderProductHSNID { get; set; }
    public string DisplayName { get; set; } = "";
    public string TariffNo { get; set; } = "";
    public string ProductCategory { get; set; } = "";
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
    public decimal ExciseTaxPercentage { get; set; }
    public int ItemGroupID { get; set; }
    public bool IsServiceHSN { get; set; }
    public bool IsExciseApplicable { get; set; }
}

/// <summary>
/// Request for updating an existing product group / HSN
/// Old VB method: UpdatePGHM()
/// </summary>
public class UpdateProductGroupRequest
{
    public int ProductHSNID { get; set; }
    public string ProductHSNName { get; set; } = "";
    public string HSNCode { get; set; } = "";
    public int UnderProductHSNID { get; set; }
    public string DisplayName { get; set; } = "";
    public string TariffNo { get; set; } = "";
    public string ProductCategory { get; set; } = "";
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
    public decimal ExciseTaxPercentage { get; set; }
    public int ItemGroupID { get; set; }
    public bool IsServiceHSN { get; set; }
    public bool IsExciseApplicable { get; set; }
}
