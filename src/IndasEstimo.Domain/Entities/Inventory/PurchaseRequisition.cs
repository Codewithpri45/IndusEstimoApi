namespace IndasEstimo.Domain.Entities.Inventory;

/// <summary>
/// Main Purchase Requisition header (ItemTransactionMain)
/// </summary>
public class PurchaseRequisition
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public DateTime VoucherDate { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? Narration { get; set; }

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
/// Purchase Requisition line items (ItemTransactionDetail)
/// </summary>
public class PurchaseRequisitionDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public int TransID { get; set; }
    public long ItemGroupID { get; set; }
    public decimal RequiredNoOfPacks { get; set; }
    public decimal QuantityPerPack { get; set; }
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string? ItemNarration { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string? RefJobCardContentNo { get; set; }
    public decimal CurrentStockInStockUnit { get; set; }
    public decimal CurrentStockInPurchaseUnit { get; set; }
    public int IsAuditApproved { get; set; }
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
/// Indent detail updates for Requisition
/// </summary>
public class PurchaseRequisitionIndentUpdate
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long RequisitionItemID { get; set; }
}
