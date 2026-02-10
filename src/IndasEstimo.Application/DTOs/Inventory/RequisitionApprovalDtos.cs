namespace IndasEstimo.Application.DTOs.Inventory;

// ==================== Request DTOs ====================

/// <summary>
/// Request for getting requisition lists (Unapproved/Approved/Cancelled)
/// </summary>
public class GetRequisitionListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

/// <summary>
/// Request for approving requisitions
/// </summary>
public class ApproveRequisitionRequest
{
    public List<RequisitionApprovalItem> RequisitionItems { get; set; } = new();
}

public class RequisitionApprovalItem
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long TransID { get; set; }
}

/// <summary>
/// Request for cancelling requisitions
/// </summary>
public class CancelRequisitionRequest
{
    public List<RequisitionCancellationItem> RequisitionItems { get; set; } = new();
}

public class RequisitionCancellationItem
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long TransID { get; set; }
}

// ==================== Response DTOs ====================

/// <summary>
/// Unapproved requisition item - matches UnApprovedRequisitions WebMethod
/// </summary>
public class UnapprovedRequisitionDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long ItemGroupID { get; set; }
    public long ItemID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string ItemNarration { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
}

/// <summary>
/// Approved requisition item - matches ApprovedRequisitions WebMethod
/// </summary>
public class ApprovedRequisitionDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long ItemGroupID { get; set; }
    public long ItemID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string ItemNarration { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalDate { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
    public long PurchaseTransactionID { get; set; }
}

/// <summary>
/// Cancelled requisition item - matches CancelledRequisitions WebMethod
/// </summary>
public class CancelledRequisitionDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long ItemGroupID { get; set; }
    public long ItemID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string ItemNarration { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string ApprovalDate { get; set; } = string.Empty;
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long CompanyID { get; set; }
    public long PurchaseTransactionID { get; set; }
}
