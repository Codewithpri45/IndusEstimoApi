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
Public Class WebServicePurchaseOrderApproval
    Inherits System.Web.Services.WebService

    Dim db As New DBConnection
    Dim js As New JavaScriptSerializer()
    Dim data As New HelloWorldData()
    Dim dataTable As New DataTable()
    Dim str As String

    Dim GBL_User_ID As String
    Dim GBLCompanyID As String
    Dim GBLFYear As String
    Dim DBType As String = ""
    Dim ProductionUnitIDStr As String = ""
    Dim ProductionUnitID As String = ""
    '-----------------------------------Get UnApproved Purchase Order's List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UnApprovedPurchaseOrders(ByVal FromDate As String, ByVal ToDate As String) As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        str = "Select  ITM.TransactionID,ITD.ClientID,NulliF(LM1.LedgerName,'') as ClientName, ITM.VoucherID, ITM.LedgerID, ITD.TransID, ITD.ItemID, ITD.ItemGroupID," &
             "(SELECT COUNT(TransactionDetailID) FROM ItemTransactionDetail WHERE (TransactionID = ITM.TransactionID) And (CompanyID = ITM.CompanyID) And (IsDeletedTransaction = 0)) AS TotalItems, " &
             "LM.LedgerName, ITM.MaxVoucherNo, ITM.VoucherNo, REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate, IM.ItemCode, IM.ItemName, ITD.PurchaseOrderQuantity," &
             "ITD.PurchaseUnit, ITD.PurchaseRate, ITD.GrossAmount, ITD.DiscountAmount, ITD.BasicAmount, ITD.GSTPercentage, ITD.CGSTAmount + ITD.SGSTAmount + ITD.IGSTAmount As GSTTaxAmount," &
             "ITD.NetAmount, ITD.RefJobCardContentNo, UA.UserName As CreatedBy, UM.UserName As ApprovedBy, ITM.FYear, 0 As ReceiptTransactionID, ITM.PurchaseDivision, ITM.CurrencyCode," &
             "ITD.AuditApprovedBy, ITM.DealerID, ITM.PurchaseReferenceRemark, ITM.ModeOfTransport, ITM.DeliveryAddress, ITM.TermsOfDelivery, ITM.TermsOfPayment, ITD.TaxableAmount," &
             "ITM.TotalTaxAmount, ITD.BasicAmount As AfterDisAmt, ITM.Narration,(SELECT Replace(Convert(Varchar(13),Max(IT.VoucherDate),106),' ','-')  FROM ItemTransactionMain AS IT  INNER JOIN ItemTransactionDetail AS ID  ON IT.TransactionID=ID.TransactionID WHERE IT.VoucherID=-11  AND Isnull(IT.IsDeletedTransaction,0) = 0 AND Isnull(ID.IsDeletedTransaction,0) = 0 AND ID.ItemID=ITD.ItemID AND IT.TransactionID < ITM.TransactionID) AS LastPODate, Isnull(PUM.ProductionUnitID, 0) As ProductionUnitID, Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName," &
             "Nullif(CM.CompanyName,'') AS CompanyName,Isnull(CM.CompanyID, 0) As CompanyID " &
             "From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD On ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID " &
             "INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID " &
             "INNER JOIN UserMaster As UA On UA.UserID=ITM.CreatedBy LEFT JOIN UserMaster As UM On UM.UserID=ITD.VoucherItemApprovedBy " &
             "INNER JOIN ItemMaster As IM On IM.ItemID=ITD.ItemID INNER JOIN LedgerMaster As LM On LM.LedgerID=ITM.LedgerID " &
             "Left Join LedgerMaster As LM1 On LM1.LedgerID = ITD.ClientID " &
             "Where ITM.VoucherID= -11 And ITM.ProductionUnitID In(" & ProductionUnitIDStr & ") And ITD.IsDeletedTransaction=0 And ITD.IsVoucherItemApproved =0 And ITD.IsCancelled =0 And " &
             "((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) " &
             "Order By ITM.FYear Desc, ITM.MaxVoucherNo Desc "

        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Approved Purchase Order's List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ApprovedPurchaseOrders(ByVal FromDate As String, ByVal ToDate As String) As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select ITM.TransactionID,ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,(Select Count(TransactionDetailID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID AND IsDeletedTransaction=0) AS TotalItems,LM.LedgerName,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,IM.ItemCode,IM.ItemName,ITD.PurchaseOrderQuantity,ITD.PurchaseUnit,ITD.PurchaseRate,ITD.GrossAmount,ITD.DiscountAmount,ITD.BasicAmount,ITD.GSTPercentage,(IFNULL(ITD.CGSTAmount,0)+IFNULL(ITD.SGSTAmount,0)+IFNULL(ITD.IGSTAmount,0)) AS GSTTaxAmount,ITD.NetAmount,ITD.RefJobCardContentNo,UA.UserName AS CreatedBy,UM.UserName AS ApprovedBy,Replace(Convert(date_format(ITD.VoucherItemApprovedDate,'%d-%b-%Y'),char(30)),' ','-') As ApprovalDate,ITD.FYear,PurchaseDivision,ITM.CurrencyCode,AuditApprovedBy,DealerID,PurchaseReferenceRemark,ModeOfTransport,DeliveryAddress,TermsOfDelivery,ITM.TermsOfPayment,TaxableAmount,ITM.NetAmount,TotalTaxAmount,ITD.BasicAmount As AfterDisAmt,Narration " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID  INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy  AND UA.CompanyID=ITM.CompanyID " &
                  " Where ITM.VoucherID= -11 And ITM.CompanyID=" & GBLCompanyID & " And ITM.FYear='" & GBLFYear & "' And ITD.IsDeletedTransaction = 0 And ITD.IsVoucherItemApproved = 1 Order By ITM.TransactionID Desc"
        Else
            str = "Select ITM.TransactionID,ITM.MaxVoucherNo, ITD.ClientID,NulliF(LM1.LedgerName,'') as ClientName, ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,(Select Count(TransactionDetailID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID AND IsDeletedTransaction=0) AS TotalItems,LM.LedgerName,ITM.VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,IM.ItemCode,IM.ItemName, " &
              " ITD.PurchaseOrderQuantity,ITD.PurchaseUnit,ITD.PurchaseRate,ITD.GrossAmount,ITD.DiscountAmount,ITD.BasicAmount,ITD.GSTPercentage,(Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,ITD.NetAmount,ITD.RefJobCardContentNo,UA.UserName AS CreatedBy,UM.UserName AS ApprovedBy,Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,ITM.FYear,/*Isnull((Select Top 1 TransactionID From ItemTransactionDetail Where PurchaseTransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID And IsDeletedTransaction=0),0) AS ReceiptTransactionID,*/PurchaseDivision,ITM.CurrencyCode,AuditApprovedBy,DealerID,PurchaseReferenceRemark,ModeOfTransport,DeliveryAddress,TermsOfDelivery,ITM.TermsOfPayment,TaxableAmount,ITM.NetAmount,TotalTaxAmount,ITD.BasicAmount As AfterDisAmt,Narration,PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName,Isnull(CM.CompanyID, 0) As CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID  Left Join LedgerMaster as LM1 on LM1.LedgerID = ITD.ClientID " &
              " Where ITM.VoucherID= -11 And ITM.ProductionUnitID In(" & ProductionUnitIDStr & ") /*And ITM.FYear='" & GBLFYear & "'*/ And ITD.IsDeletedTransaction = 0 And ITD.IsVoucherItemApproved = 1 And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) Order By ITM.FYear Desc,ITM.MaxVoucherNo Desc"
        End If

        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function IsPurchaseOrdersProcessed(ByVal TransactionID As Integer) As Boolean
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        Return db.IsDeletable("TransactionID", "ItemTransactionDetail", "WHERE (PurchaseTransactionID = " & TransactionID & ") AND (ProductionUnitID = " & ProductionUnitID & ") AND (IsDeletedTransaction = 0)")
    End Function

    '-----------------------------------Get Approved Purchase Order's List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CancelledPurchaseOrders(ByVal FromDate As String, ByVal ToDate As String) As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select ITM.TransactionID,ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,(Select Count(TransactionDetailID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID AND IFNULL(IsDeletedTransaction,0)=0) AS TotalItems,LM.LedgerName,ITM.MaxVoucherNo,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IFNULL(IM.ItemName,''),'') AS ItemName,ITD.PurchaseOrderQuantity,ITD.PurchaseUnit,ITD.PurchaseRate,ITD.GrossAmount,ITD.DiscountAmount,ITD.BasicAmount,ITD.GSTPercentage,(IFNULL(ITD.CGSTAmount,0)+IFNULL(ITD.SGSTAmount,0)+IFNULL(ITD.IGSTAmount,0)) AS GSTTaxAmount,ITD.NetAmount,ITD.RefJobCardContentNo,UA.UserName AS CreatedBy,UM.UserName AS ApprovedBy,Replace(Convert(date_format(ITD.VoucherItemApprovedDate,'%d-%b-%Y'),char(30)),' ','-') As ApprovalDate,ITD.FYear,PurchaseDivision,ITM.CurrencyCode,AuditApprovedBy,DealerID,PurchaseReferenceRemark,ModeOfTransport,DeliveryAddress,TermsOfDelivery,ITM.TermsOfPayment,TaxableAmount,ITM.NetAmount,TotalTaxAmount,ITD.BasicAmount As AfterDisAmt,Narration " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID " &
                  " Where ITM.VoucherID= -11 And ITM.CompanyID=" & GBLCompanyID & "  And ITD.IsDeletedTransaction=0 And ITD.IsVoucherItemApproved=0 And ITD.IsCancelled=1 Order By ITM.TransactionID Desc"
        Else
            str = "Select ITM.TransactionID, ITD.ClientID,NulliF(LM1.LedgerName,'') as ClientName, ITM.VoucherID,ITM.LedgerID,ITD.TransID,ITD.ItemID,ITD.ItemGroupID,(Select Count(TransactionDetailID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID AND Isnull(IsDeletedTransaction,0)=0) AS TotalItems,LM.LedgerName,ITM.MaxVoucherNo,ITM.VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(Isnull(IM.ItemName,''),'') AS ItemName, " &
              " ITD.PurchaseOrderQuantity,ITD.PurchaseUnit,ITD.PurchaseRate,ITD.GrossAmount,ITD.DiscountAmount,ITD.BasicAmount,ITD.GSTPercentage,(Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,ITD.NetAmount,ITD.RefJobCardContentNo,UA.UserName AS CreatedBy,UM.UserName AS ApprovedBy,Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,ITM.FYear,/*Isnull((Select Top 1 TransactionID From ItemTransactionDetail Where PurchaseTransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID And IsDeletedTransaction=0),0) AS ReceiptTransactionID,*/PurchaseDivision,ITM.CurrencyCode,AuditApprovedBy,DealerID,PurchaseReferenceRemark,ModeOfTransport,DeliveryAddress,TermsOfDelivery,ITM.TermsOfPayment,TaxableAmount,ITM.NetAmount,TotalTaxAmount,ITD.BasicAmount As AfterDisAmt,Narration,(SELECT Replace(Convert(Varchar(13),Max(IT.VoucherDate),106),' ','-')  FROM ItemTransactionMain AS IT  INNER JOIN ItemTransactionDetail AS ID  ON IT.TransactionID=ID.TransactionID WHERE IT.VoucherID=-11  AND Isnull(IT.IsDeletedTransaction,0) = 0  AND Isnull(ID.IsDeletedTransaction,0) = 0  AND ID.ItemID=ITD.ItemID AND IT.TransactionID < ITM.TransactionID) AS LastPODate,PUM.ProductionUnitID,PUM.ProductionUnitName,CM.CompanyName,Isnull(CM.CompanyID, 0) As CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy Inner JOIN ProductionUnitMaster As PUM on PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN CompanyMaster as CM on CM.CompanyID = PUM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy  INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID  Left Join LedgerMaster as LM1 on LM1.LedgerID = ITD.ClientID " &
              " Where ITM.VoucherID= -11  And ITM.ProductionUnitID In(" & ProductionUnitIDStr & ") And ITD.IsDeletedTransaction=0 And ITD.IsVoucherItemApproved=0 And ITD.IsCancelled=1 And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) Order By ITM.FYear Desc,ITM.MaxVoucherNo Desc "
        End If
        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    ''----------------------------Open PickListStatus  Update Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateData(ByVal BtnText As String, ByVal jsonObjectsRecordDetail As Object, ByVal recordForStockUpdate As Object, ByVal Remark As String) As String

        Dim dt As New DataTable
        Dim KeyField As String
        Dim AddColName, wherecndtn, TableName As String
        AddColName = ""

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBL_User_ID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Dim CanCrud = db.validateProductionUnit(GBL_User_ID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If
        Try

            TableName = "ItemTransactionDetail"
            If db.CheckAuthories("PurchaseOrderApproval.aspx", GBL_User_ID, GBLCompanyID, "CanSave", "For " & BtnText) = False Then Return "You are not authorized"

            If DBType = "MYSQL" Then
                If BtnText = "Approve" Then
                    AddColName = "ModifiedDate=NOW(),ModifiedBy=" & GBL_User_ID & ",IsVoucherItemApproved=1,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=NOW(),IsCancelled=0"
                ElseIf BtnText = "UnApprove" Then
                    If db.CheckAuthories("PurchaseOrderApproval.aspx", GBL_User_ID, GBLCompanyID, "CanEdit", "For " & BtnText) = False Then Return "You are not authorized"
                    AddColName = "ModifiedDate=NOW(),ModifiedBy=" & GBL_User_ID & ",IsVoucherItemApproved=0,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=NOW(),IsCancelled=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "ModifiedDate=NOW(),ModifiedBy=" & GBL_User_ID & ",IsCancelled=1,CancelledBy=" & GBL_User_ID & ",CancelledDate=NOW(),IsVoucherItemApproved=0"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "ModifiedDate=NOW(),ModifiedBy=" & GBL_User_ID & ",IsCancelled=0,CancelledBy=" & GBL_User_ID & ",CancelledDate=NOW(),IsVoucherItemApproved=0"
                End If
            Else
                If BtnText = "Approve" Then
                    AddColName = "ModifiedDate=GETDATE(),ModifiedBy=" & GBL_User_ID & ",IsVoucherItemApproved=1,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=GETDATE(),IsCancelled=0"
                ElseIf BtnText = "UnApprove" Then
                    If db.CheckAuthories("PurchaseOrderApproval.aspx", GBL_User_ID, GBLCompanyID, "CanEdit", "For " & BtnText) = False Then Return "You are not authorized"
                    AddColName = "ModifiedDate=GETDATE(),ModifiedBy=" & GBL_User_ID & ",IsVoucherItemApproved=0,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=GETDATE(),IsCancelled=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "ModifiedDate=GETDATE(),ModifiedBy=" & GBL_User_ID & ",IsCancelled=1,CancelledBy=" & GBL_User_ID & ",CancelledDate=GETDATE(),IsVoucherItemApproved=0,CancelRemark = '" & Remark & "'"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "ModifiedDate=GETDATE(),ModifiedBy=" & GBL_User_ID & ",IsCancelled=0,CancelledBy=" & GBL_User_ID & ",CancelledDate=GETDATE(),IsVoucherItemApproved=0,CancelRemark= '" & Remark & "'"
                End If
            End If

            wherecndtn = " ProductionUnitID = " & ProductionUnitID
            db.UpdateDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, 1, wherecndtn)
            KeyField = "Success"

            db.ConvertObjectToDatatable(recordForStockUpdate, dt, str)
            For Each row As DataRow In dt.Rows
                db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & row(0) & "," & row(1))
            Next

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    '---------------Close Master code---------------------------------

    Public Class HelloWorldData
        Public Message As [String]
    End Class

End Class




