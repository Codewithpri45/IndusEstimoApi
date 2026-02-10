namespace IndasEstimo.Domain.Entities.Inventory;

public class PurchaseGRN
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public DateTime? VoucherDate { get; set; }
    public string? DeliveryNoteNo { get; set; }
    public DateTime? DeliveryNoteDate { get; set; }
    public string? GateEntryNo { get; set; }
    public DateTime? GateEntryDate { get; set; }
    public string? LRNoVehicleNo { get; set; }
    public string? Transporter { get; set; }
    public long ReceivedBy { get; set; }
    public string? Narration { get; set; }
    public long GateEntryTransactionID { get; set; }
    public string? BiltyNo { get; set; }
    public DateTime? BiltyDate { get; set; }
    public string? EWayBillNumber { get; set; }
    public DateTime? EWayBillDate { get; set; }
}

public class PurchaseGRNDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public long PurchaseTransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public decimal ChallanQuantity { get; set; }
    public string? BatchNo { get; set; }
    public string? SupplierBatchNo { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long WarehouseID { get; set; }
    public decimal ReceiptWtPerPacking { get; set; }
    public string? PurchaseUnit { get; set; }
    public string? RefJobCardContentNo { get; set; }
    public string? Remark { get; set; }
    public long JobBookingID { get; set; }
}

public class PurchaseGRNPOUpdate
{
    public long PurchaseTransactionID { get; set; }
    public long ItemID { get; set; }
}
