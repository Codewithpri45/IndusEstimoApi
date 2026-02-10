Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Services
Imports System.Web.Script.Serialization
Imports Connection
Imports MySql.Data.MySqlClient

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
<System.Web.Script.Services.ScriptService()>
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class WebService_WarehouseMaster
    Inherits System.Web.Services.WebService

    Private ReadOnly DA As SqlDataAdapter
    ReadOnly db As New DBConnection
    ReadOnly FYear As String
    ReadOnly js As New JavaScriptSerializer()
    ReadOnly data As New HelloWorldData()
    Dim dataTable As New DataTable()
    Dim str As String

    Dim GBLUserID As String
    Dim GBLUserName As String
    Dim GBLCompanyID As String
    Dim GBLFYear As String
    Dim UserName As String
    Dim DBType As String = ""
    Dim ProductionUnitIDStr As String = ""
    Dim ProductionUnitID As String = ""
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetWarehouseNo() As String

        Dim MaxWarehouseCode As Long
        Dim KeyField As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLUserName = Convert.ToString(HttpContext.Current.Session("UserName"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))

        Try
            KeyField = db.GeneratePrefixedNo("WarehouseMaster", "WH", "MaxWarehouseCode", MaxWarehouseCode, "", " Where  /*CompanyID=" & GBLCompanyID & " AND*/ Isnull(IsDeletedTransaction,0) = 0")


        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField
    End Function
    <System.Web.Services.WebMethod(EnableSession:=True)>
    <ScriptMethod(UseHttpGet:=True, ResponseFormat:=ResponseFormat.Json)>
    Public Sub HelloWorld()

        'GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
        'GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))

        'GBLUserName = Convert.ToString(HttpContext.Current.Session("UserName"))
        'GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        'GBLBranchID = Convert.ToString(HttpContext.Current.Session("BranchId"))

        'data.Message = ConvertDataTableTojSonString(GetDataTable)
        'Context.Response.Write(js.Serialize(data.Message))
    End Sub

    <WebMethod()>
    <ScriptMethod()>
    Public Function ConvertDataTableTojSonString(ByVal dataTable As DataTable) As String
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

    '-----------------------------------Get City List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetCity() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
            GBLUserName = Convert.ToString(HttpContext.Current.Session("UserName"))
            GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            If DBType = "MYSQL" Then
                str = "Select distinct nullif(City,'') as City From CountryStateMaster Where IFNULL(City,'')<>'' And CompanyID= '" & GBLCompanyID & "' and IFNULL(IsDeletedTransaction, 0) <> 1"
            Else
                str = "Select distinct nullif(City,'') as City From CountryStateMaster Where Isnull(City,'')<>'' and Isnull(IsDeletedTransaction, 0) <> 1"
            End If

            db.FillDataTable(dataTable, str)
            data.Message = ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    ''----------------------------Open Warehouse  Save Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveWarehouse(ByVal prefix As String, ByVal jsonObjectsSaveRecord As Object) As String

        Dim dt As New DataTable

        Dim KeyField As String
        Dim AddColName, AddColValue, TableName As String
        AddColName = ""
        AddColValue = ""
        Dim MaxWarehouseCode As Long
        Dim WarehouseCode As String
        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        GBLUserName = Convert.ToString(HttpContext.Current.Session("UserName"))
        GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If

        Try
            'Dim con As New SqlConnection
            'con = db.OpenDataBase()
            WarehouseCode = db.GeneratePrefixedNo("WarehouseMaster", "WH", "MaxWarehouseCode", MaxWarehouseCode, "", " Where  CompanyID=" & GBLCompanyID & " AND Isnull(IsDeletedTransaction,0) = 0")

            If jsonObjectsSaveRecord.length > 0 Then
                TableName = "WarehouseMaster"
                AddColName = ""
                AddColValue = ""
                AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy,Warehouseprefix,MaxWarehouseCode,WarehouseCode"
                If DBType = "MYSQL" Then
                    AddColValue = "Now(),Now(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "'," & GBLUserID & ",'" & prefix & "','" & MaxWarehouseCode & "','" & WarehouseCode & "'"
                Else
                    AddColValue = "Getdate(),Getdate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "'," & GBLUserID & ",'" & prefix & "','" & MaxWarehouseCode & "','" & WarehouseCode & "'"
                End If
                KeyField = db.InsertDatatableToDatabase(jsonObjectsSaveRecord, TableName, AddColName, AddColValue)

            End If


            'con.Close()
            KeyField = "Success"

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    ''----------------------------Open RTS  Update Data  ------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpdateWarehouse(ByVal jsonObjectsSaveRecord As Object, ByVal jsonObjectsUpdateRecord As Object) As String
        Dim KeyField As String
        Dim AddColName, wherecndtn, TableName, AddColValue As String

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
        ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
        Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
        If CanCrud <> "Authorize" Then
            Return CanCrud
        End If

        Try
            If jsonObjectsUpdateRecord.length > 0 Then
                TableName = "WarehouseMaster"
                AddColName = "ModifiedDate=Getdate(),ModifiedBy=" & GBLUserID
                wherecndtn = "ProductionUnitID=" & ProductionUnitID & " "
                db.UpdateDatatableToDatabase(jsonObjectsUpdateRecord, TableName, AddColName, 1, wherecndtn)
            End If

            If jsonObjectsSaveRecord.length > 0 Then
                TableName = "WarehouseMaster"
                AddColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy"
                AddColValue = "Getdate(),Getdate(),'" & GBLUserID & "','" & GBLCompanyID & "','" & GBLFYear & "','" & GBLUserID & "'"
                db.InsertDatatableToDatabase(jsonObjectsSaveRecord, TableName, AddColName, AddColValue)
            End If
            KeyField = "Success"
        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    '-----------------------------------Get Warehouse List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ShowListWarehouseMaster() As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
            Dim dtUser As New DataTable
            Dim strFilter As String = ""

            str = "Select Isnull(CanAccessMultipleProductionUnitData,0) AS CanAccessMultipleProductionUnitData,Isnull(ProductionUnitID,0) AS ProductionUnitID From UserMaster Where UserID=" & GBLUserID & ""
            db.FillDataTable(dtUser, str)
            If dtUser.Rows.Count > 0 Then
                If dtUser.Rows(0)("CanAccessMultipleProductionUnitData") = False Then
                    strFilter = " AND PUM.ProductionUnitID=" & dtUser.Rows(0)("ProductionUnitID") & ""
                Else
                    strFilter = ""
                End If
            End If
            dtUser.Dispose()

            If DBType = "MYSQL" Then
                str = "Select Distinct WarehouseName, nullif(City,'') As City,WarehouseRefCode, nullif(Address,'') As Address,Convert(date_format(IfNULL(ModifiedDate,CURRENT_TIMESTAMP),'%d-%b-%Y'),char(30)) As ModifiedDate " &
                      " From WarehouseMaster Where CompanyID=" & GBLCompanyID & " And IFNULL(IsDeletedTransaction,0)=0"
            Else
                str = "Select Distinct  BM.BranchName ,BM.BranchID,WM.RefWarehouseCode,Isnull(WM.IsFloorWarehouse,0) AS IsFloorWarehouse,PUM.ProductionUnitName,PUM.ProductionUnitID,WM.WarehouseName,WM.WarehouseCode, nullif(WM.City,'') As City, nullif(WM.Address,'') As Address,replace(convert(nvarchar(30),WM.ModifiedDate,106),'','-') as ModifiedDate From WarehouseMaster as WM  Inner  join  ProductionUnitMaster as PUM on PUM.ProductionUnitID = WM.ProductionUnitID left join BranchMaster as BM on BM.BranchID = WM.BranchID Where WM.ProductionUnitID IN(" & ProductionUnitIDStr & ") And isnull(WM.IsDeletedTransaction,0)=0 "
            End If

            db.FillDataTable(dataTable, str)
            data.Message = ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------Get SelectedRow List------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectBinName(ByVal WarehouseName As String) As String
        Try
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
            GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
            GBLUserName = Convert.ToString(HttpContext.Current.Session("UserName"))
            GBLFYear = Convert.ToString(HttpContext.Current.Session("FYear"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

            If DBType = "MYSQL" Then
                str = "select IFNULL(WarehouseID,0) as WarehouseID,nullif(BinName,'') as BinName from WarehouseMaster where WarehouseName='" & WarehouseName & "'  And  IFNULL(IsDeletedTransaction,0)<>1"
            Else
                str = "select isnull(WarehouseID,0) as WarehouseID,nullif(BinName,'') as BinName from WarehouseMaster where WarehouseName='" & WarehouseName & "'  And  isnull(IsDeletedTransaction,0)<>1"
            End If

            db.FillDataTable(dataTable, str)
            data.Message = ConvertDataTableTojSonString(dataTable)
            Return js.Serialize(data.Message)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    '-----------------------------------CheckPermission------------------------------------------
    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function CheckPermission(ByVal GetRowWarehouseID As String) As String
        Dim KeyField As String
        Try

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session("UserCompanyID"))
            DBType = Convert.ToString(HttpContext.Current.Session("DBType"))
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session("ProductionUnitID"))
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session("ProductionUnitIDStr"))
            Dim CanCrud = db.validateProductionUnit(GBLUserID, "Save")
            If CanCrud <> "Authorize" Then
                Return CanCrud
            End If

            Dim dtExist As New DataTable
            Dim dtExist1 As New DataTable
            Dim SxistStr As String

            If DBType = "MYSQL" Then
                SxistStr = "Select IFNULL(WarehouseID,0) as WarehouseID From ItemTransactionDetail Where WarehouseID = '" & GetRowWarehouseID & "' And  IFNULL(IsDeletedTransaction,0)<>1 and CompanyID='" & GBLCompanyID & "' LIMIT 1"
            Else
                SxistStr = "Select top 1 isnull(WarehouseID,0) as WarehouseID From ItemTransactionDetail Where WarehouseID = '" & GetRowWarehouseID & "' And  isnull(IsDeletedTransaction,0)<>1"
            End If

            db.FillDataTable(dtExist1, SxistStr)
            Dim F As Integer = dtExist1.Rows.Count
            If F > 0 Then
                KeyField = "Exist"
            Else
                str = ""
                If DBType = "MYSQL" Then
                    Dim con As New MySqlConnection

                    con = db.OpenDataBaseMYSQL()
                    str = "Update WarehouseMaster Set ModifiedBy='" & GBLUserID & "',DeletedBy='" & GBLUserID & "',DeletedDate=Now(),ModifiedDate=Now(),IsDeletedTransaction=1  WHERE  WarehouseID = '" & GetRowWarehouseID & "'"

                    Dim cmd As New MySqlCommand(str, con)
                    cmd.CommandType = CommandType.Text
                    cmd.Connection = con
                    cmd.ExecuteNonQuery()
                    con.Close()
                Else
                    Dim con As New SqlConnection

                    con = db.OpenDataBase()
                    str = "Update WarehouseMaster Set ModifiedBy='" & GBLUserID & "',DeletedBy='" & GBLUserID & "',DeletedDate=Getdate(),ModifiedDate=Getdate(),IsDeletedTransaction=1  WHERE WarehouseID = '" & GetRowWarehouseID & "'"

                    Dim cmd As New SqlCommand(str, con)
                    cmd.CommandType = CommandType.Text
                    cmd.Connection = con
                    cmd.ExecuteNonQuery()
                    con.Close()
                End If



                KeyField = "Success"
            End If
            KeyField = KeyField

        Catch ex As Exception
            KeyField = "fail"
        End Try
        Return KeyField

    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetBranch() As String
        Context.Response.Clear()
        Context.Response.ContentType = "application/json"

        GBLCompanyID = Convert.ToString(HttpContext.Current.Session("CompanyID"))
        GBLUserID = Convert.ToString(HttpContext.Current.Session("UserID"))
        DBType = Convert.ToString(HttpContext.Current.Session("DBType"))

        str = "select * from BranchMaster where ISNULL(IsDeletedTransaction,0) <>1"
        db.FillDataTable(dataTable, str)
        data.Message = ConvertDataTableTojSonString(dataTable)
        Return js.Serialize(data.Message)
    End Function
    '---------------Close Master code---------------------------------

    Public Class HelloWorldData
        Public Message As [String]
    End Class

End Class