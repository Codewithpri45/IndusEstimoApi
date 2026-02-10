namespace IndasEstimo.Application.DTOs.Inventory;

public class SavePurchaseRequisitionRequest
{
    public string Prefix { get; set; } = "PREQ";
    public long TransactionID { get; set; }
    public List<PurchaseRequisitionMainDto> RecordMain { get; set; } = new();
    public List<PurchaseRequisitionDetailDto> RecordDetail { get; set; } = new();
    public List<UpdateIndentDetailDto> UpdateIndentDetail { get; set; } = new();
    public List<UserApprovalProcessDto> UserApprovalProcess { get; set; } = new();
    public ValidateLoginUserDto? ObjvalidateLoginUser { get; set; }
}

public class ValidateLoginUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PurchaseRequisitionMainDto
{
    public long VoucherID { get; set; } = -9;
    public string VoucherNo { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public DateTime VoucherDate { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Narration { get; set; } = string.Empty;
}

public class PurchaseRequisitionDetailDto
{
    public long ItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int TransID { get; set; }
    public long ItemGroupID { get; set; }
    public decimal RequiredNoOfPacks { get; set; }
    public decimal QuantityPerPack { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal PurchaseQty { get; set; } // Added for userApprovalProcess consistency
    public string StockUnit { get; set; } = string.Empty;
    public string OrderUnit { get; set; } = string.Empty;
    public string ItemNarration { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public long RefJobBookingJobCardContentsID { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public decimal CurrentStockInStockUnit { get; set; }
    public decimal CurrentStockInPurchaseUnit { get; set; }
    public decimal PhysicalStock { get; set; }
    public decimal PhysicalStockInPurchaseUnit { get; set; }
    public int IsAuditApproved { get; set; }
    public int AuditApprovalRequired { get; set; }
}

public class UpdateIndentDetailDto
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public long RequisitionItemID { get; set; }
}

public class UserApprovalProcessDto
{
    public long ItemID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseQty { get; set; }
}


public record SavePurchaseRequisitionResponse(long TransactionID, string VoucherNo, string Message);

public class CloseIndentRequest
{
    public List<long> ItemID { get; set; } = new();
}

public class CloseRequisitionRequest
{
    public List<RequisitionItemDto> Requisitions { get; set; } = new();
}

public class RequisitionItemDto
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
}

public class JobCardDto
{
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long RefJobBookingJobCardContentsID { get; set; }
}

public class ClientListDto
{
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
}

public class RequisitionDataDto
{
    public long RequisitionTransactionID { get; set; }
    public int VoucherItemApproved { get; set; }
    public long RequisitionMaxVoucherNo { get; set; }
    public long RequisitionVoucherID { get; set; }
    public long TransactionID { get; set; }
    public long MaxVoucherNo { get; set; }
    public long VoucherID { get; set; }
    public long RequisitionItemID { get; set; }
    public long ItemID { get; set; }
    public int TransID { get; set; }
    public long ItemGroupID { get; set; }
    public long ItemSubGroupID { get; set; }
    public long ItemGroupNameID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string RequisitionVoucherNo { get; set; } = string.Empty;
    public string RequisitionVoucherDate { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string RequisitionItemCode { get; set; } = string.Empty;
    public string RequisitionItemName { get; set; } = string.Empty;
    public string RequisitionItemDescription { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal RequiredNoOfPacks { get; set; }
    public decimal QuantityPerPack { get; set; }
    public decimal PurchaseQty { get; set; }
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
    public string ItemNarration { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public int UnitDecimalPlace { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string JobCardNo { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long RefJobBookingJobCardContentsID { get; set; }
    public long JobBookingJobCardContentsID { get; set; }
    public string LastPurchaseDate { get; set; } = string.Empty;
    public decimal UnitPerPacking { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal SizeW { get; set; }
    public decimal GSM { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public long ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public class ItemLookupDto
{
    public long ItemID { get; set; }
    public long ItemGroupID { get; set; }
    public long ItemGroupNameID { get; set; }
    public long ItemSubGroupID { get; set; }
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public decimal GSM { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Finish { get; set; } = string.Empty;
    public decimal SizeW { get; set; }
    public decimal SizeL { get; set; }
    public decimal BookedStock { get; set; }
    public decimal AllocatedStock { get; set; }
    public decimal PhysicalStock { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public int UnitDecimalPlace { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public string ConversionFormula { get; set; } = string.Empty;
    public int ConvertedUnitDecimalPlace { get; set; }
    public string LastPurchaseDate { get; set; } = string.Empty;
}

public class CommentDataDto
{
    public string Remark { get; set; } = string.Empty;
}

public class GetCommentDataRequest
{
    public string PurchaseTransactionID { get; set; } = "0";
    public string RequisitionIDs { get; set; } = "0";
}

public class FillGridRequest
{
    public string RadioValue { get; set; } = string.Empty;
    public string FilterString { get; set; } = string.Empty;
    public string FromDateValue { get; set; } = string.Empty;
    public string ToDateValue { get; set; } = string.Empty;
}