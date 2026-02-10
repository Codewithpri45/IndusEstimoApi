namespace IndasEstimo.Application.DTOs.Inventory;

// ==================== Request DTOs ====================

public class GetGRNListRequest
{
    public string RadioValue { get; set; } = "Pending Receipt Note";
    public string FromDate { get; set; } = "";
    public string ToDate { get; set; } = "";
}

public class GetGRNBatchDetailRequest
{
    public long TransactionID { get; set; }
    public string RadioValue { get; set; } = "Pending Receipt Note";
}

public class ApproveGRNRequest
{
    public long GRNTransactionID { get; set; }
    public string RadioButtonValue { get; set; } = "Pending Receipt Note";
    public List<GRNApprovalItem> Items { get; set; } = new();
}

public class GRNApprovalItem
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public decimal ApprovedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string QCApprovalNO { get; set; } = "";
    public string QCApprovedNarration { get; set; } = "";
    public int IsVoucherItemApproved { get; set; }
}

// ==================== Response DTOs ====================

public class GRNListDto
{
    public long TransactionID { get; set; }
    public long PurchaseTransactionID { get; set; }
    public long LedgerID { get; set; }
    public long MaxVoucherNo { get; set; }
    public string LedgerName { get; set; } = "";
    public string ReceiptVoucherNo { get; set; } = "";
    public string ReceiptVoucherDate { get; set; } = "";
    public string PurchaseVoucherNo { get; set; } = "";
    public string PurchaseVoucherDate { get; set; } = "";
    public decimal ChallanQuantity { get; set; }
    public string DeliveryNoteNo { get; set; } = "";
    public string DeliveryNoteDate { get; set; } = "";
    public string GateEntryNo { get; set; } = "";
    public string GateEntryDate { get; set; } = "";
    public string LRNoVehicleNo { get; set; } = "";
    public string Transporter { get; set; } = "";
    public string ReceiverName { get; set; } = "";
    public string Narration { get; set; } = "";
    public string FYear { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public long ReceivedBy { get; set; }
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = "";
    public string CompanyName { get; set; } = "";

    // For Approved Receipt Note only
    public string? ApprovedBy { get; set; }
    public string? ApprovalDate { get; set; }
}

public class GRNBatchDetailDto
{
    public long TransactionID { get; set; }
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = "";
    public string SupplierBatchNo { get; set; } = "";
    public string VOUCHERNO { get; set; } = "";
    public long QCTransactionID { get; set; }
    public string ItemSubGroupID { get; set; } = "";
    public long PurchaseTransactionID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public string PurchaseVoucherNo { get; set; } = "";
    public string PurchaseVoucherDate { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public string ItemGroupName { get; set; } = "";
    public string ItemSubGroupName { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string ItemDescription { get; set; } = "";
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = "";
    public decimal ChallanQuantity { get; set; }
    public decimal ApprovedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string QCApprovalNO { get; set; } = "";
    public string QCApprovedNarration { get; set; } = "";
    public string StockUnit { get; set; } = "";
    public decimal ReceiptWtPerPacking { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal SizeW { get; set; }
    public long WarehouseID { get; set; }
    public string Warehouse { get; set; } = "";
    public string Bin { get; set; } = "";
    public int UnitDecimalPlace { get; set; }
    public string ItemQuality { get; set; } = "";
}
