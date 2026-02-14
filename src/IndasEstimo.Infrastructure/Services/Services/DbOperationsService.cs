using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using IndasEstimo.Infrastructure.Database;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Infrastructure.Extensions;
using Dapper;

namespace IndasEstimo.Infrastructure.Services;

public class DbOperationsService : IDbOperationsService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DbOperationsService> _logger;

    // AES Encryption Keys (same as old implementation)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("9fxB7wYoJ6KD3E5pRQZha8T0r2NSclXq");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("Df7Gh2Kl5Np8Rs1T");

    public DbOperationsService(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService,
        ILogger<DbOperationsService> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    // Helper method to get tenant connection
    private SqlConnection GetTenantConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    // Helper method to get current company ID from JWT claims
    private int GetCurrentCompanyId()
    {
        return _currentUserService.GetCompanyId() ?? 0;
    }

    #region Database Query Operations

    //public string ExecuteNonSQLQuery(string query)
    //{
    //    using var connection = GetTenantConnection();
    //    try
    //    {
    //        if (connection.State == ConnectionState.Closed)
    //        {
    //            connection.Open();
    //        }
    //        SqlCommand cmd = new SqlCommand(query, connection);
    //        // cmd.LogQuery(_logger);
    //        cmd.ExecuteNonQuery();
    //        return "Success";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error executing non-query: {Query}", query);
    //        return ex.Message;
    //    }
    //    finally
    //    {
    //        if (connection.State == ConnectionState.Open)
    //        {
    //            connection.Close();
    //        }
    //    }
    //}

    //public string FillDataTable(ref DataTable dt, string query)
    //{
    //    using var connection = GetTenantConnection();
    //    try
    //    {
    //        if (connection.State == ConnectionState.Closed)
    //        {
    //            connection.Open();
    //        }
    //        SqlDataAdapter dr = new SqlDataAdapter(query, connection);
    //        dr.SelectCommand.CommandTimeout = 600; // 10 minutes
    //        // dr.SelectCommand.LogQuery(_logger);
    //        DataSet ds = new DataSet();
    //        dr.Fill(ds);
    //        dt = ds.Tables[0];
    //        return "";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error filling DataTable: {Query}", query);
    //        return ex.Message;
    //    }
    //    finally
    //    {
    //        if (connection.State == ConnectionState.Open)
    //        {
    //            connection.Close();
    //        }
    //    }
    //}

    //public string GetColumnValue(string columnName, string tableName, string whereCndtn, string orderBy = "")
    //{
    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        if (orderBy == "") orderBy = columnName;
    //        string str = "Select Distinct " + columnName + " From " + tableName + " Where " + whereCndtn + " Order By " + orderBy;
    //        FillDataTable(ref dt, str);
    //        if (dt.Rows.Count > 0)
    //        {
    //            return dt.Rows[0][0].ToString();
    //        }
    //        return "";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting column value");
    //        return "Error: " + ex.Message;
    //    }
    //}

    //public bool IsDeletable(string fieldName, string tableName, string searchCondition = "")
    //{
    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        string str = " SELECT " + fieldName + " FROM " + tableName + " " + searchCondition + " ";
    //        FillDataTable(ref dt, str);
    //        if (dt.Rows.Count == 0)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error checking if deletable");
    //        return false;
    //    }
    //}

    //public string CkeckDuplicate(string tableName, string fieldname, string searchCondition = "")
    //{
    //    DataTable dt = new DataTable();
    //    string sql = "";
    //    try
    //    {
    //        sql = "Select " + fieldname + " from " + tableName + " " + ((searchCondition.ToString().Trim() != "") ? (" Where " + searchCondition) : "");
    //        FillDataTable(ref dt, sql);

    //        if (dt.Rows.Count > 0)
    //        {
    //            sql = "Available";
    //        }
    //        else
    //        {
    //            sql = "";
    //        }
    //        return sql;
    //    }
    //    catch (Exception)
    //    {
    //        sql = "";
    //        return sql;
    //    }
    //}

    //public int GetID(string tableName, string refFieldName, string refFieldValue, string fieldName)
    //{
    //    DataTable dt1 = new DataTable();
    //    string strq = "";
    //    try
    //    {
    //        int companyId = GetCurrentCompanyId();
    //        strq = "Select " + fieldName + " From " + tableName + " Where " + refFieldName + "='" + refFieldValue + "' AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID=" + companyId;
    //        FillDataTable(ref dt1, strq);
    //        return Convert.ToInt32(dt1.Rows[0][0]);
    //    }
    //    catch (Exception)
    //    {
    //        return 0;
    //    }
    //}

    //public string GetVoucherNo(string tableName, string refFieldName, string refFieldValue, string fieldName)
    //{
    //    DataTable dt1 = new DataTable();
    //    string strq = "";
    //    try
    //    {
    //        int companyId = GetCurrentCompanyId();
    //        strq = "Select " + fieldName + " From " + tableName + " Where " + refFieldName + "='" + refFieldValue + "' AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID=" + companyId;
    //        FillDataTable(ref dt1, strq);
    //        return dt1.Rows[0][0].ToString();
    //    }
    //    catch (Exception)
    //    {
    //        return "";
    //    }
    //}

    #endregion

    #region Insert Operations

    public async Task<int> InsertDataAsync<T>(string tableName, T data, IDbConnection connection, IDbTransaction transaction, string idFieldName = "ID")
    {
        // Manage audit fields before generating SQL
        ManageAuditAndParentFields(data, 0);

        var properties = typeof(T).GetProperties()
            .Where(p => !p.Name.Equals(idFieldName, StringComparison.OrdinalIgnoreCase) && 
                        !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                        !p.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase) &&
                        !p.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var columnsList = properties.Select(p => $"[{p.Name}]").ToList();
        var parametersList = properties.Select(p => "@" + p.Name).ToList();

        // Always use GETDATE() for audit dates to match legacy VB behavior
        columnsList.Add("[CreatedDate]");
        parametersList.Add("GETDATE()");
        columnsList.Add("[ModifiedDate]");
        parametersList.Add("GETDATE()");

        var columns = string.Join(", ", columnsList);
        var parameters = string.Join(", ", parametersList);

        var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters}); SELECT CAST(SCOPE_IDENTITY() as int)";

        using var cmd = new SqlCommand(sql, connection as SqlConnection, transaction as SqlTransaction);
        foreach (var prop in properties)
        {
            // Skip automated audit fields in parameter mapping if handled by ManageAuditAndParentFields/SQL
            if (prop.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase) ||
                prop.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))
                continue;

            var val = prop.GetValue(data);
            if (val is DateTime dt && dt == DateTime.MinValue)
            {
                val = new DateTime(1753, 1, 1);
            }
            cmd.Parameters.AddWithValue("@" + prop.Name, val ?? DBNull.Value);
        }

        cmd.LogQuery(_logger);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    //public string InsertDatatableToDatabase(object jsonObject, string tableName, string addColName, string addColValue,
    //    ref SqlConnection con, ref SqlTransaction objTrans, string voucherType = "", string transactionID = "")
    //{
    //    string keyReturn = "";
    //    string colName = "";
    //    string transID = "";
    //    string colValue = "";
    //    string str = "";

    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        ConvertObjectToDatatable(jsonObject, ref dt, ref str);

    //        if (str != "Success")
    //        {
    //            return str;
    //        }

    //        colName = "";
    //        foreach (DataColumn column in dt.Columns)
    //        {
    //            if (column.ColumnName != "Id")
    //            {
    //                if (colName == "")
    //                {
    //                    colName = column.ColumnName;
    //                }
    //                else
    //                {
    //                    colName = colName + "," + column.ColumnName;
    //                }
    //            }
    //        }

    //        for (int i = 0; i < dt.Rows.Count; i++)
    //        {
    //            colValue = "";
    //            foreach (DataColumn column in dt.Columns)
    //            {
    //                if (column.ColumnName != "Id")
    //                {
    //                    if (voucherType == "Receipt Note")
    //                    {
    //                        if (column.ColumnName == "TransID")
    //                        {
    //                            transID = "" + dt.Rows[0][column] + "";
    //                        }
    //                    }

    //                    if (column.ColumnName == "BatchNo" && voucherType == "Receipt Note")
    //                    {
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + transID + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + transID + "'";
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (column.ColumnName == "ParentTransactionID")
    //                        {
    //                            if (int.Parse(dt.Rows[i][column.ColumnName].ToString()) <= 0)
    //                            {
    //                                dt.Rows[i][column.ColumnName] = transactionID;
    //                            }
    //                        }
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                    }
    //                }
    //            }

    //            if (addColName == "")
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + ") Values ( " + colValue + " ) ";
    //            }
    //            else
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + "," + addColName + " ) Values ( " + colValue + "," + addColValue + " ); ";
    //            }
    //            str += " SELECT SCOPE_IDENTITY();";

    //            SqlCommand cmd = new SqlCommand(str, con);
    //            cmd.CommandTimeout = con.ConnectionTimeout;
    //            cmd.Transaction = objTrans;
    //            // cmd.LogQuery(_logger);
    //            keyReturn = cmd.ExecuteScalar().ToString();
    //        }
    //        dt.Reset();
    //        return keyReturn;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error inserting datatable to database");
    //        return ex.Message;
    //    }
    //}

    //public string InsertDatatableToDatabaseWithouttrans(object jsonObject, string tableName, string addColName,
    //    string addColValue, string voucherType = "", string transactionID = "")
    //{
    //    string keyReturn = "";
    //    string colName = "";
    //    string transID = "";
    //    string colValue = "";
    //    string str = "";

    //    using var con = GetTenantConnection();
    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        ConvertObjectToDatatable(jsonObject, ref dt, ref str);

    //        if (str != "Success")
    //        {
    //            return str;
    //        }

    //        colName = "";
    //        foreach (DataColumn column in dt.Columns)
    //        {
    //            if (column.ColumnName != "Id")
    //            {
    //                if (colName == "")
    //                {
    //                    colName = column.ColumnName;
    //                }
    //                else
    //                {
    //                    colName = colName + "," + column.ColumnName;
    //                }
    //            }
    //        }

    //        for (int i = 0; i < dt.Rows.Count; i++)
    //        {
    //            colValue = "";
    //            foreach (DataColumn column in dt.Columns)
    //            {
    //                if (column.ColumnName != "Id")
    //                {
    //                    if (voucherType == "Receipt Note")
    //                    {
    //                        if (column.ColumnName == "TransID")
    //                        {
    //                            transID = "" + dt.Rows[0][column] + "";
    //                        }
    //                    }

    //                    if (column.ColumnName == "BatchNo" && voucherType == "Receipt Note")
    //                    {
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + transID + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + transID + "'";
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (column.ColumnName == "ParentTransactionID")
    //                        {
    //                            if (int.Parse(dt.Rows[i][column.ColumnName].ToString()) <= 0)
    //                            {
    //                                dt.Rows[i][column.ColumnName] = transactionID;
    //                            }
    //                        }
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                    }
    //                }
    //            }

    //            if (addColName == "")
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + ") Values ( " + colValue + " ) ";
    //            }
    //            else
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + "," + addColName + " ) Values ( " + colValue + "," + addColValue + " ); ";
    //            }
    //            str += " SELECT SCOPE_IDENTITY();";

    //            SqlCommand cmd = new SqlCommand(str, con);
    //            cmd.CommandTimeout = con.ConnectionTimeout;
    //            // cmd.LogQuery(_logger);
    //            keyReturn = cmd.ExecuteScalar().ToString();
    //        }
    //        dt.Reset();
    //        return keyReturn;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error inserting datatable without transaction");
    //        return ex.Message;
    //    }
    //}

    //public string InsertlistToDatabase(DataTable jsonObject, string tableName, string addColName, string addColValue,
    //    ref SqlConnection con, ref SqlTransaction objTrans, string voucherType = "", string transactionID = "")
    //{
    //    string keyReturn = "";
    //    string colName = "";
    //    string colValue = "";
    //    string str = "";

    //    try
    //    {
    //        DataTable dt = jsonObject;

    //        colName = "";
    //        foreach (DataColumn column in dt.Columns)
    //        {
    //            if (column.ColumnName != "Id")
    //            {
    //                if (colName == "")
    //                {
    //                    colName = column.ColumnName;
    //                }
    //                else
    //                {
    //                    colName = colName + "," + column.ColumnName;
    //                }
    //            }
    //        }

    //        for (int i = 0; i < dt.Rows.Count; i++)
    //        {
    //            colValue = "";
    //            foreach (DataColumn column in dt.Columns)
    //            {
    //                if (column.ColumnName != "Id")
    //                {
    //                    if (column.ColumnName == "BatchNo" && voucherType == "Receipt Note")
    //                    {
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + dt.Rows[i]["TransID"] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + transactionID + dt.Rows[i][column.ColumnName] + "_" + dt.Rows[i]["TransID"] + "'";
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (column.ColumnName == "ParentTransactionID")
    //                        {
    //                            if (Convert.ToInt32(dt.Rows[i][column.ColumnName]) <= 0 && voucherType == "Receipt Note")
    //                            {
    //                                dt.Rows[i][column.ColumnName] = Convert.ToInt32(transactionID);
    //                            }
    //                        }
    //                        if (colValue == "")
    //                        {
    //                            colValue = "N'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "N'" + dt.Rows[i][column.ColumnName] + "'";
    //                        }
    //                    }
    //                }
    //            }

    //            if (addColName == "")
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + ") Values ( " + colValue + " ) ";
    //            }
    //            else
    //            {
    //                str = "Insert Into " + tableName + " ( " + colName + "," + addColName + " ) Values ( " + colValue + "," + addColValue + " ); ";
    //            }
    //            str += " SELECT SCOPE_IDENTITY();";

    //            SqlCommand cmd = new SqlCommand(str, con);
    //            cmd.CommandTimeout = con.ConnectionTimeout;
    //            cmd.Transaction = objTrans;
    //            // cmd.LogQuery(_logger);
    //            keyReturn = cmd.ExecuteScalar().ToString();
    //        }
    //        dt.Reset();
    //        return keyReturn;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error inserting list to database");
    //        return ex.Message;
    //    }
    //}

    //public string AddToDatabaseOperation(object pObject, string sTableName, string addColName, string addColValue,
    //    ref SqlConnection con, ref SqlTransaction objTrans)
    //{
    //    string keyField = "";
    //    string colValue = "";
    //    string wherCol = "";
    //    string errMsg = "";
    //    DataTable dts = new DataTable();

    //    ConvertObjectToDatatable(pObject, ref dts, ref errMsg);

    //    try
    //    {
    //        string colName = "";
    //        foreach (DataColumn column in dts.Columns)
    //        {
    //            if (column.ColumnName != "Id")
    //            {
    //                if (colName == "")
    //                {
    //                    colName = column.ColumnName;
    //                }
    //                else
    //                {
    //                    colName = colName + "," + column.ColumnName;
    //                }
    //            }
    //        }

    //        for (int i = 0; i < dts.Rows.Count; i++)
    //        {
    //            colValue = "";
    //            foreach (DataColumn column in dts.Columns)
    //            {
    //                if (column.ColumnName != "Id")
    //                {
    //                    if (colValue == "")
    //                    {
    //                        colValue = "'" + dts.Rows[i][column.ColumnName] + "'";
    //                    }
    //                    else
    //                    {
    //                        colValue = colValue + "," + "'" + dts.Rows[i][column.ColumnName] + "'";
    //                    }
    //                }
    //                if (column.ColumnName == "PlanContQty" || column.ColumnName == "PlanContentType" || column.ColumnName == "PlanContName")
    //                {
    //                    if (wherCol == "")
    //                    {
    //                        wherCol = "Where " + "" + column.ColumnName + "='" + dts.Rows[i][column.ColumnName] + "' And ";
    //                    }
    //                    else
    //                    {
    //                        wherCol = wherCol + "" + column.ColumnName + "='" + dts.Rows[i][column.ColumnName] + "' And ";
    //                    }
    //                }
    //            }
    //            wherCol = wherCol.Remove(wherCol.Length - 4, 4);
    //            string str = "Insert Into " + sTableName + "( " + colName + "," + addColName + ",ContentsId) " +
    //                "Select " + colValue + "," + addColValue + ",(Select Max(JobContentsID) From JobBookingContents " + wherCol + ");";
    //            wherCol = "";
    //            SqlCommand cmd = new SqlCommand(str, con);
    //            cmd.CommandTimeout = con.ConnectionTimeout;
    //            cmd.Transaction = objTrans;
    //            // cmd.LogQuery(_logger);
    //            cmd.ExecuteNonQuery();
    //        }
    //        dts.Reset();
    //        keyField = "200";

    //        return keyField;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error adding to database operation");
    //        return "Error: " + ex.Message;
    //    }
    //}

    //public string InsertSecondaryDataJobCard(DataTable dts, string sTableName, string addColName, string addColValue,
    //    ref SqlConnection con, ref SqlTransaction objTrans, string pTableName, string sTableColumn,
    //    string pTblMaxColumn, string whereCndtn = "")
    //{
    //    string keyField = "";
    //    string colName = "";
    //    string colValue = "";
    //    string wherCol = "";

    //    try
    //    {
    //        if (sTableName == "ProductMasterCorrugation" || sTableName == "JobBookingJobCardCorrugation")
    //        {
    //            if (dts.Columns.Contains("ItemCode"))
    //            {
    //                dts.Columns.Remove("ItemCode");
    //            }
    //        }

    //        SqlConnection db = con;
    //        if (db.State == ConnectionState.Closed)
    //        {
    //            db.Open();
    //        }

    //        colName = "";
    //        foreach (DataColumn column in dts.Columns)
    //        {
    //            if (sTableName.ToUpper() != "ITEMTRANSACTIONDETAIL")
    //            {
    //                if (column.ColumnName != "Id")
    //                {
    //                    if (colName == "")
    //                    {
    //                        colName = column.ColumnName;
    //                    }
    //                    else
    //                    {
    //                        colName = colName + "," + column.ColumnName;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                if (column.ColumnName != "Id" && column.ColumnName != "PlanContQty" && column.ColumnName != "PlanContentType" && column.ColumnName != "PlanContName")
    //                {
    //                    if (colName == "")
    //                    {
    //                        colName = column.ColumnName;
    //                    }
    //                    else
    //                    {
    //                        colName = colName + "," + column.ColumnName;
    //                    }
    //                }
    //            }
    //        }

    //        for (int i = 0; i < dts.Rows.Count; i++)
    //        {
    //            colValue = "";
    //            foreach (DataColumn column in dts.Columns)
    //            {
    //                if (sTableName.ToUpper() != "ITEMTRANSACTIONDETAIL")
    //                {
    //                    if (column.ColumnName != "Id")
    //                    {
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + dts.Rows[i][column.ColumnName] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + dts.Rows[i][column.ColumnName] + "'";
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (column.ColumnName != "Id" && column.ColumnName != "PlanContQty" && column.ColumnName != "PlanContentType" && column.ColumnName != "PlanContName")
    //                    {
    //                        if (colValue == "")
    //                        {
    //                            colValue = "'" + dts.Rows[i][column.ColumnName] + "'";
    //                        }
    //                        else
    //                        {
    //                            colValue = colValue + "," + "'" + dts.Rows[i][column.ColumnName] + "'";
    //                        }
    //                    }
    //                }

    //                if (column.ColumnName == "PlanContQty" || column.ColumnName == "PlanContentType" || column.ColumnName == "PlanContName")
    //                {
    //                    if (wherCol == "")
    //                    {
    //                        wherCol = "Where " + column.ColumnName + "='" + dts.Rows[i][column.ColumnName] + "' And ";
    //                    }
    //                    else
    //                    {
    //                        wherCol = wherCol + column.ColumnName + "='" + dts.Rows[i][column.ColumnName] + "' And ";
    //                    }
    //                }
    //            }

    //            if (sTableColumn != "")
    //            {
    //                string con1 = whereCndtn;
    //                if (wherCol != "" && wherCol != null)
    //                {
    //                    wherCol = wherCol + " IsDeletedTransaction=0 " + whereCndtn;
    //                }
    //                if (sTableName == "JobOrderBookingDeliveryDetails" && wherCol == "")
    //                {
    //                    con1 += dts.Rows[i]["ProductMasterID"];
    //                }

    //                string str = "Insert Into " + sTableName + "( " + colName + "," + addColName + "," + sTableColumn + ") " +
    //                             "Select " + colValue + "," + addColValue + ",(Select Max(" + pTblMaxColumn + ") From " + pTableName + " " + wherCol + con1 + " );";
    //                wherCol = "";
    //                SqlCommand cmd = new SqlCommand(str, con);
    //                cmd.CommandTimeout = con.ConnectionTimeout;
    //                cmd.Transaction = objTrans;
    //                // cmd.LogQuery(_logger);
    //                cmd.ExecuteNonQuery();
    //            }
    //        }
    //        dts = null;
    //        keyField = "200";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error inserting secondary data job card");
    //        keyField = ex.Message;
    //    }
    //    return keyField;
    //}

    public async Task<long> InsertDataAsync(string tableName, object data, SqlConnection con, SqlTransaction trans, 
        string idFieldName = "TransactionID", string addColName = "", string addColValue = "", 
        string voucherType = "", long parentTransactionId = 0)
    {
        if (data == null) return 0;

        IEnumerable dataList;
        if (data is IEnumerable list && !(data is string)) 
            dataList = list;
        else 
            dataList = new[] { data };

        long lastId = 0;

        foreach (var item in dataList)
        {
            // 1. Manage Audit and Parent Linkage
            ManageAuditAndParentFields(item, parentTransactionId);

            var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new List<string>();
            var values = new List<string>();
            
            using var command = new SqlCommand("", con, trans);

            foreach (var prop in properties)
            {
                // Skip ID field and specific identity-related fields
                if (prop.Name.Equals(idFieldName, StringComparison.OrdinalIgnoreCase) || 
                    prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("TransactionDetailID", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("POTaxID", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("ScheduleID", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("OverheadID", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("RequisitionDetailID", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Only handle value types, strings, and nullables
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (!propType.IsValueType && propType != typeof(string)) continue;

                // Specific legacy logic: BatchNo for Receipt Note
                if (prop.Name.Equals("BatchNo", StringComparison.OrdinalIgnoreCase) && voucherType == "Receipt Note")
                {
                    var batchNoVal = prop.GetValue(item)?.ToString() ?? "";
                    var transIdProp = item.GetType().GetProperty("TransID");
                    var transIdVal = transIdProp?.GetValue(item)?.ToString() ?? "";
                    
                    columns.Add(prop.Name);
                    values.Add("@CustomBatchNo");
                    command.Parameters.AddWithValue("@CustomBatchNo", $"{parentTransactionId}{batchNoVal}_{transIdVal}");
                }
                else
                {
                    var val = prop.GetValue(item);
                    if (val is DateTime dt && dt == DateTime.MinValue)
                    {
                        val = new DateTime(1753, 1, 1);
                    }

                    columns.Add(prop.Name);
                    values.Add("@" + prop.Name);
                    command.Parameters.AddWithValue("@" + prop.Name, val ?? DBNull.Value);
                }
            }

            // Always add audit dates if not already present in the collection
            if (!columns.Any(c => c.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase))) 
            {
                columns.Add("CreatedDate");
                values.Add("GETDATE()");
            }
            if (!columns.Any(c => c.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))) 
            {
                columns.Add("ModifiedDate");
                values.Add("GETDATE()");
            }

            string sql;
            if (string.IsNullOrEmpty(addColName))
            {
                sql = $"INSERT INTO {tableName} ({string.Join(",", columns)}) VALUES ({string.Join(",", values)}); SELECT SCOPE_IDENTITY();";
            }
            else
            {
                sql = $"INSERT INTO {tableName} ({string.Join(",", columns)}, {addColName}) VALUES ({string.Join(",", values)}, {addColValue}); SELECT SCOPE_IDENTITY();";
            }

            command.CommandText = sql;
            command.LogQuery(_logger);
            
            var result = await command.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value) lastId = Convert.ToInt64(result);
        }

        return lastId;
    }

    private void ManageAuditAndParentFields(object item, long parentTransactionId)
    {
        if (item == null) return;
        var type = item.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var prop in props)
        {
            if (!prop.CanWrite) continue;

            if (prop.Name.Equals("CompanyID", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) prop.SetValue(item, _currentUserService.GetCompanyId() ?? 0);
            }
            else if (prop.Name.Equals("UserID", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) prop.SetValue(item, _currentUserService.GetUserId() ?? 0);
            }
            else if (prop.Name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) prop.SetValue(item, _currentUserService.GetUserId() ?? 0);
            }
            else if (prop.Name.Equals("ModifiedBy", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) prop.SetValue(item, _currentUserService.GetUserId() ?? 0);
            }
            else if (prop.Name.Equals("FYear", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || string.IsNullOrEmpty(val.ToString())) prop.SetValue(item, _currentUserService.GetFYear() ?? "");
            }
            else if (prop.Name.Equals("ProductionUnitID", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) prop.SetValue(item, _currentUserService.GetProductionUnitId() ?? 0);
            }
            else if ((prop.Name.Equals("TransactionID", StringComparison.OrdinalIgnoreCase) || 
                      prop.Name.Equals("ParentTransactionID", StringComparison.OrdinalIgnoreCase)) && 
                     parentTransactionId > 0)
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) <= 0) prop.SetValue(item, parentTransactionId);
            }
            // Ensure CreatedDate and ModifiedDate are handled
            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                var val = prop.GetValue(item);
                if (val is DateTime dt && dt == DateTime.MinValue)
                {
                    prop.SetValue(item, new DateTime(1753, 1, 1));
                }
            }
        }
    }

    #endregion

    #region Update Operations

    //public string UpdateDatatableToDatabasewithtrans(object jsonObject, string tableName, string addColName, int pvalue,
    //    ref SqlConnection con, ref SqlTransaction objTrans, string wherecndtn = "")
    //{
    //    string keyField = "";
    //    string str = "";

    //    try
    //    {
    //        string uniqueId = "";
    //        string errMsg = "";
    //        DataTable dt = null;
    //        ConvertObjectToDatatable(jsonObject, ref dt, ref errMsg);

    //        int cnt = 1;

    //        for (int i = 0; i <= dt.Rows.Count - 1; i++)
    //        {
    //            str = "";
    //            uniqueId = "";
    //            cnt = 1;
    //            foreach (DataColumn column in dt.Columns)
    //            {
    //                if (cnt <= pvalue)
    //                {
    //                    uniqueId = uniqueId + column.ColumnName + " ='" + dt.Rows[i][column.ColumnName] + "' And ";
    //                    cnt = cnt + 1;
    //                }
    //                else
    //                    str = str + column.ColumnName + "='" + dt.Rows[i][column.ColumnName] + "',";
    //            }

    //            if (str != "")
    //                str = str.Substring(0, str.Length - 1);

    //            if (uniqueId != "")
    //                uniqueId = uniqueId.Substring(0, uniqueId.Length - 4);

    //            if ((wherecndtn != ""))
    //            {
    //                if (uniqueId != "")
    //                    uniqueId = uniqueId + " And " + wherecndtn;
    //                else
    //                    uniqueId = wherecndtn;
    //            }

    //            if ((addColName != ""))
    //            {
    //                if (str != "")
    //                    str = str + " , " + addColName;
    //                else
    //                    str = addColName;
    //            }

    //            str = "Update " + tableName + " Set " + str + " Where " + uniqueId;

    //            SqlCommand cmd = new SqlCommand(str, con);
    //            cmd.CommandType = CommandType.Text;
    //            cmd.Transaction = objTrans;
    //            // cmd.LogQuery(_logger);
    //            cmd.ExecuteNonQuery();
    //        }
    //        keyField = "Success";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating datatable with transaction");
    //        return ex.Message;
    //    }
    //    return keyField;
    //}

    //public string UpdateDatatableToDatabase(object jsonObject, string tableName, string addColName, int pvalue,
    //    string wherecndtn = "")
    //{
    //    string keyField = "";
    //    string str = "";

    //    try
    //    {
    //        string uniqueId = "";
    //        string errMsg = "";
    //        DataTable dt = null;
    //        ConvertObjectToDatatable(jsonObject, ref dt, ref errMsg);
    //        using var db = GetTenantConnection();

    //        int cnt = 1;

    //        for (int i = 0; i <= dt.Rows.Count - 1; i++)
    //        {
    //            str = "";
    //            uniqueId = "";
    //            cnt = 1;
    //            foreach (DataColumn column in dt.Columns)
    //            {
    //                if (cnt <= pvalue)
    //                {
    //                    uniqueId = uniqueId + column.ColumnName + " ='" + dt.Rows[i][column.ColumnName] + "' And ";
    //                    cnt = cnt + 1;
    //                }
    //                else
    //                    str = str + column.ColumnName + "='" + dt.Rows[i][column.ColumnName] + "',";
    //            }

    //            if (str != "")
    //                str = str.Substring(0, str.Length - 1);

    //            if (uniqueId != "")
    //                uniqueId = uniqueId.Substring(0, uniqueId.Length - 4);

    //            if ((wherecndtn != ""))
    //            {
    //                if (uniqueId != "")
    //                    uniqueId = uniqueId + " And " + wherecndtn;
    //                else
    //                    uniqueId = wherecndtn;
    //            }
    //            if ((addColName != ""))
    //            {
    //                if (str != "")
    //                    str = str + " , " + addColName;
    //                else
    //                    str = addColName;
    //            }

    //            str = "Update " + tableName + " Set " + str + " Where " + uniqueId;

    //            SqlCommand cmd = new SqlCommand(str, db);
    //            cmd.CommandType = CommandType.Text;
    //            // cmd.LogQuery(_logger);
    //            cmd.ExecuteNonQuery();
    //        }
    //        keyField = "Success";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating datatable");
    //        return ex.Message;
    //    }
    //    return keyField;
    //}

    public async Task UpdateDataAsync(string tableName, object data, SqlConnection con, SqlTransaction trans, string[] whereFields, string addColName = "", string extraWhere = "")
    {
        if (data == null) return;

        IEnumerable dataList;
        if (data is IEnumerable list && !(data is string))
            dataList = list;
        else
            dataList = new[] { data };

        foreach (var item in dataList)
        {
            // 1. Manage Audit and Identity fields for Update
            ManageUpdateAuditFields(item);

            var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var updateColumns = new List<string>();
            var whereColumns = new List<string>();

            using var command = new SqlCommand("", con, trans);

            foreach (var prop in properties)
            {
                // Only handle value types, strings, and nullables
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (!propType.IsValueType && propType != typeof(string)) continue;

                var val = prop.GetValue(item);
                if (val is DateTime dt && dt == DateTime.MinValue)
                {
                    val = new DateTime(1753, 1, 1);
                }

                if (whereFields.Any(f => f.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    whereColumns.Add($"[{prop.Name}] = @W_{prop.Name}");
                    command.Parameters.AddWithValue("@W_" + prop.Name, val ?? DBNull.Value);
                }
                else
                {
                    // Skip Created* fields and primary identity field during update
                    // Also skip ModifiedDate so it falls back to GETDATE() at the end
                    if (prop.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        continue;

                    updateColumns.Add($"[{prop.Name}] = @U_{prop.Name}");
                    command.Parameters.AddWithValue("@U_" + prop.Name, val ?? DBNull.Value);
                }
            }

            // Always ensure ModifiedDate is handled via GETDATE() for updates
            if (!updateColumns.Any(c => c.Contains("ModifiedDate")))
            {
                updateColumns.Add("[ModifiedDate] = GETDATE()");
            }

            // Add legacy AddColName logic (e.g. "ModifiedBy='1', CompanyID='1'")
            if (!string.IsNullOrEmpty(addColName))
            {
                updateColumns.Add(addColName);
            }

            if (whereColumns.Count == 0)
            {
                throw new InvalidOperationException($"Update operation for table '{tableName}' failed: No WHERE fields matched provided keys ({string.Join(",", whereFields)}).");
            }

            string sql = $"UPDATE {tableName} SET {string.Join(", ", updateColumns)} WHERE {string.Join(" AND ", whereColumns)}";
            if (!string.IsNullOrEmpty(extraWhere))
            {
                sql += $" AND {extraWhere}";
            }

            command.CommandText = sql;
            command.LogQuery(_logger);
            await command.ExecuteNonQueryAsync();
        }
    }

    private void ManageUpdateAuditFields(object item)
    {
        if (item == null) return;
        var type = item.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var prop in props)
        {
            if (!prop.CanWrite) continue;

            if (prop.Name.Equals("ModifiedBy", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || string.IsNullOrEmpty(val.ToString()) || val.ToString() == "0") 
                    prop.SetValue(item, _currentUserService.GetUserId()?.ToString() ?? "0");
            }
            else if (prop.Name.Equals("CompanyID", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) 
                    prop.SetValue(item, _currentUserService.GetCompanyId() ?? 0);
            }
            else if (prop.Name.Equals("ProductionUnitID", StringComparison.OrdinalIgnoreCase))
            {
                var val = prop.GetValue(item);
                if (val == null || Convert.ToInt64(val) == 0) 
                    prop.SetValue(item, _currentUserService.GetProductionUnitId() ?? 0);
            }
            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                var val = prop.GetValue(item);
                if (val is DateTime dt && dt == DateTime.MinValue)
                {
                    prop.SetValue(item, new DateTime(1753, 1, 1));
                }
            }
        }
    }

    //public string UpdateIntegrationStatus(string tableName, string refFieldName, string refFieldValue, string whereCndtn = "")
    //{
    //    if (whereCndtn != "")
    //    {
    //        whereCndtn = " And " + whereCndtn;
    //    }
    //    string str = "";
    //    try
    //    {
    //        str = "Update " + tableName + " Set IsIntegrated = 1 Where " + refFieldName + " = '" + refFieldValue + "'" + whereCndtn;
    //        ExecuteNonSQLQuery(str);
    //        return "Success";
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating integration status");
    //        return "Error: " + ex.Message;
    //    }
    //}

    #endregion

    #region Delete Operations

    //public string DeleteData(string tableName, ref SqlConnection con, ref SqlTransaction objTrans, string searchCondition)
    //{
    //    string str = "";
    //    SqlConnection db = con;

    //    try
    //    {
    //        if (searchCondition != "")
    //        {
    //            str = "DELETE FROM " + tableName + " " + searchCondition;
    //            SqlCommand cmd = new SqlCommand(str, db);
    //            cmd.CommandType = CommandType.Text;
    //            cmd.Transaction = objTrans;
    //            // cmd.LogQuery(_logger);
    //            cmd.ExecuteNonQuery();
    //            str = "Deleted";
    //        }
    //        else
    //        {
    //            str = "Not Deleted";
    //        }
    //        return str;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error deleting data");
    //        return ex.Message;
    //    }
    //}

    #endregion

    #region Code Generation

    //public string GeneratePrefixedNo(string tableName, string prefix, string maxFieldName, ref int maxNoVariable,
    //    string fYear, string searchCondition = "")
    //{
    //    DataTable dt = new DataTable();
    //    string st;
    //    int i;
    //    int gblCodesize = 5;
    //    st = "";
    //    FillDataTable(ref dt, "Select isnull(MAX(isnull(" + maxFieldName + " ,0)),0) + 1  From  " + tableName + "  " + searchCondition);
    //    if (dt.Rows.Count > 0)
    //    {
    //        for (i = 1; i <= gblCodesize - dt.Rows[0][0].ToString().Length; i++)
    //            st = (st).Trim() + 0;
    //        maxNoVariable = int.Parse(dt.Rows[0][0].ToString());
    //        if (fYear != "")
    //            st = st.Trim() + dt.Rows[0][0] + "_" + fYear.Substring(2, 2) + "_" + fYear.Substring(7, 2);
    //        else
    //            st = st.Trim() + dt.Rows[0][0];
    //        return (prefix).Trim() + st;
    //    }
    //    else
    //    {
    //        maxNoVariable = 1;
    //        if (fYear != "")
    //            return prefix.Trim() + "00001" + "_" + fYear.Substring(2, 2) + "_" + fYear.Substring(7, 2);
    //        else
    //            return prefix.Trim() + "00001";
    //    }
    //}

    //public long GenerateMaxVoucherNo(string tableName, string fieldname, string searchCondition = "")
    //{
    //    DataTable dt = new DataTable();
    //    string sql = "";
    //    long generateMaxVoucherNo;
    //    sql = "Select Isnull(MAX(Isnull(" + fieldname + ",0)),0) From " + tableName + " " + searchCondition;
    //    FillDataTable(ref dt, sql);
    //    if (dt.Rows.Count > 0)
    //    {
    //        generateMaxVoucherNo = Convert.ToInt64(Convert.ToString(dt.Rows[0][0])) + 1;
    //    }
    //    else
    //    {
    //        generateMaxVoucherNo = 1;
    //    }
    //    return generateMaxVoucherNo;
    //}

    #endregion

    public async Task<(string VoucherNo, long MaxNo)> GenerateVoucherNoAsync(
        string tableName,
        long voucherId,
        string prefix,
        string maxFieldName = "MaxVoucherNo")
    {
        string voucherNo = "";
        long maxNo = 0;
        string fYear = _currentUserService.GetFYear() ?? string.Empty;
        long companyId = _currentUserService.GetCompanyId() ?? 0;
        long? productionUnitId = _currentUserService.GetProductionUnitId();

        try
        {
            using var connection = GetTenantConnection();

            bool generateByProductionUnit = false;

            var configSql = @"
                SELECT ISNULL(GenerateVoucherNoByProductionUnit, 0) 
                FROM CompanyMaster 
                WHERE CompanyID = @CompanyID";
            
            var configResult = await connection.ExecuteScalarAsync(configSql, new { CompanyID = companyId });
            if (configResult != null && configResult != DBNull.Value)
            {
                generateByProductionUnit = Convert.ToBoolean(configResult);
            }

            // 2. Build Max Query
            var sb = new StringBuilder();
            sb.Append($"SELECT ISNULL(MAX({maxFieldName}), 0) + 1 FROM {tableName} WHERE VoucherID = @VoucherID AND VoucherPrefix = @Prefix ");
            sb.Append(" AND FYear = @FYear ");
            sb.Append(" AND CompanyID = @CompanyID ");

            if (generateByProductionUnit && productionUnitId.HasValue)
            {
                sb.Append(" AND ProductionUnitID = @ProductionUnitID ");
            }

            maxNo = await connection.ExecuteScalarAsync<long>(sb.ToString(), new 
            { 
                VoucherID = voucherId, 
                Prefix = prefix, 
                FYear = fYear, 
                CompanyID = companyId,
                ProductionUnitID = productionUnitId ?? 0
            });

            // 3. Formatting
            string formattedMaxNo = maxNo.ToString("D5");
            string yearSuffix = "";

            if (!string.IsNullOrEmpty(fYear) && fYear.Length >= 9 && fYear.Contains("-"))
            {
                var parts = fYear.Split('-');
                if (parts.Length == 2 && parts[0].Length == 4 && parts[1].Length == 4)
                {
                   string yy1 = parts[0].Substring(2, 2);
                   string yy2 = parts[1].Substring(2, 2);
                   yearSuffix = $"_{yy1}_{yy2}";
                }
                else 
                {
                   yearSuffix = "_" + fYear; 
                }
            }
            else if (!string.IsNullOrEmpty(fYear))
            {
                 yearSuffix = "_" + fYear;
            }

            voucherNo = $"{prefix}{formattedMaxNo}{yearSuffix}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating voucher number");
            voucherNo = $"{prefix}ERROR";
            throw;
        }

        return (voucherNo, maxNo);
    }

    #region JSON/DataTable Conversion

    //public string ConvertObjectToDatatable(object jsonObject, ref DataTable datatable, ref string errMsg)
    //{
    //    try
    //    {
    //        string st = string.Empty;

    //        if (jsonObject == null)
    //        {
    //            datatable = new DataTable();
    //            errMsg = "Success";
    //            return "";
    //        }

    //        if (jsonObject is string jsonString)
    //        {
    //            jsonString = jsonString.Trim();

    //            // Handle double braces like {{...}}
    //            if (jsonString.StartsWith("{{") && jsonString.EndsWith("}}"))
    //                jsonString = jsonString.TrimStart('{').TrimEnd('}');

    //            // If it's empty or "{}" or "[]"
    //            if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "{}" || jsonString == "[]")
    //            {
    //                datatable = new DataTable();
    //                errMsg = "Success";
    //                return "";
    //            }

    //            st = jsonString;
    //        }
    //        else if (jsonObject is JToken token)
    //        {
    //            if (!token.HasValues)
    //            {
    //                datatable = new DataTable();
    //                errMsg = "Success";
    //                return "";
    //            }

    //            st = token.ToString(Formatting.None);
    //        }
    //        else
    //        {
    //            st = JsonConvert.SerializeObject(jsonObject);

    //            if (string.IsNullOrWhiteSpace(st) || st == "{}" || st == "[]")
    //            {
    //                datatable = new DataTable();
    //                errMsg = "Success";
    //                return "";
    //            }
    //        }

    //        // Detect JSON type
    //        string trimmed = st.TrimStart();

    //        if (trimmed.StartsWith("{"))
    //        {
    //            st = "[" + st + "]";
    //        }
    //        else if (!trimmed.StartsWith("["))
    //        {
    //            errMsg = "Invalid JSON format for DataTable conversion.";
    //            return "Invalid JSON format.";
    //        }

    //        // Deserialize safely
    //        datatable = (DataTable)JsonConvert.DeserializeObject(st, typeof(DataTable));

    //        if (datatable != null && datatable.Rows.Count > 0)
    //        {
    //            errMsg = "Success";
    //        }
    //        else
    //        {
    //            datatable = new DataTable();
    //            errMsg = "Success";
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error converting object to datatable");
    //        datatable = new DataTable();
    //        errMsg = ex.Message;
    //        return ex.Message;
    //    }

    //    return "";
    //}

    //public object ConvertDatatableToObject(ref object jsonObject, DataTable datatable, ref string errMsg)
    //{
    //    try
    //    {
    //        if (datatable == null || datatable.Rows.Count == 0)
    //        {
    //            errMsg = "DataTable is null or empty.";
    //            return null;
    //        }
    //        string json = JsonConvert.SerializeObject(datatable);
    //        var resultObject = JsonConvert.DeserializeObject<object>(json);

    //        errMsg = "Success";
    //        jsonObject = resultObject;
    //        return resultObject;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error converting datatable to object");
    //        errMsg = ex.Message;
    //        return null;
    //    }
    //}

    //public string ConvertListToObject<T>(List<T> list, ref object obj, ref string errMsg)
    //{
    //    try
    //    {
    //        string json = JsonConvert.SerializeObject(list);
    //        if (!string.IsNullOrEmpty(json))
    //        {
    //            errMsg = "Success";
    //        }
    //        obj = JsonConvert.DeserializeObject<object>(json);
    //        return errMsg;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error converting list to object");
    //        errMsg = ex.Message;
    //        return errMsg;
    //    }
    //}

    //public DataTable ConvertListInToDataTable<T>(List<T> dataList)
    //{
    //    DataTable dataTable = new DataTable();
    //    PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    //    foreach (PropertyInfo prop in properties)
    //    {
    //        dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
    //    }

    //    foreach (T item in dataList)
    //    {
    //        DataRow row = dataTable.NewRow();
    //        foreach (PropertyInfo prop in properties)
    //        {
    //            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
    //        }
    //        dataTable.Rows.Add(row);
    //    }

    //    return dataTable;
    //}

    //public DataTable ConvertJObjectToDataTables(JObject jsonData, string objName)
    //{
    //    DataTable dataTable = new DataTable();

    //    JObject enquiryDetails = jsonData[objName] as JObject;
    //    if (enquiryDetails != null)
    //    {
    //        FillDataTableFromJObject(dataTable, enquiryDetails);
    //    }
    //    return dataTable;
    //}

    //public DataTable ConvertJArrayToDataTables(JObject jsonData, string objName)
    //{
    //    DataTable dataTable = new DataTable();

    //    JArray jsonArray = jsonData[objName] as JArray;
    //    if (jsonArray != null && jsonArray.Count > 0)
    //    {
    //        foreach (JObject item in jsonArray)
    //        {
    //            FillDataTableFromJObject(dataTable, item);
    //        }
    //    }
    //    return dataTable;
    //}

    //private void FillDataTableFromJObject(DataTable dataTable, JObject dataObject)
    //{
    //    DataRow row = dataTable.NewRow();

    //    foreach (var property in dataObject.Properties())
    //    {
    //        string columnName = property.Name;
    //        JToken columnValue = property.Value;

    //        DataColumn column = new DataColumn(columnName, typeof(string));
    //        if (!dataTable.Columns.Contains(columnName))
    //        {
    //            dataTable.Columns.Add(column);
    //        }

    //        row[columnName] = columnValue.ToString();
    //    }

    //    dataTable.Rows.Add(row);
    //}

    //public DataTable ConvertJsonToDataTable(JObject jsonObject)
    //{
    //    DataTable table = new DataTable();
    //    foreach (var property in jsonObject.Properties())
    //    {
    //        table.Columns.Add(property.Name, typeof(string));
    //    }
    //    DataRow row = table.NewRow();
    //    foreach (var property in jsonObject.Properties())
    //    {
    //        row[property.Name] = property.Value.ToString();
    //    }
    //    table.Rows.Add(row);
    //    return table;
    //}

    //public DataTable ConvertJArrayToDataTable(JArray jsonArray)
    //{
    //    return JsonConvert.DeserializeObject<DataTable>(jsonArray.ToString());
    //}

    //public string ConvertDataTableToJsonString(DataTable dataTable)
    //{
    //    return JsonConvert.SerializeObject(dataTable, new JsonSerializerSettings
    //    {
    //        MaxDepth = int.MaxValue,
    //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    //    });
    //}

    //public DataTable ConvertJsonStringToDataTable(string jsonString)
    //{
    //    return JsonConvert.DeserializeObject<DataTable>(jsonString);
    //}

    //public string ConvertDataSetsToJsonString(DataSet dataset)
    //{
    //    try
    //    {
    //        return JsonConvert.SerializeObject(dataset, new JsonSerializerSettings
    //        {
    //            MaxDepth = int.MaxValue,
    //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error converting dataset to JSON string");
    //        return "Error: " + ex.Message;
    //    }
    //}

    //public DataSet ConvertJsonStringToDataSet(string jsonString)
    //{
    //    return JsonConvert.DeserializeObject<DataSet>(jsonString);
    //}

    #endregion

    #region Encryption/Decryption

    //public string Encrypt(string str)
    //{
    //    string rts = "";
    //    int i;

    //    for (i = 0; i <= str.Length - 1; i++)
    //    {
    //        rts = rts + (BitConverter.ToInt32(Encoding.ASCII.GetBytes(str.Substring(i, 1)), 0) * 4).ToString();
    //    }
    //    char[] stringArray = rts.ToCharArray();
    //    Array.Reverse(stringArray);
    //    string reverseString = new string(stringArray);

    //    return reverseString;
    //}

    //public string Decrypt(string str)
    //{
    //    string rts = "";
    //    int i;

    //    char[] stringArray = str.ToCharArray();
    //    Array.Reverse(stringArray);
    //    string reverseString = new string(stringArray);

    //    for (i = 0; i <= (reverseString).Length - 1; i += 3)
    //    {
    //        rts = rts + Convert.ToChar(int.Parse(reverseString.Substring(i, 3)) / 4);
    //    }

    //    return rts;
    //}

    //public string EncryptPassword(string s)
    //{
    //    string p = "";
    //    string k;
    //    long i;
    //    for (i = 0; i < s.Length; i++)
    //    {
    //        k = s.Substring(int.Parse(i.ToString()), 1);
    //        int a;
    //        if (int.TryParse(k, out a) == true)
    //        {
    //            p = p.ToString() + (int.Parse(k) + 7);
    //        }
    //        else
    //        {
    //            p = p.ToString() + (Encoding.ASCII.GetBytes(k)[0] + 7);
    //        }
    //    }
    //    s = "";
    //    long X = 0;
    //    for (i = 0; i < p.Length; i++)
    //    {
    //        if (p.Length >= X + 4)
    //        {
    //            k = p.Substring(int.Parse(X.ToString()), 4);
    //            if (k != "")
    //            {
    //                s = s.ToString() + (int.Parse(k) * 7);
    //                X += 4;
    //            }
    //        }
    //        else if (p.Length - X > 0)
    //        {
    //            k = p.Substring(int.Parse(X.ToString()), (int)(p.Length - X));
    //            if (k != "")
    //            {
    //                s = s.ToString() + (int.Parse(k) * 7);
    //                X += 4;
    //            }
    //        }
    //        else
    //            break;
    //    }
    //    return s;
    //}

    //public string ChangePassword(string s)
    //{
    //    long i = 0;
    //    string p = "", k = "";
    //    long X = 0;

    //    // First loop: Transform characters into numbers
    //    for (i = 1; i <= s.Length; i++)
    //    {
    //        k = s.Substring((int)i - 1, 1);
    //        if (char.IsDigit(k[0]))
    //        {
    //            p += (Convert.ToInt32(k) + 7).ToString();
    //        }
    //        else
    //        {
    //            p += (Convert.ToInt32(k[0]) + 7).ToString();
    //        }
    //    }

    //    s = "";
    //    X = 1;

    //    for (i = 1; i <= p.Length; i += 4)
    //    {
    //        if ((i - 1 + 4) <= p.Length)
    //        {
    //            k = p.Substring((int)i - 1, 4);
    //        }
    //        else
    //        {
    //            k = p.Substring((int)i - 1);
    //        }

    //        if (!string.IsNullOrEmpty(k))
    //        {
    //            s += (Convert.ToInt64(k) * 7).ToString();
    //        }
    //    }

    //    return s;
    //}

    public string EncryptString(string inputString)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            byte[] inputData = Encoding.UTF8.GetBytes(inputString);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(inputData, 0, inputData.Length);
                    csEncrypt.FlushFinalBlock();
                    byte[] encryptedBytes = msEncrypt.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
    }

    public string DecryptString(string encryptedString)
    {
        encryptedString = CleanBase64String(encryptedString.Replace("'", ""));

        byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream originalMemory = new MemoryStream())
                    {
                        csDecrypt.CopyTo(originalMemory);
                        byte[] decryptedBytes = originalMemory.ToArray();
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
        }
    }

    private string CleanBase64String(string base64)
    {
        base64 = base64.Trim().Replace(' ', '+').Replace('\n', '+').Replace('\r', '+');

        int padding = 4 - (base64.Length % 4);
        if (padding < 4)
        {
            base64 += new string('=', padding);
        }

        return base64;
    }

    #endregion

    #region Validation & Utilities

    //public bool IsValidEmail(string email)
    //{
    //    string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
    //    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
    //    return regex.IsMatch(email);
    //}

    //public bool CheckAuthories(string moduleName, string actionType, string transactionDetails = "")
    //{
    //    int userId = _currentUserService.GetUserId() ?? 0;
    //    int companyId = _currentUserService.GetCompanyId() ?? 0;
    //    try
    //    {
    //        DataTable dt = new DataTable();
    //        int moduleID = 0;
    //        string str = "Select Isnull(" + actionType + ",'False') As Action,MM.ModuleID From UserModuleAuthentication As A Inner Join ModuleMaster As MM On MM.ModuleID=A.ModuleID And MM.CompanyID=A.CompanyID Where Isnull(A.IsDeletedTransaction,0)=0 And A.UserID=" + userId + " And MM.ModuleName='" + moduleName + "' And A.CompanyID=" + companyId;
    //        FillDataTable(ref dt, str);
    //        if (dt.Rows.Count > 0)
    //        {
    //            if (dt.Rows[0]["Action"].ToString() == "false")
    //            {
    //                return false;
    //            }
    //            moduleID = int.Parse(dt.Rows[0]["ModuleID"].ToString());
    //        }
    //        else
    //        {
    //            return false;
    //        }

    //        ExecuteNonSQLQuery(@"Insert Into UserTransactionLogs( ModuleID, ModuleName, Details, RecordID, RecordName, ActionType, UserID, CompanyID, CreatedDate) Values(" + moduleID + ",'" + moduleName + "','" + actionType + " performed on " + transactionDetails + "',0,'" + transactionDetails + "','" + actionType + "'," + userId + "," + companyId + ",Getdate())");
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error checking authorities");
    //        return false;
    //    }
    //}

    public async Task<string> ValidateProductionUnitAsync(string method)
    {
        using var connection = GetTenantConnection();
        
        var userId = _currentUserService.GetUserId() ?? 0;
        var productionUnitID = _currentUserService.GetProductionUnitId() ?? 0;

        string sql = @"
            SELECT COUNT(1) 
            FROM UserProductionUnitAuthority 
            WHERE ISNULL(CrudOperation, 0) = 1 
              AND UserID = @UserID 
              AND ProductionUnitID = @ProductionUnitID 
              AND ISNULL(IsDeletedTransaction, 0) = 0";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserID = userId, ProductionUnitID = productionUnitID });

        if (count > 0)
        {
            return "Authorize";
        }
        else
        {
            return "You are not authorized to save changes. You only have permission to view this data.";
        }
    }

    public async Task<string> GetProductionUnitIdsAsync()
    {
        using var connection = GetTenantConnection();
        var userId = _currentUserService.GetUserId() ?? 0;

        const string sql = "Select ProductionUnitID from UserProductionUnitAuthority Where Isnull(CanView,0) = 1 AND UserID = @UserID And Isnull(IsDeletedTransaction,0) = 0";
        var ids = await connection.QueryAsync<long>(sql, new { UserID = userId });
        
        return string.Join(",", ids);
    }

    private class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }

    #endregion
}
