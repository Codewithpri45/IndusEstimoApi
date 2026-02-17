namespace IndasEstimo.Domain.Entities.Inventory;

/// <summary>
/// Entity for Return To Stock header record (ItemTransactionMain with VoucherID = -25)
/// </summary>
public class ReturnToStockMain
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; } = -25;
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
    public string Narration { get; set; } = string.Empty;
    public int ProductionUnitID { get; set; }
    public int CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public int IsDeletedTransaction { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
}

/// <summary>
/// Entity for Return To Stock detail record (ItemTransactionDetail for VoucherID = -25)
/// </summary>
public class ReturnToStockDetail
{
    public long TransactionDetailID { get; set; }
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long ParentTransactionID { get; set; }
    public long IssueTransactionID { get; set; }
    public long WarehouseID { get; set; }
    public long FloorWarehouseID { get; set; }
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal ReceiptQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public int MachineID { get; set; }
    public int ProcessID { get; set; }
    public int ProductionUnitID { get; set; }
    public int CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public int IsDeletedTransaction { get; set; }
}

/// <summary>
/// Entity for consumption reversal header (ItemConsumptionMain linked to Return transaction)
/// </summary>
public class ReturnConsumptionMain
{
    public long ConsumptionTransactionID { get; set; }
    public int VoucherID { get; set; } = -53;
    public string VoucherPrefix { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public long ReturnTransactionID { get; set; }
    public int DepartmentID { get; set; }
    public int ProductionUnitID { get; set; }
    public int CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public int IsDeletedTransaction { get; set; }
}

/// <summary>
/// Entity for consumption reversal detail (ItemConsumptionDetail for Return transaction)
/// </summary>
public class ReturnConsumptionDetail
{
    public long ConsumptionDetailID { get; set; }
    public long ConsumptionTransactionID { get; set; }
    public long IssueTransactionID { get; set; }
    public long ItemID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public int ProductionUnitID { get; set; }
    public int CompanyID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public int IsDeletedTransaction { get; set; }
}
