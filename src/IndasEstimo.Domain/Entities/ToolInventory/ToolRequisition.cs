namespace IndasEstimo.Domain.Entities.ToolInventory;
/// <summary>
/// Tool Requisition header (ToolTransactionMain, VoucherID = -115)
/// </summary>
public class ToolRequisition
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
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
/// Tool Requisition line items (ToolTransactionDetail)
/// </summary>
public class ToolRequisitionDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? ToolNarration { get; set; }
    public decimal CurrentStockInStockUnit { get; set; }
    public decimal CurrentStockInPurchaseUnit { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long JobBookingID { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string? RefJobCardContentNo { get; set; }
    public long RequisitionTransactionID { get; set; }
    public long RequisitionToolID { get; set; }
    public int IsvoucherToolApproved { get; set; }
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
