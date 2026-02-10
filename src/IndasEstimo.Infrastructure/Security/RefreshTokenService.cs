using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using IndasEstimo.Infrastructure.Extensions;
using IndasEstimo.Domain.Entities.Master;
using IndasEstimo.Infrastructure.Configuration;
using IndasEstimo.Infrastructure.Database;

namespace IndasEstimo.Infrastructure.Security;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IDbConnectionFactory connectionFactory,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings,
        ILogger<RefreshTokenService> logger)
    {
        _connectionFactory = connectionFactory;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateRefreshTokenAsync(int tenantId, int userId)
    {
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            INSERT INTO RefreshTokens (UserId, TenantId, Token, ExpiresAt, CreatedAt, IsRevoked)
            VALUES (@UserId, @TenantId, @Token, @ExpiresAt, @CreatedAt, @IsRevoked)",
            connection);

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@TenantId", tenantId);
        command.Parameters.AddWithValue("@Token", refreshToken);
        command.Parameters.AddWithValue("@ExpiresAt", DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsRevoked", false);

        // command.LogQuery(_logger);
        await command.ExecuteNonQueryAsync();

        return refreshToken;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        return refreshToken?.IsActive ?? false;
    }

    public async Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null)
    {
        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            UPDATE RefreshTokens
            SET IsRevoked = 1,
                RevokedAt = @RevokedAt,
                ReplacedByToken = @ReplacedByToken
            WHERE Token = @Token",
            connection);

        command.Parameters.AddWithValue("@RevokedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@ReplacedByToken", (object?)replacedByToken ?? DBNull.Value);
        command.Parameters.AddWithValue("@Token", token);

        // command.LogQuery(_logger);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateMasterConnection();
        await connection.OpenAsync();

        var command = new SqlCommand(@"
            SELECT Id, UserId, TenantId, Token, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken, IsRevoked
            FROM RefreshTokens
            WHERE Token = @Token",
            connection);
        command.Parameters.AddWithValue("@Token", token);

        // command.LogQuery(_logger);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new RefreshToken
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                TenantId = reader.GetInt32(2),
                Token = reader.GetString(3),
                ExpiresAt = reader.GetDateTime(4),
                CreatedAt = reader.GetDateTime(5),
                RevokedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ReplacedByToken = reader.IsDBNull(7) ? null : reader.GetString(7),
                IsRevoked = reader.GetBoolean(8)
            };
        }

        return null;
    }
}
