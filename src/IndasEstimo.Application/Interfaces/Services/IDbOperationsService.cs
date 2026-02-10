using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace IndasEstimo.Application.Interfaces.Services;

/// <summary>
/// Database operations service interface - migrated from old Db_Function.cs
/// Provides all database operations, JSON conversions, and utility functions
/// </summary>
public interface IDbOperationsService
{
    // ==================== Database Query Operations ====================
    
    /// <summary>
    /// Execute a non-query SQL statement (INSERT, UPDATE, DELETE)
    /// </summary>
    //string ExecuteNonSQLQuery(string query);
    //string FillDataTable(ref DataTable dt, string query);
    //string GetColumnValue(string columnName, string tableName, string whereCndtn, string orderBy = "");
    //bool IsDeletable(string fieldName, string tableName, string searchCondition = "");
    //string CkeckDuplicate(string tableName, string fieldname, string searchCondition = "");
    //int GetID(string tableName, string refFieldName, string refFieldValue, string fieldName);
    //string GetVoucherNo(string tableName, string refFieldName, string refFieldValue, string fieldName);
    
    // ==================== Insert Operations ====================
    
    /// <summary>
    /// Insert database from JSON object with transaction
    /// </summary>
    //string InsertDatatableToDatabase(object jsonObject, string tableName, string addColName, 
    //    string addColValue, ref SqlConnection con, ref SqlTransaction objTrans, string voucherType = "", string transactionID = "");

    /// <summary>
    /// Modern Async Insert using Dapper
    /// </summary>
    Task<int> InsertDataAsync<T>(string tableName, T data, IDbConnection connection, IDbTransaction transaction, string idFieldName = "ID");
    
    /// <summary>
    /// Insert data from JSON object to database without transaction
    /// </summary>
    //string InsertDatatableToDatabaseWithouttrans(object jsonObject, string tableName, string addColName, 
    //    string addColValue, string voucherType = "", string transactionID = "");
    
    //string InsertlistToDatabase(DataTable jsonObject, string tableName, string addColName, string addColValue, 
    //    ref SqlConnection con, ref SqlTransaction objTrans, string voucherType = "", string transactionID = "");
    
    //string AddToDatabaseOperation(object pObject, string sTableName, string addColName, string addColValue, 
    //    ref SqlConnection con, ref SqlTransaction objTrans);

    //string InsertSecondaryDataJobCard(DataTable dts, string sTableName, string addColName, string addColValue, 
    //    ref SqlConnection con, ref SqlTransaction objTrans, string pTableName, string sTableColumn, 
    //    string pTblMaxColumn, string whereCndtn = "");
    
    /// <summary>
    /// Unified dynamic insert method that handles single objects or collections.
    /// Automatically manages audit fields and parent record linking.
    /// </summary>
    Task<long> InsertDataAsync(string tableName, object data, SqlConnection con, SqlTransaction trans, 
        string idFieldName = "TransactionID", string addColName = "", string addColValue = "", 
        string voucherType = "", long parentTransactionId = 0);
    
    // ==================== Update Operations ====================
    
    /// <summary>
    /// Update database from JSON object with transaction
    /// </summary>
    //string UpdateDatatableToDatabasewithtrans(object jsonObject, string tableName, string addColName, int pvalue, 
    //    ref SqlConnection con, ref SqlTransaction objTrans, string wherecndtn = "");
    
    //string UpdateDatatableToDatabase(object jsonObject, string tableName, string addColName, int pvalue, 
    //    string wherecndtn = "");
    
    /// <summary>
    /// Update a record dynamically using Dapper
    /// </summary>
    Task UpdateDataAsync(string tableName, object data, SqlConnection con, SqlTransaction trans, string[] whereFields, string addColName = "", string extraWhere = "");

    /// <summary>
    /// Update integration status
    /// </summary>
    //string UpdateIntegrationStatus(string tableName, string refFieldName, string refFieldValue, string whereCndtn = "");
    
    // ==================== Delete Operations ====================
    
    /// <summary>
    /// Delete data from table
    /// </summary>
    //string DeleteData(string tableName, ref SqlConnection con, ref SqlTransaction objTrans, string searchCondition);
    
    //string GeneratePrefixedNo(string tableName, string prefix, string maxFieldName, ref int maxNoVariable, 
    //    string fYear, string searchCondition = "");
    
    //long GenerateMaxVoucherNo(string tableName, string fieldname, string searchCondition = "");

    /// <summary>
    /// Generate generic voucher number with prefix and year
    /// </summary>
    Task<(string VoucherNo, long MaxNo)> GenerateVoucherNoAsync(
        string tableName, 
        long voucherId, 
        string prefix, 
        string maxFieldName = "MaxVoucherNo");
    
    // ==================== JSON/DataTable Conversion ====================
    
    /// <summary>
    /// Convert JSON object to DataTable
    /// </summary>
    //string ConvertObjectToDatatable(object jsonObject, ref DataTable datatable, ref string errMsg);
    //object ConvertDatatableToObject(ref object jsonObject, DataTable datatable, ref string errMsg);
    //string ConvertListToObject<T>(List<T> list, ref object obj, ref string errMsg);
    //DataTable ConvertListInToDataTable<T>(List<T> dataList);
    //DataTable ConvertJObjectToDataTables(JObject jsonData, string objName);
    //DataTable ConvertJArrayToDataTables(JObject jsonData, string objName);
    //DataTable ConvertJsonToDataTable(JObject jsonObject);
    //DataTable ConvertJArrayToDataTable(JArray jsonArray);
    //string ConvertDataTableToJsonString(DataTable dataTable);
    //DataTable ConvertJsonStringToDataTable(string jsonString);
    //string ConvertDataSetsToJsonString(DataSet dataset);
    //DataSet ConvertJsonStringToDataSet(string jsonString);
    
    // ==================== Encryption/Decryption ====================
    
    /// <summary>
    /// Encrypt string (old method)
    /// </summary>
    //string Encrypt(string str);
    //string Decrypt(string str);
    //string EncryptPassword(string s);
    //string ChangePassword(string s);
    
    /// <summary>
    /// Encrypt string using AES
    /// </summary>
    string EncryptString(string inputString);
    
    /// <summary>
    /// Decrypt string using AES
    /// </summary>
    string DecryptString(string encryptedString);
    
    // ==================== Validation & Utilities ====================
    
    /// <summary>
    /// Validate email format
    /// </summary>
    //bool IsValidEmail(string email);
    //bool CheckAuthories(string moduleName, string actionType, string transactionDetails = "");
    
    /// <summary>
    /// Validate production unit access
    /// </summary>
    Task<string> ValidateProductionUnitAsync(string method);
    
    /// <summary>
    /// Get production unit IDs as comma-separated string for current user
    /// </summary>
    Task<string> GetProductionUnitIdsAsync();
}
