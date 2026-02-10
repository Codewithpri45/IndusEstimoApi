using System;
using System.Collections.Generic;

namespace IndasEstimo.Application.DTOs.Inventory;

// Request DTOs

public class GetReceiptNoteListRequest
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
}

public class GetPreviousReceivedQuantityRequest
{
    public long PurchaseTransactionID { get; set; }
    public long ItemID { get; set; }
    public long GRNTransactionID { get; set; }
}

public class ValidateSupplierBatchRequest
{
    public int VoucherID { get; set; }
    public List<SupplierBatchItem> Items { get; set; } = new();
}

public class SupplierBatchItem
{
    public string SupplierBatchNo { get; set; } = string.Empty;
    public long LedgerID { get; set; }
}

public class SaveReceiptDataRequest
{
    public string Prefix { get; set; } = string.Empty;
    public int VoucherID { get; set; }
    public GRNMainData MainData { get; set; } = new();
    public List<GRNDetailData> DetailData { get; set; } = new();
    public List<CompletePOItem> CompletedPOItems { get; set; } = new();
}

public class UpdateReceiptDataRequest
{
    public long TransactionID { get; set; }
    public GRNMainData MainData { get; set; } = new();
    public List<GRNDetailData> DetailData { get; set; } = new();
    public List<CompletePOItem> CompletedPOItems { get; set; } = new();
    public UserValidation UserValidation { get; set; } = new();
}

public class DeleteGRNRequest
{
    public long TransactionID { get; set; }
    public List<CompletePOItem> CompletedPOItems { get; set; } = new();
    public UserValidation UserValidation { get; set; } = new();
}

public class UserValidation
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TransactionRemark { get; set; } = string.Empty;
}

public class GRNMainData
{
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public string VoucherDate { get; set; } = string.Empty;
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string DeliveryNoteDate { get; set; } = string.Empty;
    public string GateEntryNo { get; set; } = string.Empty;
    public string GateEntryDate { get; set; } = string.Empty;
    public string LRNoVehicleNo { get; set; } = string.Empty;
    public string Transporter { get; set; } = string.Empty;
    public long ReceivedBy { get; set; }
    public string Narration { get; set; } = string.Empty;
    public long GateEntryTransactionID { get; set; }
    public string BiltyNo { get; set; } = string.Empty;
    public string BiltyDate { get; set; } = string.Empty;
    public string EWayBillNumber { get; set; } = string.Empty;
    public string EWayBillDate { get; set; } = string.Empty;
}

public class GRNDetailData
{
    public long PurchaseTransactionID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public decimal ChallanQuantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string MfgDate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
    public decimal ReceiptWtPerPacking { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public long JobBookingID { get; set; }
}

public class CompletePOItem
{
    public long PurchaseTransactionID { get; set; }
    public long ItemID { get; set; }
}

// Response DTOs

public class PurchaseSupplierDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
}

