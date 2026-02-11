namespace IndasEstimo.Application.DTOs.ToolInventory;

// ==================== Request DTOs ====================

/// <summary>
/// Save Tool Requisition request - matches SavePaperPurchaseRequisition WebMethod
/// </summary>
public record SaveToolRequisitionRequest(
    string Prefix,
    List<ToolRequisitionMainDto> JsonObjectsRecordMain,
    List<ToolRequisitionDetailDto> JsonObjectsRecordDetail,
    List<ToolRequisitionIndentUpdateDto> JsonObjectsUpdateIndentDetail
);

/// <summary>
/// Update Tool Requisition request - matches UpdatePaperPurchaseRequisition WebMethod
/// </summary>
public record UpdateToolRequisitionRequest(
    long TransactionID,
    List<ToolRequisitionMainDto> JsonObjectsRecordMain,
    List<ToolRequisitionDetailDto> JsonObjectsRecordDetail,
    List<ToolRequisitionIndentUpdateDto> JsonObjectsUpdateIndentDetail
);

/// <summary>
/// Tool Requisition header DTO
/// </summary>
public record ToolRequisitionMainDto(
    int VoucherID,
    DateTime VoucherDate,
    decimal TotalQuantity,
    string? Narration
);

/// <summary>
/// Tool Requisition detail line item DTO
/// </summary>
public record ToolRequisitionDetailDto(
    int TransID,
    long ToolID,
    long ToolGroupID,
    decimal RequiredQuantity,
    string StockUnit,
    string PurchaseUnit,
    DateTime? ExpectedDeliveryDate,
    string? ToolNarration,
    decimal CurrentStockInStockUnit,
    decimal CurrentStockInPurchaseUnit,
    long JobBookingJobCardContentsID,
    long JobBookingID,
    long RefJobBookingJobCardContentsID,
    string? RefJobCardContentNo,
    long RequisitionToolID
);

/// <summary>
/// Indent detail update DTO - links indent items to requisition
/// </summary>
public record ToolRequisitionIndentUpdateDto(
    long TransactionID,
    long ToolID,
    long CompanyID
);

/// <summary>
/// Response DTO for save/update
/// </summary>
public record SaveToolRequisitionResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== FillGrid Request DTO ====================

/// <summary>
/// Request for FillGrid - matches FillGrid WebMethod
/// </summary>
public class GetToolRequisitionListRequest
{
    public string FilterString { get; set; } = string.Empty;
    public string FromDateValue { get; set; } = string.Empty;
    public string ToDateValue { get; set; } = string.Empty;
}

// ==================== Indent List DTOs ====================

/// <summary>
/// Indent list item - tools available for requisition (VoucherID = -8 + unindented tools)
/// Matches FillGrid "Indent List" mode
/// </summary>
public class ToolIndentListDto
{
    public long TransactionID { get; set; }
    public long MaxVoucherNo { get; set; }
    public int VoucherID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public long ToolGroupNameID { get; set; }
    public long BookingID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string ToolGroupName { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public string JobBookingContentNo { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public decimal BookedStock { get; set; }
    public decimal AllocatedStock { get; set; }
    public decimal PhysicalStock { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public int UnitDecimalPlace { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal SizeW { get; set; }
    public string ConversionFormula { get; set; } = string.Empty;
    public int ConvertedUnitDecimalPlace { get; set; }
    public string LastPurchaseDate { get; set; } = string.Empty;
}

// ==================== Requisition List DTO ====================

/// <summary>
/// Created requisition list item - matches FillGrid requisition mode (VoucherID = -115)
/// </summary>
public class ToolRequisitionListDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public string VoucherDate { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

// ==================== Retrieve Requisition Data DTO ====================

/// <summary>
/// Complete requisition data for editing - matches RetriveRequisitionData WebMethod
/// </summary>
public class ToolRequisitionDataDto
{
    public long RequisitionTransactionID { get; set; }
    public int VoucherToolApproved { get; set; }
    public long RequisitionMaxVoucherNo { get; set; }
    public int RequisitionVoucherID { get; set; }
    public long TransactionID { get; set; }
    public long MaxVoucherNo { get; set; }
    public int VoucherID { get; set; }
    public long RequisitionToolID { get; set; }
    public long ToolID { get; set; }
    public int TransID { get; set; }
    public long ToolGroupID { get; set; }
    public long ToolSubGroupID { get; set; }
    public long ToolGroupNameID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string RequisitionVoucherNo { get; set; } = string.Empty;
    public string RequisitionVoucherDate { get; set; } = string.Empty;
    public string ToolGroupName { get; set; } = string.Empty;
    public string RequisitionToolCode { get; set; } = string.Empty;
    public string RequisitionToolName { get; set; } = string.Empty;
    public string RequisitionToolDescription { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolDescription { get; set; } = string.Empty;
    public decimal PurchaseQty { get; set; }
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal TotalRequisitionQty { get; set; }
    public decimal RequisitionQty { get; set; }
    public decimal RequisitionBookedStock { get; set; }
    public decimal RequisitionAllocatedStock { get; set; }
    public decimal RequisitionPhysicalStock { get; set; }
    public decimal RequisitionPhysicalStockInPurchaseUnit { get; set; }
    public decimal BookedStock { get; set; }
    public decimal AllocatedStock { get; set; }
    public decimal PhysicalStock { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string OrderUnit { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string ToolNarration { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public int UnitDecimalPlace { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long RefJobBookingJobCardContentsID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string LastPurchaseDate { get; set; } = string.Empty;
    public decimal SizeW { get; set; }
}

// ==================== Overflow Grid (Tool Master) DTO ====================

/// <summary>
/// Tool master item for selection - matches GetOverFlowGrid WebMethod
/// </summary>
public class ToolMasterItemDto
{
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Narration { get; set; } = string.Empty;
}
