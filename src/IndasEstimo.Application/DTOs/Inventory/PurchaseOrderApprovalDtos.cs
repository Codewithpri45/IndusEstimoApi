namespace IndasEstimo.Application.DTOs.Inventory;

// ==================== Request DTOs ====================

/// <summary>
/// Request for getting PO lists (Unapproved/Approved/Cancelled)
/// </summary>
public class GetPurchaseOrderApprovalListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

/// <summary>
/// Request for approving purchase orders
/// </summary>
public class ApprovePurchaseOrderRequest
{
    public List<PurchaseOrderApprovalItem> PurchaseOrderItems { get; set; } = new();
}

public class PurchaseOrderApprovalItem
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long TransID { get; set; }
}

/// <summary>
/// Request for cancelling purchase orders
/// </summary>
public class CancelPurchaseOrderRequest
{
    public List<PurchaseOrderCancellationItem> PurchaseOrderItems { get; set; } = new();
}

public class PurchaseOrderCancellationItem
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long TransID { get; set; }
}

// ==================== Response DTOs ====================

/// <summary>
/// Unapproved PO item - matches UnApprovedPurchaseOrders WebMethod
/// </summary>
public class UnapprovedPurchaseOrderDto
{
    public long TransactionID { get; set; }
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public int TotalItems { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal GSTTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public long ReceiptTransactionID { get; set; }
    public int PurchaseDivision { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public long AuditApprovedBy { get; set; }
    public long DealerID { get; set; }
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public int ModeOfTransport { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string TermsOfDelivery { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal AfterDisAmt { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string LastPODate { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
}

/// <summary>
/// Approved PO item - matches ApprovedPurchaseOrders WebMethod
/// </summary>
public class ApprovedPurchaseOrderDto
{
    public long TransactionID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public int TotalItems { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal GSTTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalDate { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public int PurchaseDivision { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public long AuditApprovedBy { get; set; }
    public long DealerID { get; set; }
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public int ModeOfTransport { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string TermsOfDelivery { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal AfterDisAmt { get; set; }
    public string Narration { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
}

/// <summary>
/// Cancelled PO item - matches CancelledPurchaseOrders WebMethod
/// </summary>
public class CancelledPurchaseOrderDto
{
    public long TransactionID { get; set; }
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public int TotalItems { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal GSTTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalDate { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public int PurchaseDivision { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public long AuditApprovedBy { get; set; }
    public long DealerID { get; set; }
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public int ModeOfTransport { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string TermsOfDelivery { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal AfterDisAmt { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string LastPODate { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
}
