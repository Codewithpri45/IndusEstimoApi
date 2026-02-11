namespace IndasEstimo.Application.DTOs.ToolInventory;

// ==================== Save/Update Request DTOs ====================

public record SaveToolReceiptNoteRequest(
    string Prefix,
    List<ToolReceiptNoteMainDto> JsonObjectsRecordMain,
    List<ToolReceiptNoteDetailDto> JsonObjectsRecordDetail
);

public record UpdateToolReceiptNoteRequest(
    long TransactionID,
    List<ToolReceiptNoteMainDto> JsonObjectsRecordMain,
    List<ToolReceiptNoteDetailDto> JsonObjectsRecordDetail
);

public record ToolReceiptNoteMainDto(
    int VoucherID,
    DateTime VoucherDate,
    long LedgerID,
    string? DeliveryNoteNo,
    DateTime? DeliveryNoteDate,
    string? GateEntryNo,
    DateTime? GateEntryDate,
    string? LRNoVehicleNo,
    string? Transporter,
    long ReceivedBy,
    string? EWayBillNumber,
    DateTime? EWayBillDate,
    string? Narration
);

public record ToolReceiptNoteDetailDto(
    int TransID,
    long ToolID,
    long ToolGroupID,
    decimal ChallanQuantity,
    string? BatchNo,
    string StockUnit,
    decimal ReceiptWtPerPacking,
    long WarehouseID,
    long PurchaseTransactionID
);

public record SaveToolReceiptNoteResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== Retrieve/Lookup DTOs ====================

/// <summary>
/// Receipt note batch detail for editing - matches GetReceiptVoucherBatchDetail
/// </summary>
public class ToolReceiptNoteDataDto
{
    public long PurchaseTransactionID { get; set; }
    public long LedgerID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal ChallanQuantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal ReceiptWtPerPacking { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public long WarehouseID { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal ReceiptQuantity { get; set; }
}

/// <summary>
/// Receipt note list/grid - matches GetReceiptNoteList
/// </summary>
public class ToolReceiptNoteListDto
{
    public string EWayBillNumber { get; set; } = string.Empty;
    public string EWayBillDate { get; set; } = string.Empty;
    public long TransactionID { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long PurchaseTransactionID { get; set; }
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string ReceiptVoucherNo { get; set; } = string.Empty;
    public string ReceiptVoucherDate { get; set; } = string.Empty;
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public decimal ChallanQuantity { get; set; }
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string DeliveryNoteDate { get; set; } = string.Empty;
    public string GateEntryNo { get; set; } = string.Empty;
    public string GateEntryDate { get; set; } = string.Empty;
    public string LRNoVehicleNo { get; set; } = string.Empty;
    public string Transporter { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public long ReceivedBy { get; set; }
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

/// <summary>
/// Pending purchase orders for receipt - matches GetPendingOrdersList
/// </summary>
public class ToolPendingPurchaseOrderDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public decimal PendingQty { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal PurchaseTolerance { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public string PurchaseDivision { get; set; } = string.Empty;
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public decimal ReceiptQuantity { get; set; }
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

/// <summary>
/// Receiver lookup - matches GetReceiverList (LedgerGroupNameID=27)
/// </summary>
public class ToolReceiverDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
}

/// <summary>
/// Previous received quantity - matches GetPreviousReceivedQuantity
/// </summary>
public class ToolPreviousReceivedQtyDto
{
    public long TransactionID { get; set; }
    public long ToolID { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PreReceiptQuantity { get; set; }
}

/// <summary>
/// Warehouse lookup - matches GetWarehouseList
/// </summary>
public class ToolWarehouseDto
{
    public string Warehouse { get; set; } = string.Empty;
}

/// <summary>
/// Bin lookup - matches GetBinsList
/// </summary>
public class ToolBinDto
{
    public string WarehouseName { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
}

/// <summary>
/// Request DTO for GetReceiptNoteList (fill-grid)
/// </summary>
public class GetToolReceiptNoteListRequest
{
    public string FromDateValue { get; set; } = string.Empty;
    public string ToDateValue { get; set; } = string.Empty;
}
