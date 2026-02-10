Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Services
Imports System.Web.Script.Serialization
Imports Connection

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
<System.Web.Script.Services.ScriptService()>
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class WebService_ReceiptGRNApproval
    Inherits System.Web.Services.WebService

    Dim db As New DBConnection
    Dim js As New JavaScriptSerializer()
    Dim data As New HelloWorldData()
    Dim dataTable As New DataTable()
    Dim str As String

    Dim GBLUserID As String
    Dim GBLCompanyID As String
    Dim GBLFYear As String
    Dim DBType As String = ""
    Dim ProductionUnitIDStr As String = ""
    Dim ProductionUnitID As String = ""

    '---------------Open Master code---------------------------------
    '-----------------------------------Get Receipt Vouchers List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function FillGrid(ByVal RadioValue As String, ByVal FromDate As String, ByVal ToDate As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
            Dim strFilter As String = ""
            If RadioValue = "Pending Receipt Note" Then
                If DBType = "MYSQL" Then
                    str = "Select IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,ROUND(SUM(IFNULL(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,IFNULL(ITM.ReceivedBy,0) AS ReceivedBy " &
                          " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID " &
                          " Where ITM.VoucherID=-14 /*And ITM.FYear='" & GBLFYear & "'*/ And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)<>1 And IFNULL(ITD.IsVoucherItemApproved,0)<>1 And IFNULL(ITD.QCApprovalNo,'')='' " &
                          " GROUP BY IFNULL(ITM.TransactionID,0),IFNULL(ITD.PurchaseTransactionID,0),IFNULL(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITMP.VoucherNo,''),Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.DeliveryNoteNo,''),Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.GateEntryNo,''),Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),IFNULL(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),IFNULL(ITM.ReceivedBy,0) Order By  FYear,MaxVoucherNo Desc"
                Else
                    If FromDate <> "" And ToDate <> "" Then
                        strFilter = " And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) "
                    End If
                    str = "Select Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo, " &
                            "Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo, " &
                            "NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,Isnull(ITM.ReceivedBy,0) AS ReceivedBy,PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName  " &
                            "From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID " &
                            "INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID  LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy  Where ITM.VoucherID=-14 /*AND ITM.FYear='" & GBLFYear & "'*/ And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") and isnull(ITM.IsDeletedTransaction,0)<>1 AND Isnull(ITD.IsVoucherItemApproved,0)<>1 AND Isnull(ITD.QCApprovalNo,'')=''   " &
                            "GROUP BY Isnull(ITM.TransactionID,0),Isnull(ITD.PurchaseTransactionID,0),Isnull(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),  NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),NullIf(ITM.DeliveryNoteNo,''),Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),NullIf(ITM.GateEntryNo,''),Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),Isnull(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0),PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName Order By  FYear,MaxVoucherNo Desc "
                End If

            Else
                If DBType = "MYSQL" Then
                    str = "Select IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,ROUND(SUM(IFNULL(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,IFNULL(ITM.ReceivedBy,0) AS ReceivedBy,Nullif(UA.UserName,'') AS ApprovedBy,Convert(date_format(IfNULL(ITD.VoucherItemApprovedDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ApprovalDate " &
                          " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UA ON UA.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID  " &
                          " Where ITM.VoucherID=-14 And ITM.FYear='" & GBLFYear & "' And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)<>1 And IFNULL(ITD.IsVoucherItemApproved,0)<>0 And IFNULL(ITD.QCApprovalNo,'')<>'' " &
                          " GROUP BY IFNULL(ITM.TransactionID,0),IFNULL(ITD.PurchaseTransactionID,0),IFNULL(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)), NullIf(ITMP.VoucherNo,''),Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITMP.VoucherNo,''),NullIf(ITM.DeliveryNoteNo,''),Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.GateEntryNo,''),Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),IFNULL(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),IFNULL(ITM.ReceivedBy,0),Nullif(UA.UserName,''),Convert(date_format(IfNULL(ITD.VoucherItemApprovedDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) Order By  FYear,MaxVoucherNo Desc"
                Else
                    If FromDate <> "" And ToDate <> "" Then
                        strFilter = " And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) "
                    End If
                    str = "Select Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo, " &
                            "Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo, " &
                            "NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,Isnull(ITM.ReceivedBy,0) AS ReceivedBy,Nullif(UA.UserName,'') AS ApprovedBy,Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName  " &
                            "From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID  LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy  LEFT JOIN UserMaster AS UA ON UA.UserID=ITD.VoucherItemApprovedBy Where ITM.VoucherID=-14 AND ITM.FYear='" & GBLFYear & "' And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And isnull(ITM.IsDeletedTransaction,0)<>1 AND Isnull(ITD.IsVoucherItemApproved,0)<>0 AND Isnull(ITD.QCApprovalNo,'')<>''  " &
                            "GROUP BY Isnull(ITM.TransactionID,0),Isnull(ITD.PurchaseTransactionID,0),Isnull(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),  NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),NullIf(ITM.DeliveryNoteNo,''),Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),NullIf(ITM.GateEntryNo,''),Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''),NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),Isnull(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0),Nullif(UA.UserName,''),Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-'),PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName Order By  FYear,MaxVoucherNo Desc "
                End If


            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------Get Receipt Note Batch Details------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetReceiptVoucherBatchDetail(ByVal TransactionID As String, ByVal RadioValue As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
            If RadioValue = "Pending Receipt Note" Then
                If DBType = "MYSQL" Then
                    str = "Select IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITD.ItemID,0) AS ItemID,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IM.ItemName,'') AS ItemName,NullIf(IM.ItemDescription,'') AS ItemDescription,IFNULL(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.ChallanQuantity,0) AS ChallanQuantity,IFNULL(ITD.ChallanQuantity,0) AS ApprovedQuantity,0 AS RejectedQuantity,NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(ITD.StockUnit,'') AS StockUnit,IFNULL(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,IFNULL(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,0 AS SizeW,IFNULL(ITD.WarehouseID,0) AS WarehouseID,Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,IFNULL(UOM.DecimalPlace,0)  AS UnitDecimalPlace " &
                          " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID AND ITMPD.ItemID=IM.ItemID AND ITMPD.TransactionID=ITD.PurchaseTransactionID AND ITMPD.CompanyID=ITMP.CompanyID INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit AND UOM.CompanyID=IM.CompanyID " &
                          " Where ITM.VoucherID=-14 And ITM.TransactionID=" & TransactionID & " And  ITM.CompanyID=" & GBLCompanyID & " Order By TransID"
                Else
                    str = "Select ITM.TransactionID, ITD.BatchID, ITD.BatchNo,ITD.SupplierBatchNo, IQC.VoucherNo AS VOUCHERNO,Isnull(IQC.QCTransactionID,0) AS QCTransactionID, NullIf(ISGM.ItemSubGroupID,'') AS ItemSubGroupID,  Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITD.ItemID,0) AS ItemID,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IM.ItemName,'') AS ItemName,NullIf(IM.ItemDescription,'') AS ItemDescription,Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.ChallanQuantity,0) AS ChallanQuantity,Isnull(ITD.ChallanQuantity,0) AS ApprovedQuantity,0 AS RejectedQuantity,NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(ITD.StockUnit,'') AS StockUnit,Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,0 AS SizeW,Isnull(ITD.WarehouseID,0) AS WarehouseID,Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,Isnull(UOM.DecimalPlace,0)  AS UnitDecimalPlace ,NullIf(IM.Quality,'') AS ItemQuality " &
                          " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID  INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID  INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID  INNER JOIN ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID AND ITMPD.ItemID=IM.ItemID AND ITMPD.TransactionID=ITD.PurchaseTransactionID  INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID  LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit  LEFT JOIN ItemQCInspectionMain AS IQC ON IQC.GRNTransactionID = ITD.TransactionID and IQC.BatchID = ITD.BatchID and  IQC.ItemID = ITD.ItemID AND Isnull(IQC.IsDeletedTransaction,0)=0 " &
                          " Where ITM.VoucherID=-14 And ITM.TransactionID=" & TransactionID & " Order By TransID"
                End If

            Else
                If DBType = "MYSQL" Then
                    str = "Select IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITD.ItemID,0) AS ItemID,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IM.ItemName,'') AS ItemName,NullIf(IM.ItemDescription,'') AS ItemDescription,IFNULL(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.ChallanQuantity,0) AS ChallanQuantity,IFNULL(ITD.ApprovedQuantity,0) AS ApprovedQuantity,IFNULL(ITD.RejectedQuantity,0) AS RejectedQuantity,nullif(ITD.QCApprovalNO,'') AS QCApprovalNO,nullif(ITD.QCApprovedNarration,'') AS QCApprovedNarration, NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(ITD.StockUnit,'') AS StockUnit,IFNULL(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,IFNULL(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,0 AS SizeW,IFNULL(ITD.WarehouseID,0) AS WarehouseID,Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,IFNULL(UOM.DecimalPlace,0)  AS UnitDecimalPlace " &
                          " From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD On ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID AND ITMPD.ItemID=IM.ItemID AND ITMPD.TransactionID=ITD.PurchaseTransactionID AND ITMPD.CompanyID=ITMP.CompanyID  INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit AND UOM.CompanyID=IM.CompanyID " &
                          " Where ITM.VoucherID=-14 And ITM.TransactionID=" & TransactionID & " And  ITM.CompanyID=" & GBLCompanyID & " Order By TransID"
                Else
                    str = "Select ITM.TransactionID, ITD.BatchID, ITD.BatchNo,ITD.SupplierBatchNo,  IQC.VoucherNo AS VOUCHERNO,Isnull(IQC.QCTransactionID,0) AS QCTransactionID, NullIf(ISGM.ItemSubGroupID,'') AS ItemSubGroupID, Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITD.ItemID,0) AS ItemID,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IM.ItemName,'') AS ItemName,NullIf(IM.ItemDescription,'') AS ItemDescription,Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.ChallanQuantity,0) AS ChallanQuantity,Isnull(ITD.ApprovedQuantity,0) AS ApprovedQuantity,Isnull(ITD.RejectedQuantity,0) AS RejectedQuantity,nullif(ITD.QCApprovalNO,'') AS QCApprovalNO,nullif(ITD.QCApprovedNarration,'') AS QCApprovedNarration, NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(ITD.StockUnit,'') AS StockUnit,Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,0 AS SizeW,Isnull(ITD.WarehouseID,0) AS WarehouseID,Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,Isnull(UOM.DecimalPlace,0)  AS UnitDecimalPlace,NullIf(IM.Quality,'') AS ItemQuality " &
                      " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID AND ITMPD.ItemID=IM.ItemID AND ITMPD.TransactionID=ITD.PurchaseTransactionID AND ITMPD.CompanyID=ITMP.CompanyID  INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID AND WM.CompanyID=ITD.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID  LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit LEFT JOIN ItemQCInspectionMain AS IQC ON IQC.GRNTransactionID = ITD.TransactionID  AND IQC.ItemID = ITD.ItemID and IQC.BatchID = ITD.BatchID and  Isnull(IQC.IsDeletedTransaction,0)=0 " &
                      " Where ITM.VoucherID=-14 AND ITM.TransactionID=" & TransactionID & "  Order By TransID"
                End If

            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    ''----------------------------Open PurchaseOrder  Update Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateGRNApproval(ByVal jsonObjectsRecordGRNApproval As Object, ByVal GRNTransactionID As String, ByVal RadioButtonValue As String) As String

        Dim dt As New DataTable
        Dim KeyField As String
        Dim AddColName, wherecndtn, TableName As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If

        If db.CheckAuthories("ReceiptGRNApproval.aspx", GBLUserID, GBLCompanyID, "CanSave", GRNTransactionID) = False Then Return "You are not authorized to save..!"

        Try
            Dim dtExist As New DataTable
            If DBType = "MYSQL" Then
                str = "Select TransactionID From ItemTransactionDetail Where Ifnull(IsDeletedTransaction, 0) = 0 And ParentTransactionID = '" & GRNTransactionID & "' And TransactionID <> ParentTransactionID"
                db.FillDataTable(dtExist, str)
                If dtExist.Rows.Count > 0 Then
                    Return "Exist"
                End If

                TableName = "ItemTransactionDetail"
                AddColName = "VoucherItemApprovedBy=" & GBLUserID & ",VoucherItemApprovedDate=NOW()"
                wherecndtn = " Ifnull(IsDeletedTransaction, 0) = 0 And ProductionUnitID=" & ProductionUnitID
                KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordGRNApproval, TableName, AddColName, 3, wherecndtn)

                For i = 0 To jsonObjectsRecordGRNApproval.length - 1
                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES_UNIT_WISE( " & GBLCompanyID & ",0," & jsonObjectsRecordGRNApproval(i)("ItemID") & ");")
                Next
            Else

                If RadioButtonValue = "Approved Receipt Note" Then
                    str = "Select GRNTransactionID From ItemQCInspectionMain Where Isnull(IsDeletedTransaction, 0) = 0 And GRNTransactionID = '" & GRNTransactionID & "'"
                    db.FillDataTable(dtExist, str)
                    If dtExist.Rows.Count > 0 Then
                        Return "ExistSample"
                    End If
                    dtExist.Clear()
                    dtExist.Dispose()
                End If

                str = "Select TransactionID From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And ParentTransactionID = '" & GRNTransactionID & "' And TransactionID <> ParentTransactionID"
                db.FillDataTable(dtExist, str)
                If dtExist.Rows.Count > 0 Then
                    Return "Exist"
                End If

                TableName = "ItemTransactionDetail"
                AddColName = "VoucherItemApprovedBy=" & GBLUserID & ",VoucherItemApprovedDate=Getdate()"
                wherecndtn = " Isnull(IsDeletedTransaction, 0) = 0 And ProductionUnitID=" & ProductionUnitID
                KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordGRNApproval, TableName, AddColName, 3, wherecndtn)

                For i = 0 To jsonObjectsRecordGRNApproval.length - 1
                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & ",0," & jsonObjectsRecordGRNApproval(i)("ItemID") & "")
                Next
            End If
        Catch ex As Exception
            KeyField = "fail " & ex.Message
        End Try
        Return KeyField

    End Function

    '---------------PrintReceiptApproval---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function HeaderNAme(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        str = ""
        If DBType = "MYSQL" Then
            str = "Select CM.CompanyName,NULL AS POReference,NullIf(LM.GSTNo,'') AS GSTNo, NullIf(LM.MailingAddress,'') As SuppAddress ,ITM.TransactionID,IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) As LedgerID,IFNULL(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ReceiptVoucherDate, NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,  Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,ROUND(SUM(IFNULL(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,IFNULL(ITM.ReceivedBy,0) AS ReceivedBy " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID Inner join CompanyMaster As CM on CM.CompanyID= ITM.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID  And ITMP.CompanyID=ITD.CompanyID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID " &
                  " Where ITM.VoucherID=-14  And ITM.CompanyID='" & GBLCompanyID & "' and ITM.TransactionID='" & transactionID & "' And IFNULL(ITM.IsDeletedTransaction,0)<>1 And IFNULL(ITD.IsVoucherItemApproved,0)<>0 And IFNULL(ITD.QCApprovalNo,'')<>''  " &
                  " GROUP BY CM.CompanyName,NullIf(LM.GSTNo,''),NullIf(LM.MailingAddress,''), ITM.TransactionID,IFNULL(ITD.PurchaseTransactionID,0),  IFNULL(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITMP.VoucherNo,''),Convert(date_format(IfNULL(ITMP.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.DeliveryNoteNo,''),Convert(date_format(IfNULL(ITM.DeliveryNoteDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.GateEntryNo,''),Convert(date_format(IfNULL(ITM.GateEntryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''), NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),IFNULL(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),IFNULL(ITM.ReceivedBy,0) Order By ITM.TransactionID"
        Else
            str = "Select CM.CompanyName,NULL AS POReference,NullIf(LM.GSTNo,'') AS GSTNo, NullIf(LM.MailingAddress,'') As SuppAddress ,  " &
               " ITM.TransactionID,Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,  " &
               " Isnull(ITM.LedgerID,0) As LedgerID,Isnull(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(LM.LedgerName,'') AS LedgerName,  " &
               " NullIf(ITM.VoucherNo,'') AS ReceiptVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,  " &
               " NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo, Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,  " &
               " ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,  " &
               " Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,  " &
               " Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,   " &
               " NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,  " &
               " NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,Isnull(ITM.ReceivedBy,0) AS ReceivedBy    " &
               " From ItemTransactionMain AS ITM   " &
               " INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID   " &
               " Inner join CompanyMaster As CM on CM.CompanyID= ITM.CompanyID INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID And ITMP.CompanyID=ITD.CompanyID   " &
               " INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND  " &
               " LM.CompanyID=ITM.CompanyID INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID  " &
               " LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID Where ITM.VoucherID=-14 AND  " &
               " ITM.CompanyID='" & GBLCompanyID & "' and ITM.TransactionID='" & transactionID & "' And isnull(ITM.IsDeletedTransaction,0)<>1 AND Isnull(ITD.IsVoucherItemApproved,0)<>0 AND Isnull(ITD.QCApprovalNo,'')<>'' " &
               " GROUP BY CM.CompanyName,NullIf(LM.GSTNo,''),NullIf(LM.MailingAddress,''), ITM.TransactionID,Isnull(ITD.PurchaseTransactionID,0), Isnull(ITM.LedgerID,0),NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'),    " &
               " NullIf(ITMP.VoucherNo,''),Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-'),NullIf(ITM.DeliveryNoteNo,''), Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-'),NullIf(ITM.GateEntryNo,''),  " &
               " Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-'),NullIf(ITM.LRNoVehicleNo,''),NullIf(ITM.Transporter,''), NullIf(ITM.Narration,''),NullIf(EM.LedgerName,''),NullIf(LM.LedgerName,''),NullIf(ITM.FYear,''),  " &
               " Isnull(ITM.MaxVoucherNo,0),NullIf(UM.UserName,''),Isnull(ITM.ReceivedBy,0) Order By ITM.TransactionID"
        End If

        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    '-----------------------------------CheckPermission------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckPermission(ByVal TransactionID As String) As String
        Dim KeyField As String
        KeyField = ""
        Try

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            Dim dtExist As New DataTable
            Dim dtExist1 As New DataTable
            Dim SxistStr As String

            SxistStr = ""
            If DBType = "MYSQL" Then
                SxistStr = "Select TransactionID From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And ParentTransactionID = '" & TransactionID & "' And CompanyID = '" & GBLCompanyID & "' And TransactionID <> ParentTransactionID"
            Else
                SxistStr = "Select TransactionID From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And ParentTransactionID = '" & TransactionID & "' And CompanyID = '" & GBLCompanyID & "' And TransactionID <> ParentTransactionID"
            End If

            db.FillDataTable(dtExist, SxistStr)
            Dim E As Integer = dtExist.Rows.Count
            If E > 0 Then
                KeyField = "Exist"
            End If

            KeyField = KeyField

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetParentItemSubGroupID(ByVal UID As String) As String
        Dim GBLUndeSubGroupIDString As String = ""

        ShowParentItemSubGroupID(UID, GBLUndeSubGroupIDString)

        Return GBLUndeSubGroupIDString
    End Function

    Public Sub ShowParentItemSubGroupID(ByVal UID As String, ByRef GBLUndeSubGroupIDString As String)

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        Dim dt As New DataTable
        db.FillDataTable(dt, "Select UnderSubGroupID From ItemSubGroupMaster Where ItemSubGroupID=" & UID & " AND CompanyID=" & GBLCompanyID & " AND Isnull(IsDeletedTransaction,0)=0 AND UnderSubGroupID<>ItemSubGroupID AND ItemSubGroupID<>1")

        If dt.Rows.Count <= 0 Then
            GBLUndeSubGroupIDString = IIf(GBLUndeSubGroupIDString.Trim() = "", "", GBLUndeSubGroupIDString & ",") & Convert.ToString(UID)
        End If

        For i = 0 To dt.Rows.Count - 1
            ShowParentItemSubGroupID(IIf(IsDBNull(dt.Rows(i)(0)), 0, dt.Rows(i)(0)), GBLUndeSubGroupIDString)
            GBLUndeSubGroupIDString = IIf(GBLUndeSubGroupIDString.Trim() = "", "", GBLUndeSubGroupIDString & ",") & Convert.ToString(UID)
        Next
    End Sub

    '---------------Close Master code---------------------------------

    Public Class HelloWorldData
        Public Message As [String]
    End Class

End Class