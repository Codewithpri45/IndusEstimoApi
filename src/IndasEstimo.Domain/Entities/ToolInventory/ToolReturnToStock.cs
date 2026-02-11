namespace IndasEstimo.Domain.Entities.ToolInventory;

/// <summary>
/// Tool Return to Stock header (ToolTransactionMain, VoucherID = -44)
/// Returns issued tools from jobs back to warehouse stock
/// </summary>
public class ToolReturnToStock
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long LedgerID { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
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
/// Tool Return to Stock line items (ToolTransactionDetail)
/// </summary>
public class ToolReturnToStockDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string? JobCardFormNo { get; set; }
    public long ItemID { get; set; }
    public decimal ReceiptQuantity { get; set; }
    public string? BatchNo { get; set; }
    public long WarehouseID { get; set; }
    public long IssueTransactionID { get; set; }
    public string? ToolNarration { get; set; }

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
