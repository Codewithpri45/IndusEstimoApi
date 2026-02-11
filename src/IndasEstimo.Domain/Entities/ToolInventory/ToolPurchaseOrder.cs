namespace IndasEstimo.Domain.Entities.ToolInventory;

/// <summary>
/// Tool Purchase Order header (ToolTransactionMain, VoucherID = -117)
/// </summary>
public class ToolPurchaseOrder
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long LedgerID { get; set; }
    public long ContactPersonID { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalBasicAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalOverheadAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? PurchaseDivision { get; set; }
    public string? PurchaseReferenceRemark { get; set; }
    public string? Narration { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? TermsOfPayment { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string? ModeOfTransport { get; set; }
    public string? DealerID { get; set; }
    public long VoucherApprovalByEmployeeID { get; set; }
    public string? AmountInWords { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Tool Purchase Order line items (ToolTransactionDetail)
/// </summary>
public class ToolPurchaseOrderDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long ProductHSNID { get; set; }
    public int IsVoucherToolApproved { get; set; }
    public long VoucherToolApprovedBy { get; set; }
    public int IsCancelled { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Tool PO tax breakdown (ToolPurchaseOrderTaxes)
/// </summary>
public class ToolPurchaseOrderTax
{
    public long POTaxID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long LedgerID { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxInAmount { get; set; }
    public bool IsComulative { get; set; }
    public bool GSTApplicable { get; set; }
    public string? CalculatedON { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Tool PO overhead charges (ToolPurchaseOverheadCharges)
/// </summary>
public class ToolPurchaseOrderOverhead
{
    public long OverheadID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long HeadID { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string ChargesType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Tool PO requisition linkage (ToolPurchaseRequisitionDetail)
/// </summary>
public class ToolPurchaseOrderRequisition
{
    public long RequisitionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public decimal RequisitionProcessQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long RequisitionTransactionID { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
