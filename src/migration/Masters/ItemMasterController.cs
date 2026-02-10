using IndusWebApi.Models;
using IndusWebApi.ValidateRequest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Policy;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace IndusWebApi.Controllers.Masters
{
    [Validate]
    [RoutePrefix("api/itemmaster")]
    public class ItemMasterController : ApiController
    {
        DataTable Dt = new DataTable();
        string str = "";
        string Errmsg = "";
        int ItemGroupId = 0;
        int i = 1;
        private SqlDataAdapter DA;
        private DataTable dataTable = new DataTable();
        private JavaScriptSerializer js = new JavaScriptSerializer();
        private HelloWorldData data = new HelloWorldData();
        private string GBLUserID;
        private string GBLUserName;
        private string GBLBranchID;
        private string GBLCompanyID;
        private string GBLFYear;
        private string DBType = "";
        private string ProductionUnitIDStr = "";
        private string ProductionUnitID = "";

        private void InitializeSessionValues()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
            GBLUserName = Convert.ToString(HttpContext.Current.Session["UserName"]);
            GBLFYear = Convert.ToString(HttpContext.Current.Session["FYear"]);
            GBLBranchID = Convert.ToString(HttpContext.Current.Session["BranchId"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            ProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);
            ProductionUnitIDStr = Convert.ToString(HttpContext.Current.Session["ProductionUnitIDStr"]);
        }

        private string ConvertDataTableTojSonString(DataTable dataTable)
        {
            var serializer = new JavaScriptSerializer
            {
                MaxJsonLength = 2147483647
            };
            var tableRows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dataTable.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                tableRows.Add(row);
            }
            return serializer.Serialize(tableRows);
        }

        private string DataSetToJSONWithJavaScriptSerializer(DataSet dataset)
        {
            var jsSerializer = new JavaScriptSerializer();
            var ssvalue = new Dictionary<string, object>();

            foreach (DataTable table in dataset.Tables)
            {
                var parentRow = new List<Dictionary<string, object>>();
                var tablename = table.TableName;

                foreach (DataRow row in table.Rows)
                {
                    var childRow = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        childRow.Add(col.ColumnName, row[col]);
                    }
                    parentRow.Add(childRow);
                }
                ssvalue.Add(tablename, parentRow);
            }
            return jsSerializer.Serialize(ssvalue);
        }

        private DataTable ConvertObjectToDatatableNEW(object jsonObject)
        {
            try
            {
                var st = JsonConvert.SerializeObject(jsonObject);
                return JsonConvert.DeserializeObject<DataTable>(st);
            }
            catch (Exception ex)
            {
                str = ex.Message;
                return new DataTable();
            }
        }

        [HttpGet]
        [Route("itemmasterlist")]
        public IHttpActionResult MasterList()
        {
            InitializeSessionValues();

            str = "SELECT Distinct IGM.ItemGroupID,IGM.ItemGroupName,nullif(GridColumnName,'') as GridColumnName,nullif(GridColumnHide,'') as GridColumnHide FROM ItemGroupMaster As IGM " +
                  "Inner Join UserSubModuleAuthentication As UMA On UMA.ItemGroupID=IGM.ItemGroupID " +
                  "And UMA.CompanyID=IGM.CompanyID And UMA.CanView=1 " +
                  "Where IGM.IsDeletedTransaction=0 And UMA.UserID=" + GBLUserID + " Order By IGM.ItemGroupID";

            DBConnection.FillDataTable(ref dataTable, str);
            //data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(dataTable);
        }

        [HttpGet]
        [Route("grid/{masterID}")]
        public IHttpActionResult MasterGrid(string masterID)
        {
            InitializeSessionValues();

            var dt = new DataTable();
            js.MaxJsonLength = 2147483647;

            var str2 = "Select nullif(SelectQuery,'') as SelectQuery From ItemGroupMaster " +
                      "Where ItemGroupID='" + masterID + "' and isnull(IsDeletedTransaction,0)<>1";

            DBConnection.FillDataTable(ref dt, str2);
            var i = dt.Rows.Count;

            if (i > 0)
            {
                if (DBNull.Value.Equals(dt.Rows[0][0]))
                {
                    return Ok(js.Serialize(""));
                }
                else
                {
                    var query = dt.Rows[0][0].ToString();
                    try
                    {
                        if (query.ToUpper().Contains("EXECUTE"))
                        {
                            str = query + " '' " + "," + masterID;
                        }
                        else
                        {
                            str = query;
                        }
                        DBConnection.FillDataTable(ref dataTable, str);
                        data.Message = ConvertDataTableTojSonString(dataTable);
                    }
                    catch (Exception ex)
                    {
                        return Ok(ex.Message);
                    }
                }
            }
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("grid-column-hide/{masterID}")]
        public IHttpActionResult MasterGridColumnHide(string masterID)
        {
            InitializeSessionValues();

            str = "SELECT nullif(GridColumnHide,'') as GridColumnHide,nullif(TabName,'') as TabName," +
                  "nullif(ItemNameFormula,'') as ItemNameFormula,nullif(ItemDescriptionFormula,'') as ItemDescriptionFormula " +
                  "FROM ItemGroupMaster Where ItemGroupID= '" + masterID + "' and isnull(IsDeletedTransaction,0)<>1";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("grid-column/{masterID}")]
        public IHttpActionResult MasterGridColumn(string masterID)
        {
            InitializeSessionValues();

            str = "select nullif(GridColumnName,'') as GridColumnName From ItemGroupMaster " +
                  "Where ItemGroupID='" + masterID + "' and isnull(IsDeletedTransaction,0)<>1";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpPost]
        [Route("save")]
        public IHttpActionResult SaveData([FromBody] SaveDataRequest request)
        {
            InitializeSessionValues();

            var dt = new DataTable();
            string keyField;
            string itemID;
            string addColName, addColValue, tableName;

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                //if (DuplicateItemGroupValidate(request.ItemGroupID, request.CostingDataItemDetailMaster))
                //{
                //    return Ok("Duplicate data found");
                //}`

                if (!DBConnection.CheckAuthories("/master/item", Convert.ToInt16(GBLUserID), Convert.ToInt16(GBLCompanyID), "CanSave"))
                    return Ok("You are not authorized to save");

                if (!(bool)Convert.ToBoolean(DBConnection.GetColumnValue("CanSave", "UserSubModuleAuthentication", " UserID=" + GBLUserID + " And ItemGroupID=" + request.ItemGroupID)))
                    return Ok("You are not authorized to save..!");

                str = "Select Nullif(StockRefCode,'') as StockRefCode from ItemMaster " +
                      "where StockRefCode = '" + request.StockRefCode + "' And ItemGroupID=" + request.ItemGroupID +
                      " And IsDeletedTransacstion = 0";
                DBConnection.FillDataTable(ref dt, str);

                if (dt.Rows.Count > 0 && !DBNull.Value.Equals(dt.Rows[0][0]))
                {
                    return Ok("Stock Ref. Code already exists.");
                }

                var dtItemCode = new DataTable();
                int maxItemID = 0;
                string itemCodePrefix, itemCode;

                if (Convert.ToInt32(request.CostingDataItemMaster[0]["ItemGroupID"]) == 1)
                {
                    request.CostingDataItemMaster[0]["ItemSubGroupID"] = -1;
                }
                else if (Convert.ToInt32(request.CostingDataItemMaster[0]["ItemGroupID"]) == 2)
                {
                    request.CostingDataItemMaster[0]["ItemSubGroupID"] = -2;
                }

                var itemCodestr2 = "Select nullif(ItemGroupPrefix,'') as ItemGroupPrefix from ItemGroupMaster " +
                                  "Where ItemGroupID=" + request.ItemGroupID + " and isnull(IsDeletedTransaction,0)<>1";
                DBConnection.FillDataTable(ref dtItemCode, itemCodestr2);
                itemCodePrefix = dtItemCode.Rows[0][0].ToString();

                itemCode = DBConnection.GeneratePrefixedNo("ItemMaster", itemCodePrefix, "MaxItemNo", ref maxItemID, "", " Where ItemCodeprefix='" + itemCodePrefix + "' And Isnull(IsDeletedTransaction,0)=0 ");

                tableName = "ItemMaster";
                addColName = "CreatedDate,UserID,CompanyID,FYear,CreatedBy,ItemCode,ItemCodePrefix,MaxItemNo";
                addColValue = "Getdate()," + GBLUserID + "," + GBLCompanyID + ",'" + GBLFYear + "'," +
                             GBLUserID + ",'" + itemCode + "','" + itemCodePrefix + "'," + maxItemID;

                itemID = DBConnection.InsertDatatableToDatabaseWithouttrans(request.CostingDataItemMaster, tableName, addColName, addColValue);

                if (!IsNumeric(itemID))
                {
                    return Ok("fail " + itemID);
                }

                if (string.IsNullOrEmpty(itemID))
                    return Ok("Error in main " + itemID);

                //tableName = "ItemMasterDetails";
                //addColName = "CreatedDate,UserID,CompanyID,ItemID,FYear,CreatedBy,ModifiedBy";
                //addColValue = "Getdate()," + GBLUserID + "," + GBLCompanyID + "," + itemID + ",'" +
                //             GBLFYear + "'," + GBLUserID + "," + GBLUserID;

                //DBConnection.InsertDatatableToDatabase(request.CostingDataItemDetailMaster, tableName, addColName, addColValue);

                //str = "Insert into ItemMasterDetails (ModifiedDate,CreatedDate,UserID,CompanyID,ItemID,FYear," +
                //      "CreatedBy,ModifiedBy,FieldValue,ParentFieldValue,ParentFieldName,FieldName,ItemGroupID) " +
                //      "values(Getdate(),Getdate()," + GBLUserID + "," + GBLCompanyID + "," + itemID + ",'" +
                //      GBLFYear + "'," + GBLUserID + "," + GBLUserID + ",'" + request.ActiveItem + "','" +
                //      request.ActiveItem + "','ISItemActive','ISItemActive','" + request.ItemGroupID + "')";

                //DBConnection.ExecuteNonSQLQuery(str);
                //DBConnection.ExecuteNonSQLQuery("EXEC [UpdateItemMasterValuesMultiUnit] " + itemID);
                //DBConnection.ExecuteNonSQLQuery("EXEC [InsertItemMasterStockValues] " + itemID);

                keyField = "Success";
            }
            catch (Exception ex)
            {
                keyField = "fail " + ex.Message;
            }
            return Ok(keyField);
        }

        [HttpPost]
        [Route("update")]
        public IHttpActionResult UpdateData([FromBody] UpdateDataRequest request)
        {
            InitializeSessionValues();

            var dt = new DataTable();
            string keyField;
            string addColName, wherecndtn, tableName;

            if (!DBConnection.CheckAuthories("Masters.aspx", Convert.ToInt16(GBLUserID), Convert.ToInt16(GBLCompanyID), "CanEdit"))
                return Ok("You are not authorized to update");

            if (!(bool)Convert.ToBoolean(DBConnection.GetColumnValue("CanEdit", "UserSubModuleAuthentication", " UserID=" + GBLUserID + " And ItemGroupID=" + request.UnderGroupID)))
                return Ok("You are not authorized to update..!");

            str = "Select Nullif(StockRefCode,'') as StockRefCode from ItemMaster " +
                  "where StockRefCode = '" + request.StockRefCode + "' And ItemID <> '" + request.ItemID +
                  "' And ItemGroupID=" + request.UnderGroupID + " And ISNULL(IsDeletedTransaction,0) = 0";

            DBConnection.FillDataTable(ref dt, str);
            if (dt.Rows.Count > 0 && !DBNull.Value.Equals(dt.Rows[0][0]))
            {
                return Ok("Stock Ref. Code already exists.");
            }

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                using (var con = DBConnection.OpenConnection())
                {
                    tableName = "ItemMaster";
                    addColName = "ModifiedDate=Getdate(),UserID=" + GBLUserID + ",CompanyID=" + GBLCompanyID +
                                ",ModifiedBy=" + GBLUserID;
                    wherecndtn = "ItemID=" + request.ItemID + " And ItemGroupID=" + request.UnderGroupID;

                    DBConnection.UpdateDatatableToDatabase(request.CostingDataItemMaster, tableName, addColName, 0, wherecndtn);

                    //var someSpcelCaseColName = "ModifiedDate,CreatedDate,UserID,CompanyID,ItemID,FYear,CreatedBy,ModifiedBy";
                    //var someSpcelCaseColValue = "Getdate(),Getdate()," + GBLUserID + "," + GBLCompanyID + "," +
                    //                           request.ItemID + ",'" + GBLFYear + "'," + GBLUserID + "," + GBLUserID;

                    //tableName = "ItemMasterDetails";
                    //addColName = "ModifiedDate=Getdate(),UserID=" + GBLUserID + ",CompanyID=" + GBLCompanyID +
                    //            ",ModifiedBy=" + GBLUserID;
                    //wherecndtn = "ItemID=" + request.ItemID + " And ItemGroupID=" + request.UnderGroupID;

                    //var dtDetail = ConvertObjectToDatatableNEW(request.CostingDataItemDetailMaster);

                    //// Process detail records (simplified version of the complex update logic)
                    //ProcessDetailRecords(con, dtDetail, tableName, addColName, wherecndtn, someSpcelCaseColName,
                    //    someSpcelCaseColValue, request.ItemID, request.UnderGroupID, request.ActiveItem);

                    con.Close();
                }
                keyField = "Success";
            }
            catch (Exception ex)
            {
                keyField = "fail";
            }
            return Ok(keyField);
        }

        [HttpPost]
        [Route("deleteitem/{itemID}/{itemgroupID}")]
        public IHttpActionResult DeleteData(string itemID, string itemgroupID)
        {
            InitializeSessionValues();

            string keyField;

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                if (!DBConnection.CheckAuthories("Masters.aspx", Convert.ToInt16(GBLUserID), Convert.ToInt16(GBLCompanyID), "CanDelete"))
                    return Ok("You are not authorized to delete");

                if (!(bool)Convert.ToBoolean(DBConnection.GetColumnValue("CanDelete", "UserSubModuleAuthentication", " UserID=" + GBLUserID + " And ItemGroupID=" + itemgroupID + " And CompanyID=" + GBLCompanyID)))
                    return Ok("You are not authorized to delete..!");

                str = "Update ItemMaster Set DeletedBy=" + GBLUserID +
                      ",DeletedDate=Getdate() ,IsDeletedTransaction=1 WHERE ItemID=" + itemID +
                      " And ItemGroupID=" + itemgroupID;

                keyField = DBConnection.ExecuteNonSQLQuery(str);
            }
            catch (Exception ex)
            {
                keyField = "fail " + ex.Message;
            }
            return Ok(keyField);
        }

        [HttpGet]
        [Route("check-permission/{transactionID}")]
        public IHttpActionResult CheckPermission(string transactionID)
        {
            InitializeSessionValues();

            string keyField = "";
            try
            {
                var dtExist = new DataTable();
                var dtExist1 = new DataTable();
                string sxistStr;

                var d1 = "";
                var d2 = "";

                sxistStr = "select Top 1 Isnull(ITD.ItemID,0) as ItemID From ItemTransactionMain AS ITM " +
                          "INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID " +
                          "Where ITM.CompanyID=" + GBLCompanyID + " And ITM.VoucherID<>-8 And ITD.ItemID= '" +
                          transactionID + "' and isnull(ITM.IsDeletedTransaction,0)<>1";

                DBConnection.FillDataTable(ref dtExist, sxistStr);
                var e = dtExist.Rows.Count;
                if (e > 0)
                {
                    d1 = dtExist.Rows[0][0].ToString();
                }

                sxistStr = "Select * From ItemTransactionDetail Where Isnull(IsDeletedTransaction, 0) = 0 " +
                          "And isnull(QCApprovalNo,'')<>'' AND TransactionID=" + transactionID +
                          " AND (Isnull(ApprovedQuantity,0)>0 OR Isnull(RejectedQuantity,0)>0)";

                DBConnection.FillDataTable(ref dtExist1, sxistStr);
                var f = dtExist1.Rows.Count;
                if (f > 0)
                {
                    d2 = dtExist1.Rows[0][0].ToString();
                }

                if (!string.IsNullOrEmpty(d1) || !string.IsNullOrEmpty(d2))
                {
                    keyField = "Exist";
                }
            }
            catch (Exception)
            {
                keyField = "fail";
            }
            return Ok(keyField);
        }

        [HttpGet]
        [Route("loaded-data/{masterID}/{itemId}")]
        public IHttpActionResult MasterGridLoadedData(string masterID, string itemId)
        {
            InitializeSessionValues();

            var selQ = "execute SelectedRowMultiUnit '',";
            str = selQ + masterID + "," + itemId;

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("drill-down/{masterID}/{tabID}")]
        public IHttpActionResult DrillDownMasterGrid(string masterID, string tabID)
        {
            InitializeSessionValues();

            var dt = new DataTable();
            var str2 = "Select nullif(SelectQuery,'') as SelectQuery From DrilDown " +
                      "Where ItemGroupID='" + masterID + "' and TabName='" + tabID +
                      "' and isnull(IsDeletedTransaction,0)<>1";

            DBConnection.FillDataTable(ref dt, str2);
            var i = dt.Rows.Count;

            if (i > 0)
            {
                if (DBNull.Value.Equals(dt.Rows[0][0]))
                {
                    return Ok(js.Serialize(""));
                }
                else
                {
                    var query = dt.Rows[0][0].ToString();
                    try
                    {
                        if (query.ToUpper().Contains("EXECUTE"))
                        {
                            str = query + " '' " + "," + masterID;
                        }
                        else
                        {
                            str = query;
                        }
                        DBConnection.FillDataTable(ref dataTable, str);
                        data.Message = ConvertDataTableTojSonString(dataTable);
                    }
                    catch (Exception ex)
                    {
                        return Ok(ex.Message);
                    }
                }
            }
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("getmasterfields/{masterID}")]
        public IHttpActionResult Master(string masterID)
        {
            InitializeSessionValues();

            str = "SELECT Distinct nullif(ItemGroupFieldID,'') as ItemGroupFieldID,nullif(ItemGroupID,'') as ItemGroupID," +
                  "nullif(FieldName,'') as FieldName,nullif(FieldDataType,'') as FieldDataType," +
                  "nullif(FieldDescription,'') as FieldDescription,nullif(IsDisplay,'') as IsDisplay," +
                  "nullif(IsCalculated,'') as IsCalculated,nullif(FieldFormula,'') as FieldFormula," +
                  "nullif(FieldTabIndex,'') as FieldTabIndex,nullif(FieldDrawSequence,'') as FieldDrawSequence," +
                  "nullif(FieldDefaultValue,'') as FieldDefaultValue,nullif(CompanyID,'') as CompanyID," +
                  "nullif(UserID,'') as UserID,nullif(ModifiedDate,'') as ModifiedDate,nullif(FYear,'') as FYear," +
                  "nullif(IsActive,'') as IsActive,nullif(IsDeleted,'') as IsDeleted," +
                  "nullif(FieldDisplayName,'') as FieldDisplayName,nullif(FieldType,'') as FieldType," +
                  "nullif(SelectBoxQueryDB,'') as SelectBoxQueryDB,nullif(SelectBoxDefault,'') as SelectBoxDefault," +
                  "nullif(ControllValidation,'') as ControllValidation,nullif(FieldFormulaString,'') as FieldFormulaString," +
                  "nullif(IsRequiredFieldValidator,'') as IsRequiredFieldValidator,nullif(UnitMeasurement,'') as UnitMeasurement," +
                  "IsLocked,Isnull(MinimumValue,0) AS MinimumValue,Isnull(MaximumValue,0) AS MaximumValue " +
                  "FROM ItemGroupFieldMaster Where ItemGroupID='" + masterID +
                  "' and isnull(IsDeletedTransaction,0)<>1 Order By FieldDrawSequence";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        internal static DataTable ConvertJArrayToDataTables(JArray jsonArray)
        {
            DataTable dataTable = new DataTable();

            if (jsonArray != null && jsonArray.Count > 0)
            {
                foreach (JObject item in jsonArray)
                {
                    // Fill DataTable for each object in the array
                    DBConnection.FillDataTableFromJObject(dataTable, item);
                }
            }
            return dataTable;
        }


        [HttpPost]
        [Route("selectboxload")]
        public IHttpActionResult SelectBoxLoad([FromBody] JArray jsonData)
        {
            InitializeSessionValues();
            DataTable DT_JSON_Data = ConvertJArrayToDataTables(jsonData);

            var ds = new DataSet();

            try
            {
                for (int i = 0; i < DT_JSON_Data.Rows.Count; i++)
                {
                    string qs = Convert.ToString(DT_JSON_Data.Rows[i]["FieldID"]);
                    string si = Convert.ToString(DT_JSON_Data.Rows[i]["FieldName"]);

                    var dtNew = new DataTable();
                    string str3 = "";

                    str3 = "Select nullif(SelectboxQueryDB,'') as SelectboxQueryDB From ItemGroupFieldMaster " +
                           "Where ItemGroupFieldID=" + qs + " and isnull(IsDeletedTransaction,0)<>1";

                    DBConnection.FillDataTable(ref dtNew, str3);

                    if (dtNew.Rows.Count == 0 || DBNull.Value.Equals(dtNew.Rows[0][0]))
                        return Ok(js.Serialize(""));

                    string qsQuery = dtNew.Rows[0][0].ToString();
                    qsQuery = qsQuery.Replace("#", "'");

                    if (string.IsNullOrEmpty(qsQuery))
                        return Ok(js.Serialize(""));

                    string str;
                    if (DBType == "MYSQL" && qsQuery.ToUpper().Contains("CALL "))
                        str = qsQuery + ");";
                    else
                        str = qsQuery;

                    var dt = new DataTable();
                    DBConnection.FillDataTable(ref dt, str);
                    if (dt.Columns.Count == 2)
                    {
                        if (dt.Rows.Count > 0)
                            dt.Rows.Add(dt.Rows[0][dt.Columns[0].ColumnName], si);
                        //else
                        //    dt.Rows.Add(0, si);
                    }
                    //else if (dt.Columns.Count == 1)
                    //{
                    //    dt.Rows.Add(si);
                    //}
                    //else
                    //{
                    //    dt.Columns.Add(si, typeof(string));
                    //    dt.Rows.Add(si);
                    //}
                    dt.TableName = "tbl_" + si;
                    ds.Tables.Add(dt.Copy());
                }

                // Step 5: Convert dataset to JSON and return
                data.Message = DataSetToJSONWithJavaScriptSerializer(ds);
                return Ok(js.Serialize(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }



        [HttpGet]
        [Route("under-group")]
        public IHttpActionResult GetUnderGroup()
        {
            InitializeSessionValues();

            str = "Select distinct ItemSubGroupID,ItemSubGroupDisplayName from ItemSubGroupMaster";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("group")]
        public IHttpActionResult GetGroup()
        {
            InitializeSessionValues();

            str = "Select distinct ISGM.ItemSubGroupUniqueID,ISGM.ItemSubGroupID,ISGM.ItemSubGroupDisplayName," +
                  "ISGM.UnderSubGroupID,ISGM.ItemSubGroupName,ISGM.ItemSubGroupLevel," +
                  "(select top 1 ItemSubGroupDisplayName from ItemSubGroupMaster where ItemSubGroupID=ISGM.UnderSubGroupID) as GroupName " +
                  "from ItemSubGroupMaster as ISGM where ISGM.ProductionUnitID IN(" + ProductionUnitIDStr +
                  ") And Isnull(IsDeletedTransaction,0)<>1";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpPost]
        [Route("save-group")]
        public IHttpActionResult SaveGroupData([FromBody] SaveGroupDataRequest request)
        {
            InitializeSessionValues();

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                var dtExist = new DataTable();
                var str2 = "select distinct nullif(ItemSubGroupName,'') as ItemSubGroupName " +
                          "from ItemSubGroupMaster where ItemSubGroupName= '" + request.GroupName + "' " +
                          "and isnull(IsDeletedTransaction,0)<>1";

                DBConnection.FillDataTable(ref dtExist, str2);
                var e = dtExist.Rows.Count;

                if (e > 0)
                {
                    return Ok("Exist");
                }
                else
                {
                    var dt1 = new DataTable();
                    str2 = "Select isnull(max(ItemSubGroupID),0) + 1 As ItemSubGroupID From ItemSubGroupMaster " +
                          "Where Isnull(IsDeletedTransaction,0)<>1";

                    DBConnection.FillDataTable(ref dt1, str2);
                    var itemSubGroupID = dt1.Rows[0][0];

                    var dt2 = new DataTable();
                    str2 = "Select isnull(ItemSubGroupLevel,0) ItemSubGroupLevel From ItemSubGroupMaster " +
                          "Where ItemSubGroupID = '" + request.UnderGroupID + "' and Isnull(IsDeletedTransaction,0)<>1";

                    DBConnection.FillDataTable(ref dt2, str2);
                    var k = dt2.Rows.Count;
                    var groupLevel = k + 1;

                    var tableName = "ItemSubGroupMaster";
                    var addColName = "ModifiedDate,CreatedDate,UserID,CompanyID,ItemSubGroupID,FYear,CreatedBy,ModifiedBy,ItemSubGroupLevel,ProductionUnitID";
                    var addColValue = "Getdate(),Getdate()," + GBLUserID + "," + GBLCompanyID + ",'" +
                                     itemSubGroupID + "','" + GBLFYear + "'," + GBLUserID + "," + GBLUserID +
                                     ",'" + groupLevel + "','" + ProductionUnitID + "'";

                    DBConnection.InsertDatatableToDatabaseWithouttrans(request.CostingDataGroupMaster, tableName, addColName, addColValue);
                    return Ok("Success");
                }
            }
            catch (Exception)
            {
                return Ok("fail");
            }
        }

        [HttpPost]
        [Route("update-group")]
        public IHttpActionResult UpdatGroupData([FromBody] UpdateGroupDataRequest request)
        {
            InitializeSessionValues();

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                var dtExist = new DataTable();
                var str2 = "select distinct nullif(ItemSubGroupName,'') as ItemSubGroupName " +
                          "from ItemSubGroupMaster where ItemSubGroupName= '" + request.GroupName +
                          "' AND ItemSubGroupUniqueID<>" + request.ItemSubGroupUniqueID +
                          " and isnull(IsDeletedTransaction,0)<>1";

                DBConnection.FillDataTable(ref dtExist, str2);
                var e = dtExist.Rows.Count;

                if (e > 0)
                {
                    return Ok("Exist");
                }
                else
                {
                    var tableName = "ItemSubGroupMaster";
                    var addColName = "ModifiedDate=Getdate(),UserID=" + GBLUserID + ",CompanyID=" + GBLCompanyID +
                                    ",ModifiedBy=" + GBLUserID + ",ItemSubGroupLevel='" + request.ItemSubGroupLevel +
                                    "',ProductionUnitID='" + ProductionUnitID + "'";
                    var wherecndtn = "ItemSubGroupUniqueID=" + request.ItemSubGroupUniqueID;

                    DBConnection.UpdateDatatableToDatabase(request.CostingDataGroupMaster, tableName, addColName, 0, wherecndtn);
                    return Ok("Success");
                }
            }
            catch (Exception)
            {
                return Ok("fail");
            }
        }

        [HttpPost]
        [Route("delete-group")]
        public IHttpActionResult DeleteGroupMasterData([FromBody] DeleteGroupDataRequest request)
        {
            InitializeSessionValues();

            var canCrud = DBConnection.validateProductionUnit(GBLUserID, "Save");
            if (canCrud != "Authorize")
            {
                return Ok(canCrud);
            }

            try
            {
                using (var con = DBConnection.OpenConnection())
                {
                    str = "Update ItemSubGroupMaster Set ModifiedBy=" + GBLUserID + ",DeletedBy=" + GBLUserID +
                          ",DeletedDate=Getdate(),ModifiedDate=Getdate(),IsDeletedTransaction=1 " +
                          "WHERE ItemSubGroupUniqueID='" + request.ItemSubGroupUniqueID + "'";

                    using (var cmd = new SqlCommand(str, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok("Success");
            }
            catch (Exception)
            {
                return Ok("fail");
            }
        }

        [HttpGet]
        [Route("items")]
        public IHttpActionResult GetItem()
        {
            InitializeSessionValues();

            str = "Select ItemGroupID,ItemGroupName from ItemGroupMaster where ISNULL(IsDeleted,0)=0";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("ledgers")]
        public IHttpActionResult GetLedger()
        {
            InitializeSessionValues();

            str = "Select LedgerGroupID,LedgerGroupName from LedgerGroupMaster " +
                  "where CompanyID = " + GBLCompanyID + " And ISNULL(IsDeleted,0)=0";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = ConvertDataTableTojSonString(dataTable);
            return Ok(js.Serialize(data.Message));
        }

        [HttpGet]
        [Route("check-permission-update/{itemID}")]
        public IHttpActionResult CheckPermissionforUpdate(string itemID)
        {
            InitializeSessionValues();

            try
            {
                // JobBookingContent
                if (!DBConnection.IsDeletable("paperID", "JobBookingContents", "Where IsDeletedTransaction=0 And paperID=" + itemID))
                {
                    return Ok("Exist");
                }

                // JobBookingJobCardContents
                if (!DBConnection.IsDeletable("paperID", "JobBookingJobCardContents", "Where IsDeletedTransaction=0 And paperID=" + itemID))
                {
                    return Ok("Exist");
                }

                // ProductMasterContents
                if (!DBConnection.IsDeletable("paperID", "ProductMasterContents", "Where IsDeletedTransaction=0 And paperID=" + itemID))
                {
                    return Ok("Exist");
                }

                // ItemTransactionDetail
                if (!DBConnection.IsDeletable("ITD.ItemID", "ItemTransactionDetail AS ITM INNER JOIN ItemTransactionDetail AS ITD ON ITM.TransactionID=ITD.TransactionID",
                    " Where Isnull(ITM.IsDeletedTransaction,0)=0 AND ITM.VoucherID<>-8 And ITD.ItemID=" + itemID))
                {
                    return Ok("Exist");
                }

                return Ok("Success");
            }
            catch (Exception)
            {
                return Ok("fail");
            }
        }

        [HttpPost]
        [Route("update-user")]
        public IHttpActionResult UpdateUserData([FromBody] UpdateUserDataRequest request)
        {
            InitializeSessionValues();

            var dt = new DataTable();
            string keyField;

            str = "Select Nullif(StockRefCode,'') as StockRefCode from ItemMaster " +
                  "where StockRefCode = '" + request.StockRefCode + "' And ItemID <> '" + request.ItemID +
                  "' And IsDeletedTransaction = 0";
            DBConnection.FillDataTable(ref dt, str);

            if (dt.Rows.Count > 0 && !DBNull.Value.Equals(dt.Rows[0][0]))
            {
                return Ok("Stock Ref. Code already exists.");
            }

            try
            {
                var tableName = "ItemMaster";
                var addColName = "ModifiedDate=Getdate(),UserID=" + GBLUserID + ",ModifiedBy=" + GBLUserID;
                var wherecndtn = "ItemID='" + request.ItemID + "'";

                keyField = DBConnection.UpdateDatatableToDatabase(request.ItemName, tableName, addColName, 0, wherecndtn);
                if (keyField != "Success")
                {
                    return Ok(keyField);
                }

                //str = "Update ItemMasterDetails set ParentFieldValue = (Select PurchaseRate from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select PurchaseRate from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'PurchaseRate'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                //str = "Update ItemMasterDetails set ParentFieldValue = (Select EstimationRate from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select EstimationRate from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'EstimationRate'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                //str = "Update ItemMasterDetails Set ParentFieldValue = (Select Top 1 ProductHSNID from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select Top 1 ProductHSNID from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'ProductHSNID'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                //str = "Update ItemMasterDetails Set ParentFieldValue = (Select Top 1 MinimumStockQty from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select Top 1 MinimumStockQty from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'MinimumStockQty'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                //str = "Update ItemMasterDetails Set ParentFieldValue = (Select Top 1 StockRefCode from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select Top 1 StockRefCode from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'StockRefCode'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                //str = "Update ItemMasterDetails Set ParentFieldValue = (Select Top 1 PurchaseOrderQuantity from ItemMaster where ItemID='" +
                //      request.ItemID + "'),FieldValue=(Select Top 1 PurchaseOrderQuantity from ItemMaster where ItemID='" +
                //      request.ItemID + "') Where ItemID='" + request.ItemID + "' And ParentFieldName = 'PurchaseOrderQuantity'";
                //keyField = DBConnection.ExecuteNonSQLQuery(str);
                //if (keyField != "Success")
                //{
                //    return Ok(keyField);
                //}

                return Ok("Success");
            }
            catch (Exception)
            {
                return Ok("fail");
            }
        }

        // Helper Methods
        private bool DuplicateItemGroupValidate(string tableID, object tblObj)
        {
            try
            {
                InitializeSessionValues();

                var dtExist = new DataTable();
                var str2 = "Select nullif(SaveAsString,'') as SaveAsString From ItemGroupMaster " +
                          "Where ItemGroupID='" + tableID + "' and Isnull(IsDeletedTransaction,0)<>1";

                DBConnection.FillDataTable(ref dtExist, str2);

                if (dtExist.Rows.Count > 0)
                {
                    if (DBNull.Value.Equals(dtExist.Rows[0][0]))
                    {
                        return false;
                    }
                    else
                    {
                        var getColumn = dtExist.Rows[0][0].ToString();
                        var dt = ConvertObjectToDatatableNEW(tblObj);
                        var colValue = "";
                        var brakCount = "";

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (getColumn.Contains(dt.Rows[i]["FieldName"].ToString()))
                            {
                                if (string.IsNullOrEmpty(colValue))
                                {
                                    colValue = " And Isnull(IsDeletedTransaction,0)<>1 And " + dt.Rows[i]["FieldName"] + " = '" + dt.Rows[i]["FieldValue"] + "'";
                                }
                                else
                                {
                                    colValue = colValue + " And Isnull(IsDeletedTransaction,0)<>1 And " + dt.Rows[i]["FieldName"] + " = '" + dt.Rows[i]["FieldValue"] + "'";
                                }
                            }
                        }

                        str2 = "Select Distinct ItemID From ItemMaster Where ItemGroupID=" + tableID + colValue;
                        dtExist = new DataTable();
                        DBConnection.FillDataTable(ref dtExist, str2);

                        if (dtExist.Rows.Count > 0)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        //private void ProcessDetailRecords(SqlConnection con, DataTable dt, string tableName, string addColName, string wherecndtn, string someSpcelCaseColName, string someSpcelCaseColValue, string itemID, string underGroupID, string activeItem)
        //{
        //    // Simplified version of the complex detail record processing logic
        //    // This would contain the full implementation from the VB.NET version
        //    // For brevity, including key operations only

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        var updateStr = "Update " + tableName + " Set " + addColName + " Where " + wherecndtn;

        //        if (con.State == ConnectionState.Closed)
        //        {
        //            con.Open();
        //        }

        //        using (var cmd = new SqlCommand(updateStr, con))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        //    // Handle ISItemActive field
        //    var activeItemUpdate = "Update ItemMasterDetails Set ModifiedDate=Getdate(),UserID=" + GBLUserID + ",CompanyID=" + GBLCompanyID + ",ModifiedBy=" + GBLUserID + ",FieldValue='" +
        //                          activeItem + "',ParentFieldValue='" + activeItem + "' Where ParentFieldName='ISItemActive' and FieldName='ISItemActive' and ItemID=" + itemID + " and ItemGroupID='" + underGroupID + "'";

        //    if (con.State == ConnectionState.Closed)
        //    {
        //        con.Open();
        //    }

        //    using (var cmdA = new SqlCommand(activeItemUpdate, con))
        //    {
        //        cmdA.CommandType = CommandType.Text;
        //        cmdA.ExecuteNonQuery();
        //    }
        //}

        private static bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }
    }
    // Request/Response Models
    public class SaveDataRequest
    {
        public Dictionary<string, object>[] CostingDataItemMaster { get; set; }
        public string MasterName { get; set; }
        public string ItemGroupID { get; set; }
        public string ActiveItem { get; set; }
        public string StockRefCode { get; set; }
    }

    public class UpdateDataRequest
    {
        public Dictionary<string, object>[] CostingDataItemMaster { get; set; }
        public Dictionary<string, object>[] CostingDataItemDetailMaster { get; set; }
        public string MasterName { get; set; }
        public string ItemID { get; set; }
        public string UnderGroupID { get; set; }
        public string ActiveItem { get; set; }
        public string StockRefCode { get; set; }
    }

    public class DeleteDataRequest
    {
        public string txtGetGridRow { get; set; }
        public string MasterName { get; set; }
        public string UnderGroupID { get; set; }
    }

    public class SaveGroupDataRequest
    {
        public Dictionary<string, object>[] CostingDataGroupMaster { get; set; }
        public string GroupName { get; set; }
        public string UnderGroupID { get; set; }
    }

    public class UpdateGroupDataRequest
    {
        public Dictionary<string, object>[] CostingDataGroupMaster { get; set; }
        public string ItemSubGroupUniqueID { get; set; }
        public string ItemSubGroupLevel { get; set; }
        public string GroupName { get; set; }
    }

    public class DeleteGroupDataRequest
    {
        public string ItemSubGroupUniqueID { get; set; }
    }

    public class UpdateUserDataRequest
    {
        public Dictionary<string, object>[] ItemName { get; set; }
        public string ItemID { get; set; }
        public string StockRefCode { get; set; }
    }

    public class HelloWorldData
    {
        public string Message { get; set; }
    }
}