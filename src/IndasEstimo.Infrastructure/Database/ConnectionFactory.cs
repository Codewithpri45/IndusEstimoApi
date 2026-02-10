using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IndasEstimo.Infrastructure.Database;

public interface IDbConnectionFactory
{
    SqlConnection CreateMasterConnection();
    SqlConnection CreateTenantConnection(string connectionString);
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _masterConnectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _masterConnectionString = configuration.GetConnectionString("MasterDatabase")
            ?? throw new InvalidOperationException("Master database connection string is not configured");
    }

    public SqlConnection CreateMasterConnection()
    {
        return new SqlConnection(_masterConnectionString);
    }

    public SqlConnection CreateTenantConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }
}
