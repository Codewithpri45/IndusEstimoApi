namespace IndasEstimo.Application.DTOs.Inventory;

/// <summary>
/// Main request DTO for saving Item Issue Direct - matches frontend SaveItemIssueDirectRequest
/// </summary>
public record SaveItemIssueDirectRequest(
    string Prefix,
    string StockType, // 'Job Consumables' | 'General Stock' | 'Other'
    string IssueType, // 'Job Allocated' | 'All'
    List<ItemIssueDirectMainDto> JsonObjectsRecordMain,
    List<ItemIssueDirectDetailDto> JsonObjectsRecordDetail,
    List<ItemIssueDirectConsumeMainDto>? ObjectsConsumeMain,
    List<ItemIssueDirectConsumeDetailDto>? ObjectsConsumeDetails
);

/// <summary>
/// Update request DTO - extends Save with TransactionID
/// </summary>
public record UpdateItemIssueDirectRequest(
    long TransactionID,
    string Prefix,
    string StockType,
    string IssueType,
    List<ItemIssueDirectMainDto> JsonObjectsRecordMain,
    List<ItemIssueDirectDetailDto> JsonObjectsRecordDetail,
    List<ItemIssueDirectConsumeMainDto>? ObjectsConsumeMain,
    List<ItemIssueDirectConsumeDetailDto>? ObjectsConsumeDetails
);

/// <summary>
/// Main entity DTO for Item Issue Direct
/// </summary>
public record ItemIssueDirectMainDto(
    int VoucherID,
    DateTime VoucherDate,
    long? JobBookingID,
    long? JobBookingJobCardContentsID,
    long? LedgerID,
    long? DepartmentID,
    long? MachineID,
    long? ProcessID,
    long FloorGodownID, // Warehouse ID
    long BinID,
    string SlipNo,
    DateTime SlipDate,
    decimal TotalIssueQuantity,
    string? Narration,
    string? JobCardNo,
    string? BookingNo,
    decimal? RequiredSheets,
    decimal? AllocatedSheets,
    decimal? RequiredQuantity,
    decimal IssuedQuantity,
    string? JobName,
    string? ContentName
);

/// <summary>
/// Detail entity DTO for Item Issue Direct (per item)
/// </summary>
public record ItemIssueDirectDetailDto(
    long ItemID,
    long ItemGroupID,
    long? ItemSubGroupID,
    long? JobBookingID,
    long? JobBookingJobCardContentsID,
    long? PicklistReleaseTransactionID,
    long? PicklistTransactionID,
    long? DepartmentID,
    long? MachineID,
    long? ProcessID,
    long FloorGodownID,
    long BinID,
    long? BatchID,
    string? BatchNo,
    string? SupplierBatchNo,
    string? GRNNo,
    DateTime? GRNDate,
    decimal IssueQuantity,
    decimal? ReleaseQuantity,
    decimal? PendingQuantity,
    decimal? PhysicalStock,
    decimal? AllocatedStock,
    decimal? FreeStock,
    decimal? BatchStock,
    int? AgeingDays,
    string? ItemCode,
    string? ItemName,
    string? ItemGroupName,
    string? ItemSubGroupName,
    string? StockUnit,
    string? PicklistNo,
    string? ReleaseNo,
    string? JobCardNo,
    string? JobName,
    string? ContentName,
    string? ProcessName,
    string? MachineName,
    string? DepartmentName,
    string? Warehouse,
    string? Bin,
    decimal? WtPerPacking,
    decimal? UnitPerPacking,
    decimal? ConversionFactor,
    int? UnitDecimalPlace
);

/// <summary>
/// Consume Main DTO (for ObjectsConsumeMain)
/// </summary>
public record ItemIssueDirectConsumeMainDto(
    long? JobBookingJobCardContentsID,
    decimal ConsumeQuantity
);

/// <summary>
/// Consume Detail DTO (for ObjectsConsumeDetails)
/// </summary>
public record ItemIssueDirectConsumeDetailDto(
    long? JobBookingJobCardContentsID,
    long ItemID,
    decimal ConsumeQuantity,
    string? StockUnit
);

/// <summary>
/// Response DTO for save/update operations
/// </summary>
public record SaveItemIssueDirectResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== Retrieve/Lookup DTOs ====================

