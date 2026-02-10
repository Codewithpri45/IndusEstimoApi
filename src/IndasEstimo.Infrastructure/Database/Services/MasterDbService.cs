using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Domain.Entities.Master;
using IndasEstimo.Domain.Enums;
using IndasEstimo.Infrastructure.Security;
using IndasEstimo.Infrastructure.Extensions;

namespace IndasEstimo.Infrastructure.Database.Services;

public class MasterDbService : IMasterDbService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAesEncryptionService _encryptionService;
    private readonly ILogger<MasterDbService> _logger;

    public MasterDbService(
        IDbConnectionFactory connectionFactory,
        IAesEncryptionService encryptionService,
        ILogger<MasterDbService> logger)
    {
        _connectionFactory = connectionFactory;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<Tenant?> GetTenantByCodeAsync(string tenantCode)
    {
        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        // Trim whitespace from input
        var trimmedCode = tenantCode?.Trim() ?? string.Empty;

        var command = new SqlCommand(@"
            SELECT
                CAST(CHECKSUM(CompanyCode) AS INT) AS TenantId,
                ISNULL(CompanyCode, '') AS TenantCode,
                ISNULL(CompanyName, '') AS TenantName,
                ISNULL(CompanyCode, '') AS CompanyCode,
                CASE WHEN IsActive = 1 THEN 1 ELSE 2 END AS Status,
                ISNULL(LastLoginDateTime, GETDATE()) AS CreatedAt
            FROM Indus_company_Authentication_For_Web_Modules
            WHERE UPPER(LTRIM(RTRIM(CompanyCode))) = UPPER(LTRIM(RTRIM(@CompanyCode))) AND IsActive = 1",
            connection);
        command.Parameters.AddWithValue("@CompanyCode", trimmedCode);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Tenant
            {
                TenantId = reader.GetInt32(0),
                TenantCode = reader.GetString(1),
                TenantName = reader.GetString(2),
                CompanyCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                Status = reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
    {
        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT
                CAST(CHECKSUM(CompanyCode) AS INT) AS TenantId,
                ISNULL(CompanyCode, '') AS TenantCode,
                ISNULL(CompanyName, '') AS TenantName,
                ISNULL(CompanyCode, '') AS CompanyCode,
                CASE WHEN IsActive = 1 THEN 1 ELSE 2 END AS Status,
                ISNULL(LastLoginDateTime, GETDATE()) AS CreatedAt
            FROM Indus_company_Authentication_For_Web_Modules
            WHERE CAST(CHECKSUM(CompanyCode) AS INT) = @TenantId AND IsActive = 1",
            connection);
        command.Parameters.AddWithValue("@TenantId", tenantId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Tenant
            {
                TenantId = reader.GetInt32(0),
                TenantCode = reader.GetString(1),
                TenantName = reader.GetString(2),
                CompanyCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                Status = reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }

        return null;
    }

    public async Task<string?> GetTenantConnectionStringAsync(int tenantId)
    {
        try
        {
            using var connection = _connectionFactory.CreateMasterConnection();
            await connection.OpenAsync();

            // Get connection string directly from Conn_String column
            var command = new SqlCommand(@"
                SELECT ISNULL(Conn_String, '') AS ConnectionString
                FROM Indus_company_Authentication_For_Web_Modules
                WHERE CAST(CHECKSUM(CompanyCode) AS INT) = @TenantId AND IsActive = 1",
                connection);
            command.Parameters.AddWithValue("@TenantId", tenantId);

            // command.LogQuery(_logger);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var connString = reader.GetString(0);

                if (string.IsNullOrEmpty(connString))
                {
                    _logger.LogWarning("Connection string is empty for tenant {TenantId}", tenantId);
                    return null;
                }

                return connString;
            }

            _logger.LogWarning("Connection string not found for tenant {TenantId}", tenantId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection string for tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<TenantStatus> GetTenantStatusAsync(int tenantId)
    {
        var tenant = await GetTenantByIdAsync(tenantId);
        return tenant != null ? (TenantStatus)tenant.Status : TenantStatus.Suspended;
    }

    public async Task<List<Tenant>> GetAllActiveTenantsAsync()
    {
        var tenants = new List<Tenant>();

        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT
                CAST(CHECKSUM(CompanyCode) AS INT) AS TenantId,
                ISNULL(CompanyCode, '') AS TenantCode,
                ISNULL(CompanyName, '') AS TenantName,
                ISNULL(CompanyCode, '') AS CompanyCode,
                CASE WHEN IsActive = 1 THEN 1 ELSE 2 END AS Status,
                ISNULL(LastLoginDateTime, GETDATE()) AS CreatedAt
            FROM Indus_company_Authentication_For_Web_Modules
            WHERE IsActive = 1
            ORDER BY CompanyName",
            connection);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tenants.Add(new Tenant
            {
                TenantId = reader.GetInt32(0),
                TenantCode = reader.GetString(1),
                TenantName = reader.GetString(2),
                CompanyCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                Status = reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }

        return tenants;
    }

    public async Task<bool> ValidateTenantLicenseAsync(int tenantId)
    {
        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        // Check if company is active
        var command = new SqlCommand(@"
            SELECT IsActive
            FROM Indus_company_Authentication_For_Web_Modules
            WHERE CAST(CHECKSUM(CompanyCode) AS INT) = @TenantId",
            connection);
        command.Parameters.AddWithValue("@TenantId", tenantId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var isActive = reader.GetBoolean(0);
            return isActive;
        }

        return false;
    }
}
