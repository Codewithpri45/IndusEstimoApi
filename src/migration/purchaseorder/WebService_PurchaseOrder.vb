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
Public Class WebService_PurchaseOrder
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
    Private Function ConvertDataTableTojSonString(ByVal dataTable As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer With {
            .MaxJsonLength = 2147483647
        }
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

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectAddressGetData() As String
        Dim str As String
        Dim dt As New DataTable
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        str = "select DeliveryAddress, CompanyID from DeliveryAddresses where CompanyID = '" & GBLCompanyID & "'"  'add CompanyID condition by ankit 25-05-2025
        db.FillDataTable(dt, str)
        data.Message = db.ConvertDataTableTojSonString(dt)

        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    Public Function DataSetToJSONWithJavaScriptSerializer(ByVal dataset As DataSet) As String
        Dim jsSerializer As JavaScriptSerializer = New JavaScriptSerializer()
        Dim ssvalue As Dictionary(Of String, Object) = New Dictionary(Of String, Object)()

        For Each table As DataTable In dataset.Tables
            Dim parentRow As List(Of Dictionary(Of String, Object)) = New List(Of Dictionary(Of String, Object))()
            Dim childRow As Dictionary(Of String, Object)
            Dim tablename As String = table.TableName

            For Each row As DataRow In table.Rows
                childRow = New Dictionary(Of String, Object)()

                For Each col As DataColumn In table.Columns
                    childRow.Add(col.ColumnName, row(col))
                Next

                parentRow.Add(childRow)
            Next

            ssvalue.Add(tablename, parentRow)
        Next

        Return jsSerializer.Serialize(ssvalue)
    End Function


    '---------------Open Master code---------------------------------
    '-----------------------------------Get Pending Requisition List Grid------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function FillGrid(ByVal RadioValue As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If RadioValue = "Pending Requisitions" Then
            If DBType = "MYSQL" Then
                str = "Select Distinct ITM.TransactionID,ITD.TransID,ITM.VoucherID,ITD.ItemID, IM.ItemGroupID ,IM.ItemSubGroupID,IGM.ItemGroupNameID,IFNULL(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IM.ItemName,'') AS ItemName,    Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,IFNULL(ITD.QuantityPerPack,0) AS QuantityPerPack,NullIf(ITD.StockUnit,'') AS OrderUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,(Select Distinct JobName From JobBookingJobCard AS JBJ Inner Join JobBookingJobCardContents AS JBC On JBC.JobBookingID=JBJ.JobBookingID And JBJ.CompanyID=JBC.CompanyID Where JBC.JobBookingJobCardContentsID IN(ITD.RefJobBookingJobCardContentsID) AND JBC.CompanyID = ITD.CompanyID ) As JobName,NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,(IFNULL(ITD.RequiredQuantity, 0) - IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantityComp,(IFNULL(ITD.RequiredQuantity, 0) - IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantity,IFNULL(Nullif(IM.PurchaseRate,''),0) as PurchaseRate,  nullif(IM.PurchaseUnit,'') as PurchaseUnit, nullif(PHM.ProductHSNName,'') as ProductHSNName,Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ExpectedDeliveryDate,nullif(PHM.HSNCode,'') as HSNCode, " &
                      " IFNULL(PHM.GSTTaxPercentage,0) as GSTTaxPercentage, IFNULL(PHM.CGSTTaxPercentage,0) as CGSTTaxPercentage, IFNULL(PHM.SGSTTaxPercentage,0) as SGSTTaxPercentage, IFNULL(PHM.IGSTTaxPercentage ,0) as IGSTTaxPercentage  ,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,IFNULL(Nullif(IM.SizeW,''),0) AS SizeW,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,Nullif(C.ConversionFormula,'') AS  ConversionFormula,IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,NullIf(IM.StockUnit,'') AS StockUnit,NullIf(IM.PurchaseUnit,'') AS PurchaseUnit,IFNULL(IM.GSM,0) AS GSM,IFNULL(IM.ReleaseGSM,0) AS ReleaseGSM,IFNULL(IM.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(IM.Thickness,0) AS Thickness,IFNULL(IM.Density,0) AS Density " &
                      " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID  INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID  And IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy And UA.CompanyID=ITM.CompanyID LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID =IM.ProductHSNID And PHM.CompanyID=IM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit And C.CompanyID=IM.CompanyID  LEFT JOIN ConversionMaster As CU ON CU.BaseUnitSymbol=IM.PurchaseUnit AND CU.ConvertedUnitSymbol=IM.StockUnit And CU.CompanyID=IM.CompanyID " &
                      " Where IFNULL(ITM.VoucherID, 0) = -9 And ITM.CompanyID = " & GBLCompanyID & " AND IFNULL(ITM.IsDeletedTransaction,0)=0 AND IFNULL(ITD.IsVoucherItemApproved,0)=1 And (IFNULL(ITD.RequiredQuantity, 0) > IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where IFNULL(IsDeletedTransaction, 0)=0 And RequisitionTransactionID=ITD.TransactionID And ItemID=ITD.ItemID And CompanyID=ITD.CompanyID),0)) Order By FYear Desc,MaxVoucherNo Desc,TransID"
            Else
                str = "Select  Distinct ITM.TransactionID,ITD.TransID,ITM.VoucherID,ITD.ItemID, IM.ItemGroupID ,IM.ItemSubGroupID,IGM.ItemGroupNameID,Isnull(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemCode,'') AS ItemCode,CONCAT(Nullif(IM.ItemName,''), '-',Nullif( IM.ManufecturerItemCode,''))  as ItemName,   Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.RequiredQuantity,0) AS RequiredQuantity,Isnull(ITD.QuantityPerPack,0) AS QuantityPerPack,NullIf(ITD.StockUnit,'') AS OrderUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,(Select Distinct JobName From JobBookingJobCard AS JBJ Inner Join JobBookingJobCardContents AS JBC On JBC.JobBookingID=JBJ.JobBookingID And JBJ.CompanyID=JBC.CompanyID Where JBC.JobBookingJobCardContentsID IN(dbo.GetFirstValueFromCSV(Replace(ITD.RefJobBookingJobCardContentsID,'0.00','0'))) AND JBC.CompanyID = ITD.CompanyID ) As JobName," &
                   "NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,  (Isnull(ITD.RequiredQuantity, 0) - Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantityComp,(Isnull(ITD.RequiredQuantity, 0) - Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantity,Isnull(Nullif(IM.PurchaseRate,''),0) as PurchaseRate,  nullif(IM.PurchaseUnit,'') as PurchaseUnit, nullif(PHM.ProductHSNName,'') as ProductHSNName, " &
                   "replace(convert(nvarchar(30), ITD.ExpectedDeliveryDate, 106),'','-') AS ExpectedDeliveryDate,nullif(PHM.HSNCode,'') as HSNCode, isnull(PHM.GSTTaxPercentage,0) as GSTTaxPercentage, isnull(PHM.CGSTTaxPercentage,0) as CGSTTaxPercentage, isnull(PHM.SGSTTaxPercentage,0) as SGSTTaxPercentage, isnull(PHM.IGSTTaxPercentage ,0) as IGSTTaxPercentage  ,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,Isnull(Nullif(IM.SizeW,''),0) AS SizeW,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,Nullif(C.ConversionFormula,'') AS  ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,Isnull(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,NullIf(IM.StockUnit,'') AS StockUnit,NullIf(IM.PurchaseUnit,'') AS PurchaseUnit,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density,Case When Isnull(ITD.PurchaseTolerance,0)> 0 Then ITD.PurchaseTolerance Else Isnull(CM.PurchaseTolerance,0) End As Tolerance, Isnull(PUM.ProductionUnitID,0) as ProductionUnitID,Isnull(CM.CompanyID,0) as CompanyID,Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,Nullif(CM.CompanyName,'') AS CompanyName  " &
                   "From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD On ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER Join ItemMaster As IM On IM.ItemID=ITD.ItemID  INNER Join ItemGroupMaster As IGM On IGM.ItemGroupID=IM.ItemGroupID  INNER Join UserMaster As UA On UA.UserID=ITM.CreatedBy  INNER Join ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID" &
                   " LEFT JOIN ProductHSNMaster As PHM On PHM.ProductHSNID =IM.ProductHSNID  LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN ConversionMaster As C On C.BaseUnitSymbol=IM.StockUnit And C.ConvertedUnitSymbol=IM.PurchaseUnit   LEFT JOIN ConversionMaster As CU On CU.BaseUnitSymbol=IM.PurchaseUnit And CU.ConvertedUnitSymbol=IM.StockUnit  " &
                   "Where Isnull(ITM.VoucherID, 0) = -9 And ITM.ProductionUnitID In(" & ProductionUnitIDStr & ") And Isnull(ITM.IsDeletedTransaction, 0) = 0 And Isnull(ITD.IsVoucherItemApproved, 0) = 1 And (Isnull(ITD.RequiredQuantity, 0) > Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) " &
                   "Order By FYear Desc,MaxVoucherNo Desc,TransID "
            End If

        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetFiledata(ByVal TransactionID As String) As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        Dim str As String = "SELECT AttachmentFileID, TransactionID, AttachmentFilesName AS AttachedFileName, " &
                    "AttachmentFilesName AS AttachedFileUrl, AttachedFileRemark AS AttachedFileRemark " &
                    "FROM ItemTransactionAttachments " &
                    "WHERE TransactionID = '" & TransactionID & "'"
        db.FillDataTable(dataTable, str)
        data.Message = db.ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Process Requisition List Grid------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ProcessFillGrid(ByVal fromDateValue As String, ByVal ToDateValue As String, ByVal chk As String, ByVal Detail As String, ByVal FilterStr As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"
        Dim dateString As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If Detail = "True" Then
            If chk = "True" Then
                'dateString = " AND  Cast(Floor(Cast(ITM.VoucherDate as Float)) as DateTime) >= ('" & FDate & "') AND Cast(Floor(Cast(ITM.VoucherDate as Float)) as DateTime) <= ('" & ToDate & "') "
            Else
                dateString = ""
            End If
            If DBType = "MYSQL" Then
                str = "Select IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITD.ItemID,0) AS ItemID, IFNULL(IM.ItemGroupID,0) As ItemGroupID,IFNULL(IGM.ItemGroupNameID,0) As ItemGroupNameID,IFNULL(IM.ItemSubGroupID,0) As ItemSubGroupID,NullIf(LM.LedgerName,'') AS LedgerName,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo, Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IFNULL(IM.ItemName,''),'') AS ItemName,NullIf(IFNULL(IM.ItemDescription,''),'') AS ItemDescription, IFNULL(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,IFNULL(ITD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.PurchaseRate,0) AS PurchaseRate,IFNULL(ITD.GrossAmount,0) AS GrossAmount,IFNULL(ITD.DiscountAmount,0) AS DiscountAmount,IFNULL(ITD.BasicAmount,0) AS BasicAmount,IFNULL(ITD.GSTPercentage,0) AS GSTPercentage,(IFNULL(ITD.CGSTAmount,0)+IFNULL(ITD.SGSTAmount,0)+IFNULL(ITD.IGSTAmount,0)) AS GSTTaxAmount,IFNULL(ITD.NetAmount,0) AS NetAmount,NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy,NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,IFNULL((Select TransactionID From ItemTransactionDetail Where PurchaseTransactionID=ITM.TransactionID AND CompanyID=ITD.CompanyID AND IFNULL(IsDeletedTransaction,0)<>1 AND IFNULL(PurchaseTransactionID,0)>0 limit 1),0) AS ReceiptTransactionID,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,(Select ROUND(Sum(IFNULL(RequisitionProcessQuantity,0)),2) From ItemPurchaseRequisitionDetail Where TransactionID=ITD.TransactionID AND ItemID=ITD.ItemID AND CompanyID=ITD.CompanyID) AS RequiredQuantity,Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As ExpectedDeliveryDate,IFNULL(ITM.TotalTaxAmount,0) AS TotalTaxAmount,IFNULL(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount,Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,IFNULL(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,IFNULL(ITD.TaxableAmount,0) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID,IFNULL(ITD.IsvoucherItemApproved,0) AS VoucherItemApproved,IFNULL(ITD.IsCancelled,0) AS VoucherCancelled,IFNULL(NullIf(ITM.CurrencyCode,''),'INR') AS CurrencyCode,IFNULL(ITM.VoucherApprovalByEmployeeID,0) AS VoucherApprovalByEmployeeID,IFNULL(ITD.PurchaseOrderQuantity, 0)-IFNULL((SELECT Case When IGM.ItemGroupNameID=-1 And (Upper(ITD.PurchaseUnit)='KG' OR Upper(ITD.PurchaseUnit)='KGS') And (Upper(ITD.StockUnit)='SHEET' OR Upper(ITD.StockUnit)='SHEETS') Then Round(SUM(ChallanQuantity*ReceiptWtPerPacking),3) Else SUM(ChallanQuantity) End AS Expr1 FROM ItemTransactionDetail WHERE (PurchaseTransactionID = ITM.TransactionID) AND (CompanyID = ITD.CompanyID) AND (IFNULL(IsDeletedTransaction, 0) <> 1) And (ItemID=ITD.ItemID) AND (IFNULL(PurchaseTransactionID, 0) > 0)), 0) AS PendingToReceiveQty " &
                      " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID  INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID  INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID  INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=IM.CompanyID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID  LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID  LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID " &
                      " Where ITM.VoucherID= -11 And ITM.FYear='" & GBLFYear & "' AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' And ITM.CompanyID=" & GBLCompanyID & "  " & FilterStr & " AND IFNULL(ITD.IsDeletedTransaction,0)<>1 Order By FYear,MaxVoucherNo Desc,TransID"
            Else
                str = "Select  NULLIF(UTL.RecordID,'') AS RecordID ,NULLIF(UTL.Details,'') AS Details ,Isnull(ITM.TransactionID, 0) As TransactionID,Isnull(ITM.VoucherID,0) As VoucherID,Isnull(ITM.LedgerID,0) As LedgerID,Isnull(ITD.TransID,0) As TransID,Isnull(ITD.ItemID,0) As ItemID, Isnull(IM.ItemGroupID,0) As ItemGroupID,Isnull(IGM.ItemGroupNameID,0) As ItemGroupNameID,Isnull(IM.ItemSubGroupID,0) As ItemSubGroupID,NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo, Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(Isnull(IM.ItemName,''),'') AS ItemName,NullIf(Isnull(IM.ItemDescription,''),'') AS ItemDescription, Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,Isnull(ITD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.PurchaseRate,0) AS PurchaseRate,Isnull(ITD.GrossAmount,0) AS GrossAmount,Isnull(ITD.DiscountAmount,0) AS DiscountAmount,Isnull(ITD.BasicAmount,0) AS BasicAmount,Isnull(ITD.GSTPercentage,0) AS GSTPercentage," &
                       "(Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,Isnull(ITD.NetAmount,0) AS NetAmount,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,Isnull((Select Top 1 TransactionID From ItemTransactionDetail Where PurchaseTransactionID=ITM.TransactionID AND CompanyID=ITD.CompanyID AND Isnull(IsDeletedTransaction,0)<>1 AND Isnull(PurchaseTransactionID,0)>0),0) AS ReceiptTransactionID,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,(Select ROUND(Sum(Isnull(RequisitionProcessQuantity,0)),2) From ItemPurchaseRequisitionDetail Where TransactionID=ITD.TransactionID AND ItemID=ITD.ItemID AND CompanyID=ITD.CompanyID) AS RequiredQuantity,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate, " &
                       "Isnull(ITM.TotalTaxAmount, 0) As TotalTaxAmount,Isnull(ITM.TotalOverheadAmount,0) As TotalOverheadAmount,Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,Isnull(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,Isnull(ITD.TaxableAmount,0) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID,Isnull(ITD.IsvoucherItemApproved,0) AS VoucherItemApproved,Isnull(ITD.IsCancelled,0) AS VoucherCancelled,Isnull(NullIf(ITM.CurrencyCode,''),'INR') AS CurrencyCode,Isnull(ITM.VoucherApprovalByEmployeeID,0) AS VoucherApprovalByEmployeeID,ISNULL(ITD.PurchaseOrderQuantity, 0)-ISNULL((SELECT Case When IGM.ItemGroupNameID=-1 And (Upper(ITD.PurchaseUnit)='KG' OR Upper(ITD.PurchaseUnit)='KGS') And (Upper(ITD.StockUnit)='SHEET' OR Upper(ITD.StockUnit)='SHEETS') Then Round(SUM(ChallanQuantity*ReceiptWtPerPacking),3) Else SUM(ChallanQuantity) End AS Expr1 FROM ItemTransactionDetail WHERE (PurchaseTransactionID = ITM.TransactionID) AND (CompanyID = ITD.CompanyID) AND (ISNULL(IsDeletedTransaction, 0) <> 1) And (ItemID=ITD.ItemID) AND (ISNULL(PurchaseTransactionID, 0) > 0)), 0) AS PendingToReceiveQty , Isnull(PUM.ProductionUnitID,0) as ProductionUnitID,Isnull(CM.CompanyID,0) as CompanyID,Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,Nullif(CM.CompanyName,'') AS CompanyName ,Isnull(CM.CompanyID,0) as CompanyID    " &
                       "From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID  INNER JOIN UserMaster As UA On UA.UserID=ITM.CreatedBy  INNER JOIN ItemMaster As IM On IM.ItemID=ITD.ItemID  INNER JOIN ItemGroupMaster As IGM On IGM.ItemGroupID=IM.ItemGroupID INNER JOIN LedgerMaster As LM On LM.LedgerID=ITM.LedgerID  INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID LEFT JOIN (SELECT RecordID, Details, CompanyID,CreatedDate  FROM (SELECT *,ROW_NUMBER() OVER (PARTITION BY RecordID ORDER BY CreatedDate  DESC) AS rn FROM UserTransactionLogs WHERE  ActionType ='CanEdit' AND ModuleName ='PurchaseOrder.aspx') AS LatestLogs WHERE rn = 1) AS UTL ON UTL.RecordID = ITD.TransactionID  LEFT JOIN ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN UserMaster As UM On UM.UserID=ITD.VoucherItemApprovedBy " &
                       "Where ITM.VoucherID= -11 /*And ITM.FYear='" & GBLFYear & "'*/   " & FilterStr & " AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND Isnull(ITD.IsDeletedTransaction,0)<>1 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ")  Order By FYear Desc,MaxVoucherNo Desc,TransID "

            End If
        Else
            If chk = "True" Then
                'dateString = " AND  Cast(Floor(Cast(ITM.VoucherDate as Float)) as DateTime) >= ('" & FDate & "') AND Cast(Floor(Cast(ITM.VoucherDate as Float)) as DateTime) <= ('" & ToDate & "') "
            Else
                dateString = ""
            End If

            If DBType = "MYSQL" Then
                str = "Select IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITM.LedgerID,0) AS LedgerID,0 AS TransID,0 AS ItemID, 0 As ItemGroupID,0 As ItemSubGroupID,0 AS ItemGroupNameID,NullIf(LM.LedgerName,'') AS LedgerName,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As VoucherDate,NullIf('','') AS ItemCode,NullIf('','') AS ItemGroupName,NullIf('','') AS ItemSubGroupName,NullIf('','') AS ItemName,NullIf('','') AS ItemDescription, ROUND(SUM(IFNULL(ITD.PurchaseOrderQuantity, 0)), 2) As PurchaseQuantity,Nullif('','') AS PurchaseUnit,0 AS PurchaseRate,ROUND(SUM(IFNULL(ITD.GrossAmount,0)),2) AS GrossAmount, 0 AS DiscountAmount,ROUND(SUM(IFNULL(ITD.BasicAmount,0)),2) AS BasicAmount,0 AS GSTPercentage,ROUND((SUM(IFNULL(ITD.CGSTAmount,0))+SUM(IFNULL(ITD.SGSTAmount,0))+SUM(IFNULL(ITD.IGSTAmount,0))),2) AS GSTTaxAmount,ROUND(SUM(IFNULL(ITD.NetAmount,0)),2) AS NetAmount, NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy,NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,NullIf(ITM.FYear,'') AS FYear,0 AS ReceiptTransactionID,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 As IsReworked, Nullif('','') AS ReworkRemark,Nullif('','') AS RefJobBookingJobCardContentsID,Nullif('','') AS RefJobCardContentNo,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision ,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,0 AS RequiredQuantity,Nullif('','') AS ExpectedDeliveryDate,IFNULL(ITM.TotalTaxAmount,0) AS TotalTaxAmount,IFNULL(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount,Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,IFNULL(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,ROUND(SUM(IFNULL(ITD.TaxableAmount,0)),2) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID,IFNULL(ITD.IsvoucherItemApproved,0) AS VoucherItemApproved,IFNULL(ITD.IsCancelled,0) AS VoucherCancelled,IFNULL(NullIf(ITM.CurrencyCode,''),'INR') AS CurrencyCode,IFNULL(ITM.VoucherApprovalByEmployeeID,0) AS VoucherApprovalByEmployeeID " &
                      " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail AS ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy And UA.CompanyID=ITM.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy And UA.CompanyID=ITM.CompanyID " &
                      " Where ITM.VoucherID = -11 And ITM.FYear='" & GBLFYear & "' And ITM.CompanyID =" & GBLCompanyID & " " & FilterStr & " AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND IFNULL(ITD.IsDeletedTransaction,0)<>1  " &
                      " GROUP BY IFNULL(ITM.TransactionID, 0),IFNULL(ITM.VoucherID,0),IFNULL(ITM.LedgerID,0), NullIf(LM.LedgerName,''),IFNULL(ITM.MaxVoucherNo,0),NullIf(ITM.VoucherNo,''),Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)), NullIf(IFNULL(UA.UserName,''),''),NullIf(IFNULL(UM.UserName,''),''),NullIf(ITM.FYear,''),IFNULL(ITD.IsVoucherItemApproved,0),Nullif(ITM.PurchaseReferenceRemark,''),Nullif(ITM.Narration,''),Nullif(ITM.PurchaseDivision,''),Nullif(ITM.ContactPersonID,''),IFNULL(ITM.TotalTaxAmount,0),IFNULL(ITM.TotalOverheadAmount,0),Nullif(ITM.DeliveryAddress,''),IFNULL(ITM.TotalQuantity,''),nullif(ITM.TermsOfPayment,''),nullif(ITM.ModeOfTransport ,''),nullif(ITM.DealerID,''),IFNULL(ITD.IsCancelled,0),IFNULL(NullIf(ITM.CurrencyCode,''),'INR')  ,IFNULL(ITM.VoucherApprovalByEmployeeID,0)   Order By NullIf(ITM.FYear,''),IFNULL(ITM.MaxVoucherNo,0) Desc"
            Else
                str = "Select  NULLIF(UTL.RecordID,'') AS RecordID ,NULLIF(UTL.Details,'') AS Details ,Isnull(ITM.TransactionID, 0) As TransactionID,Isnull(ITM.VoucherID,0) As VoucherID,Isnull(ITM.LedgerID,0) As LedgerID,0 As TransID,0 As ItemID, 0 As ItemGroupID,0 As ItemSubGroupID,0 As ItemGroupNameID,NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,NullIf('','') AS ItemCode,NullIf('','') AS ItemGroupName,NullIf('','') AS ItemSubGroupName,NullIf('','') AS ItemName,NullIf('','') AS ItemDescription, ROUND(SUM(Isnull(ITD.PurchaseOrderQuantity, 0)), 2) As PurchaseQuantity,Nullif('','') AS PurchaseUnit,0 AS PurchaseRate,ROUND(SUM(Isnull(ITD.GrossAmount,0)),2) AS GrossAmount, 0 AS DiscountAmount,ROUND(SUM(Isnull(ITD.BasicAmount,0)),2) AS BasicAmount,0 AS GSTPercentage,ROUND((SUM(Isnull(ITD.CGSTAmount,0))+SUM(Isnull(ITD.SGSTAmount,0))+SUM(Isnull(ITD.IGSTAmount,0))),2) AS GSTTaxAmount, " &
                      "Isnull(TaxesAmt, 0) + ROUND(SUM(ISNULL(ITD.NetAmount, 0)), 2) As NetAmount,  NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITM.FYear,'') AS FYear,0 AS ReceiptTransactionID,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 As IsReworked, Nullif('','') AS ReworkRemark,Nullif('','') AS RefJobBookingJobCardContentsID,Nullif('','') AS RefJobCardContentNo,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision ,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,0 AS RequiredQuantity,Nullif('','') AS ExpectedDeliveryDate,isnull(ITM.TotalTaxAmount,0) AS TotalTaxAmount,isnull(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount,Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,Isnull(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,ROUND(SUM(Isnull(ITD.TaxableAmount,0)),2) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID,Isnull(ITD.IsvoucherItemApproved,0) AS VoucherItemApproved,Isnull(ITD.IsCancelled,0) AS VoucherCancelled,ISNULL(ITD.CancelRemark,'') as CancelRemark,Isnull(NullIf(ITM.CurrencyCode,''),'INR') AS CurrencyCode,Isnull(ITM.VoucherApprovalByEmployeeID,0) AS VoucherApprovalByEmployeeID , Isnull(PUM.ProductionUnitID,0) as ProductionUnitID,Nullif(PUM.ProductionUnitName,'') AS ProductionUnitName,Nullif(CM.CompanyName,'') AS CompanyName,Isnull(CM.CompanyID,0) as CompanyID " &
                      "From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD On ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN UserMaster As UA On UA.UserID=ITM.CreatedBy  INNER JOIN LedgerMaster As LM On LM.LedgerID=ITM.LedgerID  INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = ITM.ProductionUnitID Inner Join CompanyMaster As CM On CM.CompanyID = PUM.CompanyID LEFT JOIN (SELECT RecordID, Details, CompanyID,CreatedDate FROM (SELECT *,ROW_NUMBER() OVER (PARTITION BY RecordID ORDER BY CreatedDate DESC) AS rn FROM UserTransactionLogs WHERE  ActionType ='CanEdit' AND ModuleName ='PurchaseOrder.aspx') AS LatestLogs WHERE rn = 1) AS UTL ON UTL.RecordID = ITD.TransactionID AND UTL.CompanyID = ITM.CompanyID LEFT JOIN (Select IT.TransactionID,Round(Sum(Isnull(IPT.Amount,0)),2) As TaxesAmt,IT.CompanyID from ItemTransactionMain As IT  Inner Join ItemPurchaseOrderTaxes As IPT On IT.TransactionID = IPT.TransactionID And IT.CompanyID = IPT.CompanyID And ISNULL(IPT.IsDeletedTransaction,0)=0 INNER JOIN LedgerMaster As LM On LM.LedgerID = IPT.LedgerID And LM.LedgerName Not Like '%GST%' Where ISNULL(IT.IsDeletedTransaction,0)=0 Group by IT.TransactionID,IPT.Amount,IT.CompanyID) As Tax On Tax.TransactionID = ITM.TransactionID And Tax.CompanyID = ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy " &
                      "Where ITM.VoucherID = -11 /*And ITM.FYear='" & GBLFYear & "'*/ " & FilterStr & " AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND Isnull(ITD.IsDeletedTransaction,0)<>1 And ITM.ProductionUnitID IN(" & ProductionUnitIDStr & ") GROUP BY TaxesAmt,Isnull(ITM.TransactionID, 0),Isnull(ITM.VoucherID,0),Isnull(ITM.LedgerID,0), NullIf(LM.LedgerName,''),Isnull(ITM.MaxVoucherNo,0),NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'), NullIf(Isnull(UA.UserName,''),''),NullIf(Isnull(UM.UserName,''),''),NullIf(ITM.FYear,''),Isnull(ITD.IsVoucherItemApproved,0),Nullif(ITM.PurchaseReferenceRemark,''),Nullif(ITM.Narration,''),Nullif(ITM.PurchaseDivision,''),Nullif(ITM.ContactPersonID,''),Isnull(ITM.TotalTaxAmount,0),Isnull(ITM.TotalOverheadAmount,0),Nullif(ITM.DeliveryAddress,''),Isnull(ITM.TotalQuantity,''),nullif(ITM.TermsOfPayment,''),nullif(ITM.ModeOfTransport ,''),nullif(ITM.DealerID,''),Isnull(ITD.IsCancelled,0),ISNULL(ITD.CancelRemark,''),Isnull(NullIf(ITM.CurrencyCode,''),'INR')  ,isnull(ITM.VoucherApprovalByEmployeeID,0),Isnull(PUM.ProductionUnitID,0) , NULLIF(UTL.RecordID,''),NULLIF(UTL.Details,'') ,Nullif(PUM.ProductionUnitName,'') ,Nullif(CM.CompanyName,'') ,Isnull(CM.CompanyID,0)     Order By NullIf(ITM.FYear,'') Desc,Isnull(ITM.MaxVoucherNo,0) Desc "
            End If

        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Process Retrive List Grid------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetrivePoCreateGrid(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select IFNULL(ITM.TransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.VoucherID,0) AS PurchaseVoucherID,IFNULL(IPR.RequisitionTransactionID,0) AS TransactionID,IFNULL(IR.VoucherID,0) AS VoucherID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITD.ItemID,0) AS ItemID,  IFNULL(ITD.ItemGroupID,0) As ItemGroupID, NullIf(LM.LedgerName,'') AS LedgerName,IFNULL(ITM.MaxVoucherNo,0) AS PurchaseMaxVoucherNo,IFNULL(IR.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS PurchaseVoucherNo,NullIf(IR.VoucherNo,'') AS VoucherNo, Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,Convert(date_format(IfNULL(IR.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode, NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,(Select Distinct JobName From JobBookingJobCard AS JBJ Inner Join JobBookingJobCardContents AS JBC On JBC.JobBookingID=JBJ.JobBookingID And JBJ.CompanyID=JBC.CompanyID Where JBC.JobBookingJobCardContentsID IN(ITD.RefJobBookingJobCardContentsID) AND JBC.CompanyID = ITD.CompanyID ) As JobName,NullIf(IRD.ItemNarration,'') AS ItemNarration,NullIf(IFNULL(IM.ItemName,''),'') AS ItemName,NullIf(IFNULL(IM.ItemDescription,''),'') AS ItemDescription, IFNULL(IPR.RequisitionProcessQuantity,0) AS RequiredQuantity, IFNULL(IRD.RequiredQuantity,0) AS RequisitionQty,IFNULL(IRD.StockUnit,0) AS StockUnit, IFNULL(ITD.RequiredNoOfPacks,0) AS RequiredNoOfPacks,IFNULL(ITD.QuantityPerPack,0) AS QuantityPerPack,IFNULL(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,  IFNULL(ITD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.PurchaseRate,0) AS PurchaseRate, IFNULL(ITD.GrossAmount,0) AS BasicAmount,IFNULL(ITD.DiscountPercentage,0) AS Disc,IFNULL(ITD.DiscountAmount,0) AS DiscountAmount,IFNULL(ITD.BasicAmount,0) AS AfterDisAmt,IFNULL(ITD.PurchaseTolerance,0) AS Tolerance, IFNULL(ITD.GSTPercentage,0) AS GSTTaxPercentage,(IFNULL(ITD.CGSTAmount,0)+IFNULL(ITD.SGSTAmount,0)+IFNULL(ITD.IGSTAmount,0)) AS GSTTaxAmount,IFNULL(ITD.NetAmount,0) AS TotalAmount,NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy, NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,0 AS ReceiptTransactionID,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark, Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision, IFNULL(ITD.RequiredQuantity,0) /* (Select ROUND(Sum(IFNULL(RequisitionProcessQuantity,0)),2) From ItemPurchaseRequisitionDetail Where TransactionID=ITD.TransactionID And ItemID=ITD.ItemID And CompanyID=ITD.CompanyID) */ AS TotalRequiredQuantity, Nullif(IM.StockUnit,'') AS PurchaseStockUnit,IFNULL(ITD.CGSTPercentage,0) as CGSTTaxPercentage,IFNULL(ITD.SGSTPercentage,0) as SGSTTaxPercentage,IFNULL(ITD.IGSTPercentage,0) as IGSTTaxPercentage , IFNULL(ITD.CGSTAmount,0) as CGSTAmt,IFNULL(ITD.SGSTAmount,0) as SGSTAmt,IFNULL(ITD.IGSTAmount,0) as IGSTAmt ,IFNULL(ITD.TaxableAmount,0) AS TaxableAmount,Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ExpectedDeliveryDate,Nullif(PHM.ProductHSNName,'') AS ProductHSNName,Nullif(PHM.HSNCode,'') AS HSNCode," &
                  " IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,IFNULL(IM.SizeW,0) AS SizeW,Nullif(C.ConversionFormula,'') AS  ConversionFormula,IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Nullif(ITD.RefJobCardContentNo,'') AS  PORefJobCardContentNo,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS  PORefJobBookingJobCardContentsID,Nullif(IRD.RefJobCardContentNo,'') AS  RefJobCardContentNo,Nullif(IRD.RefJobBookingJobCardContentsID,'') AS  RefJobBookingJobCardContentsID,IFNULL(PHM.ProductHSNID,0) as ProductHSNID,IFNULL(IM.GSM,0) AS GSM,IFNULL(IM.ReleaseGSM,0) AS ReleaseGSM,IFNULL(IM.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(IM.Thickness,0) AS Thickness,IFNULL(IM.Density,0) AS Density " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=ITD.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID  LEFT JOIN ItemPurchaseRequisitionDetail AS IPR ON IPR.TransactionID=ITD.TransactionID And IPR.ItemID=ITD.ItemID And IPR.CompanyID=ITD.CompanyID LEFT JOIN ItemTransactionMain AS IR ON IR.TransactionID=IPR.RequisitionTransactionID And IR.CompanyID=IPR.CompanyID LEFT JOIN ItemTransactionDetail AS IRD ON IRD.TransactionID=IPR.RequisitionTransactionID And IRD.ItemID=IPR.ItemID And IRD.CompanyID=IPR.CompanyID LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit And C.CompanyID=ITD.CompanyID  LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID=ITD.ProductHSNID AND PHM.CompanyID=ITD.CompanyID  LEFT JOIN ConversionMaster As CU ON CU.BaseUnitSymbol=IM.PurchaseUnit AND CU.ConvertedUnitSymbol=IM.StockUnit And CU.CompanyID=IM.CompanyID " &
                  " Where ITM.VoucherID= -11 And ITM.CompanyID = " & GBLCompanyID & " And ITD.TransactionID=" & transactionID & "  And IFNULL(ITD.IsDeletedTransaction,0)<>1 Order By TransID"
        Else
            str = "Select  Isnull(ITM.TransactionID, 0) As PurchaseTransactionID,ITD.ClientID,NulliF(LM1.LedgerName,'') as ClientName,Isnull(ITM.VoucherID,0) AS PurchaseVoucherID,Isnull(IPR.RequisitionTransactionID,0) AS TransactionID,Isnull(IR.VoucherID,0) AS VoucherID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITD.ItemID,0) AS ItemID,  Isnull(ITD.ItemGroupID,0) As ItemGroupID, NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS PurchaseMaxVoucherNo,Isnull(IR.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS PurchaseVoucherNo,NullIf(IR.VoucherNo,'') AS VoucherNo, Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS PurchaseVoucherDate,Replace(Convert(Varchar(13),IR.VoucherDate,106),' ','-') AS VoucherDate, NullIf(IM.ItemCode,'') AS ItemCode, NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,(Select Distinct JobName From JobBookingJobCard AS JBJ Inner Join JobBookingJobCardContents AS JBC On JBC.JobBookingID=JBJ.JobBookingID And JBJ.CompanyID=JBC.CompanyID Where JBC.JobBookingJobCardContentsID IN(replace(ITD.RefJobBookingJobCardContentsID,'.00','')) AND JBC.CompanyID = ITD.CompanyID ) As JobName,NullIf(IRD.ItemNarration,'') AS ItemNarration, " &
                  "NullIf(Isnull(IM.ItemName,''),'') AS ItemName,NullIf(Isnull(IM.ItemDescription,''),'') AS ItemDescription, Isnull(IPR.RequisitionProcessQuantity,0) AS RequiredQuantity, Isnull(IRD.RequiredQuantity,0) AS RequisitionQty,Isnull(IRD.StockUnit,0) AS StockUnit, Isnull(ITD.RequiredNoOfPacks,0) AS RequiredNoOfPacks,Isnull(ITD.QuantityPerPack,0) AS QuantityPerPack,Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,  Isnull(ITD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.PurchaseRate,0) AS PurchaseRate, Isnull(ITD.GrossAmount,0) AS BasicAmount,Isnull(ITD.DiscountPercentage,0) AS Disc,Isnull(ITD.DiscountAmount,0) AS DiscountAmount,Isnull(ITD.BasicAmount,0) AS AfterDisAmt,Isnull(ITD.PurchaseTolerance,0) AS Tolerance, Isnull(ITD.GSTPercentage,0) AS GSTTaxPercentage,(Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,Isnull(ITD.NetAmount,0) AS TotalAmount,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy, NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,0 AS ReceiptTransactionID, " &
                  "Isnull(ITD.IsVoucherItemApproved, 0) As IsVoucherItemApproved, 0 As IsReworked, Nullif('','') AS ReworkRemark, Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,  Isnull(ITD.RequiredQuantity,0) /* (Select ROUND(Sum(Isnull(RequisitionProcessQuantity,0)),2) From ItemPurchaseRequisitionDetail Where TransactionID=ITD.TransactionID And ItemID=ITD.ItemID And CompanyID=ITD.CompanyID) */ AS TotalRequiredQuantity, Nullif(IM.StockUnit,'') AS PurchaseStockUnit,Isnull(ITD.CGSTPercentage,0) as CGSTTaxPercentage,Isnull(ITD.SGSTPercentage,0) as SGSTTaxPercentage,Isnull(ITD.IGSTPercentage,0) as IGSTTaxPercentage , Isnull(ITD.CGSTAmount,0) as CGSTAmt, " &
                  "Isnull(ITD.SGSTAmount, 0) As SGSTAmt, Isnull(ITD.IGSTAmount, 0) As IGSTAmt , Isnull(ITD.TaxableAmount, 0) As TaxableAmount, Replace(Convert(Varchar(13), ITD.ExpectedDeliveryDate, 106),' ','-') AS ExpectedDeliveryDate,Nullif(PHM.ProductHSNName,'') AS ProductHSNName,Nullif(PHM.HSNCode,'') AS HSNCode,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,Isnull(IM.SizeW,0) AS SizeW,Nullif(C.ConversionFormula,'') AS  ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,Isnull(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Nullif(ITD.RefJobCardContentNo,'') AS  PORefJobCardContentNo,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS  PORefJobBookingJobCardContentsID,Nullif(IRD.RefJobCardContentNo,'') AS  RefJobCardContentNo,Nullif(IRD.RefJobBookingJobCardContentsID,'') AS  RefJobBookingJobCardContentsID,Isnull(PHM.ProductHSNID,0) as ProductHSNID,Isnull(IM.GSM,0) AS GSM,Isnull(IM.ReleaseGSM,0) AS ReleaseGSM,Isnull(IM.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(IM.Thickness,0) AS Thickness,Isnull(IM.Density,0) AS Density, NullIf(ITD.Remark,'') AS Remark  " &
                  "From ItemTransactionMain As ITM INNER Join ItemTransactionDetail As ITD On ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID INNER Join ItemMaster As IM On IM.ItemID=ITD.ItemID  INNER Join ItemGroupMaster As IGM On IGM.ItemGroupID=IM.ItemGroupID  INNER Join UserMaster As UA On UA.UserID=ITM.CreatedBy  INNER Join LedgerMaster As LM On LM.LedgerID=ITM.LedgerID  LEFT Join UserMaster As UM On UM.UserID=ITD.VoucherItemApprovedBy  LEFT Join ItemSubGroupMaster As ISGM On ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT Join ItemPurchaseRequisitionDetail As IPR On IPR.TransactionID=ITD.TransactionID And IPR.ItemID=ITD.ItemID " &
                  "LEFT JOIN ItemTransactionMain As IR On IR.TransactionID=IPR.RequisitionTransactionID And IR.CompanyID=IPR.CompanyID LEFT JOIN ItemTransactionDetail As IRD On IRD.TransactionID=IPR.RequisitionTransactionID And IRD.ItemID=IPR.ItemID And IRD.CompanyID=IPR.CompanyID LEFT JOIN ConversionMaster As C On C.BaseUnitSymbol=IM.StockUnit And C.ConvertedUnitSymbol=IM.PurchaseUnit   LEFT JOIN ProductHSNMaster As PHM On PHM.ProductHSNID=ITD.ProductHSNID  LEFT JOIN ConversionMaster As CU On CU.BaseUnitSymbol=IM.PurchaseUnit And CU.ConvertedUnitSymbol=IM.StockUnit Left Join LedgerMaster As LM1 On LM1.LedgerID = ITD.ClientID " &
                  "Where ITM.VoucherID= -11  And ITD.TransactionID=" & transactionID & "  And Isnull(ITD.IsDeletedTransaction,0)<>1 Order By TransID "
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Process Retrive Schedule Grid------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetrivePoSchedule(ByVal transactionID As String, ByVal ItemID As String) As Object
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select IFNULL(IPDS.TransID,0) AS id,IFNULL(IPDS.TransactionID,0) AS TransactionID,IFNULL(IPDS.ItemID,0) AS ItemID,Nullif(IM.ItemCode,'') AS ItemCode,nullif(IPDS.Unit,'') AS PurchaseUnit,IFNULL(IPDS.Quantity, 0) As Quantity,Convert(date_format(IfNULL(IPDS.ScheduleDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As SchDate " &
                  " From ItemPurchaseDeliverySchedule As IPDS INNER JOIN ItemMaster As IM ON IM.ItemID=IPDS.ItemID And IM.CompanyID=IPDS.CompanyID " &
                  " Where IPDS.CompanyID = '" & GBLCompanyID & "' and IPDS.TransactionID='" & transactionID & "'  And IFNULL(IPDS.IsDeletedTransaction,0)<>1"
        Else
            str = " select Isnull(IPDS.TransID,0) AS id,Isnull(IPDS.TransactionID,0) AS TransactionID,Isnull(IPDS.ItemID,0) AS ItemID,Nullif(IM.ItemCode,'') AS ItemCode,nullif(IPDS.Unit,'') AS PurchaseUnit, " &
                "Isnull(IPDS.Quantity, 0) As Quantity,Replace(Convert(Varchar(13),IPDS.ScheduleDeliveryDate,106),' ','-') AS SchDate " &
                "From ItemPurchaseDeliverySchedule As IPDS INNER JOIN ItemMaster As IM ON IM.ItemID=IPDS.ItemID Where IPDS.TransactionID='" & transactionID & "'  AND IM.ItemID = '" & ItemID & "'  AND Isnull(IPDS.IsDeletedTransaction,0)<>1"
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Process Retrive PoOverHead Grid------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetrivePoOverHead(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select IFNULL(IPOHC.TransID,0) AS TransID,IFNULL(IPOHC.TransactionID,0) AS TransactionID,IFNULL(IPOHC.headID,0) AS HeadID,IFNULL(IPOHC.Quantity,0) As Weight,nullif(IPOHC.ChargesType,'') AS RateType,IFNULL(IPOHC.Amount,0) AS HeadAmount,IFNULL(IPOHC.Rate,0) AS Rate,nullif(IPOHC.headName,'') AS Head " &
                  " From ItemPurchaseOverheadCharges as IPOHC where IPOHC.CompanyID='" & GBLCompanyID & "'  and IPOHC.TransactionID='" & transactionID & "'  AND IFNULL(IPOHC.IsDeletedTransaction,0)<>1"
        Else
            str = " select Isnull(IPOHC.TransID,0) AS TransID,Isnull(IPOHC.TransactionID,0) AS TransactionID,Isnull(IPOHC.headID,0) AS HeadID, " &
              "  Isnull(IPOHC.Quantity,0) As Weight,nullif(IPOHC.ChargesType,'') AS RateType,Isnull(IPOHC.Amount,0) AS HeadAmount,Isnull(IPOHC.Rate,0) AS Rate,nullif(IPOHC.headName,'') AS Head " &
               " from ItemPurchaseOverheadCharges as IPOHC where  IPOHC.TransactionID='" & transactionID & "'  AND Isnull(IPOHC.IsDeletedTransaction,0)<>1 "
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '-----------------------------------Get Process Retrive RequisitionDetail Grid------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetriveRequisitionDetail(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ITRM.TransactionID,0) AS TransactionID,IFNULL(ITRD.TransID,0) AS TransID,IFNULL(ITRM.VoucherID,0) AS VoucherID,IFNULL(ITD.ItemGroupID,0) AS ItemGroupID,IFNULL(ITD.ItemID,0) AS ItemID,IFNULL(ITRM.MaxVoucherNo,0) AS MaxVoucherNo,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,IFNULL(ITRM.VoucherNo,0) As VoucherNo,Convert(date_format(IfNULL(ITRM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,IFNULL(IPRD.RequisitionProcessQuantity,0) AS RequiredQuantity,IFNULL(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,Nullif(IPRD.StockUnit,'') AS StockUnit,Nullif(UM.UserName,'') AS CreatedBy,Nullif('','') AS ItemNarration,Nullif(ITD.PurchaseUnit,'') AS PurchaseUnit,0 AS GSTTaxPercentage,0 AS CGSTTaxPercentage,0 AS SGSTTaxPercentage,0 AS IGSTTaxPercentage,NullIf('','') AS Narration,Nullif(ITRD.FYear,'') AS FYear,IFNULL(ITD.PurchaseRate,0) AS PurchaseRate,Nullif('','') AS ProductHSNName,Nullif('','') AS HSNCode " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID LEFT JOIN ItemPurchaseRequisitionDetail AS IPRD ON IPRD.TransactionID=ITM.TransactionID AND IPRD.ItemID=ITD.ItemID AND IPRD.CompanyID=ITD.CompanyID LEFT JOIN ItemTransactionMain AS ITRM ON ITRM.TransactionID=IPRD.RequisitionTransactionID And ITRM.CompanyID=IPRD.CompanyID LEFT JOIN ItemTransactionDetail AS ITRD ON ITRD.TransactionID=IPRD.RequisitionTransactionID AND ITRD.ItemID=IPRD.ItemID AND ITRD.CompanyID=IPRD.CompanyID LEFT JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=ITD.ItemGroupID And IGM.CompanyID=ITD.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITRD.CreatedBy AND UM.CompanyID=ITRD.CompanyID " &
                  " Where ITM.VoucherID=-11 And ITM.TransactionID='" & transactionID & "' And ITM.CompanyID='" & GBLCompanyID & "'"
        Else
            str = "Select Distinct Isnull(ITRM.TransactionID,0) AS TransactionID,Isnull(ITRD.TransID,0) AS TransID,Isnull(ITRM.VoucherID,0) AS VoucherID,Isnull(ITD.ItemGroupID,0) AS ItemGroupID,Isnull(ITD.ItemID,0) AS ItemID,Isnull(ITRM.MaxVoucherNo,0) AS MaxVoucherNo,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,  " &
                "Isnull(ITRM.VoucherNo,0) As VoucherNo,Replace(Convert(Varchar(13),ITRM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(IM.ItemCode,'') AS ItemCode,  " &
                "Nullif(IM.ItemName,'') AS ItemName,Nullif(IM.ItemDescription,'') AS ItemDescription,Isnull(IPRD.RequisitionProcessQuantity,0) AS RequiredQuantity,  " &
                "Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,Nullif(IPRD.StockUnit,'') AS StockUnit,Nullif(UM.UserName,'') AS CreatedBy,Nullif('','') AS ItemNarration,Nullif(ITD.PurchaseUnit,'') AS PurchaseUnit,0 AS GSTTaxPercentage,0 AS CGSTTaxPercentage,0 AS SGSTTaxPercentage,  " &
                "0 AS IGSTTaxPercentage,NullIf('','') AS Narration,Nullif(ITRD.FYear,'') AS FYear,Isnull(ITD.PurchaseRate,0) AS PurchaseRate,Nullif('','') AS ProductHSNName,Nullif('','') AS HSNCode   " &
                "From ItemTransactionMain AS ITM  " &
                "INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID AND ITM.CompanyID=ITD.CompanyID  " &
                "INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID  " &
                "LEFT JOIN ItemPurchaseRequisitionDetail AS IPRD ON IPRD.TransactionID=ITM.TransactionID AND IPRD.ItemID=ITD.ItemID AND IPRD.CompanyID=ITD.CompanyID  " &
                "LEFT JOIN ItemTransactionMain AS ITRM ON ITRM.TransactionID=IPRD.RequisitionTransactionID And ITRM.CompanyID=IPRD.CompanyID  " &
                "LEFT JOIN ItemTransactionDetail AS ITRD ON ITRD.TransactionID=IPRD.RequisitionTransactionID AND ITRD.ItemID=IPRD.ItemID AND ITRD.CompanyID=IPRD.CompanyID  " &
                "LEFT JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=ITD.ItemGroupID And IGM.CompanyID=ITD.CompanyID  " &
                "LEFT JOIN UserMaster AS UM ON UM.UserID=ITRD.CreatedBy AND UM.CompanyID=ITRD.CompanyID  " &
                "Where ITM.VoucherID=-11 AND ITM.TransactionID='" & transactionID & "' AND ITM.CompanyID='" & GBLCompanyID & "'"
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    '-----------------------------------Get Process Retrive PoCreateTaxChares Grid------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetrivePoCreateTaxChares(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        ''New query 12-11-20
        If DBType = "MYSQL" Then
            str = "SELECT LM.LedgerID, IPOT.TransID, IPOT.TransactionID, IFNULL(LM.TaxRatePer, 0) AS TaxRatePer, IFNULL(IPOT.Amount, 0) AS ChargesAmount, IFNULL(LM.InAmount, 0) AS InAmount, IFNULL(LM.IsCumulative, 0) AS IsCumulative, IFNULL(IPOT.GSTApplicable, 0) AS GSTApplicable,IPOT.CalculatedON As CalculateON, LM.LedgerName, NULLIF (LM.TaxType, '') AS TaxType, NULLIF (LM.GSTLedgerType, '') AS GSTLedgerType " &
                  " FROM ItemPurchaseOrderTaxes AS IPOT INNER JOIN LedgerMaster AS LM ON LM.LedgerID = IPOT.LedgerID AND LM.CompanyID = IPOT.CompanyID WHERE (IPOT.CompanyID = " & GBLCompanyID & ") AND (IPOT.TransactionID = " & transactionID & " ) AND (IFNULL(IPOT.IsDeletedTransaction, 0) = 0) ORDER BY IPOT.TransID"
        Else
            str = "SELECT LM.LedgerID, IPOT.TransID, IPOT.TransactionID, ISNULL(LM.TaxRatePer, 0) AS TaxRatePer, ISNULL(IPOT.Amount, 0) AS ChargesAmount, ISNULL(LM.InAmount, 0) AS InAmount, ISNULL(LM.IsCumulative, 0) AS IsCumulative, ISNULL(IPOT.GSTApplicable, 0) AS GSTApplicable,IPOT.CalculatedON As CalculateON, LM.LedgerName, NULLIF (LM.TaxType, '') AS TaxType, NULLIF (LM.GSTLedgerType, '') AS GSTLedgerType " &
            "FROM ItemPurchaseOrderTaxes AS IPOT INNER JOIN LedgerMaster AS LM ON LM.LedgerID = IPOT.LedgerID  WHERE   (IPOT.TransactionID = " & transactionID & " ) AND (ISNULL(IPOT.IsDeletedTransaction, 0) = 0) ORDER BY IPOT.TransID"
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    ''----------------------------Open ProcessPurchaseOrder Delete  Save Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function DeletePaperPurchaseOrder(ByVal TxtPOID As String, ByVal ObjvalidateLoginUser As Object) As String

        Dim KeyField As String
        Dim dtExist As New DataTable
        Dim DataTable1 As New DataTable

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
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

        dtExist = New DataTable()
        If DBType = "MYSQL" Then
            str = "Select TransactionID From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And IFNULL(QCApprovalNo,'')<>'' AND TransactionID=" & TxtPOID & " And IFNULL(IsDeletedTransaction,0)=0 AND (IFNULL(ApprovedQuantity,0)>0 OR IFNULL(RejectedQuantity,0)>0)"
        Else
            str = "Select TransactionID From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And isnull(QCApprovalNo,'')<>'' AND TransactionID=" & TxtPOID & " And Isnull(IsDeletedTransaction,0)=0 AND (Isnull(ApprovedQuantity,0)>0 OR Isnull(RejectedQuantity,0)>0)"
        End If

        db.FillDataTable(dtExist, str)
        If dtExist.Rows.Count > 0 Then
            Return "This transaction is used in another process..! Record can not be delete..."
        End If

        Dim dtExist1 As New DataTable
        Dim TransactionCheckdtExist As New DataTable
        str = "Select TransactionID  from ItemTransactionDetail where PurchaseTransactionID = '" & TxtPOID & "' And ProductionUnitID = '" & ProductionUnitID & "' And ISNULL(IsDeletedTransaction,0)=0 And ISNULL(IsBlocked,0)=0 And ISNULL(IsLocked,0)=0 "
        db.FillDataTable(TransactionCheckdtExist, str)
        If (TransactionCheckdtExist.Rows.Count > 0) Then
            KeyField = "TransactionUsed"
            Return KeyField
        Else
            str = "Select UFS.UserFormsApprovalTransactionID,UFS.ModuleID,MM.ModuleDisplayName + ' (' + MM.ModuleHeadName + ')' As ModuleName,UFS.ApprovalBy,UFS.ActionType,UFS.CompanyID,UFS.ModifyDate,UFS.ApprovalType from UserFormsApprovalSetting As UFS Inner Join ModuleMaster As MM On MM.ModuleID = UFS.ModuleID And MM.CompanyID = UFS.CompanyID And ISNULL(MM.IsLocked,0)=0 and ISNULL(MM.IsDeletedTransaction,0)=0 Where UFS.ApprovalBy = '" & GBLUserID & "' And UFS.ModuleID = (Select top(1) ModuleID from ModuleMaster Where ModuleID<>0 And ModuleName ='PurchaseOrder.aspx' And ProductionUnitID = '" & ProductionUnitID & "' And ISNULL(IsDeletedTransaction,0)=0 And ISNULL(IsLocked,0)=0)"
            db.FillDataTable(dtExist1, str)
            If (dtExist1.Rows.Count > 0) Then
                Dim StrIsApproved As String = ""
                Dim dtExistIsApproved As New DataTable
                StrIsApproved = " Select top(1) IsApproved from UserApprovalTransactionsDetail As UAT Where RecordTransactionDetailID In(Select TransactionDetailID from ItemTransactionDetail Where TransactionID = '" & TxtPOID & "' And ISNULL(IsvoucherItemApproved,0)<>0 And ISNULL(IsDeletedTransaction,0)=0) "
                db.FillDataTable(dtExistIsApproved, StrIsApproved)
                If (dtExistIsApproved.Rows.Count > 0) Then
                    'KeyField = "Sorry Your Requisition is Approved. Please go and Unapprove this Requisition first and then Delete it."
                    KeyField = "PurchaseOrderApproved"
                    Return KeyField
                Else
                    KeyField = db.ExecuteNonSQLQuery("Delete From UserApprovalTransactionsDetail Where RecordTransactionID = '" & TxtPOID & "' And ProductionUnitID = '" & ProductionUnitID & "'")
                    If KeyField <> "Success" Then
                        Return KeyField
                    End If
                End If
            End If
        End If

        If db.CheckAuthories("PurchaseOrder.aspx", GBLUserID, GBLCompanyID, "CanDelete", TxtPOID, ObjvalidateLoginUser("transactionRemark")) = False Then Return "You are not authorized to delete..!"

        'Dim IsPOApprovalRequired As Boolean
        'str = "Select IsPOApprovalRequired from CompanyMaster where CompanyID = '" & GBLCompanyID & "'"
        'Dim approvalQuery As String = "SELECT ISNULL(IsPOApprovalRequired,0) As IsPOApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
        'db.FillDataTable(DataTable1, approvalQuery)

        'IsPOApprovalRequired = DataTable1.Rows(0)("IsPOApprovalRequired").ToString()

        'If DBType = "MYSQL" Then
        '    str = "Select TransactionID from ItemTransactionDetail where CompanyID='" & GBLCompanyID & "' and  TransactionID='" & TxtPOID & "' and IFNULL(IsvoucherItemApproved,0)=1 And IFNULL(IsDeletedTransaction,0)<>1"
        'Else
        '    If IsPOApprovalRequired = True Then
        '        str = "Select TransactionID from ItemTransactionDetail where CompanyID='" & GBLCompanyID & "' and  TransactionID='" & TxtPOID & "' and Isnull(IsvoucherItemApproved,0)=1 And isnull(IsDeletedTransaction,0)<>1"
        '    Else
        '        str = ""
        '    End If

        'End If

        'db.FillDataTable(dtExist, str)
        'If dtExist.Rows.Count > 0 Then
        '    Return "This transaction is used in another process..! Record can not be delete..."
        'End If

        'dtExist = New DataTable()

        'If DBType = "MYSQL" Then
        '    str = "Select TransactionID From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And IFNULL(QCApprovalNo,'')<>'' AND TransactionID=" & TxtPOID & " And IFNULL(IsDeletedTransaction,0)=0 AND (IFNULL(ApprovedQuantity,0)>0 OR IFNULL(RejectedQuantity,0)>0)"
        'Else
        '    str = "Select TransactionID From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And isnull(QCApprovalNo,'')<>'' AND TransactionID=" & TxtPOID & " And Isnull(IsDeletedTransaction,0)=0 AND (Isnull(ApprovedQuantity,0)>0 OR Isnull(RejectedQuantity,0)>0)"
        'End If

        'db.FillDataTable(dtExist, str)
        'If dtExist.Rows.Count > 0 Then
        '    Return "This transaction is used in another process..! Record can not be delete..."
        'End If

        Try
            Using updtTran As New Transactions.TransactionScope
                If DBType = "MYSQL" Then
                    str = "Update ItemTransactionMain Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Main:- " & KeyField
                    End If

                    str = "Update ItemTransactionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Detail:- " & KeyField
                    End If

                    str = "Update ItemPurchaseOverheadCharges Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseDeliverySchedule Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseOrderTaxes Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseRequisitionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=Now(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Detail:- " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES_UNIT_WISE( " & GBLCompanyID & "," & TxtPOID & ",0);")
                Else
                    str = "Update ItemTransactionMain Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Main:- " & KeyField
                    End If

                    str = "Update ItemTransactionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error:Detail:- " & KeyField
                    End If

                    str = "Update ItemPurchaseOverheadCharges Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseDeliverySchedule Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseOrderTaxes Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    str = "Update ItemPurchaseRequisitionDetail Set DeletedBy='" & GBLUserID & "',DeletedDate=GetDate(),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    KeyField = db.ExecuteNonSQLQuery(str)
                    If KeyField <> "Success" Then
                        updtTran.Dispose()
                        Return "Error: " & KeyField
                    End If

                    'str = "Update ItemTransactionAttachments Set DeletedBy='" & GBLUserID & "',DeletedDate = CAST(GETDATE() AS DATE),IsDeletedTransaction=1  WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TxtPOID & "'"
                    'KeyField = db.ExecuteNonSQLQuery(str)
                    'If KeyField <> "Success" Then
                    '    updtTran.Dispose()
                    '    Return "Error:Detail:- " & KeyField
                    'End If

                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TxtPOID & ",0")
                End If
                KeyField = "Success"
                updtTran.Complete()
            End Using
        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function



    '-----------------------------------CheckPermission------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckPermission(ByVal TransactionID As String) As String
        Dim KeyField As String = ""
        Try

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            Dim dtExist As New DataTable
            Dim dtExist1 As New DataTable
            Dim SxistStr As String

            Dim D1 As String = "", D2 As String = ""

            Dim IsPOApprovalRequired As Boolean
            str = "Select IsPOApprovalRequired from CompanyMaster where CompanyID = '" & GBLCompanyID & "'"
            Dim approvalQuery As String = "SELECT ISNULL(IsPOApprovalRequired,0) As IsPOApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
            db.FillDataTable(dataTable, approvalQuery)

            IsPOApprovalRequired = dataTable.Rows(0)("IsPOApprovalRequired").ToString()


            SxistStr = ""
            If DBType = "MYSQL" Then
                SxistStr = "Select IFNULL(TransactionID,0) as TransactionID from ItemTransactionDetail Where CompanyID='" & GBLCompanyID & "' and  TransactionID='" & TransactionID & "' and IFNULL(IsvoucherItemApproved,0)=1  and IFNULL(IsDeletedTransaction,0)<>1"
            Else
                SxistStr = "select isnull(TransactionID,0) as TransactionID from ItemTransactionDetail Where CompanyID='" & GBLCompanyID & "' and  TransactionID='" & TransactionID & "' and Isnull(IsvoucherItemApproved,0)=1  and isnull(IsDeletedTransaction,0)<>1"
            End If

            db.FillDataTable(dtExist, SxistStr)
            Dim E As Integer = dtExist.Rows.Count
            If E > 0 Then
                D1 = dtExist.Rows(0)(0)
            End If
            SxistStr = ""
            If DBType = "MYSQL" Then
                SxistStr = "Select  * From ItemTransactionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And IFNULL(QCApprovalNo,'')<>'' AND TransactionID=" & TransactionID & "  AND (IFNULL(ApprovedQuantity,0)>0 OR  IFNULL(RejectedQuantity,0)>0)"
            Else
                If IsPOApprovalRequired = True Then
                    SxistStr = "Select  * From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And isnull(QCApprovalNo,'')<>'' AND TransactionID=" & TransactionID & "  AND (Isnull(ApprovedQuantity,0)>0 OR  Isnull(RejectedQuantity,0)>0)"
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

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetOverFlowGrid(ByVal SelSupplierName As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If SelSupplierName = "" Then
            If DBType = "MYSQL" Then
                str = "Select  A.ItemID,A.CompanyID,A.ItemGroupID,IFNULL(IGM.ItemGroupNameID,0) As ItemGroupNameID,IFNULL(PH.ProductHSNID,0) As ProductHSNID,IFNULL(A.ItemSubGroupID,0) As ItemSubGroupID,Nullif(IGM.ItemGroupName,'') as ItemGroupName,nullif(A.ItemCode,'') AS ItemCode,nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,  Nullif(A.ItemName,'') as ItemName,Nullif(A.ItemDescription,'') as ItemDescription, IFNULL(A.BookedStock,0) As BookedStock,IFNULL(A.AllocatedStock,0) As AllocatedStock,IFNULL(A.PhysicalStock,0) As PhysicalStock,Nullif(A.StockUnit,'') as StockUnit,IFNULL(nullif(A.PurchaseOrderQuantity,''),0) AS PurchaseOrderQuantity,IFNULL(nullif(A.PurchaseRate,''),0) AS PurchaseRate,nullif(A.PurchaseUnit,'') AS PurchaseUnit,Nullif(PH.HSNCode,'')  AS HSNCode,Nullif(PH.ProductHSNName,'')  AS ProductHSNName,IFNULL(PH.GSTTaxPercentage,0) AS GSTTaxPercentage,IFNULL(PH.CGSTTaxPercentage,0) AS CGSTTaxPercentage,IFNULL(PH.SGSTTaxPercentage,0) AS SGSTTaxPercentage,IFNULL(PH.IGSTTaxPercentage,0) AS IGSTTaxPercentage,IFNULL(A.WtPerPacking,0) as WtPerPacking,IFNULL(A.UnitPerPacking,0) as UnitPerPacking,IFNULL(A.ConversionFactor,0) as ConversionFactor,Nullif(C.ConversionFormula,'') AS ConversionFormula, Nullif(C.ConvertedUnitDecimalPlace,'') AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,IFNULL(A.SizeW,0) AS SizeW,IFNULL(A.GSM,0) AS GSM,IFNULL(A.ReleaseGSM,0) AS ReleaseGSM,IFNULL(A.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(A.Thickness,0) AS Thickness,IFNULL(A.Density,0) AS Density " &
                      " From ItemMaster As A INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=A.ItemGroupID And IFNULL(A.IsDeletedTransaction,0)=0 And IGM.CompanyID=A.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=A.ItemSubGroupID  And ISGM.CompanyID=A.CompanyID LEFT JOIN ProductHSNMaster As PH ON PH.ProductHSNID=A.ProductHSNID And A.CompanyID=A.CompanyID LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=A.StockUnit And C.ConvertedUnitSymbol=A.PurchaseUnit And C.CompanyID=A.CompanyID LEFT JOIN ConversionMaster As CU ON CU.BaseUnitSymbol=A.PurchaseUnit And CU.ConvertedUnitSymbol=A.StockUnit And CU.CompanyID=A.CompanyID " &
                      " Where A.IsDeletedTransaction=0 And A.CompanyID = " & GBLCompanyID & " And Isnull(A.ISItemActive,0)<>0 Order by A.ItemGroupID,A.ItemName"
            Else
                str = "Select  A.[ItemID],A.[CompanyID],A.[ItemGroupID],Isnull(IGM.[ItemGroupNameID],0) As ItemGroupNameID,Isnull(PH.[ProductHSNID],0) As ProductHSNID,Isnull(A.[ItemSubGroupID],0) As ItemSubGroupID,Nullif(IGM.[ItemGroupName],'') as ItemGroupName,nullif(A.[ItemCode],'') AS ItemCode,nullif(ISGM.[ItemSubGroupName],'') AS ItemSubGroupName, A.Quality,A.Manufecturer,A.Finish, Nullif(A.[ItemName],'') as ItemName,Nullif(A.[ItemDescription],'') as ItemDescription, Isnull(IMS.[BookedStock],0) As BookedStock,Isnull(IMS.[AllocatedStock],0) As AllocatedStock,Isnull(IMS.[PhysicalStock],0) As PhysicalStock, Nullif(A.[StockUnit],'') as StockUnit,Isnull(nullif(A.[PurchaseOrderQuantity],''),0) AS PurchaseOrderQuantity,Isnull(nullif(A.[PurchaseRate],''),0) AS PurchaseRate, " &
                      "nullif(A.[PurchaseUnit],'') AS PurchaseUnit,Nullif(PH.HSNCode,'')  AS HSNCode,Nullif(PH.ProductHSNName,'')  AS ProductHSNName,Isnull(PH.GSTTaxPercentage,0) AS GSTTaxPercentage,Isnull(PH.CGSTTaxPercentage,0) AS CGSTTaxPercentage,Isnull(PH.SGSTTaxPercentage,0) AS SGSTTaxPercentage,Isnull(PH.IGSTTaxPercentage,0) AS IGSTTaxPercentage,Isnull(A.WtPerPacking,0) as WtPerPacking,Isnull(A.UnitPerPacking,0) as UnitPerPacking,Isnull(A.ConversionFactor,0) as ConversionFactor,Nullif(C.ConversionFormula,'') AS ConversionFormula, Nullif(C.ConvertedUnitDecimalPlace,'') AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,Isnull(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Isnull(A.SizeW,0) AS SizeW,Isnull(A.SizeL,0) AS SizeL,Isnull(A.GSM,0) AS GSM,Isnull(A.ReleaseGSM,0) AS ReleaseGSM,Isnull(A.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(A.Thickness,0) AS Thickness,Isnull(A.Density,0) AS Density, Nullif(A. StockRefCode,'') AS  StockRefCode,Isnull(CM.PurchaseTolerance,0) As Tolerance  " &
                      "From ItemMaster As A INNER Join ItemGroupMaster As IGM ON IGM.ItemGroupID=A.ItemGroupID And Isnull(A.IsDeletedTransaction,0)=0  LEFT Join ItemMasterStock As IMS On IMS.ItemID = A.ItemID And IMS.ProductionUnitId in(" & ProductionUnitIDStr & ") LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=A.ItemSubGroupID  AND Isnull(ISGM.IsDeletedTransaction,0)=0 " &
                        "LEFT JOIN ProductHSNMaster As PH On PH.ProductHSNID=A.ProductHSNID  LEFT JOIN ConversionMaster As C On C.BaseUnitSymbol=A.StockUnit And C.ConvertedUnitSymbol=A.PurchaseUnit  LEFT JOIN ConversionMaster As CU On CU.BaseUnitSymbol=A.PurchaseUnit And CU.ConvertedUnitSymbol=A.StockUnit  LEFT JOIN CompanyMaster As CM On CM.CompanyID = A.CompanyID Where IMS.ProductionUnitID = '" & ProductionUnitID & "' And ISNULL(A.IsDeletedTransaction,0)=0  And Isnull(A.ISItemActive,0)<>0 Order by A.[ItemGroupID],A.[ItemName]  "
            End If

        Else
            If DBType = "MYSQL" Then
                str = "Select Distinct IFNULL(SGA.LedgerID,0) AS LedgerID,A.ItemID,A.CompanyID,A.ItemGroupID,IFNULL(PH.ProductHSNID,0) AS ProductHSNID,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,IFNULL(A.ItemSubGroupID,0) AS ItemSubGroupID,Nullif(IGM.ItemGroupName,'') as ItemGroupName,nullif(A.ItemCode,'') AS ItemCode,nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(A.ItemName,'') as ItemName,Nullif(A.ItemDescription,'') as ItemDescription, IFNULL(A.BookedStock,0) As BookedStock,IFNULL(A.AllocatedStock,0) As AllocatedStock,IFNULL(A.PhysicalStock,0) As PhysicalStock,Nullif(A.StockUnit,'') as StockUnit,IFNULL(nullif(A.PurchaseOrderQuantity,''),0) AS PurchaseOrderQuantity,IFNULL(nullif(A.PurchaseRate,''),0) AS PurchaseRate,nullif(A.PurchaseUnit,'') AS PurchaseUnit,Nullif(PH.HSNCode,'')  AS HSNCode,Nullif(PH.ProductHSNName,'')  AS ProductHSNName,IFNULL(PH.GSTTaxPercentage,0) AS GSTTaxPercentage,IFNULL(PH.CGSTTaxPercentage,0) AS CGSTTaxPercentage,IFNULL(PH.SGSTTaxPercentage,0) AS SGSTTaxPercentage,IFNULL(PH.IGSTTaxPercentage,0) AS IGSTTaxPercentage,IFNULL(A.WtPerPacking,0) as WtPerPacking,IFNULL(A.UnitPerPacking,0) as UnitPerPacking,IFNULL(A.ConversionFactor,0) as ConversionFactor,Nullif(C.ConversionFormula,'') AS ConversionFormula, Nullif(C.ConvertedUnitDecimalPlace,'') AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,IFNULL(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,IFNULL(A.SizeW,0) AS SizeW,IFNULL(A.GSM,0) AS GSM,IFNULL(A.ReleaseGSM,0) AS ReleaseGSM,IFNULL(A.AdhesiveGSM,0) AS AdhesiveGSM,IFNULL(A.Thickness,0) AS Thickness,IFNULL(A.Density,0) AS Density " &
                      " From ItemMaster AS A INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=A.ItemGroupID And IFNULL(A.IsDeletedTransaction,0)=0 And IGM.CompanyID=A.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=A.ItemSubGroupID  And ISGM.CompanyID=A.CompanyID INNER JOIN SupplierWisePurchaseSetting As SGA ON SGA.ItemGroupID=IGM.ItemGroupID And SGA.ItemID=A.ItemID And SGA.CompanyID=IGM.CompanyID LEFT JOIN ProductHSNMaster As PH ON PH.ProductHSNID=A.ProductHSNID And A.CompanyID=A.CompanyID  LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=A.StockUnit And C.ConvertedUnitSymbol=A.PurchaseUnit And C.CompanyID=A.CompanyID LEFT JOIN ConversionMaster As CU ON CU.BaseUnitSymbol=A.PurchaseUnit And CU.ConvertedUnitSymbol=A.StockUnit And CU.CompanyID=A.CompanyID " &
                      " Where SGA.IsDeletedTransaction=0 And A.CompanyID=" & GBLCompanyID & " And SGA.LedgerID=" & SelSupplierName & " And Isnull(A.ISItemActive,0)<>0 Order by ItemGroupID,ItemName"
            Else
                str = "Select  Distinct Isnull(SGA.LedgerID,0) As LedgerID,A.[ItemID],A.[CompanyID],A.[ItemGroupID],Isnull(PH.[ProductHSNID],0) As ProductHSNID,Isnull(IGM.[ItemGroupNameID],0) As ItemGroupNameID,Isnull(A.[ItemSubGroupID],0) As ItemSubGroupID,Nullif(IGM.[ItemGroupName],'') as ItemGroupName,nullif(A.[ItemCode],'') AS ItemCode,nullif(ISGM.[ItemSubGroupName],'') AS ItemSubGroupName,A.Quality,A.Manufecturer,A.Finish,Nullif(A.[ItemName],'') as ItemName,Nullif(A.[ItemDescription],'') as ItemDescription, Isnull(IMS.[BookedStock],0) As BookedStock,Isnull(IMS.[AllocatedStock],0) As AllocatedStock,Isnull(IMS.[PhysicalStock],0) As PhysicalStock,Nullif(A.[StockUnit],'') as StockUnit,Isnull(nullif(A.[PurchaseOrderQuantity],''),0) AS PurchaseOrderQuantity,Isnull(nullif(A.[PurchaseRate],''),0) AS PurchaseRate,  " &
                       "nullif(A.[PurchaseUnit],'') AS PurchaseUnit,Nullif(PH.HSNCode,'')  AS HSNCode,Nullif(PH.ProductHSNName,'')  AS ProductHSNName,Isnull(PH.GSTTaxPercentage,0) AS GSTTaxPercentage,Isnull(PH.CGSTTaxPercentage,0) AS CGSTTaxPercentage,Isnull(PH.SGSTTaxPercentage,0) AS SGSTTaxPercentage,Isnull(PH.IGSTTaxPercentage,0) AS IGSTTaxPercentage,Isnull(A.WtPerPacking,0) as WtPerPacking,Isnull(A.UnitPerPacking,0) as UnitPerPacking,Isnull(A.ConversionFactor,0) as ConversionFactor,Nullif(C.ConversionFormula,'') AS ConversionFormula, Nullif(C.ConvertedUnitDecimalPlace,'') AS UnitDecimalPlace,Nullif(CU.ConversionFormula,'') AS  ConversionFormulaStockUnit,Isnull(CU.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlaceStockUnit,Isnull(A.SizeW,0) AS SizeW,Isnull(A.SizeL,0) AS SizeL,Isnull(A.GSM,0) AS GSM,Isnull(A.ReleaseGSM,0) AS ReleaseGSM,Isnull(A.AdhesiveGSM,0) AS AdhesiveGSM,Isnull(A.Thickness,0) AS Thickness,Isnull(A.Density,0) AS Density,Isnull(nullif(A.StockRefCode,''),'-') As StockRefCode,Isnull(CM.PurchaseTolerance,0) As Tolerance  " &
                       "From ItemMaster As A INNER Join ItemGroupMaster As IGM ON IGM.ItemGroupID=A.ItemGroupID And Isnull(A.IsDeletedTransaction,0)=0 Left Join ItemMasterStock As IMS On IMS.ItemID = A.ItemID And IMS.ProductionUnitId in(" & ProductionUnitIDStr & ") LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=A.ItemSubGroupID  AND Isnull(ISGM.IsDeletedTransaction,0)=0 " &
                       "LEFT JOIN SupplierWisePurchaseSetting As SGA On SGA.ItemGroupID=IGM.ItemGroupID And SGA.ItemID=A.ItemID  And Isnull(SGA.IsDeletedTransaction,0)=0  " &
                       "And Isnull(SGA.LedgerID,0)=" & SelSupplierName & "  LEFT JOIN ProductHSNMaster As PH ON PH.ProductHSNID=A.ProductHSNID   " &
                       "LEFT JOIN ConversionMaster As C On C.BaseUnitSymbol=A.StockUnit And C.ConvertedUnitSymbol=A.PurchaseUnit  " &
                       "LEFT JOIN ConversionMaster As CU On CU.BaseUnitSymbol=A.PurchaseUnit And CU.ConvertedUnitSymbol=A.StockUnit  " &
                       "LEFT JOIN CompanyMaster As CM On CM.CompanyID = A.CompanyID   " &
                       "Where IMS.ProductionUnitID = '" & ProductionUnitID & "' And Isnull(A.IsDeletedTransaction, 0) = 0 And Isnull(A.ISItemActive, 0) <> 0 Order by [ItemGroupID],[ItemName]  "
            End If

        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function



    ''----------------------------Open Get Purchase Order No  Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetPONO(ByVal prefix As String) As String

        Dim dt As New DataTable
        Dim PONo As String
        Dim MaxVoucherNo As Long
        Dim KeyField As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Try

            PONo = db.GeneratePrefixedNo("ItemTransactionMain", prefix, "MaxVoucherNo", MaxVoucherNo, GBLFYear, " Where VoucherPrefix='" & prefix & "' And  CompanyID=" & GBLCompanyID & " And FYear='" & GBLFYear & "' ")

            KeyField = PONo

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    ''----------------------------Open Get Purchase Order No  Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckIsAdmin() As String

        Dim IsAdminUser As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        Dim UserName As String = Convert.ToString(HttpContext.Current.Session("UserName"))

        Try
            If UserName = "Admin" Then
                IsAdminUser = True
            Else
                IsAdminUser = False
            End If
        Catch ex As Exception
            IsAdminUser = False
        End Try
        Return IsAdminUser

    End Function

    '---------------Supplier code---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function Supplier() As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select Distinct A.LedgerID,B.LedgerGroupID,B.LedgerGroupNameID,A.CompanyID,A.LedgerName,nullif(A.MailingName,'') as MailingName,nullif(A.City,'') as City,nullif(A.State,'') as SupState,nullif(S.StateCode,'') AS StateCode,IFNULL(S.StateTinNo,0) AS StateTinNo,nullif(A.Country,'') as Country,nullif(A.MobileNo,'') as MobileNo,nullif(A.GSTNo,'') AS GSTNo,nullif(A.CurrencyCode,'') AS CurrencyCode,IFNULL(A.GSTApplicable,0) AS GSTApplicable,IFNULL(C.stateTinNo,0) AS CompanyStateTinNo " &
                  " From LedgerMaster AS A INNER JOIN LedgerGroupMaster AS B ON A.LedgerGroupID=B.LedgerGroupID And A.CompanyID = B.CompanyID INNER JOIN CompanyMaster AS C ON C.CompanyID =A.CompanyID LEFT JOIN CountryStateMaster AS S ON S.State=A.State " &
                  " Where A.IsLedgerActive=1 And A.IsDeletedTransaction=0 And A.CompanyID='" & GBLCompanyID & "'  and LedgerGroupNameID=23"
        Else
            'str = "Select Distinct A.[LedgerID],B.[LedgerGroupID],B.[LedgerGroupNameID],A.[CompanyID],A.[LedgerName],nullif(A.[MailingName],'') as MailingName,nullif(A.[City],'') as City,nullif(A.[State],'') as SupState,nullif(S.[StateCode],'') AS StateCode,Isnull(S.[StateTinNo],0) AS StateTinNo,nullif(A.[Country],'') as Country,nullif(A.[MobileNo],'') as MobileNo,nullif(A.[GSTNo],'') AS GSTNo,nullif(A.[CurrencyCode],'') AS CurrencyCode,Isnull(A.[GSTApplicable],0) AS GSTApplicable,Isnull(C.stateTinNo,0) AS CompanyStateTinNo " &
            '    "From LedgerMaster AS A " &
            '    "INNER JOIN LedgerGroupMaster AS B ON A.LedgerGroupID=B.LedgerGroupID And A.CompanyID = B.CompanyID INNER JOIN CompanyMaster AS C ON C.CompanyID =A.CompanyID LEFT JOIN CountryStateMaster AS S ON S.State=A.State " &
            '    "Where A.IsLedgerActive=1 And A.IsDeletedTransaction=0 And A.CompanyID='" & GBLCompanyID & "'  and LedgerGroupNameID=23 "

            str = " Select   A.[LedgerID],B.[LedgerGroupID],B.[LedgerGroupNameID],A.[CompanyID],(A.[LedgerName]   + '-' + Isnull(A.[RefCode],'') + ', State : ' + nullif(A.[State],'')  + ', Country : ' + nullif(A.[Country],'')) as LedgerName,nullif(A.[MailingName],'') as MailingName,nullif(A.[City],'') as City,nullif(A.[State],'') as SupState, nullif(S.[StateCode],'') AS StateCode,Isnull(S.[StateTinNo],0) AS StateTinNo,nullif(A.[Country],'') as Country,nullif(A.[MobileNo],'') as MobileNo,nullif(A.[GSTNo],'') AS GSTNo, nullif(A.[CurrencyCode],'') AS CurrencyCode,Isnull(A.[GSTApplicable],0) AS GSTApplicable,    (SELECT ISNULL(StateTinNo, 0) FROM CompanyMaster WHERE CompanyID = '" & GBLCompanyID & "') AS CompanyStateTinNo  From LedgerMasterData AS A " &
                  "INNER JOIN LedgerGroupMaster As B On A.LedgerGroupID=B.LedgerGroupID INNER JOIN CountryStateMaster As S On S.State=A.State Where  B.LedgerGroupNameID=23 " &
                  "Group BY  A.[LedgerID],B.[LedgerGroupID],B.[LedgerGroupNameID],A.[CompanyID],(A.[LedgerName]   + '-' + Isnull(A.[RefCode],''))  ,nullif(A.[MailingName],'')  ,nullif(A.[City],'') ,nullif(A.[State],''),nullif(S.[StateCode],'')  ,Isnull(S.[StateTinNo],0)  ,nullif(A.[Country],'') ,nullif(A.[MobileNo],'')  ,nullif(A.[GSTNo],''),nullif(A.[CurrencyCode],'') ,Isnull(A.[GSTApplicable],0) "

        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Contact Peson code---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetContactPerson(ByVal ContactPerson As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))

        str = "Select ConcernPersonID,Name from ConcernPersonMaster Where IsDeletedTransaction=0 And LedgerID='" & ContactPerson & "' Order By Name "

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Contact Peson code---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CHLname() As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "SELECT LedgerID,LedgerName,IFNULL(TaxPercentage,0) as TaxPercentage,nullif(TaxType,'') as TaxType,IFNULL(GSTApplicable,'False') as GSTApplicable,nullif(GSTLedgerType,'') as GSTLedgerType,nullif(GSTCalculationOn,'') as GSTCalculationOn " &
                  " FROM LedgerMaster Where IsLedgerActive=1 And CompanyID='" & GBLCompanyID & "' AND IsDeletedTransaction=0 And LedgerGroupID IN(Select Distinct LedgerGroupID From LedgerGroupMaster Where CompanyID='" & GBLCompanyID & "' AND LedgerGroupNameID=43)"
        Else
            str = "SELECT LedgerID,LedgerName,isnull([TaxPercentage],0) as TaxPercentage,nullif([TaxType],'') as TaxType,Isnull([GSTApplicable],'False') as GSTApplicable,nullif([GSTLedgerType],'') as GSTLedgerType,nullif([GSTCalculationOn],'') as GSTCalculationOn " &
               " FROM LedgerMaster Where IsLedgerActive=1  AND IsDeletedTransaction=0 AND LedgerGroupID IN(Select Distinct LedgerGroupID From LedgerGroupMaster Where LedgerGroupNameID=43) "
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Head Data code---------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function HeadFun() As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select IFNULL(HeadID,0) as HeadID,nullif(Head,'') as Head,nullif(RateType,'') as RateType,0 as Weight,0 as Rate,0 as HeadAmount From PurchaseHeadMaster"
        Else
            str = "Select isnull(HeadID,0) as HeadID,nullif(Head,'') as Head,nullif(RateType,'') as RateType,0 as Weight,0 as Rate,0 as HeadAmount From PurchaseHeadMaster"
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Get Item rate code---------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetItemRate(ByVal LedgerId As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select Distinct IFNULL(ItemID,0) AS ItemID,QuantityTolerance,IFNULL(PurchaseRate,0) AS PurchaseRate,IFNULL(LedgerID,'') as LedgerID  From SupplierWisePurchaseSetting Where IsDeletedTransaction=0 And LedgerID='" & LedgerId & "' AND CompanyID='" & GBLCompanyID & "'"
        Else
            str = "Select Distinct isnull(ItemID,0) AS ItemID,QuantityTolerance,Isnull(PurchaseRate,0) AS PurchaseRate,isnull(LedgerID,'') as LedgerID  From SupplierWisePurchaseSetting Where IsDeletedTransaction=0 And LedgerID='" & LedgerId & "'"
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    ''----------------------------Open PaaperPurchaseOrder  Save Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SavePaperPurchaseOrder(ByVal prefix As String, ByVal jsonObjectsRecordMain As Object, ByVal jsonObjectsRecordDetail As Object, ByVal jsonObjectsRecordOverHead As Object, ByVal jsonObjectsRecordTax As Object, ByVal jsonObjectsRecordSchedule As Object, ByVal jsonObjectsRecordRequisition As Object, ByVal TxtNetAmt As String, ByVal CurrencyCode As String, ByVal jsonObjectsUserApprovalProcessArray As Object, ByVal FilejsonObjectsTransactionMain As Object) As String

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

        If db.CheckAuthories("PurchaseOrder.aspx", GBLUserID, GBLCompanyID, "CanSave") = False Then Return "You are not authorized to save..!"

        Dim StrDt As New DataTable
        Dim VoucherNo As String = ""
        Dim IsVoucherItemApproved As String = 0
        Dim IsApprovalRequired As Boolean = False
        Dim ItemDataTable As New DataTable
        Dim dt As New DataTable
        Dim dtCurrency As New DataTable 'For Currency
        Dim CurrencyHeadName, CurrencyChildName As String 'For Currency
        Dim PONo As String
        Dim MaxPONo As Long
        Dim KeyField, str2, TransactionID, NumberToWord As String
        Dim AddColName, AddColValue, TableName As String
        Dim result As String
        AddColName = ""
        AddColValue = ""

        Try
            NumberToWord = ""
            If CurrencyCode = "INR" Or CurrencyCode = "" Then
                CurrencyHeadName = ""
                CurrencyChildName = ""
                CurrencyCode = "INR"
                NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
            Else
                NumberToWord = ""
                str2 = ""
                str2 = "Select nullif(CurrencyHeadName,'') as CurrencyHeadName,Nullif(CurrencyChildName,'') as CurrencyChildName From CurrencyMaster Where  CurrencyCode='" & CurrencyCode & "'"
                db.FillDataTable(dtCurrency, str2)
                Dim j As Integer = dtCurrency.Rows.Count
                If j > 0 Then
                    CurrencyHeadName = IIf(IsDBNull(dtCurrency.Rows(0)(0)), "", dtCurrency.Rows(0)(0))
                    CurrencyChildName = IIf(IsDBNull(dtCurrency.Rows(0)(1)), "", dtCurrency.Rows(0)(1))
                    NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
                End If
            End If

            'Dim IsPOApprovalRequired As Boolean
            'Dim Approvalstatus As Boolean
            'Dim approvalQuery As String = "SELECT Isnull(IsPOApprovalRequired,0) as IsPOApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
            'db.FillDataTable(dataTable, approvalQuery)

            'IsPOApprovalRequired = dataTable.Rows(0)("IsPOApprovalRequired").ToString()

            'If IsPOApprovalRequired = True Then
            '    Approvalstatus = False
            'Else
            '    Approvalstatus = True
            'End If

            PONo = db.GeneratePrefixedNo("ItemTransactionMain", prefix, "MaxVoucherNo", MaxPONo, GBLFYear, " Where VoucherID = -11 And VoucherPrefix='" & prefix & "' And  CompanyID=" & GBLCompanyID & " And FYear='" & GBLFYear & "' ")
            Using updtTran As New Transactions.TransactionScope
                If DBType = "MYSQL" Then
                    TableName = "ItemTransactionMain"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,VoucherPrefix,MaxVoucherNo,VoucherNo,AmountInWords,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & prefix & "','" & MaxPONo & "','" & PONo & "','" & NumberToWord & "','" & ProductionUnitID & "'"
                    TransactionID = db.InsertDatatableToDatabase(jsonObjectsRecordMain, TableName, AddColName, AddColValue)

                    If IsNumeric(TransactionID) = False Then
                        updtTran.Dispose()
                        Return "Error: " & TransactionID
                    End If

                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error: " & result
                    End If

                    TableName = "ItemPurchaseOrderTaxes"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordTax, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error: " & result
                    End If

                    TableName = "ItemPurchaseDeliverySchedule"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordSchedule, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error: " & result
                    End If

                    TableName = "ItemPurchaseOverheadCharges"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordOverHead, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOverheadCharges Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error: " & result
                    End If

                    TableName = "ItemPurchaseRequisitionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordRequisition, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOverheadCharges Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseRequisitionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionAttachment Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error: " & result
                    End If

                    TableName = "ItemTransactionAttachments"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(FilejsonObjectsTransactionMain, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        Return "Error:Main:- " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES_UNIT_WISE( " & GBLCompanyID & "," & TransactionID & ",0);")
                Else
                    Dim str1 As String = ""
                    Dim ModuleID As Long = 0
                    Dim DisplayModuleName As String = ""
                    Dim ApprovalByUserID As Long = 0
                    IsVoucherItemApproved = 0
                    db.checkDynamicTransactionApprovalRequirement(GBLCompanyID, "PurchaseOrder.aspx", IsApprovalRequired, IsVoucherItemApproved, ApprovalByUserID, ModuleID, DisplayModuleName)

                    TableName = "ItemTransactionMain"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,VoucherPrefix,MaxVoucherNo,VoucherNo,AmountInWords,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & prefix & "','" & MaxPONo & "','" & PONo & "','" & NumberToWord & "','" & ProductionUnitID & "'"
                    TransactionID = db.InsertDatatableToDatabase(jsonObjectsRecordMain, TableName, AddColName, AddColValue)

                    If IsNumeric(TransactionID) = False Then
                        updtTran.Dispose()
                        Return "Error Main: " & TransactionID
                    End If

                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,IsVoucherItemApproved,VoucherItemApprovedBy,ProductionUnitID,VoucherItemApprovedDate"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & IsVoucherItemApproved & "','" & ApprovalByUserID & "','" & ProductionUnitID & "',GetDate()"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error Detail: " & result
                    End If

                    TableName = "ItemPurchaseOrderTaxes"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordTax, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error Taxes: " & result
                    End If

                    TableName = "ItemPurchaseDeliverySchedule"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordSchedule, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error Delivery Schedule: " & result
                    End If

                    TableName = "ItemPurchaseOverheadCharges"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordOverHead, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOverheadCharges Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error overheads: " & result
                    End If

                    TableName = "ItemPurchaseRequisitionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    result = db.InsertDatatableToDatabase(jsonObjectsRecordRequisition, TableName, AddColName, AddColValue)

                    If IsNumeric(result) = False Then
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionMain Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemTransactionDetail Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOrderTaxes Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseDeliverySchedule Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseOverheadCharges Where TransactionID=" & TransactionID)
                        db.ExecuteNonSQLQuery("Delete From ItemPurchaseRequisitionDetail Where TransactionID=" & TransactionID)
                        updtTran.Dispose()
                        Return "Error Requisition update: " & result
                    End If

                    TableName = "ItemTransactionAttachments"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(FilejsonObjectsTransactionMain, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        Return "Error:Main:- " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TransactionID & ",0")

                    If Val(IsVoucherItemApproved) = 0 Then
                        db.ConvertObjectToDatatable(jsonObjectsUserApprovalProcessArray, ItemDataTable, str)
                        If str <> "Success" Then
                            Return str
                        End If
                        If (ItemDataTable.Rows.Count > 0) Then
                            For i As Integer = 0 To ItemDataTable.Rows.Count - 1
                                Dim TransactionDetailID = db.GetColumnValue("TransactionDetailID", "ItemTransactionDetail", "TransactionID = '" & TransactionID & "' and ItemID = '" & Trim(ItemDataTable.Rows(i)("ItemID")) & "' And ISNULL(IsDeletedTransaction,0)=0 And ProductionUnitID = '" & ProductionUnitID & "'")
                                If (Val(TransactionDetailID) > 0) Then
                                    Dim ItemDescription As String = "LedgerID =  " & Trim(ItemDataTable.Rows(i)("LedgerID")) & " And LedgerName=  " & Trim(ItemDataTable.Rows(i)("LedgerName")) & "  And ItemName=  " & Trim(ItemDataTable.Rows(i)("ItemName")) & " And ItemCode=  " & Trim(ItemDataTable.Rows(i)("ItemCode")) & " And ExpectedDeliveryDate=  " & Trim(ItemDataTable.Rows(i)("ExpectedDeliveryDate")) & "  And PurchaseQty=  " & Trim(ItemDataTable.Rows(i)("PurchaseQty")) & "  And ItemRate = " & Trim(ItemDataTable.Rows(i)("ItemRate")) & " And ItemID=  " & Trim(ItemDataTable.Rows(i)("ItemID")) & " And ItemAmount = " & Trim(ItemDataTable.Rows(i)("ItemAmount"))
                                    KeyField = db.ExecuteNonSQLQuery("EXEC UserApprovalProcessMultiUnit  '" & DisplayModuleName & "'," & ModuleID & "," & GBLCompanyID & "," & ProductionUnitID & ",'" & TransactionID & "'," & TransactionDetailID & ",-11,'Paper Purchase Order','" & PONo & "','',''," & Trim(ItemDataTable.Rows(i)("ItemID")) & ",'" & ItemDescription & "','" & GBLFYear & "', " & GBLUserID & ",0,'ItemTransactionDetail','','TransactionID','TransactionDetailID','VoucherNo','ItemID','','IsVoucherItemApproved', '',''," & Trim(ItemDataTable.Rows(i)("LedgerID")) & ",'" & Trim(ItemDataTable.Rows(i)("ItemName")) & "','" & Trim(ItemDataTable.Rows(i)("PurchaseQty")) & "','" & Trim(ItemDataTable.Rows(i)("ItemRate")) & "','" & Trim(ItemDataTable.Rows(i)("ItemAmount")) & "','PurchaseOrder.aspx?TransactionID=" & TransactionID & "'")
                                    If KeyField <> "Success" Then
                                        Return KeyField
                                    End If
                                End If
                            Next
                        End If
                    End If

                End If
                KeyField = "Success,TransactionID: " & TransactionID
                updtTran.Complete()
            End Using
        Catch ex As Exception
            KeyField = "Error: " & ex.Message
        End Try
        Return KeyField

    End Function

    ''----------------------------Open PurchaseOrder  Update Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdatePurchaseOrder(ByVal TransactionID As String, ByVal jsonObjectsRecordMain As Object, ByVal jsonObjectsRecordDetail As Object, ByVal jsonObjectsRecordOverHead As Object, ByVal jsonObjectsRecordTax As Object, ByVal jsonObjectsRecordSchedule As Object, ByVal jsonObjectsRecordRequisition As Object, ByVal TxtNetAmt As String, ByVal CurrencyCode As String, ByVal jsonObjectsUserApprovalProcessArray As Object, ByVal ObjvalidateLoginUser As Object, ByVal voucherNo As String, ByVal FilejsonObjectsTransactionMain As Object) As String

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

        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        Dim dt As New DataTable
        Dim dtCurrency As New DataTable 'For Currency
        Dim CurrencyHeadName, CurrencyChildName, str2 As String 'For Currency
        Dim KeyField, NumberToWord As String
        Dim AddColName, wherecndtn, TableName, AddColValue As String
        Dim IsVoucherItemApproved As String = 0
        Dim ApprovalbyUserID As Long = 0
        Dim IsApprovalRequired As Boolean = False
        AddColName = ""

        NumberToWord = ""
        If CurrencyCode = "INR" Or CurrencyCode = "" Then
            CurrencyHeadName = ""
            CurrencyChildName = ""
            CurrencyCode = "INR"
            NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
        Else
            NumberToWord = ""
            str2 = ""
            str2 = "Select nullif(CurrencyHeadName,'') as CurrencyHeadName,Nullif(CurrencyChildName,'') as CurrencyChildName From CurrencyMaster Where CurrencyCode='" & CurrencyCode & "'"
            db.FillDataTable(dtCurrency, str2)
            Dim j As Integer = dtCurrency.Rows.Count
            If j > 0 Then
                CurrencyHeadName = IIf(IsDBNull(dtCurrency.Rows(0)(0)), "", dtCurrency.Rows(0)(0))
                CurrencyChildName = IIf(IsDBNull(dtCurrency.Rows(0)(1)), "", dtCurrency.Rows(0)(1))
                NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
            End If
        End If

        'Dim IsPOApprovalRequired As Boolean
        'Dim Approvalstatus As Boolean
        'Dim approvalQuery As String = "SELECT Isnull(IsPOApprovalRequired,0) as IsPOApprovalRequired FROM CompanyMaster WHERE CompanyID = " & GBLCompanyID
        'db.FillDataTable(dataTable, approvalQuery)

        'IsPOApprovalRequired = dataTable.Rows(0)("IsPOApprovalRequired").ToString()

        'If IsPOApprovalRequired = True Then
        '    Approvalstatus = False
        'Else
        '    Approvalstatus = True
        'End If

        If DBType = "MYSQL" Then
            Using updateTransaction As New Transactions.TransactionScope
                Try
                    Dim dtExist As New DataTable

                    TableName = "ItemTransactionMain"
                    AddColName = "ModifiedDate=Now(),UserID=" & GBLUserID & ",CompanyID=" & GBLCompanyID & ",ModifiedBy='" & GBLUserID & "',AmountInWords='" & NumberToWord & "',ProductionUnitID='" & ProductionUnitID & "'"
                    wherecndtn = "ProductionUnitID=" & ProductionUnitID & " And TransactionID='" & TransactionID & "' "
                    KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordMain, TableName, AddColName, 1, wherecndtn)

                    If KeyField <> "Success" Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("Delete from ItemTransactionDetail WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseOrderTaxes WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseDeliverySchedule WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseOverheadCharges WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseRequisitionDetail WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")

                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseOrderTaxes"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordTax, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseDeliverySchedule"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordSchedule, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseOverheadCharges"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordOverHead, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseRequisitionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordRequisition, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    updateTransaction.Complete()
                    KeyField = "Success"
                    db.ExecuteNonSQLQuery("CALL UPDATE_ITEM_STOCK_VALUES_UNIT_WISE( " & GBLCompanyID & "," & TransactionID & ",0);")

                Catch ex As Exception
                    updateTransaction.Dispose()
                    KeyField = "Error: " & ex.Message
                End Try
                Return KeyField
            End Using
        Else
            Using updateTransaction As New Transactions.TransactionScope
                Try
                    'Dim dtExist As New DataTable
                    Dim ModuleID As Long = 0
                    Dim DisplayModuleName As String = ""
                    Dim TransactionCheckdtExist As New DataTable
                    str = "Select TransactionID  from ItemTransactionDetail where PurchaseTransactionID = '" & TransactionID & "' And ISNULL(IsDeletedTransaction,0)=0 And ISNULL(IsBlocked,0)=0 And ISNULL(IsLocked,0)=0 "
                    db.FillDataTable(TransactionCheckdtExist, str)
                    If (TransactionCheckdtExist.Rows.Count > 0) Then
                        KeyField = "TransactionUsed"
                        updateTransaction.Dispose()
                        Return KeyField
                    Else

                        IsVoucherItemApproved = 0
                        db.checkDynamicTransactionApprovalRequirement(GBLCompanyID, "PurchaseOrder.aspx", IsApprovalRequired, IsVoucherItemApproved, ApprovalbyUserID, ModuleID, DisplayModuleName)

                        If IsApprovalRequired = True Then
                            Dim StrIsApproved As String = ""
                            Dim dtExistIsApproved As New DataTable
                            StrIsApproved = " Select top(1) IsApproved from UserApprovalTransactionsDetail As UAT Where RecordTransactionDetailID In(Select TransactionDetailID from ItemTransactionDetail Where TransactionID = '" & TransactionID & "' And ISNULL(IsvoucherItemApproved,0)<>0 And ISNULL(IsDeletedTransaction,0)=0) AND ModuleID=" & ModuleID & ""
                            db.FillDataTable(dtExistIsApproved, StrIsApproved)
                            If (dtExistIsApproved.Rows.Count > 0) Then
                                'KeyField = "Sorry Your Requisition is Approved. Please go and Unapprove this Requisition first and then Delete it."
                                KeyField = "PurchaseOrderApproved"
                                updateTransaction.Dispose()
                                Return KeyField
                            End If
                        End If
                    End If

                    If db.CheckAuthories("PurchaseOrder.aspx", GBLUserID, GBLCompanyID, "CanEdit", TransactionID, ObjvalidateLoginUser("transactionRemark")) = False Then
                        updateTransaction.Dispose()
                        Return "You are not authorized to update..!"
                    End If

                    If IsApprovalRequired = True Then
                        str = "Delete from UserApprovalTransactionsDetail Where RecordTransactionDetailID In(Select TransactionDetailID From ItemTransactionDetail Where TransactionID ='" & TransactionID & "' And Isnull(IsDeletedTransaction,0)=0 And  Isnull(IsVoucherItemApproved,0)=0 )  AND ModuleID=" & ModuleID & ""
                        KeyField = db.ExecuteNonSQLQuery(str)
                        If KeyField <> "Success" Then
                            updateTransaction.Dispose()
                            Return KeyField
                        End If
                    End If

                    TableName = "ItemTransactionMain"
                    AddColName = "ModifiedDate=GetDate(),UserID=" & GBLUserID & ",CompanyID=" & GBLCompanyID & ",ModifiedBy='" & GBLUserID & "',AmountInWords='" & NumberToWord & "',ProductionUnitID='" & ProductionUnitID & "'"
                    wherecndtn = "ProductionUnitID=" & ProductionUnitID & " And TransactionID='" & TransactionID & "' "
                    KeyField = db.UpdateDatatableToDatabase(jsonObjectsRecordMain, TableName, AddColName, 1, wherecndtn)

                    If KeyField <> "Success" Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    db.ExecuteNonSQLQuery("Delete from ItemTransactionDetail WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseOrderTaxes WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseDeliverySchedule WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseOverheadCharges WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")
                    db.ExecuteNonSQLQuery("Delete from ItemPurchaseRequisitionDetail WHERE ProductionUnitID='" & ProductionUnitID & "' and TransactionID='" & TransactionID & "' ")

                    TableName = "ItemTransactionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,IsVoucherItemApproved,VoucherItemApprovedBy,VoucherItemApprovedDate,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & IsVoucherItemApproved & "','" & ApprovalbyUserID & "',GetDate(),'" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordDetail, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseOrderTaxes"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordTax, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseDeliverySchedule"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordSchedule, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseOverheadCharges"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordOverHead, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemPurchaseRequisitionDetail"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(),GetDate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "','" & GBLUserID & "','" & TransactionID & "','" & ProductionUnitID & "'"
                    KeyField = db.InsertDatatableToDatabase(jsonObjectsRecordRequisition, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        updateTransaction.Dispose()
                        Return "Error: " & KeyField
                    End If

                    TableName = "ItemTransactionAttachments"
                    AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,CreatedBy,ModifiedBy,TransactionID,ProductionUnitID"
                    AddColValue = "GetDate(), GetDate(), " & GBLUserID & ", " & GBLCompanyID & ", " & GBLUserID & ", " & GBLUserID & ", " & TransactionID & ", " & ProductionUnitID
                    Dim sqlDel As String = "DELETE FROM ItemTransactionAttachments WHERE TransactionID = " & TransactionID & " "
                    db.ExecuteNonSQLQuery(sqlDel)
                    KeyField = db.InsertDatatableToDatabase(FilejsonObjectsTransactionMain, TableName, AddColName, AddColValue)
                    If IsNumeric(KeyField) = False Then
                        Return "Error:Main:- " & KeyField
                    End If

                    updateTransaction.Complete()
                    updateTransaction.Dispose()
                    KeyField = "Success"
                    db.ExecuteNonSQLQuery("EXEC UPDATE_ITEM_STOCK_VALUES_UNIT_WISE " & GBLCompanyID & "," & TransactionID & ",0")

                    If Val(IsVoucherItemApproved) = 0 Then
                        Dim StrDt As New DataTable
                        Dim ItemDataTable As New DataTable
                        db.ConvertObjectToDatatable(jsonObjectsUserApprovalProcessArray, ItemDataTable, str)
                        If str <> "Success" Then
                            Return str
                        End If
                        If (ItemDataTable.Rows.Count > 0) Then
                            For i As Integer = 0 To ItemDataTable.Rows.Count - 1
                                Dim TransactionDetailID = db.GetColumnValue("TransactionDetailID", "ItemTransactionDetail", "TransactionID = '" & TransactionID & "' and ItemID = '" & Trim(ItemDataTable.Rows(i)("ItemID")) & "' And ISNULL(IsDeletedTransaction,0)=0 And ProductionUnitID = '" & ProductionUnitID & "'")
                                If (Val(TransactionDetailID) > 0) Then
                                    Dim ItemDescription As String = "LedgerID =  " & Trim(ItemDataTable.Rows(i)("LedgerID")) & " And LedgerName=  " & Trim(ItemDataTable.Rows(i)("LedgerName")) & "  And ItemName=  " & Trim(ItemDataTable.Rows(i)("ItemName")) & " And ItemCode=  " & Trim(ItemDataTable.Rows(i)("ItemCode")) & " And ExpectedDeliveryDate=  " & Trim(ItemDataTable.Rows(i)("ExpectedDeliveryDate")) & "  And PurchaseQty=  " & Trim(ItemDataTable.Rows(i)("PurchaseQty")) & "  And ItemRate = " & Trim(ItemDataTable.Rows(i)("ItemRate")) & " And ItemID=  " & Trim(ItemDataTable.Rows(i)("ItemID"))
                                    Dim StoreStr As String = "EXEC UserApprovalProcessMultiUnit  '" & DisplayModuleName & "'," & ModuleID & "," & GBLCompanyID & "," & ProductionUnitID & ",'" & TransactionID & "'," & TransactionDetailID & ",-11,'Paper Purchase Order','" & voucherNo & "','',''," & Trim(ItemDataTable.Rows(i)("ItemID")) & ",'" & ItemDescription & "','" & GBLFYear & "', " & GBLUserID & ",0,'ItemTransactionDetail','','TransactionID','TransactionDetailID','VoucherNo','ItemID','','IsVoucherItemApproved', '',''," & Trim(ItemDataTable.Rows(i)("LedgerID")) & ",'" & Trim(ItemDataTable.Rows(i)("ItemName")) & "','" & Trim(ItemDataTable.Rows(i)("PurchaseQty")) & "','0','0','PurchaseOrder.aspx?TransactionID=" & TransactionID & "'"
                                    KeyField = db.ExecuteNonSQLQuery(StoreStr)
                                    If KeyField <> "Success" Then
                                        Return KeyField
                                    End If
                                End If
                            Next
                        End If
                    End If


                Catch ex As Exception
                    updateTransaction.Dispose()
                    KeyField = "Error: " & ex.Message
                End Try
                Return KeyField
            End Using
        End If
    End Function

    '---------------Allocated Supp ItemGroupWise---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetAllotedSupp(ByVal ItemGroupID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        'str = "Select Distinct LM.LedgerID,LM.LedgerName from SupplierWisePurchaseSetting As STGA inner join LedgerMaster as LM on STGA.LedgerID=LM.LedgerID And STGA.CompanyID=LM.CompanyID where STGA.ItemGroupID='" & ItemGroupID & "'  AND STGA.CompanyID='" & GBLCompanyID & "' and Isnull(STGA.IsDeletedTransaction,0)<>1 "
        If DBType = "MYSQL" Then
            str = "Select Distinct LM.LedgerID,LM.LedgerName from SupplierWisePurchaseSetting As STGA RIGHT JOIN LedgerMaster as LM on STGA.LedgerID=LM.LedgerID And STGA.CompanyID=LM.CompanyID where STGA.ItemGroupID='" & ItemGroupID & "'  AND STGA.CompanyID='" & GBLCompanyID & "' and IFNULL(STGA.IsDeletedTransaction,0)<>1"
        Else
            str = "Select Distinct LM.LedgerID,LM.LedgerName from SupplierItemGroupAllocation As STGA RIGHT JOIN LedgerMaster as LM on LM.LedgerID=STGA.LedgerID And LM.CompanyID=STGA.CompanyID where STGA.ItemGroupID='" & ItemGroupID & "'  AND STGA.CompanyID='" & GBLCompanyID & "' and Isnull(STGA.IsDeletedTransaction,0)<>1 "
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    '---------------PrintPO---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function PrintPO(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        If DBType = "MYSQL" Then
            str = "Select IFNULL(ITM.NetAmount,0) as NetAmount,IFNULL(ITM.TotalOverHeadAmount,0) as TotalOverHeadAmount,IFNULL(ITM.AmountInWords,0) as AmountInWords,NullIf(CM.CompanyName,'') AS CompanyName,NullIf(CM.GSTIN,'') AS GSTIN,NullIf(CM.Address1,'')+' , '+NullIf(CM.Address2,'')+' , '+NullIf(CM.City,'')+' , '+NullIf(CM.State,'')+' ,'+NullIf(CM.Country,'')+' - '+NullIf(CM.Pincode,'') As CompanyAddress,NullIf(CM.State,'') as CompanyState,NullIf(LM.GSTNo,'') AS GSTNo,NullIf(LM.Address1,'') +' , '+NullIf(LM.Address2,'') +' , '+NullIf(LM.City,'') +' , '+NullIf(LM.State,'')+' , '+NullIf(LM.Country,'') As SuppAddress,IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITM.LedgerID,0) AS LedgerID, 0 AS TransID,0 AS ItemID, 0 As ItemGroupID,NullIf(LM.LedgerName,'') AS LedgerName,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As VoucherDate,NullIf('','') AS ItemCode,NullIf('','') AS ItemName,NullIf('','') AS ItemDescription,ROUND(SUM(IFNULL(ITD.PurchaseOrderQuantity, 0)), 2) As PurchaseQuantity, Nullif('','') AS PurchaseUnit,0 AS PurchaseRate,ROUND(SUM(IFNULL(ITD.GrossAmount, 0)), 2) As GrossAmount, 0 As DiscountAmount, ROUND(SUM(IFNULL(ITD.BasicAmount, 0)), 2) As BasicAmount, 0 AS GSTPercentage,ROUND((SUM(IFNULL(ITD.CGSTAmount,0))+SUM(IFNULL(ITD.SGSTAmount,0))+SUM(IFNULL(ITD.IGSTAmount,0))),2) AS GSTTaxAmount, ROUND(SUM(IFNULL(ITD.NetAmount, 0)), 2) As NetAmount, NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy,NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,NullIf(ITM.FYear,'') AS FYear,0 AS ReceiptTransactionID,IFNULL(ITD.IsVoucherItemApproved, 0) As IsVoucherItemApproved, 0 As IsReworked, Nullif('','') AS ReworkRemark,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision ,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,0 AS RequiredQuantity,Nullif('','') AS ExpectedDeliveryDate,IFNULL(ITM.TotalTaxAmount,0) AS TotalTaxAmount,IFNULL(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount, Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,IFNULL(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,IFNULL(ITD.TaxableAmount,0) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID " &
                  " From ItemTransactionMain As ITM  INNER Join CompanyMaster as CM on CM.CompanyID=ITM.CompanyID INNER Join ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID  INNER Join UserMaster AS UA ON UA.UserID=ITM.CreatedBy And UA.CompanyID=ITM.CompanyID INNER Join LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID And LM.CompanyID=ITM.CompanyID  Left Join UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy  And UA.CompanyID=ITM.CompanyID " &
                  " Where ITM.VoucherID = -11 And ITM.CompanyID ='" & GBLCompanyID & "' And ITM.TransactionID='" & transactionID & "'  And IFNULL(ITD.IsDeletedTransaction,0)<>1 " &
                  " Group BY IFNULL(ITM.NetAmount,0),NullIf(CM.Address1,''),NullIf(CM.Address2,''),NullIf(CM.City,''),NullIf(CM.State,''),NullIf(CM.Country,''),NullIf(CM.Pincode,''), IFNULL(ITM.TransactionID, 0),IFNULL(ITM.VoucherID,0),IFNULL(ITM.LedgerID,0),NullIf(LM.Address1,''), NullIf(LM.Address2,''), NullIf(LM.GSTNo,''), NullIf(CM.GSTIN,''),NullIf(CM.CompanyName,''),NullIf(CM.Address1,'') +' , '+NullIf(CM.Address2,'') +' , '+NullIf(CM.City,'') +' , '+ NullIf(CM.State,'') +' , '+NullIf(CM.Country,'') +' - '+NullIf(CM.Pincode,''),NullIf(CM.State,''),NullIf(LM.Country,''),NullIf(LM.State,''), NullIf(LM.City,''),NullIf(LM.LedgerName,''),IFNULL(ITM.MaxVoucherNo,0),NullIf(ITM.VoucherNo,''),Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)), NullIf(IFNULL(UA.UserName,''),''),NullIf(IFNULL(UM.UserName,''),''),NullIf(ITM.FYear,''),IFNULL(ITD.IsVoucherItemApproved,0),Nullif(ITM.PurchaseReferenceRemark,''),Nullif(ITM.Narration,''),Nullif(ITM.PurchaseDivision,''),Nullif(ITM.ContactPersonID,''),IFNULL(ITM.TotalTaxAmount,0),IFNULL(ITM.TotalOverheadAmount,0),Nullif(ITM.DeliveryAddress,''),IFNULL(ITM.TotalQuantity,''),nullif(ITM.TermsOfPayment,''),IFNULL(ITD.TaxableAmount,0),nullif(ITM.ModeOfTransport ,''),nullif(ITM.DealerID,''),IFNULL(ITM.NetAmount,0),IFNULL(ITM.TotalOverHeadAmount,0),IFNULL(ITM.AmountInWords,0)  Order By NullIf(ITM.FYear,''),IFNULL(ITM.MaxVoucherNo,0) limit 1"
        Else
            str = "Select top 1 Isnull(ITM.NetAmount,0) as NetAmount,Isnull(ITM.TotalOverHeadAmount,0) as TotalOverHeadAmount,Isnull(ITM.AmountInWords,0) as AmountInWords,NullIf(CM.CompanyName,'') AS CompanyName,NullIf(CM.GSTIN,'') AS GSTIN,NullIf(CM.Address1,'') +' , '+NullIf(CM.Address2,'') +' , '+NullIf(CM.City,'') +' , '+NullIf(CM.State,'') +' , '+NullIf(CM.Country,'') +' - '+NullIf(CM.Pincode,'') As CompanyAddress,NullIf(CM.State,'') as CompanyState,  " &
               " NullIf(LM.GSTNo,'') AS GSTNo,NullIf(LM.Address1,'') +' , '+NullIf(LM.Address2,'') +' , '+NullIf(LM.City,'') +' , '+NullIf(LM.State,'') +' , '+NullIf(LM.Country,'') As SuppAddress  " &
               " ,Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITM.LedgerID,0) AS LedgerID,  " &
               " 0 AS TransID,0 AS ItemID, 0 As ItemGroupID,NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,  " &
               " NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,  " &
               " NullIf('','') AS ItemCode,NullIf('','') AS ItemName,NullIf('','') AS ItemDescription,   " &
               " ROUND(SUM(Isnull(ITD.PurchaseOrderQuantity, 0)), 2) As PurchaseQuantity, Nullif('','') AS PurchaseUnit,0 AS PurchaseRate,  " &
               " ROUND(SUM(Isnull(ITD.GrossAmount, 0)), 2) As GrossAmount, 0 As DiscountAmount, ROUND(SUM(Isnull(ITD.BasicAmount, 0)), 2) As BasicAmount,  " &
               " 0 AS GSTPercentage,ROUND((SUM(Isnull(ITD.CGSTAmount,0))+SUM(Isnull(ITD.SGSTAmount,0))+SUM(Isnull(ITD.IGSTAmount,0))),2) AS  " &
               " GSTTaxAmount, ROUND(SUM(Isnull(ITD.NetAmount, 0)), 2) As NetAmount, NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,  " &
               " NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITM.FYear,'') AS FYear,0 AS ReceiptTransactionID,  " &
               " Isnull(ITD.IsVoucherItemApproved, 0) As IsVoucherItemApproved, 0 As IsReworked, Nullif('','') AS ReworkRemark,  " &
               " Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,  " &
               " Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision ,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,0 AS RequiredQuantity,  " &
               " Nullif('','') AS ExpectedDeliveryDate,isnull(ITM.TotalTaxAmount,0) AS TotalTaxAmount,isnull(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount,  " &
               " Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,Isnull(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,Isnull(ITD.TaxableAmount,0) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID     " &
               " From ItemTransactionMain As ITM   " &
               " INNER Join CompanyMaster as CM on CM.CompanyID=ITM.CompanyID  " &
               " INNER Join ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID    " &
               " INNER Join UserMaster AS UA ON UA.UserID=ITM.CreatedBy And UA.CompanyID=ITM.CompanyID    " &
               " INNER Join LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID And LM.CompanyID=ITM.CompanyID  Left Join UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy And UA.CompanyID=ITM.CompanyID  " &
               " Where ITM.VoucherID = -11 And ITM.CompanyID ='" & GBLCompanyID & "' and ITM.TransactionID='" & transactionID & "'  AND Isnull(ITD.IsDeletedTransaction,0)<>1     " &
               " Group BY Isnull(ITM.TransactionID, 0),Isnull(ITM.VoucherID,0),Isnull(ITM.LedgerID,0),NullIf(LM.Address1,''), NullIf(LM.Address2,''), NullIf(LM.GSTNo,''), NullIf(CM.GSTIN,''),NullIf(CM.CompanyName,''),NullIf(CM.Address1,'') +' , '+NullIf(CM.Address2,'') +' , '+NullIf(CM.City,'') +' , '+NullIf(CM.State,'') +' , '+NullIf(CM.Country,'') +' - '+NullIf(CM.Pincode,''),NullIf(CM.State,''),  " &
               " NullIf(LM.Country,''),NullIf(LM.State,''), NullIf(LM.City,''),NullIf(LM.LedgerName,''),Isnull(ITM.MaxVoucherNo,0),NullIf(ITM.VoucherNo,''),Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-'), NullIf(Isnull(UA.UserName,''),''),NullIf(Isnull(UM.UserName,''),''),NullIf(ITM.FYear,''),Isnull(ITD.IsVoucherItemApproved,0),Nullif(ITM.PurchaseReferenceRemark,''),Nullif(ITM.Narration,''),Nullif(ITM.PurchaseDivision,''),Nullif(ITM.ContactPersonID,''),isnull(ITM.TotalTaxAmount,0),isnull(ITM.TotalOverheadAmount,0),Nullif(ITM.DeliveryAddress,''),Isnull(ITM.TotalQuantity,''),nullif(ITM.TermsOfPayment,''),Isnull(ITD.TaxableAmount,0),nullif(ITM.ModeOfTransport ,''),nullif(ITM.DealerID,''),Isnull(ITM.NetAmount,0),Isnull(ITM.TotalOverHeadAmount,0),Isnull(ITM.AmountInWords,0)     Order By NullIf(ITM.FYear,''),Isnull(ITM.MaxVoucherNo,0) "
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------InWordFunction---------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function InWords(ByVal TxtNetAmt As String) As String

        Dim dt As New DataTable
        Dim dtCurrency As New DataTable 'For Currency
        Dim CurrencyHeadName, CurrencyChildName, CurrencyCode, str2 As String 'For Currency
        Dim KeyField, NumberToWord As String

        Try
            CurrencyCode = "INR"
            NumberToWord = ""
            If CurrencyCode = "INR" Or CurrencyCode = "" Then
                CurrencyHeadName = ""
                CurrencyChildName = ""
                CurrencyCode = "INR"
                NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
            Else
                NumberToWord = ""
                str2 = ""
                str2 = "Select nullif(CurrencyHeadName,'') as CurrencyHeadName,Nullif(CurrencyChildName,'') as CurrencyChildName From CurrencyMaster Where CurrencyCode='" & CurrencyCode & "'"

                db.FillDataTable(dtCurrency, str2)
                Dim j As Integer = dtCurrency.Rows.Count
                If j > 0 Then
                    CurrencyHeadName = dtCurrency.Rows(0)(0)
                    CurrencyChildName = dtCurrency.Rows(0)(1)
                    NumberToWord = db.ReadNumber(TxtNetAmt, CurrencyHeadName, CurrencyChildName, CurrencyCode)
                End If
            End If

            KeyField = NumberToWord

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    '---------------Head RetrivePoCreateGrid_ForPrint code---------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function RetrivePoCreateGrid_ForPrint(ByVal transactionID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQL" Then
            str = "Select distinct IFNULL(ITM.TransactionID,0) AS PurchaseTransactionID,IFNULL(ITM.VoucherID,0) AS PurchaseVoucherID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITM.LedgerID,0) AS LedgerID,IFNULL(ITD.TransID,0) As TransID,IFNULL(ITD.ItemID,0) As ItemID,  IFNULL(ITD.ItemGroupID,0) As ItemGroupID,NullIf(LM.LedgerName,'') AS LedgerName,IFNULL(ITM.MaxVoucherNo,0) AS PurchaseMaxVoucherNo,IFNULL(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS PurchaseVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As PurchaseVoucherDate,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(IFNULL(IM.ItemName,''),'') AS ItemName,NullIf(IFNULL(IM.ItemDescription,''),'') AS ItemDescription, IFNULL(ITD.RequiredQuantity,0) AS RequisitionQty,IFNULL(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,  IFNULL(ITD.PurchaseUnit,'') AS PurchaseUnit,IFNULL(ITD.PurchaseRate,0) AS PurchaseRate, IFNULL(ITD.GrossAmount,0) AS BasicAmount,IFNULL(ITD.DiscountPercentage,0) AS Disc, IFNULL(ITD.DiscountAmount,0) AS DiscountAmount,IFNULL(ITD.BasicAmount,0) AS AfterDisAmt,IFNULL(ITD.PurchaseTolerance,0) AS Tolerance, IFNULL(ITD.GSTPercentage,0) AS GSTTaxPercentage,(IFNULL(ITD.CGSTAmount,0)+IFNULL(ITD.SGSTAmount,0)+IFNULL(ITD.IGSTAmount,0)) AS GSTTaxAmount,IFNULL(ITD.NetAmount,0) AS TotalAmount,NullIf(IFNULL(UA.UserName,''),'') AS CreatedBy,NullIf(IFNULL(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,0 AS ReceiptTransactionID,IFNULL(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ExpectedDeliveryDate,Nullif(IM.StockUnit,'') AS StockUnit,IFNULL(ITD.CGSTPercentage,0) as CGSTTaxPercentage,IFNULL(ITD.SGSTPercentage,0) as SGSTTaxPercentage,IFNULL(ITD.IGSTPercentage,0) as IGSTTaxPercentage , IFNULL(ITD.CGSTAmount,0) as CGSTAmt,  IFNULL(ITD.SGSTAmount,0) as SGSTAmt,IFNULL(ITD.IGSTAmount,0) as IGSTAmt , IFNULL(ITD.TaxableAmount,0) AS TaxableAmount, Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As ExpectedDeliveryDate,Nullif(PHM.ProductHSNName,'') AS ProductHSNName,Nullif(PHM.HSNCode,'') AS HSNCode,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,Nullif(C.ConversionFormula,'') AS  ConversionFormula,IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace " &
                  " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID And ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=ITD.CompanyID INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID And LM.CompanyID=ITM.CompanyID LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=ITD.PurchaseUnit And C.CompanyID=ITD.CompanyID LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID=IM.ProductHSNID AND PHM.CompanyID=IM.CompanyID  " &
                  " Where ITM.VoucherID= -11 And ITM.CompanyID = '" & GBLCompanyID & "' and ITD.TransactionID='" & transactionID & "'  And IFNULL(ITD.IsDeletedTransaction,0)<>1 Order By TransID"
        Else
            str = "Select distinct Isnull(ITM.TransactionID,0) AS PurchaseTransactionID,Isnull(ITM.VoucherID,0) AS PurchaseVoucherID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITM.LedgerID,0) AS LedgerID,  " &
                    "Isnull(ITD.TransID,0) As TransID,Isnull(ITD.ItemID,0) As ItemID,  Isnull(ITD.ItemGroupID,0) As ItemGroupID,  " &
                    "NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS PurchaseMaxVoucherNo,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,  " &
                   " NullIf(ITM.VoucherNo,'') AS PurchaseVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,   " &
                   " Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS PurchaseVoucherDate,  " &
                   " Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate, NullIf(IM.ItemCode,'') AS ItemCode,  " &
                   " NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,   " &
                   " NullIf(Isnull(IM.ItemName,''),'') AS ItemName,NullIf(Isnull(IM.ItemDescription,''),'') AS ItemDescription, Isnull(ITD.RequiredQuantity,0) AS RequisitionQty,   " &
                   " Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,  Isnull(ITD.PurchaseUnit,'') AS PurchaseUnit,  " &
                   " Isnull(ITD.PurchaseRate,0) AS PurchaseRate, Isnull(ITD.GrossAmount,0) AS BasicAmount,Isnull(ITD.DiscountPercentage,0) AS Disc,  " &
                   " Isnull(ITD.DiscountAmount,0) AS DiscountAmount,Isnull(ITD.BasicAmount,0) AS AfterDisAmt,  " &
                   " Isnull(ITD.PurchaseTolerance,0) AS Tolerance, Isnull(ITD.GSTPercentage,0) AS GSTTaxPercentage,  " &
                   " (Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,  " &
                   " Isnull(ITD.NetAmount,0) AS TotalAmount,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,   " &
                   " NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,0 AS ReceiptTransactionID,   " &
                   " Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark,   " &
                   " Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,  	  " &
                   " Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,   " &
                   " Nullif(IM.StockUnit,'') AS StockUnit,Isnull(ITD.CGSTPercentage,0) as CGSTTaxPercentage,  " &
                   " Isnull(ITD.SGSTPercentage,0) as SGSTTaxPercentage,Isnull(ITD.IGSTPercentage,0) as IGSTTaxPercentage ,   " &
                   " Isnull(ITD.CGSTAmount,0) as CGSTAmt,  Isnull(ITD.SGSTAmount,0) as SGSTAmt,Isnull(ITD.IGSTAmount,0) as IGSTAmt ,Isnull(ITD.TaxableAmount,0) AS TaxableAmount,  " &
                   " Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate,  " &
                   " Nullif(PHM.ProductHSNName,'') AS ProductHSNName,Nullif(PHM.HSNCode,'') AS HSNCode,Isnull(IM.WtPerPacking,0) AS WtPerPacking,  " &
                   " Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,  " &
                   " Nullif(C.ConversionFormula,'') AS  ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace  " &
                   " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID   " &
                   " And ITM.CompanyID=ITD.CompanyID INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID   " &
                   " INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID " &
                   " INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy  " &
                   " INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID  " &
                   " LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy   " &
                   " LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0		  " &
                   " LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=IM.StockUnit AND C.ConvertedUnitSymbol=ITD.PurchaseUnit  " &
                   " LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID=IM.ProductHSNID  Where ITM.VoucherID= -11 and ITD.TransactionID='" & transactionID & "'  AND Isnull(ITD.IsDeletedTransaction,0)<>1 Order By TransID "
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Open Master code---------------------------------
    '-----------------------------------Get Pending Requisition List Grid------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SupplierItemRates(ByVal itemIds As String, ByVal supID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        If itemIds <> "" And supID <> "" Then
            If DBType = "MYSQL" Then
                str = "Select Distinct IFNULL(ITM.TransactionID,0) AS TransactionID,IFNULL(ITD.TransID,0) AS TransID,IFNULL(ITM.VoucherID,0) AS VoucherID,IFNULL(ITD.ItemID,0) AS ItemID,IFNULL(IM.ItemGroupID,0) AS ItemGroupID,IFNULL(IM.ItemSubGroupID,0) AS ItemSubGroupID,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,IFNULL(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Convert(date_format(IfNULL(ITM.VoucherDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(13)) As VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IM.ItemName,'') AS ItemName,    Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,IFNULL(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration, NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,   (IFNULL(ITD.RequiredQuantity, 0) - IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail  Where IFNULL(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0))  As PurchaseQuantityComp,(IFNULL(ITD.RequiredQuantity, 0) - IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))   From ItemPurchaseRequisitionDetail Where IFNULL(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID  And CompanyID = ITD.CompanyID),0)) As PurchaseQuantity,IFNULL(Nullif(IM.PurchaseRate,''),0) as PurchaseRate,  nullif(IM.PurchaseUnit,'') as PurchaseUnit,  nullif(PHM.ProductHSNName,'') as ProductHSNName,Convert(date_format(IfNULL(ITD.ExpectedDeliveryDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As ExpectedDeliveryDate,nullif(PHM.HSNCode,'') as HSNCode, IFNULL(PHM.GSTTaxPercentage,0) as GSTTaxPercentage, IFNULL(PHM.CGSTTaxPercentage,0) as CGSTTaxPercentage, IFNULL(PHM.SGSTTaxPercentage,0) as SGSTTaxPercentage, IFNULL(PHM.IGSTTaxPercentage ,0) as IGSTTaxPercentage  ,IFNULL(IM.WtPerPacking,0) AS WtPerPacking,IFNULL(IM.UnitPerPacking,1) AS UnitPerPacking,IFNULL(IM.ConversionFactor,1) AS ConversionFactor,IFNULL(Nullif(IM.SizeW,''),0) AS SizeW,IFNULL(IGM.ItemGroupNameID,0) AS ItemGroupNameID,Nullif(C.ConversionFormula,'') AS  ConversionFormula,IFNULL(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace " &
                      " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=IM.CompanyID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy And UA.CompanyID=ITM.CompanyID LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID =IM.ProductHSNID And PHM.CompanyID=IM.CompanyID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=ITD.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit And C.CompanyID=ITD.CompanyID " &
                      " Where IFNULL(ITM.VoucherID, 0) = -9 And ITM.CompanyID = " & GBLCompanyID & " And IFNULL(ITM.IsDeletedTransaction,0)=0 And IFNULL(ITD.IsVoucherItemApproved,0)=1 And (IFNULL(ITD.RequiredQuantity, 0) - IFNULL((Select Sum(IFNULL(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where IFNULL(IsDeletedTransaction, 0)=0    And RequisitionTransactionID=ITD.TransactionID And ItemID=ITD.ItemID And CompanyID=ITD.CompanyID),0))>0 Order By FYear Desc,MaxVoucherNo Desc,TransID"
            Else
                str = " Select Distinct Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITD.ItemID,0) AS ItemID,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(IM.ItemSubGroupID,0) AS ItemSubGroupID,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,Isnull(ITM.MaxVoucherNo,0) As MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo,Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,Nullif(IGM.ItemGroupName,'') AS ItemGroupName,Nullif(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,Nullif(IM.ItemCode,'') AS ItemCode,Nullif(IM.ItemName,'') AS ItemName,    Nullif(IM.ItemDescription,'') AS ItemDescription,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Isnull(ITD.RequiredQuantity,0) AS RequiredQuantity,NullIf(ITD.StockUnit,'') AS StockUnit,NullIf(ITD.ItemNarration,'') AS ItemNarration,     " &
                " NullIf(ITM.Narration,'') AS Narration,NullIf(ITM.FYear,'') AS FYear,NullIf(UA.UserName,'') AS CreatedBy,  (Isnull(ITD.RequiredQuantity, 0) - Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantityComp,(Isnull(ITD.RequiredQuantity, 0) - Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0) = 0 And RequisitionTransactionID = ITD.TransactionID And ItemID = ITD.ItemID And CompanyID = ITD.CompanyID),0)) As PurchaseQuantity,Isnull(Nullif(IM.PurchaseRate,''),0) as PurchaseRate,  nullif(IM.PurchaseUnit,'') as PurchaseUnit, nullif(PHM.ProductHSNName,'') as ProductHSNName,  " &
                " replace(convert(nvarchar(30),ITD.ExpectedDeliveryDate,106),'','-') AS ExpectedDeliveryDate,nullif(PHM.HSNCode,'') as HSNCode, isnull(PHM.GSTTaxPercentage,0) as GSTTaxPercentage, isnull(PHM.CGSTTaxPercentage,0) as CGSTTaxPercentage, isnull(PHM.SGSTTaxPercentage,0) as SGSTTaxPercentage, isnull(PHM.IGSTTaxPercentage ,0) as IGSTTaxPercentage  ,Isnull(IM.WtPerPacking,0) AS WtPerPacking,Isnull(IM.UnitPerPacking,1) AS UnitPerPacking,Isnull(IM.ConversionFactor,1) AS ConversionFactor,Isnull(Nullif(IM.SizeW,''),0) AS SizeW,Isnull(IGM.ItemGroupNameID,0) AS ItemGroupNameID,Nullif(C.ConversionFormula,'') AS  ConversionFormula,Isnull(C.ConvertedUnitDecimalPlace,0) AS UnitDecimalPlace   " &
                " From ItemTransactionMain As ITM INNER JOIN ItemTransactionDetail As ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID INNER JOIN ItemMaster As IM ON IM.ItemID=ITD.ItemID INNER JOIN ItemGroupMaster As IGM ON IGM.ItemGroupID=IM.ItemGroupID INNER JOIN UserMaster As UA ON UA.UserID=ITM.CreatedBy " &
                " LEFT JOIN ProductHSNMaster As PHM ON PHM.ProductHSNID =IM.ProductHSNID LEFT JOIN ItemSubGroupMaster As ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN ConversionMaster As C ON C.BaseUnitSymbol=ITD.StockUnit AND C.ConvertedUnitSymbol=IM.PurchaseUnit  " &
                " Where Isnull(ITM.VoucherID, 0) = -9 AND Isnull(ITM.IsDeletedTransaction,0)=0 AND Isnull(ITD.IsVoucherItemApproved,0)=1 And (Isnull(ITD.RequiredQuantity, 0) - Isnull((Select Sum(Isnull(RequisitionProcessQuantity, 0))  From ItemPurchaseRequisitionDetail Where Isnull(IsDeletedTransaction, 0)=0    And RequisitionTransactionID=ITD.TransactionID And ItemID=ITD.ItemID And CompanyID=ITD.CompanyID),0))>0    " &
                " Order By FYear Desc,MaxVoucherNo Desc,TransID "
            End If
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    ''Get Currency Code List From Database
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetCurrencyList() As String

        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQl" Then
            str = "Select Distinct CurrencyCode From CurrencyMaster Where IFNULL(CurrencyCode,'')<>'' Order by CurrencyCode"
        Else
            str = " Select Distinct CurrencyCode From CurrencyMaster Where Isnull(CurrencyCode,'')<>'' Order by CurrencyCode"
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    ''Get POApprovalBy List From Database
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetPOApprovalBy() As String

        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        If DBType = "MYSQl" Then
            str = "Select Distinct IFNULL(LM.LedgerID,0) as LedgerID,Nullif(LM.LedgerName,'') as LedgerName  From LedgerMaster As LM where LM.IsLedgerActive=1  And IFNULL(LM.IsDeletedTransaction,0)<>1 AND  LM.LedgerGroupID IN(Select Distinct LedgerGroupID From LedgerGroupMaster Where LedgerGroupNameID=27 AND CompanyID='" & GBLCompanyID & "')"
        Else
            str = "Select Distinct Isnull(LM.LedgerID,0) as LedgerID,Nullif(LM.LedgerName,'') as LedgerName  From LedgerMaster As LM where LM.IsLedgerActive=1  And Isnull(LM.IsDeletedTransaction,0)<>1 AND  LM.LedgerGroupID IN(Select Distinct LedgerGroupID From LedgerGroupMaster Where LedgerGroupNameID=27)"
        End If


        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    '--------------- Get Requisition and purchase order Comment Data---------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetCommentData(ByVal PurchaseTransactionID As String, ByVal requisitionIDs As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        If DBType = "MYSQl" Then
            If PurchaseTransactionID <> "0" Then
                str = " CALL GetCommentData( " & GBLCompanyID & ",'Purchase Order',0," & PurchaseTransactionID & ",0,0,0,0,0,0);"
            Else
                str = " CALL GetCommentData( " & GBLCompanyID & ",'Purchase Order','" & requisitionIDs & "',0,0,0,0,0,0);"
            End If
        Else
            If PurchaseTransactionID <> "0" Then
                str = " EXEC GetCommentData " & GBLCompanyID & ",'Purchase Order',0," & PurchaseTransactionID & ",0,0,0,0,0,0"
            Else
                str = " EXEC GetCommentData " & GBLCompanyID & ",'Purchase Order','" & requisitionIDs & "',0,0,0,0,0,0"
            End If
        End If
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function


    ''----------------------------Save Comment Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveCommentData(ByVal jsonObjectCommentDetail As Object) As String

        Dim dt As New DataTable
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
            If DBType = "MYSQl" Then
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

    ''Get GetAllHSN List From Database
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetAllHSN() As String

        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        str = ""

        If DBType = "MYSQl" Then
            str = "Select Distinct IFNULL(ProductHSNID,0) as ProductHSNID,nullif(HSNCode,'') as HSNCode,nullif(ProductHSNName,'') as ProductHSNName,IFNULL(GSTTaxPercentage,0) As GSTTaxPercentage,IFNULL(CGSTTaxPercentage,0) As CGSTTaxPercentage,IFNULL(SGSTTaxPercentage,0) As SGSTTaxPercentage,IFNULL(IGSTTaxPercentage,0) as IGSTTaxPercentage " &
                  " From ProductHSNMaster Where CompanyID='" & GBLCompanyID & "' AND  IFNULL(IsDeletedTransaction,0)=0 AND ProductCategory='Raw Material' Order By nullif(ProductHSNName,'') asc"
        Else
            str = "Select Distinct Isnull(ProductHSNID,0) as ProductHSNID,nullif(HSNCode,'') as HSNCode,nullif(ProductHSNName,'') as ProductHSNName,  " &
               " Isnull(GSTTaxPercentage,0) As GSTTaxPercentage,Isnull(CGSTTaxPercentage,0) As CGSTTaxPercentage,Isnull(SGSTTaxPercentage,0) As SGSTTaxPercentage,Isnull(IGSTTaxPercentage,0) as IGSTTaxPercentage   " &
               "  From ProductHSNMaster Where   Isnull(IsDeletedTransaction,0)=0 AND ProductCategory='Raw Material' Order By nullif(ProductHSNName,'') asc "
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetLastTransactionDate() As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Dim Str As String = ""
        Dim lastTransactionDate As String = ""
        Dim whereCondition As String = " VoucherID=-11 AND ProductionUnitID=" & ProductionUnitID & " AND Isnull(IsDeletedTransaction,0)=0 "

        Str = db.getLastVoucherDate("ItemTransactionMain", "VoucherDate", whereCondition)
        Dim parsedDate As DateTime
        If DateTime.TryParse(Str, parsedDate) Then
            lastTransactionDate = parsedDate.ToString("dd-MMM-yyyy")
        End If
        Return js.Serialize(lastTransactionDate)
    End Function

    '-----------------------------------Get Process Requisition List Grid------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function OldPOHistoryGrid(ByVal fromDateValue As String, ByVal ToDateValue As String, ByVal LedgerID As String) As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"
        Dim dateString As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("ReportFYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))


        If DBType = "MYSQL" Then
            str = ""
        Else
            str = " Select NULLIF(UTL.RecordID,'') AS RecordID ,NULLIF(UTL.Details,'') AS Details ,Isnull(ITM.TransactionID,0) AS TransactionID,Isnull(ITM.VoucherID,0) AS VoucherID,Isnull(ITM.LedgerID,0) AS LedgerID,Isnull(ITD.TransID,0) AS TransID,Isnull(ITD.ItemID,0) AS ItemID, Isnull(IM.ItemGroupID,0) As ItemGroupID,Isnull(IGM.ItemGroupNameID,0) As ItemGroupNameID,Isnull(IM.ItemSubGroupID,0) As ItemSubGroupID,NullIf(LM.LedgerName,'') AS LedgerName,Isnull(ITM.MaxVoucherNo,0) AS MaxVoucherNo,NullIf(ITM.VoucherNo,'') AS VoucherNo, Replace(Convert(Varchar(13),ITM.VoucherDate,106),' ','-') AS VoucherDate,NullIf(IM.ItemCode,'') AS ItemCode,NullIf(IGM.ItemGroupName,'') AS ItemGroupName,NullIf(ISGM.ItemSubGroupName,'') AS ItemSubGroupName,NullIf(Isnull(IM.ItemName,''),'') AS ItemName,NullIf(Isnull(IM.ItemDescription,''),'') AS ItemDescription, Isnull(ITD.PurchaseOrderQuantity,0) AS PurchaseQuantity,Isnull(ITD.PurchaseUnit,'') AS PurchaseUnit,Isnull(ITD.PurchaseRate,0) AS PurchaseRate,Isnull(ITD.GrossAmount,0) AS GrossAmount,Isnull(ITD.DiscountAmount,0) AS DiscountAmount,Isnull(ITD.BasicAmount,0) AS BasicAmount,Isnull(ITD.GSTPercentage,0) AS GSTPercentage, " &
            " (Isnull(ITD.CGSTAmount,0)+Isnull(ITD.SGSTAmount,0)+Isnull(ITD.IGSTAmount,0)) AS GSTTaxAmount,Isnull(ITD.NetAmount,0) AS NetAmount,NullIf(Isnull(UA.UserName,''),'') AS CreatedBy,NullIf(Isnull(UM.UserName,''),'') AS ApprovedBy,NullIf(ITD.FYear,'') AS FYear,Isnull((Select Top 1 TransactionID From ItemTransactionDetail Where PurchaseTransactionID=ITM.TransactionID AND CompanyID=ITD.CompanyID AND Isnull(IsDeletedTransaction,0)<>1 AND Isnull(PurchaseTransactionID,0)>0),0) AS ReceiptTransactionID,Isnull(ITD.IsVoucherItemApproved,0) AS IsVoucherItemApproved, 0 AS IsReworked,Nullif('','') AS ReworkRemark,Nullif(ITD.RefJobBookingJobCardContentsID,'') AS RefJobBookingJobCardContentsID,Nullif(ITD.RefJobCardContentNo,'') AS RefJobCardContentNo,Nullif(ITM.PurchaseReferenceRemark,'') AS PurchaseReference,Nullif(ITM.Narration,'') AS Narration,Nullif(ITM.PurchaseDivision,'') AS PurchaseDivision,Nullif(ITM.ContactPersonID,'') AS ContactPersonID,(Select ROUND(Sum(Isnull(RequisitionProcessQuantity,0)),2) From ItemPurchaseRequisitionDetail Where TransactionID=ITD.TransactionID AND ItemID=ITD.ItemID AND CompanyID=ITD.CompanyID) AS RequiredQuantity,Replace(Convert(Varchar(13),ITD.ExpectedDeliveryDate,106),' ','-') AS ExpectedDeliveryDate, " &
            " Isnull(ITM.TotalTaxAmount,0) AS TotalTaxAmount,Isnull(ITM.TotalOverheadAmount,0) AS TotalOverheadAmount,Nullif(ITM.DeliveryAddress,'') as DeliveryAddress,Isnull(ITM.TotalQuantity,'') as TotalQuantity,nullif(ITM.TermsOfPayment,'') as TermsOfPayment,Isnull(ITD.TaxableAmount,0) AS TaxableAmount,nullif(ITM.ModeOfTransport ,'') as ModeOfTransport ,nullif(ITM.DealerID,'') as DealerID,Isnull(ITD.IsvoucherItemApproved,0) AS VoucherItemApproved,Isnull(ITD.IsCancelled,0) AS VoucherCancelled,Isnull(NullIf(ITM.CurrencyCode,''),'INR') AS CurrencyCode,Isnull(ITM.VoucherApprovalByEmployeeID,0) AS VoucherApprovalByEmployeeID,ISNULL(ITD.PurchaseOrderQuantity, 0)-ISNULL((SELECT Case When IGM.ItemGroupNameID=-1 And (Upper(ITD.PurchaseUnit)='KG' OR Upper(ITD.PurchaseUnit)='KGS') And (Upper(ITD.StockUnit)='SHEET' OR Upper(ITD.StockUnit)='SHEETS') Then Round(SUM(ChallanQuantity*ReceiptWtPerPacking),3) Else SUM(ChallanQuantity) End AS Expr1 FROM ItemTransactionDetail WHERE (PurchaseTransactionID = ITM.TransactionID) AND (CompanyID = ITD.CompanyID) AND (ISNULL(IsDeletedTransaction, 0) <> 1) And (ItemID=ITD.ItemID) AND (ISNULL(PurchaseTransactionID, 0) > 0)), 0) AS PendingToReceiveQty  " &
            " From ItemTransactionMain AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITD.TransactionID=ITM.TransactionID And ITD.CompanyID=ITM.CompanyID  INNER JOIN UserMaster AS UA ON UA.UserID=ITM.CreatedBy AND UA.CompanyID=ITM.CompanyID  INNER JOIN ItemMaster AS IM ON IM.ItemID=ITD.ItemID And IM.CompanyID=ITD.CompanyID  INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID And IGM.CompanyID=IM.CompanyID  INNER JOIN LedgerMaster AS LM ON LM.LedgerID=ITM.LedgerID AND LM.CompanyID=ITM.CompanyID LEFT JOIN (SELECT RecordID, Details, CompanyID,CreatedDate  FROM (SELECT *,ROW_NUMBER() OVER (PARTITION BY RecordID ORDER BY CreatedDate  DESC) AS rn FROM UserTransactionLogs) AS LatestLogs WHERE rn = 1) AS UTL ON UTL.RecordID = ITD.TransactionID AND UTL.CompanyID = ITM.CompanyID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID And ISGM.CompanyID=IM.CompanyID AND Isnull(ISGM.IsDeletedTransaction,0)<>1 LEFT JOIN UserMaster AS UM ON UM.UserID=ITD.VoucherItemApprovedBy AND UA.CompanyID=ITM.CompanyID  " &
            " Where ITM.VoucherID= -11 /*And ITM.FYear='" & GBLFYear & "'*/ And ITM.CompanyID=" & GBLCompanyID & " and   ITM.LedgerID=" & LedgerID & " AND ITM.VoucherDate BETWEEN '" & fromDateValue & "' AND '" & ToDateValue & "' AND Isnull(ITD.IsDeletedTransaction,0)<>1 Order By FYear Desc,MaxVoucherNo Desc,TransID "
        End If

        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        js.MaxJsonLength = 2147483647
        Return js.Serialize(data.Message)
    End Function

    '---------------Close Master code---------------------------------

    Public Class HelloWorldData
        Public Message As [String]
    End Class

End Class