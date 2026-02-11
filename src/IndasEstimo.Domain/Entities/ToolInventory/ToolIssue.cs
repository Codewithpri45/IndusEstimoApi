namespace IndasEstimo.Domain.Entities.ToolInventory;

/// <summary>
/// Tool Issue header (ToolTransactionMain, VoucherID = -43)
/// Issues tools from warehouse for job cards/production
/// </summary>
public class ToolIssue
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; } // -43
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string? DeliveryNoteNo { get; set; }
    public DateTime? DeliveryNoteDate { get; set; }
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
/// Tool Issue line items (ToolTransactionDetail)
/// Tracks individual tools issued with batch, warehouse, and bin location
/// </summary>
public class ToolIssueDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ItemID { get; set; }
    public decimal IssueQuantity { get; set; }
    public string? BatchNo { get; set; }
    public long WarehouseID { get; set; }
    public long FloorWarehouseID { get; set; } // BIN location
    public string? ToolNarration { get; set; }
    public long ParentTransactionID { get; set; } // Links to source transaction (e.g., Requisition)
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string? JobCardFormNo { get; set; }

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
