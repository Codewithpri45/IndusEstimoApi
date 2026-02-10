using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using System.Text;

namespace IndasEstimo.Infrastructure.Extensions;

public static class SqlCommandExtensions
{
    public static void LogQuery(this SqlCommand command, ILogger logger)
    {
        var fullQuery = command.ToDebugString();
        logger.LogWarning("FULL QUERY WITH VALUES:\n{FullQuery}", fullQuery);
    }

    public static string ToDebugString(this SqlCommand command)
    {
        var query = command.CommandText;

        // Sort parameters by name length descending to avoid partial replacements (e.g., @ID replacing part of @ID2)
        var parameters = command.Parameters.Cast<SqlParameter>()
            .OrderByDescending(p => p.ParameterName.Length)
            .ToList();

        foreach (var param in parameters)
        {
            var value = param.Value == null || param.Value == DBNull.Value ? "NULL" :
                        (IsStringOrDate(param.SqlDbType) ? FormatValue(param.Value) : 
                        (param.Value is bool b ? (b ? "1" : "0") : 
                        (param.Value?.ToString() ?? "NULL")));

            // Use simple string replacement for now, as parameter names in this project are distinctive.
            // Using Regex would be safer but might require more complex handling of special characters.
            query = query.Replace(param.ParameterName, value);
        }

        return query;
    }

    private static bool IsStringOrDate(SqlDbType dbType)
    {
        return dbType == SqlDbType.Char || 
               dbType == SqlDbType.NChar || 
               dbType == SqlDbType.VarChar || 
               dbType == SqlDbType.NVarChar || 
               dbType == SqlDbType.Text || 
               dbType == SqlDbType.NText || 
               dbType == SqlDbType.Date || 
               dbType == SqlDbType.DateTime || 
               dbType == SqlDbType.DateTime2 || 
               dbType == SqlDbType.SmallDateTime || 
               dbType == SqlDbType.UniqueIdentifier ||
               dbType == SqlDbType.Time;
    }

    private static string FormatValue(object value)
    {
        if (value is DateTime dt)
        {
            return $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'";
        }
        return $"'{value}'";
    }
}
