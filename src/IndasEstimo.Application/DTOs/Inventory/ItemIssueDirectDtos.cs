namespace IndasEstimo.Application.DTOs.Inventory;

// ─── Request DTOs ────────────────────────────────────────────────────────────

public class GetIssueListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

public class GetStockBatchWiseRequest
{
    public long ItemId { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
}

public class SaveIssueDataRequest
{
    public string Prefix { get; set; } = string.Empty;
    public IssueMainData MainData { get; set; } = new();
    public List<IssueDetailData> DetailData { get; set; } = new();
    public IssueConsumeMainData ConsumeMainData { get; set; } = new();
    public List<IssueConsumeDetailData> ConsumeDetailData { get; set; } = new();
}

public class UpdateIssueDataRequest
{
    public long TransactionID { get; set; }
    public IssueMainData MainData { get; set; } = new();
    public List<IssueDetailData> DetailData { get; set; } = new();
    public IssueConsumeMainData ConsumeMainData { get; set; } = new();
    public List<IssueConsumeDetailData> ConsumeDetailData { get; set; } = new();
}

public class DeleteIssueRequest
{
    public long TransactionID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
}

// ─── Nested Save/Update Request Models ───────────────────────────────────────

public class IssueMainData
{
    public int VoucherID { get; set; } = -19;
    public string VoucherDate { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

public class IssueDetailData
{
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long PicklistTransactionID { get; set; }
    public long PicklistReleaseTransactionID { get; set; }
    public long ParentTransactionID { get; set; }
    public long WarehouseID { get; set; }
    public long FloorWarehouseID { get; set; }
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal IssueQuantity { get; set; }
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public int MachineID { get; set; }
    public int ProcessID { get; set; }
    public int DepartmentID { get; set; }
}

public class IssueConsumeMainData
{
    public string VoucherDate { get; set; } = string.Empty;
    public int DepartmentID { get; set; }
}

public class IssueConsumeDetailData
{
    public long ItemID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
}

// ─── Response DTOs ────────────────────────────────────────────────────────────

public class IssueVoucherNoDto
{
    public string VoucherNo { get; set; } = string.Empty;
}

public class IssueListDto
{
    public long TransactionID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long WarehouseID { get; set; }
    public long FloorWarehouseID { get; set; }
    public int DepartmentID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long PicklistReleaseTransactionID { get; set; }
    public long PicklistTransactionID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string PicklistNo { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal IssueQuantity { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public int MachineId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyID { get; set; }
}

public class IssueVoucherDetailDto
{
    public long PicklistTransactionID { get; set; }
    public long TransID { get; set; }
    public long PicklistReleaseTransactionID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public int DepartmentID { get; set; }
    public int MachineID { get; set; }
    public int ProcessID { get; set; }
    public long ParentTransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long WarehouseID { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal BatchStock { get; set; }
    public decimal IssueQuantity { get; set; }
    public string PicklistNo { get; set; } = string.Empty;
    public string PicklistDate { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string MfgDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
}

public class IssueHeaderDto
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public int ItemSubGroupID { get; set; }
    public long WarehouseID { get; set; }
    public long FloorWarehouseID { get; set; }
    public int DepartmentID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string MaxVoucherNo { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string PicklistNo { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public string IssueQuantity { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

public class JobCardRenderDto
{
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long OrderBookingID { get; set; }
    public long OrderBookingDetailsID { get; set; }
    public long ProductMasterID { get; set; }
    public long BookingID { get; set; }
    public long LedgerID { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public decimal FullSheets { get; set; }
    public string CutSize { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public decimal AllocatedQuantity { get; set; }
}

public class JobAllocatedPicklistDto
{
    public long PicklistTransactionID { get; set; }
    public long PicklistReleaseTransactionID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public int DepartmentID { get; set; }
    public int MachineID { get; set; }
    public int ProcessID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public string PicklistNo { get; set; } = string.Empty;
    public decimal ReleaseNo { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal PhysicalStock { get; set; }
    public decimal AllocatedStock { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal ReleaseQuantity { get; set; }
    public decimal IssueQuantity { get; set; }
    public decimal PendingQuantity { get; set; }
    public int AllowIssueExtraQuantity { get; set; }
}

public class AllPicklistDto
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long GRNTransactionID { get; set; }
    public long WarehouseID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string Picklist_Order_No { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal BatchStock { get; set; }
    public decimal TotalPhysicalStock { get; set; }
    public decimal TotalAllocatedStock { get; set; }
    public string GRNNo { get; set; } = string.Empty;
    public string GRNDate { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal Issue_Qty { get; set; }
    public decimal PendingQuantity { get; set; }
}

public class StockBatchWiseDto
{
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public int ItemSubGroupID { get; set; }
    public long ParentTransactionID { get; set; }
    public long WarehouseID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal BatchStock { get; set; }
    public string GRNNo { get; set; } = string.Empty;
    public string GRNDate { get; set; } = string.Empty;
    public long BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal IssueQuantity { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
}

public class IssueSaveResultDto
{
    public string VoucherNo { get; set; } = string.Empty;
    public long TransactionID { get; set; }
}

/// <summary>
/// Floor warehouse for Item Issue Direct.
/// Carries WarehouseID so the caller has a valid ID even when no bins exist.
/// When bins are present, BinDto.WarehouseID (per-bin row) is used as FloorWarehouseID.
/// When no bins exist, this WarehouseID is used directly as FloorWarehouseID.
/// </summary>
public class FloorWarehouseDto
{
    public long WarehouseID { get; set; }
    public string Warehouse { get; set; } = string.Empty;
}

/// <summary>
/// Bin row for a floor warehouse selection.
/// WarehouseID here is the row-level ID from WarehouseMaster for the
/// specific WarehouseName+BinName combination — store this as FloorWarehouseID.
/// </summary>
public class FloorBinDto
{
    public long WarehouseID { get; set; }
    public string Bin { get; set; } = string.Empty;
}
