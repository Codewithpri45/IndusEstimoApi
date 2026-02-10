using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using IndasEstimo.Domain.Entities.Tenant;
using IndasEstimo.Domain.Enums;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Infrastructure.Security;
using IndasEstimo.Infrastructure.Extensions;

namespace IndasEstimo.Infrastructure.Database.Services;

public class TenantDbService : ITenantDbService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<TenantDbService> _logger;

    public TenantDbService(
        IDbConnectionFactory connectionFactory,
        ITenantProvider tenantProvider,
        IPasswordHasher passwordHasher,
        ILogger<TenantDbService> logger)
    {
        _connectionFactory = connectionFactory;
        _tenantProvider = tenantProvider;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    private SqlConnection GetTenantConnection()
    {
        var tenantInfo = _tenantProvider.GetCurrentTenant();
        return _connectionFactory.CreateTenantConnection(tenantInfo.ConnectionString);
    }

    //public async Task<User?> GetUserByUsernameAsync(string username)
    //{
    //    using var connection = GetTenantConnection();
    //    await connection.OpenAsync();

    //    var command = new SqlCommand(
    //        "SELECT UA.UserID, UA.UserName, UA.UnderUserID, UA.Password,UA.CompanyID,UA.ProductionUnitID,UA.FYear FROM UserMaster AS UA INNER JOIN CompanyMaster AS CCM ON UA.CompanyID = CCM.CompanyID WHERE ISNULL(UA.IsBlocked,0) = 0  AND ISNULL(UA.IsDeletedUser,0) = 0 AND UA.UserName = @Username",
    //        connection);
    //    command.Parameters.AddWithValue("@Username", username);

    //    // command.LogQuery(_logger);
    //    using var reader = await command.ExecuteReaderAsync();
    //    if (await reader.ReadAsync())
    //    {
    //        return new User
    //        {
    //            UserId = reader.GetInt32(0),
    //            Username = reader.GetString(1),
    //            PasswordHash = "99811",
    //            //Email = reader.IsDBNull(3) ? null : reader.GetString(3),
    //            //FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
    //            Status = 1,
    //            //CreatedAt = reader.GetDateTime(6),
    //            //LastLoginAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
    //            CompanyId = reader.GetInt32(4),
    //            FYear = reader.GetString(6),
    //            ProductionUnitId = reader.GetInt32(5)
    //        };
    //    }

    //    return null;
    //}

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(
            "SELECT UA.UserID, UA.UserName, UA.UnderUserID, UA.Password, UA.CompanyID, UA.ProductionUnitID, UA.FYear FROM UserMaster AS UA INNER JOIN CompanyMaster AS CCM ON UA.CompanyID = CCM.CompanyID WHERE ISNULL(UA.IsBlocked,0) = 0 AND ISNULL(UA.IsDeletedUser,0) = 0 AND UA.UserName = @Username",
            connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserId = Convert.ToInt32(reader.GetValue(0)),

                Username = reader.GetString(1),
                PasswordHash = "99811",
                Status = 1,
                CompanyId = Convert.ToInt32(reader.GetValue(4)),
                ProductionUnitId = Convert.ToInt32(reader.GetValue(5)),

                FYear = reader.GetString(6)
            };
        }

        return null;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(
            "SELECT UserId, Username, Password as PasswordHash,EmailID as Email,UserName as FullName,Case When ISNULL(IsBlocked,0) = 1 then 'Blocked' ELSE 'Active' END as Status,CreationDate as CreatedAt,LastLoginAt as LastLoginAt,CompanyID,FYear,ProductionUnitID FROM UserMaster WHERE UserId = @UserId",
            connection);
        command.Parameters.AddWithValue("@UserId", userId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                Status = reader.GetInt32(5),
                CreatedAt = reader.GetDateTime(6),
                LastLoginAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                CompanyId = reader.GetInt32(8),                
                FYear = reader.GetString(9),
                ProductionUnitId = reader.GetInt32(10)
            };
        }

        return null;
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);

        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return false;
        }

        if (user.Status != (int)UserStatus.Active)
        {
            _logger.LogWarning("User account is not active: {Username}, Status: {Status}",
                username, (UserStatus)user.Status);
            return false;
        }
        var Pass = _passwordHasher.HashPassword(user.PasswordHash);
        var isValidPassword = _passwordHasher.VerifyPassword(password, Pass);

        if (!isValidPassword)
        {
            _logger.LogWarning("Invalid password for user: {Username}", username);
        }

        return isValidPassword;
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        var roles = new List<string>();

        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"SELECT Case When ISNULL(IsAdmin,0) = 1 Then 'Admin' Else 'User' End as RoleName FROM UserMaster WHERE UserId = @UserId", connection);
        command.Parameters.AddWithValue("@UserId", userId);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var connection = GetTenantConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(
            "UPDATE UserMaster SET LastLoginAt = @LastLoginAt WHERE UserId = @UserId",
            connection);
        command.Parameters.AddWithValue("@LastLoginAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UserId", userId);

        // command.LogQuery(_logger);
        await command.ExecuteNonQueryAsync();
    }
}