public class PendingPurchaseOrderDto
{
    public long TransactionID { get; set; }
    public long ClientID { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int VoucherID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public long MaxVoucherNo { get; set; }
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemGroupName { get; set; } = string.Empty;
    public string ItemSubGroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public decimal PendingQty { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal PurchaseTolerance { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public string FYear { get; set; } = string.Empty;
    public decimal ApprovedQuantity { get; set; }
    public int IsVoucherItemApproved { get; set; }
    public string PurchaseDivision { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public decimal SizeW { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public string FormulaStockToPurchaseUnit { get; set; } = string.Empty;
    public int UnitDecimalPlacePurchaseUnit { get; set; }
    public decimal ReceiptQuantity { get; set; }
    public string FormulaPurchaseToStockUnit { get; set; } = string.Empty;
    public int UnitDecimalPlaceStockUnit { get; set; }
    public decimal GSM { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyID { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string BiltyNo { get; set; } = string.Empty;
    public string BiltyDate { get; set; } = string.Empty;
}

public class ReceiptNoteListDto
{
    public long TransactionID { get; set; }
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long PurchaseTransactionID { get; set; }
    public string Remark { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public long MaxVoucherNo { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string ReceiptVoucherNo { get; set; } = string.Empty;
    public string ReceiptVoucherDate { get; set; } = string.Empty;
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public decimal ChallanQuantity { get; set; }
    public string DeliveryNoteNo { get; set; } = string.Empty;
    public string DeliveryNoteDate { get; set; } = string.Empty;
    public string GateEntryNo { get; set; } = string.Empty;
    public string GateEntryDate { get; set; } = string.Empty;
    public string LRNoVehicleNo { get; set; } = string.Empty;
    public string Transporter { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public long GateEntryTransactionID { get; set; }
    public string FYear { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public long ReceivedBy { get; set; }
    public int IsVoucherItemApproved { get; set; }
    public bool IsGRNApprovalRequired { get; set; }
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyID { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string BiltyNo { get; set; } = string.Empty;
    public string BiltyDate { get; set; } = string.Empty;
    public string EWayBillNumber { get; set; } = string.Empty;
    public string EWayBillDate { get; set; } = string.Empty;
}

public class ReceiptVoucherBatchDetailDto
{
    public string MFGdate { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string RefJobCardContentNo { get; set; } = string.Empty;
    public long PurchaseTransactionID { get; set; }
    public long LedgerID { get; set; }
    public long TransID { get; set; }
    public long ItemID { get; set; }
    public int ItemGroupID { get; set; }
    public int ItemSubGroupID { get; set; }
    public int ItemGroupNameID { get; set; }
    public string PurchaseVoucherNo { get; set; } = string.Empty;
    public string PurchaseVoucherDate { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal ChallanQuantity { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public string StockUnit { get; set; } = string.Empty;
    public decimal ReceiptWtPerPacking { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal WtPerPacking { get; set; }
    public decimal UnitPerPacking { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal SizeW { get; set; }
    public long WarehouseID { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal ReceiptQuantity { get; set; }
    public string FormulaStockToPurchaseUnit { get; set; } = string.Empty;
    public int UnitDecimalPlacePurchaseUnit { get; set; }
    public string FormulaPurchaseToStockUnit { get; set; } = string.Empty;
    public int UnitDecimalPlaceStockUnit { get; set; }
    public decimal GSM { get; set; }
    public decimal ReleaseGSM { get; set; }
    public decimal AdhesiveGSM { get; set; }
    public decimal Thickness { get; set; }
    public decimal Density { get; set; }
    public string ItemSubGroupName { get; set; } = string.Empty;
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public class PreviousReceivedQuantityDto
{
    public long TransactionID { get; set; }
    public long ItemID { get; set; }
    public decimal PurchaseTolerance { get; set; }
    public decimal PurchaseOrderQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal PreReceiptQuantity { get; set; }
    public string StockUnit { get; set; } = string.Empty;
    public string FormulaPurchaseToStockUnit { get; set; } = string.Empty;
    public int UnitDecimalPlaceStockUnit { get; set; }
    public int ProductionUnitID { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public class ReceiverDto
{
    public long LedgerID { get; set; }
    public string LedgerName { get; set; } = string.Empty;
}

public class WarehouseDto
{
    public string Warehouse { get; set; } = string.Empty;
}

public class BinDto
{
    public string Bin { get; set; } = string.Empty;
    public long WarehouseID { get; set; }
}

public class GatePassDto
{
    public long TransactionID { get; set; }
    public int VoucherID { get; set; }
    public string DCNo { get; set; } = string.Empty;
    public string VoucherNo { get; set; } = string.Empty;
    public string VoucherDate { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string GateEntryType { get; set; } = string.Empty;
    public string VehicleNo { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public long LedgerID { get; set; }
    public string MaterialSentTo { get; set; } = string.Empty;
    public string SendThrough { get; set; } = string.Empty;
    public string SendThroughName { get; set; } = string.Empty;
    public long GatePassTransactionID { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
}

public class GRNItemDto
{
    public string VoucherDate { get; set; } = string.Empty;
    public long TransactionID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public string SupplierBatchNo { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public long ItemID { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPerPacking { get; set; }
}

public class UserAuthorityDto
{
    public bool CanReceiveExcessMaterial { get; set; }
}