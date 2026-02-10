Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Services
Imports System.Web.Script.Serialization
Imports Connection
Imports System.Collections.Generic
Imports System.Web.UI.WebControls
Imports System.Net
Imports System.IO
Imports System.Net.Mail

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
<System.Web.Script.Services.ScriptService()>
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class WebServiceRequisitionApproval
    Inherits System.Web.Services.WebService

    Dim db As New DBConnection
    Dim js As New JavaScriptSerializer()
    Dim data As New HelloWorldData()
    Dim dataTable As New DataTable()
    Dim str As String

    Dim GBL_User_ID As String
    Dim GBLCompanyID As String
    Dim GBL_F_Year As String
    Dim DBType As String = ""
    Dim ProductionUnitIDStr As String = ""
    Dim ProductionUnitID As String = ""

    Private Function ConvertDataTableTojSonString(ByVal dataTable As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        serializer.MaxJsonLength = 2147483647
        Dim tableRows As New List(Of Dictionary(Of [String], [Object]))()
        Dim row As Dictionary(Of [String], [Object])
        For Each dr As DataRow In dataTable.Rows
            row = New Dictionary(Of [String], [Object])()
            For Each col As DataColumn In dataTable.Columns
                row.Add(col.ColumnName, dr(col))
                System.Console.WriteLine(dr(col))
            Next
            tableRows.Add(row)
        Next
        Return serializer.Serialize(tableRows)
    End Function

    '-----------------------------------Get UnApproved Requisition List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UnApprovedRequisitions(ByVal FromDate As String, ByVal ToDate As String) As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select Distinct ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,(Select Count(TransactionID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID) AS TotalItems,IFNULL(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsVoucherItemApproved,0)=0 And IFNULL(ITD.IsCancelled,0)=0 AND IFNULL(ITD.IsAuditApproved,0)=1  Order By ITM.TransactionID Desc"
        Else
            str = "Select  DISTINCT ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID, ITM.VoucherNo, REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate, NULLIF (IM.ItemCode, '') AS ItemCode, NULLIF (IGM.ItemGroupName, '') AS ItemGroupName,  NULLIF (ISGM.ItemSubGroupName, '') AS ItemSubGroupName, NULLIF (IM.ItemName, '') AS ItemName, NULLIF (IM.ItemDescription, '') AS ItemDescription, NULLIF (ITD.RefJobCardContentNo, '') AS RefJobCardContentNo, ISNULL(ITD.RequiredQuantity, 0) AS RequiredQuantity,  NULLIF (ITD.StockUnit, '') AS StockUnit, NULLIF (ITD.ItemNarration, '') AS ItemNarration, REPLACE(CONVERT(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate, NULLIF (ITM.Narration, '') AS Narration, " &
                   "(SELECT COUNT(TransactionID) AS Expr1 FROM    ItemTransactionDetail WHERE (TransactionID = ITM.TransactionID) And (CompanyID = ITM.CompanyID)) AS TotalItems, ISNULL(ITM.TotalQuantity, 0) AS TotalQuantity, NULLIF (ITM.FYear, '') AS FYear, NULLIF (UA.UserName, '') AS CreatedBy, ISNULL(PUM.ProductionUnitID, 0) AS ProductionUnitID, NULLIF (PUM.ProductionUnitName, '') AS ProductionUnitName, NULLIF (CM.CompanyName, '') AS CompanyName, ISNULL(CM.CompanyID, 0) AS CompanyID " &
                   "FROM   ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID = ITM.TransactionID And ITD.CompanyID = ITM.CompanyID INNER JOIN ItemMaster As IM On IM.ItemID = ITD.ItemID INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID = IM.ItemGroupID INNER JOIN UserMaster As UA On UA.UserID = ITM.CreatedBy Inner JOIN  ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner JOIN  CompanyMaster As CM On CM.CompanyID = PUM.CompanyID LEFT OUTER JOIN  ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID = IM.ItemSubGroupID " &
                   "WHERE(ISNULL(ITM.VoucherID, 0) = -9) And (ISNULL(ITM.IsDeletedTransaction, 0) = 0) And (ISNULL(ITD.IsVoucherItemApproved, 0) = 0) And (ISNULL(ITD.IsCancelled, 0) = 0) And (ISNULL(ITD.IsAuditApproved, 0) = 1) And (CAST(FLOOR(CAST(ITM.VoucherDate As float)) As DateTime) >= '" + FromDate + "') AND (CAST(FLOOR(CAST(ITM.VoucherDate AS float)) AS DateTime) <= '" + ToDate + "')  AND (ITM.ProductionUnitID IN (" & ProductionUnitIDStr & ")) ORDER BY FYear Desc, ITM.MaxVoucherNo DESC "
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get UnApproved Requisition List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ApprovedRequisitions(ByVal FromDate As String, ByVal ToDate As String) As String

        GBL_F_Year = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select Distinct ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,IM.ItemCode, NULLIF (IGM.ItemGroupName, '') AS ItemGroupName,NULLIF (ISGM.ItemSubGroupName, '') AS ItemSubGroupName, IM.ItemName, NULLIF (ITD.RefJobCardContentNo, '') AS RefJobCardContentNo, ITD.RequiredQuantity, NULLIF (ITD.StockUnit, '') AS StockUnit, Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NULLIF (ITD.ItemNarration, '') AS ItemNarration, NULLIF (ITM.Narration, '') AS Narration,(SELECT COUNT(TransactionID) AS Expr1 FROM ItemTransactionDetail WHERE (TransactionID = ITM.TransactionID) AND (CompanyID = ITM.CompanyID)) AS TotalItems, ITM.TotalQuantity, NULLIF (ITM.FYear, '') AS FYear, NULLIF (UA.UserName, '') AS CreatedBy, NULLIF (U.UserName, '') AS ApprovedBy,Replace(Convert(date_format(ITD.VoucherItemApprovedDate,'%d-%b-%Y'),char(30)),' ','-') As ApprovalDate,(SELECT TransactionID FROM ItemPurchaseRequisitionDetail WHERE (RequisitionTransactionID = ITD.TransactionID) AND (ItemID = ITD.ItemID) AND (CompanyID = ITD.CompanyID) And IsDeletedTransaction=0 limit 1) AS PurchaseTransactionID " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID  INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UserMaster As U ON U.UserID=ITD.VoucherItemApprovedBy AND U.CompanyID=ITD.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9  And ITM.CompanyID=" & GBLCompanyID & " And ITM.FYear='" & GBL_F_Year & "'  And IFNULL(ITM.IsDeletedTransaction,0)=0  And IFNULL(ITD.IsVoucherItemApproved,0)=1  Order By ITM.TransactionID Desc"
        Else
            str = "Select  Distinct ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID,ITM.VoucherNo, REPLACE(CONVERT(Varchar(13), ITM.VoucherDate, 106), ' ', '-') AS VoucherDate, IM.ItemCode, NULLIF (IGM.ItemGroupName, '') AS ItemGroupName, NULLIF (ISGM.ItemSubGroupName, '') AS ItemSubGroupName, IM.ItemName, NULLIF (ITD.RefJobCardContentNo, '') AS RefJobCardContentNo, ITD.RequiredQuantity, NULLIF (ITD.StockUnit, '') AS StockUnit, REPLACE(CONVERT(Varchar(13), ITD.ExpectedDeliveryDate, 106), ' ', '-') AS ExpectedDeliveryDate, NULLIF (ITD.ItemNarration, '') AS ItemNarration, NULLIF (ITM.Narration, '') AS Narration, " &
                  "(SELECT COUNT(TransactionID) AS Expr1 FROM ItemTransactionDetail WHERE (TransactionID = ITM.TransactionID) And (CompanyID = ITM.CompanyID)) AS TotalItems, ITM.TotalQuantity, NULLIF (ITM.FYear, '') AS FYear, NULLIF (UA.UserName, '') AS CreatedBy, NULLIF (U.UserName, '') AS ApprovedBy, REPLACE(CONVERT(Varchar(13), ITD.VoucherItemApprovedDate, 106), ' ', '-') AS ApprovalDate,Isnull(PUM.ProductionUnitID,0) as ProductionUnitID,Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,Nullif(CM.CompanyName,'') AS CompanyName, ISNULL(CM.CompanyID, 0) AS CompanyID,  (SELECT TOP (1) TransactionID FROM ItemPurchaseRequisitionDetail WHERE (RequisitionTransactionID = ITD.TransactionID) AND (ItemID = ITD.ItemID) AND (CompanyID = ITD.CompanyID) And IsDeletedTransaction=0) AS PurchaseTransactionID " &
                  "From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM On IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID=IM.ItemGroupID  INNER JOIN UserMaster As UA On UA.UserID=ITM.CreatedBy INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID  LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID=IM.ItemSubGroupID  LEFT JOIN UserMaster As U On U.UserID=ITD.VoucherItemApprovedBy " &
                  "Where Isnull(ITM.VoucherID, 0) = -9 /*And ITM.FYear ='" & GBL_F_Year & "'*/ And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsVoucherItemApproved,0)=1 And ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ")   Order By FYear Desc, ITM.TransactionID Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Cancelled Requisition ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CancelledRequisitions(ByVal FromDate As String, ByVal ToDate As String) As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select Distinct ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(ITD.ItemNarration,'') AS ItemNarration,NullIf(ITM.Narration,'') AS Narration,(Select Count(TransactionID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID) AS TotalItems,IFNULL(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf(U.UserName,'') AS ApprovedBy,Replace(Convert(date_format(ITD.VoucherItemApprovedDate,'%d-%b-%Y'),char(30)),' ','-') As ApprovalDate,IFNULL((Select TransactionID From ItemPurchaseRequisitionDetail Where RequisitionTransactionID= ITD.TransactionID AND ItemID=ITD.ItemID AND CompanyID=ITD.CompanyID limit 1),0) AS  PurchaseTransactionID " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID LEFT JOIN UserMaster As U ON U.UserID=ITD.VoucherItemApprovedBy AND U.CompanyID=ITD.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsVoucherItemApproved,0)=0 And IFNULL(ITD.IsCancelled,0)=1  Order By ITM.TransactionID Desc"
        Else

            str = "Select  Distinct ITM.TransactionID, ITM.VoucherID, ITM.MaxVoucherNo, ITD.ItemGroupID, ITD.ItemID,ITM.VoucherNo, Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,  " &
                   " Isnull(ITD.RequiredQuantity, 0) As RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(ITD.ItemNarration,'') AS ItemNarration,NullIf(ITM.Narration,'') AS Narration,(Select Count(TransactionID) From ItemTransactionDetail Where TransactionID=ITM.TransactionID AND CompanyID=ITM.CompanyID) AS TotalItems,Isnull(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf(U.UserName,'') AS ApprovedBy,Replace(Convert(Varchar(13),ITD.VoucherItemApprovedDate,106),' ','-') AS ApprovalDate,Isnull((Select Top 1  TransactionID From ItemPurchaseRequisitionDetail Where RequisitionTransactionID= ITD.TransactionID AND ItemID=ITD.ItemID AND CompanyID=ITD.CompanyID),0) AS  PurchaseTransactionID  , Isnull(PUM.ProductionUnitID,0) as ProductionUnitID,Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,Nullif(CM.CompanyName,'') AS CompanyName, ISNULL(CM.CompanyID, 0) AS CompanyID " &
                   " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM On IM.ItemID=ITD.ItemID  INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID=IM.ItemGroupID  INNER JOIN UserMaster As UA On UA.UserID=ITM.CreatedBy INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID=IM.ItemSubGroupID  LEFT JOIN UserMaster As U On U.UserID=ITD.VoucherItemApprovedBy   " &
                   " Where Isnull(ITM.VoucherID, 0) = -9 And Isnull(ITM.IsDeletedTransaction, 0) = 0 And Isnull(ITD.IsVoucherItemApproved, 0) = 0 And Isnull(ITD.IsCancelled, 0) = 1 And ((Cast(Floor(cast(ITM.VoucherDate As float)) As DateTime) >= '" + FromDate + "'))  AND ((Cast(Floor(cast(ITM.VoucherDate as float)) as DateTime) <= '" + ToDate + "')) And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ")    Order By FYear Desc, ITM.TransactionID Desc "
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit UnApproved Requisition List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function AuditUnApprovedRequisitions() As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct ITM.TransactionID,ITM.VoucherID,ITM.MaxVoucherNo,ITD.ItemGroupID,ITD.ItemID AS RequisitionItemID,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,IFNULL(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsVoucherItemApproved,0)=0 And IFNULL(ITD.IsAuditApproved,0)=0 And IFNULL(ITD.IsAuditCancelled,0)=0 Order By ITM.TransactionID Desc"
        Else
            str = "Select Distinct ITM.TransactionID,ITM.VoucherID,ITM.MaxVoucherNo,ITD.ItemGroupID,ITD.ItemID AS RequisitionItemID,ITM.VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,Isnull(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsVoucherItemApproved,0)=0 And Isnull(ITD.IsAuditApproved,0)=0 And Isnull(ITD.IsAuditCancelled,0)=0 Order By ITM.TransactionID Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit UnApproved Requisition Detail List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function AuditUnApprovedRequisitionDetails() As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ITM.TransactionID,0) AS RequisitionTransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(IDM.MaxVoucherNo,0) AS MaxVoucherNo,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(ITD.ItemID,0) AS RequisitionItemID,IFNULL(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(date_format(IDM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID AND IM.CompanyID=ID.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy AND UA.CompanyID=IDM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsVoucherItemApproved,0)=0 And IFNULL(ITD.IsAuditApproved,0)=0 And IFNULL(ITD.IsAuditCancelled,0)=0 Order By FYear,MaxVoucherNo Desc"
        Else
            str = "Select Distinct Isnull(ITM.TransactionID,0) AS RequisitionTransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(IDM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(ITD.ItemID,0) AS RequisitionItemID,Isnull(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),IDM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID  " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsVoucherItemApproved,0)=0 And Isnull(ITD.IsAuditApproved,0)=0 And Isnull(ITD.IsAuditCancelled,0)=0  Order By FYear,MaxVoucherNo Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit UnApproved Requisition List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function AuditApprovedRequisitions() As String
        GBL_F_Year = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct ITM.TransactionID,ITM.VoucherID,ITM.MaxVoucherNo,ITD.ItemGroupID,ITD.ItemID AS RequisitionItemID,ITM.VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,IFNULL(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf(U.UserName,'') AS ApprovedBy,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN UserMaster As U ON U.UserID=ITD.AuditApprovedBy AND U.CompanyID=ITD.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9  And ITM.CompanyID=" & GBLCompanyID & " And ITM.FYear='" & GBL_F_Year & "'  And IFNULL(ITM.IsDeletedTransaction,0)=0  And IFNULL(ITD.IsAuditApproved,0)=1 And IFNULL(ITD.IsAuditCancelled,0)=0 Order By ITM.TransactionID Desc"
        Else
            str = "Select Distinct ITM.TransactionID,ITM.VoucherID,ITM.MaxVoucherNo,ITD.ItemGroupID,ITD.ItemID AS RequisitionItemID,ITM.VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,Isnull(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf(U.UserName,'') AS ApprovedBy,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy INNER JOIN UserMaster As U ON U.UserID=ITD.AuditApprovedBy LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And ITM.FYear='" & GBL_F_Year & "' And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsAuditApproved,0)=1 And Isnull(ITD.IsAuditCancelled,0)=0 Order By ITM.TransactionID Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit UnApproved Requisition Detail List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function AuditApprovedRequisitionDetails() As String

        GBL_F_Year = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ITM.TransactionID,0) AS RequisitionTransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(IDM.MaxVoucherNo,0) AS MaxVoucherNo,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(ITD.ItemID,0) AS RequisitionItemID,IFNULL(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(date_format(IDM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID AND IM.CompanyID=ID.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy AND UA.CompanyID=IDM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And ITM.FYear='" & GBL_F_Year & "' And IFNULL(ITM.IsDeletedTransaction,0)=0  And IFNULL(ITD.IsAuditApproved,0)=1 AND IFNULL(ITD.AuditApprovedBy,0)>0  And IFNULL(ITD.IsAuditCancelled,0)=0 Order By FYear,MaxVoucherNo Desc"
        Else
            str = "Select Distinct Isnull(ITM.TransactionID,0) AS RequisitionTransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(IDM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(ITD.ItemID,0) AS RequisitionItemID,Isnull(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),IDM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And ITM.FYear='" & GBL_F_Year & "' And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsAuditApproved,0)=1 AND Isnull(ITD.AuditApprovedBy,0)>0  And Isnull(ITD.IsAuditCancelled,0)=0 Order By FYear,MaxVoucherNo Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit Cancelled Requisition List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CancelledAuditRequisitions() As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,IFNULL(ITD.ItemGroupID,0) AS ItemGroupID,IFNULL(ITD.ItemID,0) AS RequisitionItemID,NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(date_format(ITM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,IFNULL(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf('','') AS ApprovedBy,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID AND IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsAuditApproved,0)=0 AND IFNULL(ITD.IsAuditCancelled,0)=1  Order By FYear,MaxVoucherNo Desc"
        Else
            str = "Select Distinct Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(ITD.ItemGroupID,0) AS ItemGroupID,Isnull(ITD.ItemID,0) AS RequisitionItemID,NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(ITM.Narration,'') AS Narration,Isnull(ITM.TotalQuantity,0) AS TotalQuantity,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,NullIf('','') AS ApprovedBy,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved ,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsAuditApproved,0)=0 AND Isnull(ITD.IsAuditCancelled,0)=1  Order By FYear,MaxVoucherNo Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Audit Cancelled Requisition Detail List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CancelledAuditRequisitionDetails() As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ITM.TransactionID,0) AS RequisitionTransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(IDM.MaxVoucherNo,0) AS MaxVoucherNo,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(ITD.ItemID,0) AS RequisitionItemID,IFNULL(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(date_format(IDM.VoucherDate,'%d-%b-%Y'),char(30)),' ','-') As VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(date_format(ITD.ExpectedDeliveryDate,'%d-%b-%Y'),char(30)),' ','-') As ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID AND IM.CompanyID=ID.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy AND UA.CompanyID=IDM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID " &
                  " Where IFNULL(ITM.VoucherID,0) =-9 And ITM.CompanyID=" & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsAuditApproved,0)=0  AND IFNULL(ITD.IsAuditCancelled,0)=1 Order By FYear,MaxVoucherNo Desc"
        Else
            str = "Select Distinct Isnull(ITM.TransactionID,0) AS RequisitionTransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(IDM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(ITD.ItemID,0) AS RequisitionItemID,Isnull(ID.ItemID,0) AS IndentItemID,NullIf(IDM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),IDM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ID.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ID.ItemNarration,'') AS ItemNarration,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,NullIf(IDM.Narration,'') AS Narration,NullIf(IDM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,ISNULL(PUM.ProductionUnitID,0) as ProductionUnitID,ISNULL(PUM.ProductionUnitName,0) as ProductionUnitName,ISNULL(CM.CompanyName,0) as CompanyName,CM.CompanyID " &
              " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID AND ITD.CompanyID=ITM.CompanyID INNER JOIN ItemTransactionDetail AS ID ON ID.RequisitionTransactionID=ITM.TransactionID AND ID.RequisitionItemID=ITD.ItemID AND ID.CompanyID=ITD.CompanyID INNER JOIN ItemTransactionMain AS IDM ON IDM.TransactionID=ID.TransactionID AND IDM.CompanyID=ID.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ID.ItemID AND IM.CompanyID=ID.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID AND IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=IDM.CreatedBy AND UA.CompanyID=IDM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND ISGM.CompanyID=IM.CompanyID INNER JOIN ProductionUnitMaster As PUM ON PUM.ProductionUnitID = ITM.ProductionUnitID And ISNULL(PUM.IsDeletedTransaction,0) =0 INNER JOIN CompanyMaster As CM ON CM.CompanyID = PUM.CompanyID " &
              " Where Isnull(ITM.VoucherID,0) =-9 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And Isnull(ITM.IsDeletedTransaction,0)=0 And Isnull(ITD.IsAuditApproved,0)=0  AND Isnull(ITD.IsAuditCancelled,0)=1 Order By FYear,MaxVoucherNo Desc"
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Close Master code---------------------------------

    ''----------------------------Open PickListStatus  Update Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateData(ByVal BtnText As String, ByVal jsonObjectsRecordDetail As Object) As String

        Dim dt As New DataTable
        Dim KeyField As String
        Dim AddColName, wherecndtn, TableName As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBL_User_ID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Try
            Dim CanCrud = db.validateProductionUnit(GBL_User_ID, "Save")
            If CanCrud <> "Authorize" Then
                Return CanCrud
            End If

            If (db.CheckAuthories("RequisitionApproval.aspx", GBL_User_ID, GBLCompanyID, "CanSave", "For " & BtnText) = False) Then Return "You are not authorized to save..!, Can't Approve"

            TableName = "ItemTransactionDetail"
            AddColName = ""
            wherecndtn = ""
            If DBType = "MYSQL" Then
                If BtnText = "Approve" Then
                    AddColName = "IsVoucherItemApproved=1,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=NOW(),IsCancelled=0"
                ElseIf BtnText = "UnApprove" Then
                    AddColName = "IsVoucherItemApproved=0,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=NOW(),IsCancelled=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "IsCancelled=1,CancelledBy=" & GBL_User_ID & ",CancelledDate=NOW(),IsVoucherItemApproved=0"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "IsCancelled=0,CancelledBy=" & GBL_User_ID & ",CancelledDate=NOW(),IsVoucherItemApproved=0"
                End If
            Else
                If BtnText = "Approve" Then
                    AddColName = "IsVoucherItemApproved=1,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=GetDate(),IsCancelled=0"
                ElseIf BtnText = "UnApprove" Then
                    AddColName = "IsVoucherItemApproved=0,VoucherItemApprovedBy=" & GBL_User_ID & ",VoucherItemApprovedDate=GetDate(),IsCancelled=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "IsCancelled=1,CancelledBy=" & GBL_User_ID & ",CancelledDate=GetDate(),IsVoucherItemApproved=0"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "IsCancelled=0,CancelledBy=" & GBL_User_ID & ",CancelledDate=GetDate(),IsVoucherItemApproved=0"
                End If
            End If

            wherecndtn = "ProductionUnitID=" & ProductionUnitID & " "
            KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, 2, wherecndtn)

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    ''----------------------------Update Audit Approval Data------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateRequisitionAuditData(ByVal BtnText As String, ByVal jsonObjectsRecordDetail As Object) As String

        Dim KeyField As String
        Dim AddColName, wherecndtn, TableName As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBL_User_ID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBL_F_Year = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        Dim CanCrud = db.validateProductionUnit(GBL_User_ID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If
        Try

            If (db.CheckAuthories("AuditApproval.aspx", GBL_User_ID, GBLCompanyID, "CanEdit", "For " & BtnText) = False) Then Return "You are not authorized to update..!, Can't Approve"

            TableName = "ItemTransactionDetail"
            AddColName = ""
            wherecndtn = ""
            If DBType = "MYSQL" Then
                If BtnText = "Approve" Then
                    AddColName = "IsAuditApproved=1,AuditApprovedBy=" & GBL_User_ID & ",AuditApprovedDate=NOW(),IsAuditCancelled=0,AuditCancelledBy=0"
                ElseIf BtnText = "UnApprove" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=" & GBL_User_ID & ",AuditApprovedDate=NOW(),IsAuditCancelled=0,AuditCancelledBy=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=0,AuditApprovedDate=NOW(),IsAuditCancelled=1,AuditCancelledBy=" & GBL_User_ID & ",AuditCancelledDate=NOW()"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=" & GBL_User_ID & ",IsAuditCancelled=0,AuditCancelledBy=0,AuditCancelledDate=NOW()"
                End If
            Else
                If BtnText = "Approve" Then
                    AddColName = "IsAuditApproved=1,AuditApprovedBy=" & GBL_User_ID & ",AuditApprovedDate=GETDATE(),IsAuditCancelled=0,AuditCancelledBy=0"
                ElseIf BtnText = "UnApprove" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=" & GBL_User_ID & ",AuditApprovedDate=GETDATE(),IsAuditCancelled=0,AuditCancelledBy=0"
                ElseIf BtnText = "Cancel" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=0,AuditApprovedDate=GETDATE(),IsAuditCancelled=1,AuditCancelledBy=" & GBL_User_ID & ",AuditCancelledDate=GETDATE()"
                ElseIf BtnText = "UnCancel" Then
                    AddColName = "IsAuditApproved=0,AuditApprovedBy=" & GBL_User_ID & ",IsAuditCancelled=0,AuditCancelledBy=0,AuditCancelledDate=GETDATE()"
                End If
            End If

            wherecndtn = "CompanyID=" & GBLCompanyID & " "
            KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, 2, wherecndtn)

        Catch ex As Exception
            KeyField = "fail " & ex.Message
        End Try
        Return KeyField
    End Function

    Public Class HelloWorldData
        Public Message As [String]
    End Class
End Class