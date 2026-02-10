namespace IndasEstimo.Domain.Entities.Inventory;
/// <summary>
/// Main Purchase Order header (ItemTransactionMain)
/// </summary>
public class PurchaseOrder
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
    public decimal TotalCGSTTaxAmount { get; set; }
    public decimal TotalSGSTTaxAmount { get; set; }
    public decimal TotalIGSTTaxAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalOverheadAmount { get; set; }
    public int PurchaseDivision { get; set; }
    public string? PurchaseReferenceRemark { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? TermsOfPayment { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public int ModeOfTransport { get; set; }
    public long DealerID { get; set; }
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
/// Purchase Order line items (ItemTransactionDetail)
/// </summary>
public class PurchaseOrderDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public int TransID { get; set; }
    public long ItemGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal RequiredNoOfPacks { get; set; }
    public decimal QuantityPerPack { get; set; }
    public decimal PurchaseOrderQuantity { get; set; }
    public decimal ChallanWeight { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
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
    public string? ItemNarration { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string? RefJobCardContentNo { get; set; }
    public long ClientID { get; set; }
    public string? Remark { get; set; }
    public long ProductHSNID { get; set; }
    public int IsVoucherItemApproved { get; set; }
    public long VoucherItemApprovedBy { get; set; }
    public DateTime? VoucherItemApprovedDate { get; set; }

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
/// Tax breakdown (ItemPurchaseOrderTaxes)
/// </summary>
public class PurchaseOrderTax
{
    public long POTaxID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long LedgerID { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal Amount { get; set; }
    public bool TaxInAmount { get; set; }
    public bool IsComulative { get; set; }
    public bool GSTApplicable { get; set; }
    public string CalculatedON { get; set; } = string.Empty;

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
/// Delivery schedule (ItemPurchaseDeliverySchedule)
/// </summary>
public class PurchaseOrderSchedule
{
    public long ScheduleID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ItemID { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime ScheduleDeliveryDate { get; set; }

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
/// Overhead charges (ItemPurchaseOverheadCharges)
/// </summary>
public class PurchaseOrderOverhead
{
    public long OverheadID { get; set; }
    public long TransactionID { get; set; }
    public long HeadID { get; set; }
    public int TransID { get; set; }
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
/// Requisition updates (ItemPurchaseRequisitionDetail)
/// </summary>
public class PurchaseOrderRequisition
{
    public long RequisitionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ItemID { get; set; }
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
/*
/// <summary>
/// Attachments (ItemTransactionAttachments)
/// </summary>
public class PurchaseOrderAttachment
{
    public long AttachmentID { get; set; }
    public long TransactionID { get; set; }
    public string AttachmentFilesName { get; set; } = string.Empty;
    public string? AttachedFileRemark { get; set; }

    // Audit fields
    public long CompanyID { get; set; }
    public long ProductionUnitID { get; set; }
    public long UserID { get; set; }
    public long CreatedBy { get; set; }
    public long ModifiedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
*/