/// <summary>
/// Complete Item Issue data for editing - matches GetIssueVoucherDetails WebMethod
/// </summary>
public class ItemIssueDirectDataDto
{
    public long TransactionID { get; set; }
    public long TransID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }

    // Job Details
    public long? JobBookingID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public long? LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public long? MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public long? ProcessID { get; set; }
    public string ProcessName { get; set; } = string.Empty;

    // Warehouse/Bin
    public long FloorGodownID { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public long BinID { get; set; }
    public string Bin { get; set; } = string.Empty;

    // Slip Details
    public string SlipNo { get; set; } = string.Empty;
    public DateTime SlipDate { get; set; }

    // Item Details
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public long? ItemSubGroupID { get; set; }
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;

    // Batch Details
    public long? BatchID { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string GRNNo { get; set; } = string.Empty;
    public DateTime? GRNDate { get; set; }

    // Quantities
    public decimal IssueQuantity { get; set; }
    public decimal TotalIssueQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public decimal? ReleaseQuantity { get; set; }
    public decimal? PendingQuantity { get; set; }
    public decimal? BatchStock { get; set; }
    public int? AgeingDays { get; set; }

    // Job Info
    public string JobCardNo { get; set; } = string.Empty;
    public string BookingNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public decimal? RequiredSheets { get; set; }
    public decimal? AllocatedSheets { get; set; }
    public decimal? RequiredQuantity { get; set; }

    // Picklist References
    public long? PicklistTransactionID { get; set; }
    public long? PicklistReleaseTransactionID { get; set; }
    public string PicklistNo { get; set; } = string.Empty;
    public string ReleaseNo { get; set; } = string.Empty;

    // Other
    public string Narration { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    // Conversions
    public decimal? WtPerPacking { get; set; }
    public decimal? UnitPerPacking { get; set; }
    public decimal? ConversionFactor { get; set; }
    public int? UnitDecimalPlace { get; set; }
}

/// <summary>
/// Item Issue list/grid item - matches Showlist WebMethod
/// </summary>
public class ItemIssueDirectListDto
{
    public long TransactionID { get; set; }
    public long TransID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string PicklistNo { get; set; } = string.Empty;

    // Item Info
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public long? ItemSubGroupID { get; set; }
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;

    // Job Details
    public long? JobBookingID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;

    // Department/Machine/Process
    public long? DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public long? MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public long? ProcessID { get; set; }
    public string ProcessName { get; set; } = string.Empty;

    // Quantity
    public decimal IssueQuantity { get; set; }

    // Client
    public long? LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;

    // Other
    public string Narration { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// Picklist item for item selection - matches JobAllocatedPicklist/AllPicklist WebMethods
/// </summary>
public class DirectPicklistDto
{
    // IDs
    public long? PicklistTransactionID { get; set; }
    public long? PicklistReleaseTransactionID { get; set; }
    public long? JobBookingID { get; set; }
    public long? JobBookingJobCardContentsID { get; set; }
    public long? DepartmentID { get; set; }
    public long? MachineID { get; set; }
    public long? ProcessID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public long? ItemGroupNameID { get; set; }
    public long? ItemSubGroupID { get; set; }

    // Picklist Info
    public string PicklistNo { get; set; } = string.Empty;
    public string ReleaseNo { get; set; } = string.Empty;
    public string BookingNo { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;

    // Item Info
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroup { get; set; } = string.Empty;
    public string SubGroup { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;

    // Stock Info
    public string SupplierReference { get; set; } = string.Empty;
    public decimal PhysicalStock { get; set; }
    public decimal AllocatedStock { get; set; }
    public decimal FreeStock { get; set; }
    public decimal IncomingStock { get; set; }
    public decimal UnapprovedStock { get; set; }
    public decimal BookedQuantity { get; set; }
    public decimal BookedQuantityKG { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal PendingToPick { get; set; }
    public decimal ReleaseQuantity { get; set; }
    public decimal IssueQuantity { get; set; }
    public decimal PendingQuantity { get; set; }

    // Conversions
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public int UnitDecimalPlace { get; set; }
}

/// <summary>
/// Stock batch item - matches GetStockBatchWise WebMethod
/// </summary>
public class StockBatchDirectDto
{
    // IDs
    public long? ParentTransactionID { get; set; }
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public long? ItemGroupNameID { get; set; }
    public long? ItemSubGroupID { get; set; }
    public long? WarehouseID { get; set; }
    public long? BatchID { get; set; }

    // Item Info
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;

    // Stock Info
    public decimal BatchStock { get; set; }
    public int? AgeingDays { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string GRNNo { get; set; } = string.Empty;
    public string GRNDate { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;

    // Conversions
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public int UnitDecimalPlace { get; set; }
}

/// <summary>
/// Job card filter item - matches JobCardRender WebMethod
/// </summary>
public class JobCardDirectDto
{
    // IDs
    public long JobBookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long? LedgerID { get; set; }
    public long? ItemGroupID { get; set; }

    // Job Info
    public string ReleasedDate { get; set; } = string.Empty;
    public string BookingNo { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;

    // Required Quantities
    public decimal FullSheets { get; set; }
    public decimal FullSheetsQuantityKG { get; set; }
    public decimal TotalRequiredRunningMeter { get; set; }

    // Allocated/Issued Quantities
    public decimal AllocatedQuantity { get; set; }
    public decimal AllocatedQuantityKG { get; set; }
    public decimal AllocatedRunningMeter { get; set; }

    // Pending Quantities
    public decimal PendingQty { get; set; }
    public decimal PendingQtyKG { get; set; }
    public decimal PendingRunningMeter { get; set; }

    // Hidden fields from JobCard
    public decimal TxtRequiredSheets { get; set; }
    public decimal TxtAllocatedSheets { get; set; }
}

/// <summary>
/// Warehouse lookup DTO for Item Issue - includes WarehouseID
/// Note: Different from GRN's WarehouseDto which only has name
/// </summary>
public class ItemIssueWarehouseDto
{
    public long WarehouseID { get; set; }
    public string Warehouse { get; set; } = string.Empty;
}

/// <summary>
/// Bin lookup DTO for Item Issue - includes BinID
/// Note: Different from GRN's BinDto which only has name
/// </summary>
public class ItemIssueBinDto
{
    public long BinID { get; set; }
    public string Bin { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
}

/// <summary>
/// Department lookup DTO (Inventory-specific)
/// </summary>
public class DepartmentDto
{
    public long DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}

/// <summary>
/// Machine lookup DTO (Inventory-specific)
/// </summary>
public class MachineDto
{
    public long MachineID { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public long? DepartmentID { get; set; }
}

/// <summary>
/// Process lookup DTO (Inventory-specific)
/// </summary>
public class ProcessDto
{
    public long ProcessID { get; set; }
    public string ProcessName { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for GetItemIssuesList (fill-grid)
/// </summary>
public class GetItemIssuesDirectListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public bool ApplyDateFilter { get; set; } = true;
}
