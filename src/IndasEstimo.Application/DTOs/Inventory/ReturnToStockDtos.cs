namespace IndasEstimo.Application.DTOs.Inventory;

// ─── Request DTOs ────────────────────────────────────────────────────────────

public class GetReturnListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

public class SaveReturnDataRequest
{
    public string Prefix { get; set; } = string.Empty;
    public ReturnMainData MainData { get; set; } = new();
    public List<ReturnDetailData> DetailData { get; set; } = new();
    public ReturnConsumeMainData ConsumeMainData { get; set; } = new();
    public List<ReturnConsumeDetailData> ConsumeDetailData { get; set; } = new();
}

public class UpdateReturnDataRequest
{
    public long TransactionID { get; set; }
    public ReturnMainData MainData { get; set; } = new();
    public List<ReturnDetailData> DetailData { get; set; } = new();
    public ReturnConsumeMainData ConsumeMainData { get; set; } = new();
    public List<ReturnConsumeDetailData> ConsumeDetailData { get; set; } = new();
}

public class DeleteReturnRequest
{
    public long TransactionID { get; set; }
}

// ─── Nested Save/Update Request Models ───────────────────────────────────────

public class ReturnMainData
{
    public int VoucherID { get; set; } = -25;
    public string VoucherDate { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
    public string Narration { get; set; } = string.Empty;
}

public class ReturnDetailData
{
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
}

public class ReturnConsumeMainData
{
    public string VoucherDate { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
}

public class ReturnConsumeDetailData
{
    public long ItemID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
}

// ─── Response DTOs ────────────────────────────────────────────────────────────

public class ReturnVoucherNoDto
{
    public string VoucherNo { get; set; } = string.Empty;
}

public class ReturnListDto
{
    public long TransactionID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public int DepartmentID { get; set; }
    public long GRNTransactionID { get; set; }
    public long ItemID { get; set; }
    public long IssueTransactionID { get; set; }
    public long WarehouseID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string IssueVoucherNo { get; set; } = string.Empty;
    public string IssueVoucherDate { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal ReturnQuantity { get; set; }
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string MfgDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyID { get; set; }
}

public class ReturnDetailDto
{
    public long ReturnTransactionID { get; set; }
    public int DepartmentID { get; set; }
    public int MachineID { get; set; }
    public long FloorWarehouseID { get; set; }
    public int ProcessID { get; set; }
    public long ParentTransactionID { get; set; }
    public long ItemID { get; set; }
    public long TransactionID { get; set; }
    public long BinID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string ReturnVoucherNo { get; set; } = string.Empty;
    public string ReturnVoucherDate { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal FloorStock { get; set; }
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string MfgDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string GRNNo { get; set; } = string.Empty;
    public string GRNDate { get; set; } = string.Empty;
    public decimal ConsumeQuantity { get; set; }
    public string MachineName { get; set; } = string.Empty;
}

public class MachineDto
{
    public int MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
}

/// <summary>
/// Floor Stock DTO - Items issued to floor that can be returned
/// Calculated as: FloorStock = IssueQuantity - ConsumedStock
/// </summary>
public class FloorStockDto
{
    public long JobBookingID { get; set; }
    public long ParentTransactionID { get; set; }
    public long TransactionID { get; set; }
    public int DepartmentID { get; set; }
    public long FloorWarehouseID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public int MachineID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockType { get; set; } = string.Empty;
    public string StockCategory { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string MfgDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string GRNNo { get; set; } = string.Empty;
    public string GRNDate { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public decimal IssueQuantity { get; set; }
    public decimal ConsumeQuantity { get; set; }
    public decimal FloorStock { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for getting floor stock data
/// </summary>
public class GetFloorStockRequest
{
    /// <summary>
    /// Filter type: "AllIssueVouchers", "JobIssueVouchers", or "NonJobIssueVouchers"
    /// </summary>
    public string IssueType { get; set; } = "AllIssueVouchers";
}

/// <summary>
/// Common response DTO for save/update/delete operations
/// </summary>
public class ReturnOperationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReturnNo { get; set; } = string.Empty;
    public long TransactionId { get; set; }
}
