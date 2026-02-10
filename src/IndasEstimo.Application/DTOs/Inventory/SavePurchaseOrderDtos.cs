namespace IndasEstimo.Application.DTOs.Inventory;
/// <summary>
/// Main request DTO - matches your JSON structure
/// </summary>
public record SavePurchaseOrderRequest(
    string Prefix,
    List<PurchaseOrderMainDto> JsonObjectsRecordMain,
    List<PurchaseOrderDetailDto> JsonObjectsRecordDetail,
    List<PurchaseOrderOverheadDto> JsonObjectsRecordOverHead,
    List<PurchaseOrderTaxDto> JsonObjectsRecordTax,
    List<PurchaseOrderScheduleDto> JsonObjectsRecordSchedule,
    List<PurchaseOrderRequisitionDto> JsonObjectsRecordRequisition,
    string TxtNetAmt,
    string CurrencyCode,
    List<UserApprovalDto>? UserApprovalProcessArray,
    List<AttachmentDto>? FilejsonObjectsTransactionMain
);
public record UpdatePurchaseOrderRequest(
    long TransactionID,
    string Prefix,
    List<PurchaseOrderMainDto> JsonObjectsRecordMain,
    List<PurchaseOrderDetailDto> JsonObjectsRecordDetail,
    List<PurchaseOrderOverheadDto> JsonObjectsRecordOverHead,
    List<PurchaseOrderTaxDto> JsonObjectsRecordTax,
    List<PurchaseOrderScheduleDto> JsonObjectsRecordSchedule,
    List<PurchaseOrderRequisitionDto> JsonObjectsRecordRequisition,
    string TxtNetAmt,
    string CurrencyCode,
    List<UserApprovalDto>? UserApprovalProcessArray,
    List<AttachmentDto>? FilejsonObjectsTransactionMain
);
public record PurchaseOrderMainDto(
    int VoucherID,
    DateTime VoucherDate,
    long LedgerID,
    long ContactPersonID,
    decimal TotalQuantity,
    decimal TotalBasicAmount,
    decimal TotalCGSTTaxAmount,
    decimal TotalSGSTTaxAmount,
    decimal TotalIGSTTaxAmount,
    decimal TotalTaxAmount,
    decimal NetAmount,
    decimal TotalOverheadAmount,
    int PurchaseDivision,
    string? PurchaseReferenceRemark,
    string? DeliveryAddress,
    string? TermsOfPayment,
    string CurrencyCode,
    int ModeOfTransport,
    long DealerID,
    long VoucherApprovalByEmployeeID
);
public record PurchaseOrderDetailDto(
    long ItemID,
    int TransID,
    long ItemGroupID,
    decimal RequiredQuantity,
    decimal RequiredNoOfPacks,
    decimal QuantityPerPack,
    decimal PurchaseOrderQuantity,
    decimal ChallanWeight,
    string PurchaseUnit,
    string StockUnit,
    string? ItemDescription,
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
    string? ItemNarration,
    DateTime? ExpectedDeliveryDate,
    long RefJobBookingJobCardContentsID,
    string? RefJobCardContentNo,
    long ClientID,
    string? Remark,
    long ProductHSNID
);
public record PurchaseOrderOverheadDto(
    long HeadID,
    int TransID,
    string HeadName,
    decimal Quantity,
    string ChargesType,
    decimal Rate,
    decimal Amount
);
public record PurchaseOrderTaxDto(
    int TransID,
    long LedgerID,
    decimal TaxPercentage,
    decimal Amount,
    bool TaxInAmount,
    bool IsComulative,
    bool GSTApplicable,
    string CalculatedON
);
public record PurchaseOrderScheduleDto(
    int TransID,
    long ItemID,
    decimal Quantity,
    string Unit,
    DateTime ScheduleDeliveryDate
);
public record PurchaseOrderRequisitionDto(
    int TransID,
    long ItemID,
    decimal RequisitionProcessQuantity,
    string StockUnit,
    long RequisitionTransactionID
);
public record UserApprovalDto(
    long LedgerID,
    string LedgerName,
    decimal ItemRate,
    decimal ItemAmount,
    long ItemID,
    string ItemName,
    string ItemCode,
    DateTime ExpectedDeliveryDate,
    decimal PurchaseQty
);
public record AttachmentDto(
    string AttachmentFilesName,
    string? AttachedFileRemark
);
/// <summary>
/// Response DTO
/// </summary>
public record SavePurchaseOrderResponse(
    long TransactionID,
    string VoucherNo,
    string Message
);

