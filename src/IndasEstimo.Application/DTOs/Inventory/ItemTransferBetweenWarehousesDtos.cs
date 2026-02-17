namespace IndasEstimo.Application.DTOs.Inventory;

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public class GetTransferListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

public class SaveTransferRequest
{
    public string Prefix { get; set; } = "TRN";
    public TransferMainData MainData { get; set; } = new();
    public List<TransferIssueDetailData> IssueDetails { get; set; } = new();
    public List<TransferReceiptDetailData> ReceiptDetails { get; set; } = new();
}

public class UpdateTransferRequest
{
    public long TransactionID { get; set; }
    public TransferMainData MainData { get; set; } = new();
    public List<TransferIssueDetailData> IssueDetails { get; set; } = new();
    public List<TransferReceiptDetailData> ReceiptDetails { get; set; } = new();
}

public class DeleteTransferRequest
{
    public long TransactionID { get; set; }
}

// ─── Nested Save/Update Request Models ───────────────────────────────────────

public class TransferMainData
{
    public int VoucherID { get; set; } = -22;
    public string VoucherDate { get; set; } = string.Empty;
    public int SourceWarehouseID { get; set; }
    public int DestinationWarehouseID { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Particular { get; set; } = string.Empty;
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string DeliveryNoteDate { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

public class TransferIssueDetailData
{
    public long ParentTransactionID { get; set; }
    public int ItemGroupID { get; set; }
    public long ItemID { get; set; }
    public decimal IssueQuantity { get; set; }
    public decimal ReceiptQuantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
}

public class TransferReceiptDetailData
{
    public long ParentTransactionID { get; set; }
    public int ItemGroupID { get; set; }
    public long ItemID { get; set; }
    public decimal ReceiptQuantity { get; set; }
    public decimal IssueQuantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
}

// ─── Response DTOs ────────────────────────────────────────────────────────────

public class TransferOperationResponseDto
{
    public long TransactionID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
}

public class TransferListDto
{
    public long TransactionID { get; set; }
    public long ParentTransactionID { get; set; }
    public int VoucherID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public long WarehouseID { get; set; }
    public long DestinationWarehouseID { get; set; }
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public int TotalTransferItems { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string? ItemSubGroupName { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string? SupplierBatchNo { get; set; }
    public string? MfgDate { get; set; }
    public string? ExpiryDate { get; set; }
    public decimal BatchStock { get; set; }
    public decimal TransferStock { get; set; }
    public string? GRNNo { get; set; }
    public string? GRNDate { get; set; }
    public string? Warehouse { get; set; }
    public string? Bin { get; set; }
    public string? DestinationWarehouse { get; set; }
    public string? DestinationBin { get; set; }
    public string? CreatedBy { get; set; }
    public string? Narration { get; set; }
    public string? DeliveryNoteNo { get; set; }
    public string? DeliveryNoteDate { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal? UnitPerPacking { get; set; }
    public decimal? ConversionFactor { get; set; }
    public string? FYear { get; set; }
    public int ProductionUnitID { get; set; }
    public string? ProductionUnitName { get; set; }
    public string? CompanyName { get; set; }
}

public class WarehouseStockDto
{
    public long ParentTransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public long WarehouseID { get; set; }
    public string? ItemGroupName { get; set; }
    public string? ItemSubGroupName { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? StockUnit { get; set; }
    public decimal BatchStock { get; set; }
    public decimal TransferStock { get; set; }
    public string? GRNNo { get; set; }
    public string? GRNDate { get; set; }
    public long BatchID { get; set; }
    public string? BatchNo { get; set; }
    public string? SupplierBatchNo { get; set; }
    public string? MfgDate { get; set; }
    public string? ExpiryDate { get; set; }
    public string? Warehouse { get; set; }
    public string? Bin { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
}
