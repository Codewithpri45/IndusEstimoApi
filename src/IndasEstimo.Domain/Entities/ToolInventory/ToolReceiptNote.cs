namespace IndasEstimo.Domain.Entities.ToolInventory;

/// <summary>
/// Tool Receipt Note / GRN header (ToolTransactionMain, VoucherID = -116)
/// </summary>
public class ToolReceiptNote
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long LedgerID { get; set; }
    public string? DeliveryNoteNo { get; set; }
    public DateTime? DeliveryNoteDate { get; set; }
    public string? GateEntryNo { get; set; }
    public DateTime? GateEntryDate { get; set; }
    public string? LRNoVehicleNo { get; set; }
    public string? Transporter { get; set; }
    public long ReceivedBy { get; set; }
    public string? EWayBillNumber { get; set; }
    public DateTime? EWayBillDate { get; set; }
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
/// Tool Receipt Note line items (ToolTransactionDetail)
/// </summary>
public class ToolReceiptNoteDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public decimal ChallanQuantity { get; set; }
    public string? BatchNo { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public decimal ReceiptWtPerPacking { get; set; }
    public long WarehouseID { get; set; }
    public long PurchaseTransactionID { get; set; }
    public long ParentTransactionID { get; set; }

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