// ==================== Retrieve/Lookup DTOs ====================

/// <summary>
/// Complete PO data for editing - matches RetrivePoCreateGrid WebMethod
/// </summary>
public class PurchaseOrderDataDto
{
    public long TransactionID { get; set; }
    public long TransID { get; set; }
    public int VoucherID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long ContactPersonID { get; set; }
    public string ContactPersonName { get; set; } = string.Empty;
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal RequiredNoOfPacks { get; set; }
    public decimal QuantityPerPack { get; set; }
    public decimal PurchaseOrderQuantity { get; set; }
    public decimal ChallanWeight { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal PurchaseRate { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string ItemNarration { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public long ProductHSNID { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalBasicAmount { get; set; }
    public decimal TotalCGSTTaxAmount { get; set; }
    public decimal TotalSGSTTaxAmount { get; set; }
    public decimal TotalIGSTTaxAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalOverheadAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int PurchaseDivision { get; set; }
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string TermsOfPayment { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public int ModeOfTransport { get; set; }
    public long DealerID { get; set; }
    public long VoucherApprovalByEmployeeID { get; set; }
    public string Narration { get; set; } = string.Empty;
    public int IsVoucherItemApproved { get; set; }
    public long VoucherItemApprovedBy { get; set; }
    public DateTime? VoucherItemApprovedDate { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public string ConversionFormula { get; set; } = string.Empty;
}

/// <summary>
/// PO list/grid item - matches ProcessFillGrid WebMethod
/// </summary>
public class PurchaseOrderListDto
{
    public long TransactionID { get; set; }
    public long TransID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int IsVoucherItemApproved { get; set; }
    public decimal PendingToReceiveQty { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string PurchaseReferenceRemark { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
}

/// <summary>
/// Pending requisitions for PO creation - matches FillGrid WebMethod
/// </summary>
public class PendingRequisitionDto
{
    public long TransactionID { get; set; }
    public long TransID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public long ItemGroupID { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal PurchaseQuantity { get; set; }
    public decimal PendingQuantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long ProductHSNID { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal GSTPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public string ConversionFormula { get; set; } = string.Empty;
}

/// <summary>
/// Supplier/Ledger lookup - matches Supplier WebMethod
/// </summary>
public class SupplierDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string GSTNo { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public bool GSTApplicable { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string StateTinNo { get; set; } = string.Empty;
}

/// <summary>
/// Contact person lookup - matches GetContactPerson WebMethod
/// </summary>
public class ContactPersonDto
{
    public long ConcernPersonID { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Delivery address lookup - matches SelectAddressGetData WebMethod
/// </summary>
public class DeliveryAddressDto
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public long CompanyID { get; set; }
}

/// <summary>
/// Overhead charge head - matches HeadFun WebMethod
/// </summary>
public class OverheadChargeHeadDto
{
    public long HeadID { get; set; }
    public string HeadName { get; set; } = string.Empty;
}

/// <summary>
/// Tax charge ledger - matches CHLname WebMethod
/// </summary>
public class TaxChargeLedgerDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public bool GSTApplicable { get; set; }
    public string GSTLedgerType { get; set; } = string.Empty;
}

/// <summary>
/// Currency lookup - matches GetCurrencyList WebMethod
/// </summary>
public class CurrencyDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyHeadName { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}

/// <summary>
/// HSN code lookup - matches GetAllHSN WebMethod
/// </summary>
public class HSNCodeDto
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
/// File attachment - matches GetFiledata WebMethod
/// </summary>
public class AttachmentFileDto
{
    public long AttachmentFileID { get; set; }
    public string AttachmentFilesName { get; set; } = string.Empty;
    public string AttachedFileRemark { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for ProcessFillGrid
/// </summary>
public class GetPurchaseOrderListRequest
{
    public string FromDateValue { get; set; } = string.Empty;
    public string ToDateValue { get; set; } = string.Empty;
    public string FilterStr { get; set; } = string.Empty;
    public bool Detail { get; set; } = true;
}