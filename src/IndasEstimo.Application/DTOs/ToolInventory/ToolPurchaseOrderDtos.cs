namespace IndasEstimo.Application.DTOs.ToolInventory;

// ==================== Save/Update Request DTOs ====================

public record SaveToolPurchaseOrderRequest(
    string Prefix,
    List<ToolPurchaseOrderMainDto> JsonObjectsRecordMain,
    List<ToolPurchaseOrderDetailDto> JsonObjectsRecordDetail,
    List<ToolPurchaseOrderOverheadDto> JsonObjectsRecordOverHead,
    List<ToolPurchaseOrderTaxDto> JsonObjectsRecordTax,
    List<ToolPurchaseOrderRequisitionDto> JsonObjectsRecordRequisition,
    string TxtNetAmt,
    string CurrencyCode
);

public record UpdateToolPurchaseOrderRequest(
    long TransactionID,
    List<ToolPurchaseOrderMainDto> JsonObjectsRecordMain,
    List<ToolPurchaseOrderDetailDto> JsonObjectsRecordDetail,
    List<ToolPurchaseOrderOverheadDto> JsonObjectsRecordOverHead,
    List<ToolPurchaseOrderTaxDto> JsonObjectsRecordTax,
    List<ToolPurchaseOrderRequisitionDto> JsonObjectsRecordRequisition,
    string TxtNetAmt,
    string CurrencyCode
);

public record ToolPurchaseOrderMainDto(
    int VoucherID,
    DateTime VoucherDate,
    long LedgerID,
    long ContactPersonID,
    decimal TotalQuantity,
    decimal TotalBasicAmount,
    decimal TotalTaxAmount,
    decimal TotalOverheadAmount,
    decimal NetAmount,
    string? PurchaseDivision,
    string? PurchaseReferenceRemark,
    string? Narration,
    string? DeliveryAddress,
    string? TermsOfPayment,
    string CurrencyCode,
    string? ModeOfTransport,
    string? DealerID,
    long VoucherApprovalByEmployeeID
);

public record ToolPurchaseOrderDetailDto(
    int TransID,
    long ToolID,
    long ToolGroupID,
    decimal RequiredQuantity,
    decimal PurchaseOrderQuantity,
    string PurchaseUnit,
    string StockUnit,
    decimal PurchaseRate,
    decimal PurchaseTolerance,
    decimal GrossAmount,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal BasicAmount,
    decimal TaxableAmount,
    decimal GSTPercentage,
    decimal CGSTPercentage,
    decimal SGSTPercentage,
    decimal IGSTPercentage,
    decimal CGSTAmount,
    decimal SGSTAmount,
    decimal IGSTAmount,
    decimal NetAmount,
    DateTime? ExpectedDeliveryDate,
    long ProductHSNID
);

public record ToolPurchaseOrderOverheadDto(
    long HeadID,
    int TransID,
    string HeadName,
    decimal Quantity,
    string ChargesType,
    decimal Rate,
    decimal Amount
);

public record ToolPurchaseOrderTaxDto(
    int TransID,
    long LedgerID,
    decimal TaxPercentage,
    decimal Amount,
    decimal TaxInAmount,
    bool IsComulative,
    bool GSTApplicable,
    string? CalculatedON
);

public record ToolPurchaseOrderRequisitionDto(
    int TransID,
    long ToolID,
    decimal RequisitionProcessQuantity,
    string StockUnit,
    long RequisitionTransactionID
);

public record SaveToolPurchaseOrderResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== Retrieve/Lookup DTOs ====================

