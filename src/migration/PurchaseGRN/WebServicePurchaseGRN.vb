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
Public Class WebServicePurchaseGRN
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
    '-----------------------------------Get Supplier List From Purchase------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetPurchaseSuppliersList() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            str = "Select Distinct LM.LedgerID,LM.LedgerName From ItemTransactionMain AS ITM INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID Inner Join LedgerGroupMaster AS LGM On LGM.LedgerGroupID=LM.LedgerGroupID And LGM.CompanyID=LM.CompanyID  AND LGM.LedgerGroupNameID=23 Where ITM.CompanyID=" & GBLCompanyID & " AND ITM.VoucherID=-11 Order By LM.LedgerName"
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    '-----------------------------------Get Pending Purchase List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetPendingOrdersList() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                str = "Select ITM.TransactionID,ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,IGM.ItemGroupNameID,LM.LedgerName,ITM.MaxVoucherNo,ITM.VoucherNo AS PurchaseVoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As PurchaseVoucherDate,IM.ItemCode ,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,IFNULL(ITD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,IFNULL(ITD.PurchaseOrderQuantity,0) AS PendingQty, IFNULL(IM.PurchaseUnit,'') AS PurchaseUnit,IFNULL(IM.StockUnit,'') AS StockUnit,IFNULL(ITD.PurchaseTolerance,0) AS PurchaseTolerance,NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy,NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,NullIf(ITD.FYear,'') AS FYear,NullIf(ITM.PurchaseDivision,'') AS PurchaseDivision,NULLIf(ITM.PurchaseReferenceRemark,'') AS PurchaseReferenceRemark,IFNULL(IM.SizeW,1) AS SizeW,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,Nullif(C.ConversionFormula,'') AS FormulaStockToPurchaseUnit,IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlacePurchaseUnit,(Select ROUND(Sum(IFNULL(ChallanQuantity,0)),3) From ItemTransactionDetail Where PurchaseTransactionID=ITD.TransactionID AND ItemID=ITD.ItemID AND IFNULL(IsDeletedTransaction,0)<>1) AS ReceiptQuantity,Nullif(CU.ConversionFormula,'') AS FormulaPurchaseToStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,IFNULL(IM.GSM,0) AS GSM,IFNULL(IM.ReleaseGSM,0) AS ReleaseGSM,IFNULL(IM.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(IM.Thickness,0) AS Thickness,IFNULL(IM.Density,0) AS Density " &
                      " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID AND IFNULL(ITM.IsDeletedTransaction,0)=0 INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ConversionMaster AS C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit LEFT JOIN ConversionMaster AS CU ON CU.BaseUnitSymbol=IM.PurchaseUnit AND CU.ConvertedUnitSymbol=IM.StockUnit " &
                      " Where ITM.VoucherID= -11 AND ITM.CompanyID=" & GBLCompanyID & " AND IFNULL(ITD.IsDeletedTransaction,0)<>1  AND IFNULL(ITD.IsCompleted,0)<>1 AND IFNULL(ITD.IsVoucherItemApproved,0)=1 Order By ITM.TransactionID Desc"
            Else
                str = " SELECT ITM.TransactionID, ITD.ClientID, LM1.LedgerName AS ClientName, ITM.VoucherID, ITM.LedgerID, ITD.TransID, ITD.ItemID, ITD.ItemGroupID, IM.ItemSubGroupID, IGM.ItemGroupNameID, LM.LedgerName, ITM.MaxVoucherNo,  ITM.VoucherNo AS PurchaseVoucherNo, REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate, IM.ItemCode, IGM.ItemGroupName, ISGM.ItemSubGroupName, IM.ItemName,  ISNULL(ITD.PurchaseOrderQuantity, 0) AS PurchaseOrderQuantity, ISNULL(ITD.PurchaseOrderQuantity, 0) AS PendingQty, ISNULL(IM.PurchaseUnit, '') AS PurchaseUnit, ISNULL(IM.StockUnit, '') AS StockUnit,  ISNULL(ITD.PurchaseTolerance, 0) AS PurchaseTolerance, NULLIF (ISNULL(UA.UserName, ''), '') AS CreatedBy, NULLIF (ISNULL(UM.UserName, ''), '') AS ApprovedBy, NULLIF (ITD.RefJobCardContentNo, '')  AS RefJobCardContentNo, NULLIF (ITD.FYear, '') AS FYear, ISNULL(ITD.ApprovedQuantity, 0) AS ApprovedQuantity, ISNULL(ITD.IsVoucherItemApproved, 0) AS IsVoucherItemApproved, NULLIF (ITM.PurchaseDivision, '')  AS PurchaseDivision, NULLIF (ITD.Remark, '') AS Remark, ISNULL(IM.SizeW, 1) AS SizeW, ISNULL(IM.WtPerPacking, 0) AS WtPerPacking, ISNULL(IM.UnitPerPacking, 1)  AS UnitPerPacking, ISNULL(IM.ConversionFactor, 1) AS ConversionFactor, NULLIF (C.ConversionFormula, '') AS FormulaStockToPurchaseUnit, ISNULL(C.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlacePurchaseUnit, (SELECT        ROUND(SUM(ISNULL(ChallanQuantity, 0)), 3) AS Expr1 FROM            ItemTransactionDetail WHERE        (PurchaseTransactionID = ITD.TransactionID) AND (ItemID = ITD.ItemID) AND (ISNULL(IsDeletedTransaction, 0) <> 1)) AS ReceiptQuantity, NULLIF (CU.ConversionFormula, '') AS FormulaPurchaseToStockUnit,  ISNULL(CU.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit, ISNULL(IM.GSM, 0) AS GSM, ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM, ISNULL(IM.AdhesiveGSM, 0) AS AdhesiveGSM, ISNULL(IM.Thickness, 0)  AS Thickness, ISNULL(IM.Density, 0) AS Density, PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName,CM.CompanyID,JB.JobName,isnull(ITM.BiltyNo,'') as BiltyNo,REPLACE(CONVERT(Varchar(13), ITM.BiltyDate, 106), ' ', '-') AS BiltyDate " &
                      " From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD ON ITM.TransactionID = ITD.TransactionID And ITM.CompanyID = ITD.CompanyID And ISNULL(ITM.IsDeletedTransaction, 0) = 0 INNER Join UserMaster As UA On UA.UserID = ITM.CreatedBy Inner Join ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID = ITD.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID INNER JOIN LedgerMaster AS LM ON LM.LedgerID = ITM.LedgerID LEFT OUTER JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID And ISNULL(ISGM.IsDeletedTransaction,0) = 0 LEFT OUTER JOIN UserMaster AS UM ON UM.UserID = ITD.VoucherItemApprovedBy LEFT OUTER JOIN ConversionMaster AS C ON C.BaseUnitSymbol = IM.StockUnit AND C.ConvertedUnitSymbol = IM.PurchaseUnit LEFT OUTER JOIN ConversionMaster AS CU ON CU.BaseUnitSymbol = IM.PurchaseUnit AND CU.ConvertedUnitSymbol = IM.StockUnit LEFT OUTER JOIN LedgerMaster AS LM1 ON LM1.LedgerID = ITD.ClientID  LEFT JOIN JobBooking AS JB ON JB.BookingID = ITD.JobBookingID AND ISNULL(JB.IsDeletedTransaction, 0) = 0 " &
                      " Where (ITM.VoucherID = -11) And (ISNULL(ITD.IsDeletedTransaction, 0) <> 1) And ITM.ProductionUnitID In(" & ProductionUnitIDStr & ") And  (ISNULL(ITD.IsCompleted, 0) <> 1) And (ISNULL(ITD.IsVoucherItemApproved, 0) = 1) ORDER BY ITM.TransactionID DESC "
                '  "Select ITM.TransactionID,ITD.ClientID,LM1.LedgerName as ClientName, ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,IM.ItemSubGroupID,IGM.ItemGroupNameID,LM.LedgerName,ITM.MaxVoucherNo,ITM.VoucherNo AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS PurchaseVoucherDate,IM.ItemCode ,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,Isnull(ITD.PurchaseOrderQuantity,0) AS PendingQty, Isnull(IM.PurchaseUnit,'') AS PurchaseUnit,Isnull(IM.StockUnit,'') AS StockUnit,Isnull(ITD.PurchaseTolerance,0) AS PurchaseTolerance,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy, " &
                '" nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,NullIf(ITD.FYear,'') AS FYear,Isnull(ITD.ApprovedQuantity,0) AS ApprovedQuantity,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved,NullIf(ITM.PurchaseDivision,'') AS PurchaseDivision,NULLIf(ITM.PurchaseReferenceRemark,'') AS PurchaseReferenceRemark,Isnull(IM.SizeW,1) AS SizeW,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,Nullif(C.ConversionFormula,'') AS FormulaStockToPurchaseUnit,ISNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlacePurchaseUnit,(Select ROUND(Sum(Isnull(ChallanQuantity,0)),3) From ItemTransactionDetail Where PurchaseTransactionID=ITD.TransactionID AND ItemID=ITD.ItemID AND Isnull(IsDeletedTransaction,0)<>1) AS ReceiptQuantity,Nullif(CU.ConversionFormula,'') AS FormulaPurchaseToStockUnit,ISNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density " &
                '" From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID AND Isnull(ITM.IsDeletedTransaction,0)=0 INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID  LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ConversionMaster AS C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit  LEFT JOIN ConversionMaster AS CU ON CU.BaseUnitSymbol=IM.PurchaseUnit AND CU.ConvertedUnitSymbol=IM.StockUnit  Left Join LedgerMaster as LM1 on LM1.LedgerID = ITD.ClientID " &
                '" Where ITM.VoucherID= -11 AND ITM.CompanyID=" & GBLCompanyID & " AND Isnull(ITD.IsDeletedTransaction,0)<>1  AND Isnull(ITD.IsCompleted,0)<>1 AND Isnull(ITD.IsVoucherItemApproved,0)=1 Order By ITM.TransactionID Desc "
            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    '-----------------------------------Get Receipt Vouchers List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetReceiptNoteList(ByVal fromDateValue As String, ByVal ToDateValue As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                str = "Select ITM.TransactionID,nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,ITD.PurchaseTransactionID,ITM.LedgerID,ITM.MaxVoucherNo,LM.LedgerName,ITM.VoucherNo AS ReceiptVoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(date_format(ITMP.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As PurchaseVoucherDate,ROUND(SUM(IFNULL(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Replace(Convert(date_format(ITM.DeliveryNoteDate,'%d-%b-%Y'),char(30)),' ','-') As DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Replace(Convert(date_format(ITM.GateEntryDate,'%d-%b-%Y'),char(30)),' ','-') As GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,IFNULL(ITM.ReceivedBy,0) AS ReceivedBy,ITD.IsVoucherItemApproved,CM.IsGRNApprovalRequired " &
                      " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID AND IFNULL(ITM.IsDeletedTransaction,0)=0 AND IFNULL(ITD.IsDeletedTransaction,0)=0 INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID AND IFNULL(ITMP.IsDeletedTransaction,0)=0 INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID And LM.IsDeletedTransaction=0 INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID And EM.IsDeletedTransaction=0 " &
                      " Where ITM.VoucherID=-14 AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND  ITM.CompanyID=" & GBLCompanyID & " " &
                      " GROUP BY ITM.TransactionID,ITD.PurchaseTransactionID,ITM.LedgerID,ITM.VoucherNo,ITM.VoucherDate,ITMP.VoucherNo,ITMP.VoucherDate,ITM.DeliveryNoteNo,ITM.DeliveryNoteDate,ITM.GateEntryNo,ITM.GateEntryDate,ITM.LRNoVehicleNo,ITM.Transporter,ITM.Narration,EM.LedgerName,LM.LedgerName,ITM.FYear,ITM.MaxVoucherNo,UM.UserName,ITM.ReceivedBy,ITD.RefJobCardContentNo,ITD.IsVoucherItemApproved Order By ITM.TransactionID Desc"
            Else
                str = " Select Isnull(ITM.EWayBillNumber,'') AS EWayBillNumber,Replace(Convert(Varchar(13),ITM.EWayBillDate,106),' ','-') AS EWayBillDate,ITM.TransactionID, NULLIF (ITD.RefJobCardContentNo, '') AS RefJobCardContentNo, ITD.PurchaseTransactionID, NULLIF (ITD.Remark, '') AS Remark ,ITM.LedgerID, ITM.MaxVoucherNo, LM.LedgerName, ITM.VoucherNo AS ReceiptVoucherNo,  REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS ReceiptVoucherDate, NULLIF (ITMP.VoucherNo, '') AS PurchaseVoucherNo, REPLACE(CONVERT(Varchar(13), ITMP.VoucherDate, 106), ' ', '-')  AS PurchaseVoucherDate, ROUND(SUM(ISNULL(ITD.ChallanQuantity, 0)), 2) As ChallanQuantity, NULLIF(ITM.DeliveryNoteNo, '') AS DeliveryNoteNo, REPLACE(CONVERT(Varchar(13), ITM.DeliveryNoteDate, 106), ' ', '-')  AS DeliveryNoteDate, NULLIF(ITM.GateEntryNo, '') AS GateEntryNo, REPLACE(CONVERT(Varchar(13), ITM.GateEntryDate, 106), ' ', '-') AS GateEntryDate, NULLIF (ITM.LRNoVehicleNo, '') AS LRNoVehicleNo,  NULLIF(ITM.Transporter, '') AS Transporter, NULLIF (EM.LedgerName, '') AS ReceiverName, NULLIF (ITM.Narration, '') AS Narration, ISNULL(ITM.GateEntryTransactionID, 0) AS GateEntryTransactionID, NULLIF (ITM.FYear, '')  AS FYear, NULLIF(UM.UserName, '') AS CreatedBy, ISNULL(ITM.ReceivedBy, 0) AS ReceivedBy, ITD.IsVoucherItemApproved, CM.IsGRNApprovalRequired, PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName,CM.CompanyID ,JB.JobName,isnull(ITM.BiltyNo,'') as BiltyNo,REPLACE(CONVERT(Varchar(13), ITM.BiltyDate, 106), ' ', '-') AS BiltyDate" &
                      " From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD On ITM.TransactionID = ITD.TransactionID And ITM.CompanyID = ITD.CompanyID And ISNULL(ITM.IsDeletedTransaction, 0) = 0 And ISNULL(ITD.IsDeletedTransaction, 0) = 0 Inner Join ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID INNER Join ItemTransactionMain As ITMP On ITMP.TransactionID = ITD.PurchaseTransactionID And ISNULL(ITMP.IsDeletedTransaction, 0) = 0 INNER Join LedgerMaster As LM On LM.LedgerID = ITM.LedgerID And LM.IsDeletedTransaction = 0 INNER Join UserMaster As UM On UM.UserID = ITM.CreatedBy INNER Join LedgerMaster As EM On EM.LedgerID = ITM.ReceivedBy And EM.IsDeletedTransaction = 0 LEFT JOIN JobBooking AS JB   ON JB.BookingID = ITD.JobBookingID AND ISNULL(JB.IsDeletedTransaction, 0) = 0" &
                      " Where (ITM.VoucherID = -14) And ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ")  " &
                      " Group BY Isnull(ITM.EWayBillNumber,''),Replace(Convert(Varchar(13),ITM.EWayBillDate,106),' ','-'),ITM.TransactionID, ITD.PurchaseTransactionID,ITD.Remark, ITM.LedgerID, ITM.VoucherNo, ITM.VoucherDate, ITMP.VoucherNo, ITMP.VoucherDate, ITM.DeliveryNoteNo, ITM.DeliveryNoteDate, ITM.GateEntryNo, ITM.GateEntryDate, ITM.LRNoVehicleNo, ITM.Transporter, ITM.Narration, ISNULL(ITM.GateEntryTransactionID, 0), EM.LedgerName, LM.LedgerName, ITM.FYear, ITM.MaxVoucherNo, UM.UserName, ITM.ReceivedBy, ITD.RefJobCardContentNo, ITD.IsVoucherItemApproved, CM.IsGRNApprovalRequired, PUM.ProductionUnitID, PUM.ProductionUnitName, CM.CompanyName, CM.CompanyID,JB.JobName,ITM.BiltyDate,ITM.BiltyNo ORDER BY FYear DESC,ITM.MaxVoucherNo Desc "
                '  "Select ITM.TransactionID,nullif(ITD.RefJobCardContentNo ,'') AS RefJobCardContentNo,ITD.PurchaseTransactionID,ITM.LedgerID,ITM.MaxVoucherNo,LM.LedgerName,ITM.VoucherNo AS ReceiptVoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS ReceiptVoucherDate,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,ROUND(SUM(Isnull(ITD.ChallanQuantity,0)),2) AS ChallanQuantity,NullIf(ITM.DeliveryNoteNo,'') AS DeliveryNoteNo,Replace(Convert(Varchar(13),ITM.DeliveryNoteDate,106),' ','-') AS DeliveryNoteDate,NullIf(ITM.GateEntryNo,'') AS GateEntryNo,Replace(Convert(Varchar(13),ITM.GateEntryDate,106),' ','-') AS GateEntryDate,NullIf(ITM.LRNoVehicleNo,'') AS LRNoVehicleNo,NullIf(ITM.Transporter,'') AS Transporter,NullIf(EM.LedgerName,'') AS ReceiverName,NullIf(ITM.Narration,'') AS Narration,Isnull(ITM.GateEntryTransactionID,0) AS GateEntryTransactionID,NullIf(ITM.FYear,'') AS FYear,NullIf(UM.UserName,'') AS CreatedBy,Isnull(ITM.ReceivedBy,0) AS ReceivedBy,ITD.IsVoucherItemApproved,CM.IsGRNApprovalRequired " &
                '" From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID AND Isnull(ITM.IsDeletedTransaction,0)=0 AND Isnull(ITD.IsDeletedTransaction,0)=0 INNER JOIN ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID AND ITMP.CompanyID=ITD.CompanyID AND Isnull(ITMP.IsDeletedTransaction,0)=0 INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID And LM.IsDeletedTransaction=0" &
                '" INNER JOIN UserMaster AS UM ON UM.UserID=ITM.CreatedBy AND UM.CompanyID=ITM.CompanyID INNER JOIN CompanyMaster As CM ON CM.CompanyID = ITM.CompanyID And CM.IsDeletedTransaction=0 LEFT JOIN LedgerMaster AS EM ON EM.LedgerID=ITM.ReceivedBy AND EM.CompanyID=ITM.CompanyID And EM.IsDeletedTransaction=0 Where ITM.VoucherID=-14 AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND  ITM.CompanyID=" & GBLCompanyID & " GROUP BY ITM.TransactionID,ITD.PurchaseTransactionID,ITM.LedgerID,ITM.VoucherNo,ITM.VoucherDate, " &
                '" ITMP.VoucherNo,ITMP.VoucherDate,ITM.DeliveryNoteNo,ITM.DeliveryNoteDate,ITM.GateEntryNo,ITM.GateEntryDate,ITM.LRNoVehicleNo,ITM.Transporter,ITM.Narration,Isnull(ITM.GateEntryTransactionID,0),EM.LedgerName,LM.LedgerName,ITM.FYear,ITM.MaxVoucherNo,UM.UserName,ITM.ReceivedBy,ITD.RefJobCardContentNo,ITD.IsVoucherItemApproved,CM.IsGRNApprovalRequired Order By ITM.TransactionID Desc "
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
    Public Function GetReceiptVoucherBatchDetail(ByVal TransactionID As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                str = "Select Replace(Convert(date_format(ITD.ExpiryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpiryDate,Replace(Convert(date_format(ITD.MfgDate,'%d-%b-%Y'),char(30)),' ','-') As MfgDate,nullif(ITD.SupplierBatchNo,'') AS SupplierBatchNo, nullif(ITMPD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITD.ItemID,0) AS ItemID,IFNULL(IM.ItemGroupID,0) As ItemGroupID,IFNULL(IGM.ItemGroupNameID,0) As ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(date_format(ITMP.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As PurchaseVoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IM.ItemName,'') AS ItemName,IFNULL(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.ChallanQuantity, 0) As ChallanQuantity, NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(IM.StockUnit,'') AS StockUnit,IFNULL(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,IFNULL(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking, 1) As UnitPerPacking, IFNULL(IM.ConversionFactor, 1) As ConversionFactor, IFNULL(IM.SizeW, 1) As SizeW, IFNULL(ITD.WarehouseID, 0) As WarehouseID, Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,IFNULL((Select Sum(IFNULL(ChallanQuantity,0))  From ItemTransactionDetail Where IFNULL(IsDeletedTransaction,0)=0 AND IFNULL(PurchaseTransactionID,0)>0 AND IFNULL(ChallanQuantity,0)>0 AND PurchaseTransactionID=ITMPD.TransactionID AND ItemID=ITMPD.ItemID),0) AS ReceiptQuantity,Nullif(CM.ConversionFormula,'') AS FormulaStockToPurchaseUnit,IFNULL(CM.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlacePurchaseUnit,Nullif(CU.ConversionFormula,'') AS FormulaPurchaseToStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,IFNULL(IM.GSM,0) AS GSM,IFNULL(IM.ReleaseGSM,0) AS ReleaseGSM,IFNULL(IM.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(IM.Thickness,0) AS Thickness,IFNULL(IM.Density,0) AS Density " &
                      " From ItemTransactionMain As ITM INNER Join ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID AND IFNULL(ITM.IsDeletedTransaction,0)=0 AND IFNULL(ITD.IsDeletedTransaction,0)=0 INNER Join ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER Join ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=IM.CompanyID INNER Join ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID And ITMP.CompanyID=ITD.CompanyID INNER Join ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID And ITMPD.ItemID=IM.ItemID And ITMPD.TransactionID=ITD.PurchaseTransactionID And ITMPD.CompanyID=ITMP.CompanyID AND IFNULL(ITMP.IsDeletedTransaction,0)=0 AND IFNULL(ITMPD.IsDeletedTransaction,0)=0 INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID And WM.CompanyID=ITD.CompanyID Left Join ConversionMaster AS CM ON CM.BaseUnitSymbol=IM.StockUnit And CM.ConvertedUnitSymbol=IM.PurchaseUnit And CM.CompanyID=IM.CompanyID Left Join ConversionMaster AS CU ON CU.BaseUnitSymbol=IM.PurchaseUnit And CU.ConvertedUnitSymbol=IM.StockUnit And CU.CompanyID=IM.CompanyID " &
                      " Where ITM.VoucherID = -14 And ITM.TransactionID ='" & TransactionID & "' AND  ITM.CompanyID='" & GBLCompanyID & "'  Order By TransID"
            Else
                str = " SELECT        ISNULL(REPLACE(ITD.MfgDate, '1900-01-01', ''), '') AS MFGdate, ISNULL(REPLACE(ITD.ExpiryDate, '1900-01-01', ''), '') AS ExpiryDate, NULLIF (ITD.SupplierBatchNo, '') AS SupplierBatchNo, NULLIF (ITMPD.RefJobCardContentNo, " &
                        " '') AS RefJobCardContentNo, ISNULL(ITD.PurchaseTransactionID, 0) AS PurchaseTransactionID, ISNULL(ITM.LedgerID, 0) AS LedgerID, ISNULL(ITD.TransID, 0) AS TransID, ISNULL(ITD.ItemID, 0) AS ItemID, " &
                        " ISNULL(IM.ItemGroupID, 0) AS ItemGroupID, ISNULL(IM.ItemSubGroupID, 0) AS ItemSubGroupID, ISNULL(IGM.ItemGroupNameID, 0) AS ItemGroupNameID, NULLIF (ITMP.VoucherNo, '') AS PurchaseVoucherNo, " &
                        " REPLACE(CONVERT(Varchar(13), ITMP.VoucherDate, 106), ' ', '-') AS PurchaseVoucherDate, NULLIF (IM.ItemCode, '') AS ItemCode, NULLIF (IM.ItemName, '') AS ItemName, ISNULL(ITMPD.PurchaseOrderQuantity, 0) " &
                        " AS PurchaseOrderQuantity, NULLIF (ITMPD.PurchaseUnit, '') AS PurchaseUnit, ISNULL(ITD.ChallanQuantity, 0) AS ChallanQuantity, NULLIF (ITD.BatchNo, '') AS BatchNo, NULLIF (IM.StockUnit, '') AS StockUnit, " &
                        " ISNULL(ITD.ReceiptWtPerPacking, 0) AS ReceiptWtPerPacking, ISNULL(ITMPD.PurchaseTolerance, 0) AS PurchaseTolerance, ISNULL(IM.WtPerPacking, 0) AS WtPerPacking, ISNULL(IM.UnitPerPacking, 1) AS UnitPerPacking, " &
                        " ISNULL(IM.ConversionFactor, 1) AS ConversionFactor, ISNULL(IM.SizeW, 1) AS SizeW, ISNULL(ITD.WarehouseID, 0) AS WarehouseID, NULLIF (WM.WarehouseName, '') AS Warehouse, NULLIF (WM.BinName, '') AS Bin, " &
                        " ISNULL" &
                        " ((SELECT        SUM(ISNULL(ChallanQuantity, 0)) AS Expr1" &
                        " FROM            ItemTransactionDetail" &
                        " WHERE        (ISNULL(IsDeletedTransaction, 0) = 0) AND (ISNULL(PurchaseTransactionID, 0) > 0) AND (ISNULL(ChallanQuantity, 0) > 0) AND (PurchaseTransactionID = ITMPD.TransactionID) AND (ItemID = ITMPD.ItemID)), 0) " &
                        " AS ReceiptQuantity, NULLIF (CNM.ConversionFormula, '') AS FormulaStockToPurchaseUnit, ISNULL(CNM.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlacePurchaseUnit, NULLIF (CU.ConversionFormula, '') " &
                        " AS FormulaPurchaseToStockUnit, ISNULL(CU.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit, ISNULL(IM.GSM, 0) AS GSM, ISNULL(IM.ReleaseGSM, 0) AS ReleaseGSM, ISNULL(IM.AdhesiveGSM, 0) " &
                        " AS AdhesiveGSM, ISNULL(IM.Thickness, 0) AS Thickness, ISNULL(IM.Density, 0) AS Density, ISGM.ItemSubGroupName,PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName" &
                        " FROM            ItemTransactionMain AS ITM INNER JOIN" &
                        " ItemTransactionDetail AS ITD ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID AND ISNULL(ITM.IsDeletedTransaction, 0) = 0 AND ISNULL(ITD.IsDeletedTransaction, 0) = 0 INNER JOIN" &
                        " ItemMaster AS IM ON IM.ItemID = ITD.ItemID Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID INNER JOIN" &
                        " ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID INNER JOIN" &
                        " ItemTransactionMain AS ITMP ON ITMP.TransactionID = ITD.PurchaseTransactionID INNER JOIN" &
                        " ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID = ITMP.TransactionID AND ITMPD.ItemID = IM.ItemID AND ITMPD.TransactionID = ITD.PurchaseTransactionID AND " &
                        " ISNULL(ITMP.IsDeletedTransaction, 0) = 0 AND ISNULL(ITMPD.IsDeletedTransaction, 0) = 0 INNER JOIN" &
                        " WarehouseMaster AS WM ON WM.WarehouseID = ITD.WarehouseID AND WM.CompanyID = ITD.CompanyID LEFT OUTER JOIN" &
                        " ConversionMaster AS CNM ON CNM.BaseUnitSymbol = IM.StockUnit AND CNM.ConvertedUnitSymbol = IM.PurchaseUnit LEFT OUTER JOIN" &
                        " ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID = IM.ItemSubGroupID AND ISNULL(ISGM.IsDeletedTransaction, 0) = 0 LEFT OUTER JOIN" &
                        " ConversionMaster AS CU ON CU.BaseUnitSymbol = IM.PurchaseUnit AND CU.ConvertedUnitSymbol = IM.StockUnit" &
                        " WHERE        (ITM.VoucherID = - 14) AND ITM.TransactionID ='" & TransactionID & "' AND ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ")" &
                        " ORDER BY TransID"
                '  "Select /*Replace(Convert(Nvarchar(30),ITD.ExpiryDate,106), ' ','') as ExpiryDate ,    Replace(Convert(Nvarchar(30),ITD.MfgDate,106), ' ','')   as MfgDate ,*/ ISNULL(Replace( ITD.MFGdate,'1900-01-01',''),'')AS MFGdate,ISNULL(Replace( ITD.ExpiryDate,'1900-01-01',''),'') AS ExpiryDate, nullif(ITD.SupplierBatchNo,'') AS SupplierBatchNo, nullif(ITMPD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.PurchaseTransactionID,0) AS PurchaseTransactionID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITD.ItemID,0) AS ItemID,Isnull(IM.ItemGroupID,0) As ItemGroupID,Isnull(IM.ItemSubGroupID,0) As ItemSubGroupID,Isnull(IGM.ItemGroupNameID,0) As ItemGroupNameID,NullIf(ITMP.VoucherNo,'') AS PurchaseVoucherNo,Replace(Convert(Varchar(13),ITMP.VoucherDate,106),' ','-') AS PurchaseVoucherDate,  " &
                '"  NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IM.ItemName,'') AS ItemName,Isnull(ITMPD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,NullIf(ITMPD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.ChallanQuantity, 0) As ChallanQuantity, NullIf(ITD.BatchNo,'') AS BatchNo,NullIf(IM.StockUnit,'') AS StockUnit,Isnull(ITD.ReceiptWtPerPacking,0) AS ReceiptWtPerPacking,Isnull(ITMPD.PurchaseTolerance,0) AS PurchaseTolerance,Isnull(IM.WtPerPacking,0) AS WtPerPacking,  " &
                '"  Isnull(IM.UnitPerPacking, 1) As UnitPerPacking, Isnull(IM.ConversionFactor, 1) As ConversionFactor, Isnull(IM.SizeW, 1) As SizeW, Isnull(ITD.WarehouseID, 0) As WarehouseID, Nullif(WM.WarehouseName,'') AS Warehouse,Nullif(WM.BinName,'') AS Bin,Isnull((Select Sum(Isnull(ChallanQuantity,0))  From ItemTransactionDetail Where Isnull(IsDeletedTransaction,0)=0 AND Isnull(PurchaseTransactionID,0)>0 AND Isnull(ChallanQuantity,0)>0 AND PurchaseTransactionID=ITMPD.TransactionID AND ItemID=ITMPD.ItemID),0) AS ReceiptQuantity,Nullif(CM.ConversionFormula,'') AS FormulaStockToPurchaseUnit,Isnull(CM.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlacePurchaseUnit,Nullif(CU.ConversionFormula,'') AS FormulaPurchaseToStockUnit,Isnull(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density,ISGM.ItemSubGroupName  " &
                '"  From ItemTransactionMain As ITM INNER Join ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID AND Isnull(ITM.IsDeletedTransaction,0)=0 AND Isnull(ITD.IsDeletedTransaction,0)=0 INNER Join ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER Join ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=IM.CompanyID INNER Join ItemTransactionMain AS ITMP ON ITMP.TransactionID=ITD.PurchaseTransactionID And ITMP.CompanyID=ITD.CompanyID INNER Join ItemTransactionDetail AS ITMPD ON ITMPD.TransactionID=ITMP.TransactionID And ITMPD.ItemID=IM.ItemID And ITMPD.TransactionID=ITD.PurchaseTransactionID And ITMPD.CompanyID=ITMP.CompanyID AND Isnull(ITMP.IsDeletedTransaction,0)=0 AND Isnull(ITMPD.IsDeletedTransaction,0)=0 " &
                '"  INNER JOIN WarehouseMaster AS WM ON WM.WarehouseID=ITD.WarehouseID And WM.CompanyID=ITD.CompanyID Left Join ConversionMaster AS CM ON CM.BaseUnitSymbol=IM.StockUnit And CM.ConvertedUnitSymbol=IM.PurchaseUnit And CM.CompanyID=IM.CompanyID Left Join ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID AND isnull(ISGM.IsDeletedTransaction,0)=0 Left Join ConversionMaster AS CU ON CU.BaseUnitSymbol=IM.PurchaseUnit And CU.ConvertedUnitSymbol=IM.StockUnit And CU.CompanyID=IM.CompanyID Where ITM.VoucherID = -14 And ITM.TransactionID ='" & TransactionID & "' AND  ITM.CompanyID='" & GBLCompanyID & "'  Order By TransID"
            End If

            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------Get Receivers List From Employee------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetReceiverList() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
            ''str = "Select Distinct LM.LedgerID,LM.LedgerName From LedgerMaster As LM Inner Join LedgerGroupMaster AS LGM On LGM.LedgerGroupID=LM.LedgerGroupID And LGM.CompanyID=LM.CompanyID  AND LGM.LedgerGroupNameID=27 Where LM.CompanyID=" & GBLCompanyID & " And LM.IsDeletedTransaction=0 Order By LM.LedgerName"
            '' Changed By Mohini 
            str = "Select Distinct LM.LedgerID,LM.LedgerName From LedgerMaster As LM Left Join DepartmentMaster as DM on DM.DepartmentID = LM.DepartmentID   Inner Join LedgerGroupMaster AS LGM On LGM.LedgerGroupID=LM.LedgerGroupID  AND LGM.LedgerGroupNameID=27 Where  Isnull(LM.IsDeletedTransaction,0)=0  and Dm.DepartmentName like '%Inventory%'  Order By LM.LedgerName"
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------Get Warehouse List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetWarehouseList() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                str = "Select DISTINCT WarehouseName AS Warehouse From WarehouseMaster Where IFNULL(IsDeletedTransaction,0)=0 AND IFNULL(WarehouseName,'')<>'' AND CompanyID=" & GBLCompanyID & " AND IFNULL(IsFloorWarehouse,0)=0 Order By WarehouseName"
            Else
                str = "Select DISTINCT WarehouseName AS Warehouse From WarehouseMaster Where Isnull(IsDeletedTransaction,0)=0 AND Isnull(WarehouseName,'')<>'' AND CompanyID=" & GBLCompanyID & " AND Isnull(ProductionUnitID,0)=" & ProductionUnitID & " AND Isnull(IsFloorWarehouse,0)=0 Order By WarehouseName"
            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------Get Bins List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetBinsList(ByVal warehousename As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                If warehousename = "" Then
                    str = "SELECT Distinct Nullif(BinName,'') AS Bin,IFNULL(WarehouseID,0) AS WarehouseID FROM WarehouseMaster Where IFNULL(IsDeletedTransaction,0)=0 AND IFNULL(BinName,'')<>'' AND CompanyID=" & GBLCompanyID & " Order By Bin"
                Else str = "SELECT Distinct Nullif(BinName,'') AS Bin,IFNULL(WarehouseID,0) AS WarehouseID FROM WarehouseMaster Where IFNULL(IsDeletedTransaction,0)=0 AND WarehouseName='" & warehousename & "' AND IFNULL(BinName,'')<>'' AND CompanyID=" & GBLCompanyID & " Order By Bin"
                End If
            Else
                If warehousename = "" Then
                    str = "SELECT Distinct Nullif(BinName,'') AS Bin,Isnull(WarehouseID,0) AS WarehouseID FROM WarehouseMaster Where Isnull(IsDeletedTransaction,0)=0 AND Isnull(BinName,'')<>'' /*AND CompanyID=" & GBLCompanyID & "*/ AND Isnull(ProductionUnitID,0)=" & ProductionUnitID & " Order By Bin"
                Else
                    str = "SELECT Distinct Nullif(BinName,'') AS Bin,Isnull(WarehouseID,0) AS WarehouseID FROM WarehouseMaster Where Isnull(IsDeletedTransaction,0)=0 AND WarehouseName='" & warehousename & "' AND Isnull(BinName,'')<>'' /*AND CompanyID=" & GBLCompanyID & "*/ AND Isnull(ProductionUnitID,0)=" & ProductionUnitID & " Order By Bin"
                End If
            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------GetPreviousReceivedQuantity------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetPreviousReceivedQuantity(ByVal PurchaseTransactionID As String, ByVal ItemID As String, ByVal GRNTransactionID As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

            If DBType = "MYSQL" Then
                str = "Select IFNULL(PTM.TransactionID,0) AS TransactionID,IFNULL(PTD.ItemID,0) AS ItemID,IFNULL(PTD.ItemID,0) AS ItemID,IFNULL(PTD.PurchaseTolerance,0) AS PurchaseTolerance,IFNULL(PTD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,IM.PurchaseUnit,IFNULL((Select Sum(IFNULL(ChallanQuantity,0)) From ItemTransactionDetail Where IFNULL(ChallanQuantity,0)>0 AND PurchaseTransactionID=PTM.TransactionID  AND TransactionID<>" & GRNTransactionID & " AND ItemID=PTD.ItemID AND CompanyID=PTM.CompanyID  AND IFNULL(IsDeletedTransaction,0)<>1),0 ) AS PreReceiptQuantity,IM.StockUnit,Nullif(C.ConversionFormula,'') AS FormulaPurchaseToStockUnit, IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit " &
                      " From ItemTransactionMain AS PTM INNER JOIN ItemTransactionDetail AS PTD ON PTD.TransactionID=PTM.TransactionID AND PTM.CompanyID=PTD.CompanyID AND IFNULL(PTM.IsDeletedTransaction,0)=0 AND IFNULL(PTD.IsDeletedTransaction,0)=0 INNER JOIN ItemMaster AS IM ON IM.ItemID=PTD.ItemID AND IM.CompanyID=PTD.CompanyID LEFT JOIN ConversionMaster AS C ON C.BaseUnitSymbol=IM.PurchaseUnit AND C.ConvertedUnitSymbol=IM.StockUnit AND C.CompanyID=IM.CompanyID " &
                      " Where PTM.VoucherID=-11 AND PTM.TransactionID=" & PurchaseTransactionID & " AND PTD.ItemID=" & ItemID & "  AND PTM.CompanyID=" & GBLCompanyID & ""
            Else
                str = " SELECT        ISNULL(PTM.TransactionID, 0) AS TransactionID, ISNULL(PTD.ItemID, 0) AS ItemID, ISNULL(PTD.ItemID, 0) AS ItemID, ISNULL(PTD.PurchaseTolerance, 0) AS PurchaseTolerance, ISNULL(PTD.PurchaseOrderQuantity, 0) AS PurchaseOrderQuantity, IM.PurchaseUnit, ISNULL((SELECT SUM(ISNULL(ChallanQuantity, 0)) AS Expr1 " &
                        " FROM            ItemTransactionDetail " &
                        " WHERE        (ISNULL(ChallanQuantity, 0) > 0) AND (PurchaseTransactionID = PTM.TransactionID) AND TransactionID<>" & GRNTransactionID & " AND (ItemID = PTD.ItemID) AND (CompanyID = PTM.CompanyID) AND (ISNULL(IsDeletedTransaction,  " &
                        " 0) <> 1)), 0) AS PreReceiptQuantity, IM.StockUnit, NULLIF (C.ConversionFormula, '') AS FormulaPurchaseToStockUnit, ISNULL(C.ConvertedUnitDecimalPlace, 0) AS UnitDecimalPlaceStockUnit, PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName " &
                        " FROM            ItemTransactionMain AS PTM INNER JOIN " &
                        " ItemTransactionDetail AS PTD ON PTD.TransactionID = PTM.TransactionID AND PTM.CompanyID = PTD.CompanyID AND ISNULL(PTM.IsDeletedTransaction, 0) = 0 AND ISNULL(PTD.IsDeletedTransaction, 0) = 0 Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = PTM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID INNER JOIN " &
                        " ItemMaster AS IM ON IM.ItemID = PTD.ItemID LEFT OUTER JOIN " &
                        " ConversionMaster AS C ON C.BaseUnitSymbol = IM.PurchaseUnit AND C.ConvertedUnitSymbol = IM.StockUnit " &
                        " WHERE        (PTM.VoucherID = - 11) AND PTM.TransactionID=" & PurchaseTransactionID & " AND PTD.ItemID=" & ItemID & "  "

                '"Select Isnull(PTM.TransactionID,0) AS TransactionID,Isnull(PTD.ItemID,0) AS ItemID,Isnull(PTD.ItemID,0) AS ItemID,Isnull(PTD.PurchaseTolerance,0) AS PurchaseTolerance,Isnull(PTD.PurchaseOrderQuantity,0) AS PurchaseOrderQuantity,IM.PurchaseUnit,Isnull((Select Sum(Isnull(ChallanQuantity,0)) From ItemTransactionDetail Where ISNULL(ChallanQuantity,0)>0 AND PurchaseTransactionID=PTM.TransactionID AND TransactionID<>" & GRNTransactionID & " AND ItemID=PTD.ItemID AND CompanyID=PTM.CompanyID AND Isnull(IsDeletedTransaction,0)<>1),0 ) AS PreReceiptQuantity,IM.StockUnit,Nullif(C.ConversionFormula,'') AS FormulaPurchaseToStockUnit,Isnull(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit  " &
                '      " From ItemTransactionMain AS PTM INNER JOIN ItemTransactionDetail AS PTD ON PTD.TransactionID=PTM.TransactionID AND PTM.CompanyID=PTD.CompanyID AND Isnull(PTM.IsDeletedTransaction,0)=0 AND Isnull(PTD.IsDeletedTransaction,0)=0 INNER JOIN ItemMaster AS IM ON IM.ItemID=PTD.ItemID AND IM.CompanyID=PTD.CompanyID LEFT JOIN ConversionMaster AS C ON C.BaseUnitSymbol=IM.PurchaseUnit AND C.ConvertedUnitSymbol=IM.StockUnit AND C.CompanyID=IM.CompanyID Where PTM.VoucherID=-11 AND PTM.TransactionID=" & PurchaseTransactionID & " AND PTD.ItemID=" & ItemID & "  AND PTM.CompanyID=" & GBLCompanyID & ""
            End If
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function
    '---------------Close Master code---------------------------------

    ''----------------------------Generate Receipt No ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetReceiptNo(ByVal prefix As String) As String

        Dim MaxVoucherNo As Long
        Dim KeyField As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))

        Try
            KeyField = db.GeneratePrefixedNo("ItemTransactionMain", prefix, "MaxVoucherNo", MaxVoucherNo, GBLFYear, " Where VoucherPrefix='" & prefix & "' And  CompanyID=" & GBLCompanyID & " And FYear='" & GBLFYear & "' AND VoucherID=-14 AND Isnull(IsDeletedTransaction,0)=0")

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField
    End Function

    ''----------------------------Save Receipt Note Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ValidateSupplierBatchReceiptData(ByVal voucherid As Integer, ByVal jsonObjectsTransactionMain As Object, ByVal jsonObjectsTransactionDetail As Object) As String

        Dim dt As New DataTable
        Dim KeyField As String
        Dim str As String, strerr As String = "", dt1 As New DataTable, dt2 As New DataTable
        Dim LedgerID As Long = 0, i As Integer = 0

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        Try

            If db.CheckAuthories("PurchaseGRN.aspx", GBLUserID, GBLCompanyID, "CanSave") = False Then Return "You are not authorized to save..!, Can't Save"

            db.ConvertObjectToDatatable(jsonObjectsTransactionMain, dt1, strerr)
            If strerr <> "Success" Then
                Return strerr
            End If
            If dt1.Rows.Count > 0 Then
                LedgerID = dt1.Rows(0)("LedgerID")
            End If

            db.ConvertObjectToDatatable(jsonObjectsTransactionDetail, dt2, strerr)

            If strerr <> "Success" Then
                Return strerr
            End If

            If dt2.Rows.Count > 0 Then
                For i = 0 To dt2.Rows.Count - 1
                    dt.Dispose()
                    str = "Select ITD.SupplierBatchNo AS SupplierBatchNo From ItemTransactionDetail ITD Inner Join ItemTransactionMain as ITM ON ITM.TransactionID = ITD.TransactionID AND ITM.CompanyID = ITD.CompanyID Where ITM.LedgerID = '" & LedgerID & "' and ITD.SupplierBatchNo = '" & dt2.Rows(i)("SupplierBatchNo") & "' and ITM.CompanyID = '" & GBLCompanyID & "' and Isnull(ITM.IsDeletedTransaction,0) = 0 and Isnull(ITD.IsDeletedTransaction,0) = 0 and ITM.VoucherID = " & voucherid & "  "
                    db.FillDataTable(dt, str)
                    If dt.Rows.Count > 0 Then
                        Return "Supplier batch no - '" & dt.Rows(i)("SupplierBatchNo") & "' already saved."
                    End If
                Next
            End If

            KeyField = "Success"

        Catch ex As Exception
            Return "Error:Ex " & ex.Message
        End Try

        Return KeyField

    End Function

    ''----------------------------Save Receipt Note Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveReceiptData(ByVal prefix As String, ByVal voucherid As Integer, ByVal jsonObjectsTransactionMain As Object, ByVal jsonObjectsTransactionDetail As Object, ByVal ReceiptQuantity As String, ByVal ApprovedQuantity As String, ByVal jsonObjectsPO As Object) As String

        Dim dt As New DataTable
        Dim VoucherNo As String = ""
        Dim MaxVoucherNo As Long = 0
        Dim KeyField, TransactionID As String
        Dim AddColName, AddColValue, TableName As String
        Dim str As String, strerr As String = "", dt1 As New DataTable, dt2 As New DataTable, dt3 As New DataTable
        Dim LedgerID As Long = 0, i As Integer = 0

        AddColName = ""
        AddColValue = ""

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If

        If db.CheckAuthories("PurchaseGRN.aspx", GBLUserID, GBLCompanyID, "CanSave") = False Then Return "You are not authorized to save..!, Can't Save"

        db.ConvertObjectToDatatable(jsonObjectsTransactionMain, dt1, strerr)
        If strerr <> "Success" Then
            Return strerr
        End If
        If dt1.Rows.Count > 0 Then
            LedgerID = dt1.Rows(0)("LedgerID")
        End If

        db.ConvertObjectToDatatable(jsonObjectsTransactionDetail, dt2, strerr)

        If strerr <> "Success" Then
            Return strerr
        End If

        Try
            Dim IsGRNApprovalRequired As Boolean
            Dim status As Boolean
            str = "Select IsGRNApprovalRequired from CompanyMaster where CompanyID = '" & GBLCompanyID & "'"
            Dim approvalQuery As String = "SELECT ISNULL(IsGRNApprovalRequired,0) As IsGRNApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
            db.FillDataTable(dataTable, approvalQuery)

            If dataTable.Rows.Count > 0 Then
                IsGRNApprovalRequired = dataTable.Rows(0)("IsGRNApprovalRequired").ToString()
                If IsGRNApprovalRequired = True Then
                    status = False
                Else
                    status = True
                End If
            End If

            VoucherNo = db.GeneratePrefixedNo("ItemTransactionMain", prefix, "MaxVoucherNo", MaxVoucherNo, GBLFYear, " Where VoucherPrefix='" & prefix & "' AND VoucherID=" & voucherid & " And  CompanyID=" & GBLCompanyID & " And FYear='" & GBLFYear & "' AND Isnull(IsDeletedTransaction,0)=0 ")

            Using updtTran As New Transactions.TransactionScope
                TableName = "ItemTransactionMain"
                AddColName = "CreatedDate,UserID,CompanyID,FYear,CreatedBy,VoucherPrefix,MaxVoucherNo,VoucherNo,ProductionUnitID"
                If DBType = "MYSQL" Then
                    AddColValue = "Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & prefix & "','" & MaxVoucherNo & "','" & VoucherNo & "','" & ProductionUnitID & "'"
                Else
                    AddColValue = "GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & prefix & "','" & MaxVoucherNo & "','" & VoucherNo & "','" & ProductionUnitID & "'"
                End If
                TransactionID = db.InsertDatatableToDatabase(jsonObjectsTransactionMain, TableName, AddColName, AddColValue)
                If IsNumeric(TransactionID) = False Then
                    updtTran.Dispose()
                    Return "Error:Main " & TransactionID
                End If
                If IsGRNApprovalRequired = True Then
                    TableName = "ItemTransactionDetail"
                    AddColName = "CreatedDate,UserID,CompanyID,FYear,CreatedBy,TransactionID,ParentTransactionID,ProductionUnitID"
                    If DBType = "MYSQL" Then
                        AddColValue = "Now(),'" & GBLUserID & "'," & GBLCompanyID & ",'" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    Else
                        AddColValue = "GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    End If
                    str = db.InsertDatatableToDatabase(jsonObjectsTransactionDetail, TableName, AddColName, AddColValue, "Receipt Note", TransactionID)
                    If IsNumeric(str) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error:Detail " & str
                    End If
                Else
                    'TableName = "ItemTransactionDetail"
                    'AddColName = "CreatedDate,UserID,CompanyID,FYear,CreatedBy,TransactionID,ParentTransactionID,ReceiptQuantity,IsVoucherItemApproved,ApprovedQuantity,VoucherItemApprovedBy,QCApprovalNo"
                    'If DBType = "MYSQL" Then
                    '    AddColValue = "Now(),'" & GBLUserID & "'," & GBLCompanyID & ",'" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ReceiptQuantity & "','" & status & "','" & ApprovedQuantity & "','" & GBLUserID & "','" & VoucherNo & "'"
                    'Else
                    '    AddColValue = "GetDate(),'" & GBLUserID & "'," & GBLCompanyID & ",'" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ReceiptQuantity & "','" & status & "','" & ApprovedQuantity & "','" & GBLUserID & "','" & VoucherNo & "'"
                    'End If

                    TableName = "ItemTransactionDetail"
                    AddColName = "CreatedDate,UserID,CompanyID,FYear,CreatedBy,TransactionID,ParentTransactionID,ProductionUnitID"
                    If DBType = "MYSQL" Then
                        AddColValue = "Now(),'" & GBLUserID & "'," & GBLCompanyID & ",'" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "'"
                    Else
                        AddColValue = "GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    End If

                    str = db.InsertDatatableToDatabase(jsonObjectsTransactionDetail, TableName, AddColName, AddColValue, "Receipt Note", TransactionID)
                    If IsNumeric(str) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        Return "Error:Detail " & str
                    End If
                End If

                db.ConvertObjectToDatatable(jsonObjectsPO, dt3, strerr)
                If strerr <> "Success" Then
                    Return strerr
                End If
                If dt3.Rows.Count > 0 Then

                    For i = 0 To dt3.Rows.Count - 1
                        Dim PurchaseTransactionID = dt3.Rows(i)("PurchaseTransactionID")
                        Dim ItemID = dt3.Rows(i)("ItemID")
                        db.ExecuteNonSQLQuery("Update ItemTransactionDetail Set IsCompleted = 1,CompletedDate = GetDate(), CompletedBy = " & GBLUserID & " Where TransactionID = " & PurchaseTransactionID & " AND ItemID = " & ItemID & " AND ProductionUnitID = " & ProductionUnitID & " ")
                    Next
                End If

                db.ExecuteNonSQLQuery("Update ItemTransactionDetail Set BatchID = TransactionDetailID Where TransactionID = " & TransactionID & " ")
                db.ExecuteNonSQLQuery("INSERT INTO ItemTransactionBatchDetail(BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate,CompanyID, FYear, CreatedBy, CreatedDate)(Select BatchID,BatchNo,SupplierBatchNo, MfgDate, ExpiryDate, CompanyID, FYear, CreatedBy, CreatedDate From ItemTransactionDetail Where TransactionID = " & TransactionID & " )")

                If DBType = "MYSQL" Then
                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES (" & GBLCompanyID & "," & TransactionID & ",0);")
                Else
                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TransactionID & ",0")
                End If
                KeyField = "Success"
                updtTran.Complete()

            End Using
        Catch ex As Exception
            Return "Error:Ex " & ex.Message
        End Try
        Return KeyField

    End Function

    ''----------------------------Update Receipt Note Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateReceiptData(ByVal TransactionID As String, ByVal jsonObjectsTransactionMain As Object, ByVal jsonObjectsTransactionDetail As Object, ByVal ReceiptQuantity As String, ByVal ApprovedQuantity As String, ByVal ObjvalidateLoginUser As Object, ByVal jsonObjectsPO As Object) As String

        Dim dt As New DataTable
        Dim dtValidate As New DataTable
        Dim KeyField As String = ""
        Dim AddColName, wherecndtn, TableName, AddColValue As String
        AddColName = ""

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If


        If db.ValidUserAuthentication("", ObjvalidateLoginUser("userName"), ObjvalidateLoginUser("password")) = False Then
            Return "InvalidUser"
        End If

        Dim IsGRNApprovalRequired As Boolean
        Dim status As Boolean
        str = "Select IsGRNApprovalRequired from CompanyMaster where CompanyID = '" & GBLCompanyID & "'"
        Dim approvalQuery As String = "SELECT ISNULL(IsGRNApprovalRequired,0) As IsGRNApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
        db.FillDataTable(dataTable, approvalQuery)

        If dataTable.Rows.Count > 0 Then
            IsGRNApprovalRequired = dataTable.Rows(0)("IsGRNApprovalRequired").ToString()
            If IsGRNApprovalRequired = True Then
                status = False
            Else
                status = True
            End If
        End If




        str = "Select Top 1 TransactionID From ItemTransactionDetail Where Isnull(ParentTransactionID,0)=" & TransactionID & " AND TransactionID<>" & TransactionID & " AND isnull(IsDeletedTransaction,0) =0"
        db.FillDataTable(dtValidate, str)
        If dtValidate.Rows.Count > 0 Then
            Return "Exist"
        End If
        dtValidate.Clear()
        If IsGRNApprovalRequired = True Then
            str = "Select Top 1TransactionID From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And isnull(QCApprovalNo,'')<>'' AND TransactionID=" & TransactionID & "  AND (Isnull(ApprovedQuantity,0)>0 OR  Isnull(RejectedQuantity,0)>0)"
            db.FillDataTable(dtValidate, str)
            If dtValidate.Rows.Count > 0 Then
                Return "Exist"
            End If
        End If
        If db.CheckAuthories("PurchaseGRN.aspx", GBLUserID, GBLCompanyID, "CanEdit", TransactionID, ObjvalidateLoginUser("transactionRemark")) = False Then Return "You are not authorized to update..!, Can't Update"

        Try

            Using trans As New Transactions.TransactionScope
                TableName = "ItemTransactionMain"
                If DBType = "MYSQL" Then
                    AddColName = "ModifiedDate=Now(),CompanyID=" & GBLCompanyID & ",ModifiedBy='" & GBLUserID & "'"
                Else
                    AddColName = "ModifiedDate=GetDate(),CompanyID=" & GBLCompanyID & ",ModifiedBy='" & GBLUserID & "', ProductionUnitID = '" & ProductionUnitID & "'"
                End If
                wherecndtn = "CompanyID=" & GBLCompanyID & " And TransactionID='" & TransactionID & "' AND ProductionUnitID = '" & ProductionUnitID & "'"
                str = db.UpdateDatatableToDatabase(jsonObjectsTransactionMain, TableName, AddColName, 1, wherecndtn)
                If str <> "Success" Then
                    trans.Dispose()
                    Return "Error:Main " & str
                End If
                db.ExecuteNonSQLQuery("Delete from ItemTransactionDetail WHERE CompanyID='" & GBLCompanyID & "' and TransactionID='" & TransactionID & "' ")
                If IsGRNApprovalRequired = True Then
                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ParentTransactionID, ProductionUnitID"
                    If DBType = "MYSQL" Then
                        AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "'"
                    Else
                        AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    End If
                    str = db.InsertDatatableToDatabase(jsonObjectsTransactionDetail, TableName, AddColName, AddColValue, "Receipt Note", TransactionID)
                    If IsNumeric(str) = False Then
                        trans.Dispose()
                        Return "Error:Detail " & str
                    End If
                Else
                    'TableName = "ItemTransactionDetail"
                    'AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ParentTransactionID,ReceiptQuantity,ApprovedQuantity,IsVoucherItemApproved"
                    'If DBType = "MYSQL" Then
                    '    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ReceiptQuantity & "','" & ApprovedQuantity & "','" & status & "'"
                    'Else
                    '    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ReceiptQuantity & "','" & ApprovedQuantity & "','" & status & "'"
                    'End If

                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ParentTransactionID,ProductionUnitID"
                    If DBType = "MYSQL" Then
                        AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "'"
                    Else
                        AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    End If

                    str = db.InsertDatatableToDatabase(jsonObjectsTransactionDetail, TableName, AddColName, AddColValue, "Receipt Note", TransactionID)
                    If IsNumeric(str) = False Then
                        trans.Dispose()
                        Return "Error:Detail " & str
                    End If
                End If


                db.ExecuteNonSQLQuery("Update ItemTransactionDetail Set BatchID = TransactionDetailID Where TransactionID = " & TransactionID & " ")
                db.ExecuteNonSQLQuery("INSERT INTO ItemTransactionBatchDetail(BatchID, BatchNo, SupplierBatchNo, MfgDate, ExpiryDate, CompanyID, FYear, CreatedBy, CreatedDate)(Select BatchID,BatchNo,SupplierBatchNo, MfgDate, ExpiryDate, CompanyID, FYear, CreatedBy, CreatedDate From ItemTransactionDetail Where CompanyID = " & GBLCompanyID & " AND TransactionID = " & TransactionID & " AND ProductionUnitID = '" & ProductionUnitID & "')")

                trans.Complete()
                KeyField = "Success"
            End Using
            If DBType = "MYSQL" Then
                db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES( " & GBLCompanyID & "," & TransactionID & ",0);")
            Else
                db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TransactionID & ",0")
            End If

        Catch ex As Exception
            KeyField = "Error:Ex " & ex.Message
        End Try
        Return KeyField

    End Function

    ''----------------------------Open Purchase GRN Delete Data ------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function DeletePGRN(ByVal TransactionID As String, ByVal ObjvalidateLoginUser As Object, ByVal PurchaseTransactionID As String, ByVal jsonObjectsPO As Object) As String

        Dim KeyField As String
        Dim str As String
        Dim dt3 As New DataTable
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If

        If db.ValidUserAuthentication("", ObjvalidateLoginUser("userName"), ObjvalidateLoginUser("password")) = False Then
            Return "InvalidUser"
        End If

        Dim dtExist As New DataTable
        str = "Select TransactionID From ItemPurchaseInvoiceDetail Where Isnull(IsDeletedTransaction, 0) = 0  AND ParentTransactionID=" & TransactionID & " "
        db.FillDataTable(dtExist, str)

        If dtExist.Rows.Count > 0 Then
            Return "This transaction is used in another process..! Record can not be delete..."
        End If

        If (db.CheckAuthories("PurchaseGRN.aspx", GBLUserID, GBLCompanyID, "CanDelete", TransactionID, ObjvalidateLoginUser("transactionRemark")) = False) Then Return "You are not authorized to delete..!, Can't Delete"
        'If db.IsDeletable("TransactionID", "ItemTransactionDetail", "Where IsDeletedTransaction=0  and IsCompleted = 1 And CompanyID = " & GBLCompanyID & " and TransactionID = " & PurchaseTransactionID & "") = False Then
        '    Return "You can not delete the GRN, GRN is already closed, "
        'End If
        Try
            Using updtTran As New Transactions.TransactionScope
                If DBType = "MYSQL" Then
                    str = "Update ItemTransactionMain Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE CompanyID='" & GBLCompanyID & "' and TransactionID='" & TransactionID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Main:- " & KeyField
                    End If

                    str = "Update ItemTransactionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE TransactionID='" & TransactionID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Main:- " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES( " & GBLCompanyID & "," & TransactionID & ",0);")
                Else
                    str = "Update ItemTransactionMain Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE TransactionID='" & TransactionID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Main:- " & KeyField
                    End If

                    str = "Update ItemTransactionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE  TransactionID='" & TransactionID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Detail:- " & KeyField
                    End If

                    db.ConvertObjectToDatatable(jsonObjectsPO, dt3, str)
                    If str <> "Success" Then
                        Return str
                    End If
                    If dt3.Rows.Count > 0 Then

                        For i = 0 To dt3.Rows.Count - 1
                            Dim PurchaseTransactionIDs = dt3.Rows(i)("purchaseTransactionID")
                            Dim ItemID = dt3.Rows(i)("ItemID")
                            db.ExecuteNonSQLQuery("Update ItemTransactionDetail Set IsCompleted = 0,CompletedDate = GetDate(), CompletedBy = " & GBLUserID & " Where CompanyID = " & GBLCompanyID & " AND TransactionID = " & PurchaseTransactionIDs & " AND ItemID = " & ItemID & "  ")
                        Next
                    End If

                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TransactionID & ",0")
                End If

                KeyField = "Success"
                updtTran.Complete()
            End Using
        Catch ex As Exception
            KeyField = "Error: " & ex.Message
        End Try
        Return KeyField

    End Function

    '-----------------------------------CheckPermission------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckPermission(ByVal TransactionID As String) As String
        Dim KeyField As String = ""

        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Try

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            Dim IsGRNApprovalRequired As Boolean
            str = "Select IsGRNApprovalRequired from CompanyMaster where CompanyID = '" & GBLCompanyID & "'"
            Dim approvalQuery As String = "SELECT ISNULL(IsGRNApprovalRequired,0) As IsGRNApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
            db.FillDataTable(dataTable, approvalQuery)

            IsGRNApprovalRequired = dataTable.Rows(0)("IsGRNApprovalRequired").ToString()

            Dim dtExist As New DataTable
            Dim dtExist1 As New DataTable
            Dim SxistStr As String

            Dim D1 As String = "", D2 As String = ""
            D1 = ""
            D2 = ""
            KeyField = ""

            SxistStr = ""
            If DBType = "MYSQL" Then
                If IsGRNApprovalRequired = True Then
                    SxistStr = "Select * From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And ParentTransactionID = " & TransactionID & " And ProductionUnitID ='" & ProductionUnitID & "'  And TransactionID <> ParentTransactionID"
                Else
                    SxistStr = "Select * From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And ParentTransactionID = " & TransactionID & " And ProductionUnitID ='" & ProductionUnitID & "' And TransactionID <> ParentTransactionID"
                End If
            Else
                SxistStr = "Select * From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And ParentTransactionID = " & TransactionID & " And ProductionUnitID ='" & ProductionUnitID & "'  And TransactionID <> ParentTransactionID"
            End If

            db.FillDataTable(dtExist, SxistStr)
            Dim E As Integer = dtExist.Rows.Count
            If E > 0 Then
                D1 = dtExist.Rows(0)(0)
            End If
            SxistStr = ""
            If DBType = "MYSQL" Then
                If IsGRNApprovalRequired = True Then
                    SxistStr = "Select  * From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And IFNULL(QCApprovalNo,'')<>'' AND TransactionID=" & TransactionID & "  AND (IFNULL(ApprovedQuantity,0)>0 OR IFNULL(RejectedQuantity,0)>0)"
                Else
                    SxistStr = " "
                End If
            Else
                If IsGRNApprovalRequired = True Then
                    SxistStr = "Select  * From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And isnull(QCApprovalNo,'')<>'' AND TransactionID=" & TransactionID & " AND ProductionUnitID ='" & ProductionUnitID & "'   AND (Isnull(ApprovedQuantity,0)>0 OR  Isnull(RejectedQuantity,0)>0)"
                Else
                    SxistStr = " "
                End If
            End If
            db.FillDataTable(dtExist1, SxistStr)
            Dim F As Integer = dtExist1.Rows.Count
            If F > 0 Then
                D2 = dtExist1.Rows(0)(0)
            End If

            If D1 <> "" Or D2 <> "" Then
                KeyField = "Exist"
            End If

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function


    '--------------- Get Requisition and purchase order Comment Data---------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetCommentData(ByVal receiptTransactionID As String, ByVal purchaseTransactionIDs As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        str = ""
        If DBType = "MYSQL" Then
            If receiptTransactionID <> "0" Then
                str = "CALL GetCommentData( " & GBLCompanyID & ",'Goods Receipt Note',0,0," & receiptTransactionID & ",0,0,0,0,0);"
            Else
                str = " CALL GetCommentData( " & GBLCompanyID & ",'Goods Receipt Note',0,'" & purchaseTransactionIDs & "',0,0,0,0,0,0);"
            End If
        Else
            If receiptTransactionID <> "0" Then
                str = " EXEC GetCommentData " & GBLCompanyID & ",'Goods Receipt Note',0,0," & receiptTransactionID & ",0,0,0,0,0"
            Else
                str = " EXEC GetCommentData " & GBLCompanyID & ",'Goods Receipt Note',0,'" & purchaseTransactionIDs & "',0,0,0,0,0,0"
            End If
        End If

        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    ''----------------------------Save Comment Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveCommentData(ByVal jsonObjectCommentDetail As Object) As String

        Dim KeyField As String
        Dim AddColName, AddColValue, TableName As String
        AddColName = ""
        AddColValue = ""

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        Try

            TableName = "CommentChainMaster"
            AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy"
            If DBType = "MYSQL" Then
                AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "'"
            Else
                AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "'"
            End If
            db.InsertDatatableToDatabase(jsonObjectCommentDetail, TableName, AddColName, AddColValue)

            KeyField = "Success"
        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    ''''Item Stock Update ''''---------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetVouchersList() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            str = "Select DISTINCT TransactionID,VoucherNo From ItemTransactionMain Where IsDeletedTransaction=0 AND VoucherID Not IN(-8,-17,-11,-9) And CompanyID=" & GBLCompanyID & "  Order By VoucherNo"
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function TransactionWiseStockData(ByVal TransID As Integer) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            If DBType = "MYSQL" Then
                str = "SELECT IM.ItemID,IM.ItemGroupID,ISGM.ItemSubGroupID,IM.ItemCode,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,IM.StockUnit,IM.PhysicalStock,IM.BookedStock,IM.AllocatedStock,IM.UnapprovedStock,IM.PhysicalStock - IM.AllocatedStock AS FreeStock,IM.IncomingStock,IM.FloorStock,IM.PhysicalStock - IM.AllocatedStock + IM.IncomingStock - IM.BookedStock AS TheoriticalStock,IM.WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,IFNULL(UOM.DecimalPlace,0) AS UnitDecimalPlace " &
                      " From ItemMaster AS IM INNER JOIN ItemTransactionDetail As ITD ON ITD.ItemID=IM.ItemID And IM.CompanyID=ITD.CompanyID And ITD.IsDeletedTransaction = 0 INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID And IM.IsDeletedTransaction=0 LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit AND UOM.CompanyID=IM.CompanyID " &
                      " Where IM.CompanyID=" & GBLCompanyID & "  And ITD.TransactionID=" & TransID & " Order By IM.ItemName"
            Else
                str = "SELECT IM.ItemID,IM.ItemGroupID,ISGM.ItemSubGroupID,IM.ItemCode,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,IM.StockUnit,IM.PhysicalStock,IM.BookedStock,IM.AllocatedStock,IM.UnapprovedStock,IM.PhysicalStock - IM.AllocatedStock AS FreeStock,IM.IncomingStock,IM.FloorStock,IM.PhysicalStock - IM.AllocatedStock + IM.IncomingStock - IM.BookedStock AS TheoriticalStock,IM.WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,Isnull(UOM.DecimalPlace,0) AS UnitDecimalPlace  " &
                  " From ItemMaster AS IM INNER JOIN ItemTransactionDetail As ITD ON ITD.ItemID=IM.ItemID And Isnull(ITD.IsDeletedTransaction,0) = 0 INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And Isnull(IM.IsDeletedTransaction,0)=0 LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN UnitMaster AS UOM ON UOM.UnitSymbol=IM.StockUnit  Where ITD.TransactionID=" & TransID & " Order By IM.ItemName"
            End If

            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetGatePass(ByVal LedgerId As Integer) As String
        Try
            GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))

            str = "Select GPM.TransactionID,GPM.VoucherID,Isnull(GP.VoucherNo,'') As DCNo,GPM.VoucherNo,replace(Convert(varchar, GPM.VoucherDate,106),' ','-') as VoucherDate,GPM.Prefix,GPM.GatePassEntryType as GateEntryType ,GPM.VehicleNo,GPM.Remark,GPM.LedgerID,Case When isnull(LM.LedgerName,'') = '' Then GPM.MaterialSentTo else isnull(LM.LedgerName,'') END AS  MaterialSentTo,GPM.MaterialSentThrough as SendThrough,GPM.MaterialSentThroughName as SendThroughName,GPM.GatePassTransactionID, Nullif(GPM.DocumentNo,'') as DocumentNo  from GatePassEntryMain  as GPM left Join GatePassEntryMain as GP on GP.TransactionID = GPM.GatePassTransactionID left Join LedgerMaster as LM On LM.LedgerID = GPM.LedgerID Where GPM.VoucherID = '-129' AND GPM.GatePassEntryType like '%Item%' And IsNull(GPM.IsDeletedTransaction,0) <> 1 And GPM.CompanyId =" & GBLCompanyID & " and GPM.LedgerID = " & LedgerId & " And GPM.TransactionID Not In(Select distinct Isnull(GateEntryTransactionID,0) as GateEntryTransactionID from ItemTransactionMain  Where ISnull(IsDeletedTransaction,0) <> 1)"
            db.FillDataTable(dataTable, str)

            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckQCParameterExists(ByVal jsonItemList As Object) As String
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        Dim dtItems As New DataTable
        Dim dtParams As New DataTable
        Dim dtParams1 As New DataTable
        Dim dtAdminUser As New DataTable
        Dim StrQuery As String = ""
        Dim AdminUserID As String = ""
        Dim GBLUnderSubGroupIDString As String = ""
        Dim i As Integer = 0
        Try

            db.ConvertObjectToDatatable(jsonItemList, dtItems, StrQuery)
            If StrQuery <> "Success" Then
                Return StrQuery
            End If

            StrQuery = "Select Top 1 UserID From UserMaster Where /*CompanyID=" & GBLCompanyID & " AND*/ Isnull(IsDeletedUser,0)=0 AND Isnull(IsBlocked,0)=0 AND Isnull(Isadmin,0)=1 Order By UserID"
            db.FillDataTable(dtAdminUser, StrQuery)
            If dtAdminUser.Rows.Count > 0 Then
                AdminUserID = IIf(IsDBNull(dtAdminUser.Rows(0)("UserID")), 0, dtAdminUser.Rows(0)("UserID"))
            End If
            dtAdminUser.Dispose()
            For i = 0 To dtItems.Rows.Count - 1
                If Val(dtItems.Rows(i)("ItemID")) > 0 Then
                    dtItems.Rows(i)("VoucherItemApprovedBy") = Val(AdminUserID)
                    If Val(dtItems.Rows(i)("ItemGroupID")) > 0 And Val(dtItems.Rows(i)("ItemSubGroupID")) = 0 Then
                        StrQuery = "Select Count(ItemQCID) AS QCParams From ItemQCParameterSetting Where ItemGroupID=" & Val(dtItems.Rows(i)("ItemGroupID")) & " AND isnull(IsDeletedTransaction,0)=0 AND CompanyID=" & GBLCompanyID & " Having Count(ItemQCID)>0"
                        db.FillDataTable(dtParams, StrQuery)
                        If dtParams.Rows.Count > 0 Then
                            If Val(dtParams.Rows(0)("QCParams")) > 0 Then dtItems.Rows(i)("QCParametersCount") = Val(dtParams.Rows(0)("QCParams"))
                        End If
                    Else
                        StrQuery = "Select Count(ItemQCID) AS QCParams From ItemQCParameterSetting Where ItemGroupID=" & Val(dtItems.Rows(i)("ItemGroupID")) & " AND ItemSubGroupUniqueID=" & Val(dtItems.Rows(i)("ItemSubGroupID")) & " AND isnull(IsDeletedTransaction,0)=0 AND CompanyID=" & GBLCompanyID & " Having Count(ItemQCID)>0"
                        db.FillDataTable(dtParams, StrQuery)
                        If dtParams.Rows.Count > 0 Then
                            If Val(dtParams.Rows(0)("QCParams")) > 0 Then
                                dtItems.Rows(i)("QCParametersCount") = Val(dtParams.Rows(0)("QCParams"))
                            End If
                        Else
                            ShowParentItemSubGroupID(Val(dtItems.Rows(i)("ItemSubGroupID")), GBLUnderSubGroupIDString)
                            If Trim(GBLUnderSubGroupIDString) <> "" Then
                                StrQuery = "Select Count(ItemQCID) AS QCParams From ItemQCParameterSetting Where ItemGroupID=" & Val(dtItems.Rows(i)("ItemGroupID")) & " AND ItemSubGroupUniqueID IN(" & GBLUnderSubGroupIDString & ") AND isnull(IsDeletedTransaction,0)=0 AND CompanyID=" & GBLCompanyID & " Having Count(ItemQCID)>0"
                                db.FillDataTable(dtParams1, StrQuery)
                                If dtParams1.Rows.Count > 0 Then
                                    If Val(dtParams1.Rows(0)("QCParams")) > 0 Then dtItems.Rows(i)("QCParametersCount") = Val(dtParams1.Rows(0)("QCParams"))
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            'db.FillDataTable(dataTable, str)
            Dim dataTable As DataTable = dtItems.Copy()
            'data.Message = db.ConvertDataTableTojSonString(dtItems)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Sub ShowParentItemSubGroupID(ByVal UID As String, ByRef GBLUnderSubGroupIDString As String)

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        Dim dt As New DataTable
        db.FillDataTable(dt, "Select UnderSubGroupID From ItemSubGroupMaster Where ItemSubGroupID=" & UID & " /*AND CompanyID=" & GBLCompanyID & "*/ AND Isnull(IsDeletedTransaction,0)=0 AND UnderSubGroupID<>ItemSubGroupID AND ItemSubGroupID<>1")

        If dt.Rows.Count <= 0 Then
            GBLUnderSubGroupIDString = IIf(GBLUnderSubGroupIDString.Trim() = "", "", GBLUnderSubGroupIDString & ",") & Convert.ToString(UID)
        End If

        For i = 0 To dt.Rows.Count - 1
            ShowParentItemSubGroupID(IIf(IsDBNull(dt.Rows(i)(0)), 0, dt.Rows(i)(0)), GBLUnderSubGroupIDString)
            GBLUnderSubGroupIDString = IIf(GBLUnderSubGroupIDString.Trim() = "", "", GBLUnderSubGroupIDString & ",") & Convert.ToString(UID)
        Next
    End Sub

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetLastTransactionDate() As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        Dim lastTransactionDate As String = ""
        Dim whereCondition As String = " VoucherID=-14 AND T.CompanyID=" & GBLCompanyID & " AND Isnull(T.IsDeletedTransaction,0)=0 "

        lastTransactionDate = db.getLastVoucherDate("ItemTransactionMain", "VoucherDate", whereCondition)

        Return js.Serialize(lastTransactionDate)
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function getUserAuthority() As String
        Try
            GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            str = "Select CanReceiveExcessMaterial from UserMaster where /*CompanyID='" & GBLCompanyID & "' And*/ Isnull(IsDeletedUser,0)<>1 and UserID = '" & GBLUserID & "'"
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            js.MaxJsonLength = 2147483647
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    '-----------------------------------Get Receivers List From Employee------------------------------------------
    ' GetGrnItemList Function added By Mohini - 22May
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetGrnItemList(ByVal TransactionID As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            ''str = "Select Distinct LM.LedgerID,LM.LedgerName From LedgerMaster As LM Inner Join LedgerGroupMaster AS LGM On LGM.LedgerGroupID=LM.LedgerGroupID And LGM.CompanyID=LM.CompanyID  AND LGM.LedgerGroupNameID=27 Where LM.CompanyID=" & GBLCompanyID & " And LM.IsDeletedTransaction=0 Order By LM.LedgerName"
            '' Changed By Mohini 
            str = "SELECT Replace(Convert(Varchar(13),A.VoucherDate,106),' ','-') AS VoucherDate,  A.TransactionID,A.VoucherNo,A.VoucherDate, NULLIF(B.SupplierBatchNo,'') AS SupplierBatchNo, NullIf(B.BatchNo,'') AS BatchNo,ISNULL(B.ItemID,0) AS ItemID,  NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IM.ItemName,'') AS ItemName,Isnull(IM.UnitPerPacking, 1) As UnitPerPacking FROM ItemTransactionMain AS A INNER JOIN  ItemTransactionDetail AS B ON A.TransactionID =B.TransactionID  AND ISNULL(B.IsDeletedTransaction,0)=0 INNER JOIN ItemMaster AS IM ON IM.ItemID=B.ItemID And IM.CompanyID=B.CompanyID  AND ISNULL(IM.IsDeletedTransaction,0)=0 Where A.VoucherID = -14 And A.TransactionID ='" & TransactionID & "' AND  A.CompanyID='" & GBLCompanyID & "' AND ISNULL(A.IsDeletedTransaction,0)=0 "
            db.FillDataTable(dataTable, str)
            data.Message = db.ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    Public Class HelloWorldData
        Public Message As [String]
    End Class

End Class