namespace IndasEstimo.Application.DTOs.ToolInventory;

// ==================== Save Request DTOs ====================

public record SaveToolReturnToStockRequest(
    string Prefix,
    List<ToolReturnToStockMainDto> JsonObjectsRecordMain,
    List<ToolReturnToStockDetailDto> JsonObjectsRecordDetail
);

public record ToolReturnToStockMainDto(
    int VoucherID,
    DateTime VoucherDate,
    long LedgerID,
    long JobBookingID,
    long JobBookingJobCardContentsID,
    string? Narration
);

public record ToolReturnToStockDetailDto(
    int TransID,
    long ToolID,
    long JobBookingID,
    long JobBookingJobCardContentsID,
    string? JobCardFormNo,
    long ItemID,
    decimal ReceiptQuantity,
    string? BatchNo,
    long WarehouseID,
    long IssueTransactionID,
    string? ToolNarration
);

public record SaveToolReturnToStockResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== Retrieve/Lookup DTOs ====================

/// <summary>
/// Available tools for return - matches GetFirstGridData
/// Shows tools that were issued and can be returned to stock
/// </summary>
public class ToolAvailableForReturnDto
{
    public string ToolCode { get; set; } = string.Empty;
    public long ToolID { get; set; }
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JobCardContentNo { get; set; } = string.Empty;
    public long ParentTransactionID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public long ItemID { get; set; }
    public decimal Stock { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}

/// <summary>
/// Return to stock list/grid - matches PlateReturnToStockShowlist
/// </summary>
public class ToolReturnToStockListDto
{
    public long TransactionID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public long ToolID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string BinName { get; set; } = string.Empty;
    public string VoucherPrefix { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public long UserID { get; set; }
}