/// <summary>
/// Complete Tool PO data for editing - matches RetrivePoCreateGrid
/// </summary>
public class ToolPurchaseOrderDataDto
{
    public long PurchaseTransactionID { get; set; }
    public int PurchaseVoucherID { get; set; }
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public long PurchaseMaxVoucherNo { get; set; }
    public long MaxVoucherNo { get; set; }
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public decimal RequisitionQty { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public decimal PurchaseQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal Disc { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AfterDisAmt { get; set; }
    public decimal Tolerance { get; set; }
    public decimal GSTTaxPercentage { get; set; }
    public decimal GSTTaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public long ReceiptTransactionID { get; set; }
    public int IsVoucherToolApproved { get; set; }
    public int IsReworked { get; set; }
    public string ReworkRemark { get; set; } = string.Empty;
    public string PurchaseReference { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string PurchaseDivision { get; set; } = string.Empty;
    public decimal TotalRequiredQuantity { get; set; }
    public string PurchaseStockUnit { get; set; } = string.Empty;
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
    public decimal CGSTAmt { get; set; }
    public decimal SGSTAmt { get; set; }
    public decimal IGSTAmt { get; set; }
    public decimal TaxableAmount { get; set; }
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string ProductHSNName { get; set; } = string.Empty;
    public string HSNCode { get; set; } = string.Empty;
    public long ProductHSNID { get; set; }
}

/// <summary>
/// Tool PO list/grid item - matches ProcessFillGrid (Detail view)
/// </summary>
public class ToolPurchaseOrderListDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public int TransID { get; set; }
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ToolCode { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public decimal PurchaseQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal GSTTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long ReceiptTransactionID { get; set; }
    public int IsVoucherToolApproved { get; set; }
    public int IsReworked { get; set; }
    public string ReworkRemark { get; set; } = string.Empty;
    public string PurchaseReference { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string PurchaseDivision { get; set; } = string.Empty;
    public string ContactPersonID { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalOverheadAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string TotalQuantity { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public string ModeOfTransport { get; set; } = string.Empty;
    public string DealerID { get; set; } = string.Empty;
    public int VoucherToolApproved { get; set; }
    public int VoucherCancelled { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public long VoucherApprovalByEmployeeID { get; set; }
}

/// <summary>
/// Pending tool requisitions for PO creation - matches FillGrid("Pending Requisitions")
/// VoucherID=-115, IsVoucherToolApproved=1, pending qty calculation
/// </summary>
public class ToolPendingRequisitionDto
{
    public long TransactionID { get; set; }
    public int TransID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public long ToolID { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public long ToolGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal PurchaseQuantity { get; set; }
    public decimal PendingQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public long ProductHSNID { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
}

/// <summary>
/// Tool PO overhead data for retrieve - matches RetrivePoOverHead
/// </summary>
public class ToolPOOverheadDataDto
{
    public int TransID { get; set; }
    public long TransactionID { get; set; }
    public long HeadID { get; set; }
    public decimal Weight { get; set; }
    public string RateType { get; set; } = string.Empty;
    public decimal HeadAmount { get; set; }
    public decimal Rate { get; set; }
    public string Head { get; set; } = string.Empty;
}

/// <summary>
/// Tool PO tax data for retrieve - matches RetrivePoCreateTaxChares
/// </summary>
public class ToolPOTaxDataDto
{
    public long LedgerID { get; set; }
    public int TransID { get; set; }
    public long TransactionID { get; set; }
    public decimal TaxRatePer { get; set; }
    public decimal ChargesAmount { get; set; }
    public decimal InAmount { get; set; }
    public bool IsCumulative { get; set; }
    public bool GSTApplicable { get; set; }
    public string CalculateON { get; set; } = string.Empty;
    public string LedgerName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public string GSTLedgerType { get; set; } = string.Empty;
}

/// <summary>
/// Supplier lookup - matches Supplier WebMethod (LedgerGroupNameID=23)
/// </summary>
public class ToolSupplierDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string GSTNo { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string GSTApplicable { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string StateTinNo { get; set; } = string.Empty;
}

/// <summary>
/// Contact person lookup - matches GetContactPerson
/// </summary>
public class ToolContactPersonDto
{
    public long ConcernPersonID { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Tool item rate by supplier - matches GetItemRate
/// </summary>
public class ToolItemRateDto
{
    public decimal ItemRate { get; set; }
}

/// <summary>
/// Allotted suppliers for a tool group - matches GetAllotedSupp
/// </summary>
public class ToolAllottedSupplierDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
}

/// <summary>
/// Tool overflow grid - matches GetOverFlowGrid
/// </summary>
public class ToolOverflowGridDto
{
    public long ToolID { get; set; }
    public long ToolGroupID { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public long ProductHSNID { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public string ProductHSNName { get; set; } = string.Empty;
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
}

/// <summary>
/// Overhead charge head - matches HeadFun
/// </summary>
public class ToolOverheadChargeHeadDto
{
    public long HeadID { get; set; }
    public string Head { get; set; } = string.Empty;
    public string RateType { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Rate { get; set; }
    public decimal HeadAmount { get; set; }
}

/// <summary>
/// Tax charge ledger - matches CHLname (LedgerGroupNameID=43)
/// </summary>
public class ToolTaxChargeLedgerDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string TaxPercentage { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public string GSTApplicable { get; set; } = string.Empty;
    public string GSTLedgerType { get; set; } = string.Empty;
    public string GSTCalculationOn { get; set; } = string.Empty;
}

/// <summary>
/// HSN code lookup - matches GetAllHSN
/// </summary>
public class ToolHSNCodeDto
{
    public long ProductHSNID { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public string ProductHSNName { get; set; } = string.Empty;
    public decimal GSTTaxPercentage { get; set; }
    public decimal CGSTTaxPercentage { get; set; }
    public decimal SGSTTaxPercentage { get; set; }
    public decimal IGSTTaxPercentage { get; set; }
}

/// <summary>
/// Currency lookup - matches GetCurrencyList
/// </summary>
public class ToolCurrencyDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyHeadName { get; set; } = string.Empty;
    public string CurrencyChildName { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}

/// <summary>
/// PO Approval By employee lookup - matches GetPOApprovalBy (LedgerGroupNameID=27)
/// </summary>
public class ToolPOApprovalByDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for ProcessFillGrid
/// </summary>
public class GetToolPurchaseOrderListRequest
{
    public string FromDateValue { get; set; } = string.Empty;
    public string ToDateValue { get; set; } = string.Empty;
    public string FilterStr { get; set; } = string.Empty;
    public bool Detail { get; set; } = true;
}
