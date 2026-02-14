using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IndasEstimo.Application.Common;
using IndasEstimo.Application.DTOs.Auth;
using IndasEstimo.Application.Interfaces.Services;
using IndasEstimo.Domain.Enums;
using IndasEstimo.Infrastructure.Database.Services;
using IndasEstimo.Infrastructure.Security;
using IndasEstimo.Infrastructure.MultiTenancy;
using IndasEstimo.Shared.Constants;

namespace IndasEstimo.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IMasterDbService _masterDbService;
    private readonly ITenantDbService _tenantDbService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IMasterDbService masterDbService,
        ITenantDbService tenantDbService,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _masterDbService = masterDbService;
        _tenantDbService = tenantDbService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> AuthenticateAsync(string tenantCode, string username, string password)
    {
        try
        {
            // Step 1: Validate tenant exists and is active
            var tenant = await _masterDbService.GetTenantByCodeAsync(tenantCode);
            if (tenant == null)
            {
                return Result<LoginResponse>.Failure("Tenant not found");
            }

            if (tenant.Status != (int)TenantStatus.Active)
            {
                return Result<LoginResponse>.Failure($"Tenant is {(TenantStatus)tenant.Status}");
            }

            // Step 2: Get tenant connection string
            var connectionString = await _masterDbService.GetTenantConnectionStringAsync(tenant.TenantId);
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string not found for tenant {TenantId}", tenant.TenantId);
                return Result<LoginResponse>.Failure("Tenant database configuration not found");
            }

            // Step 3: Temporarily set tenant context for TenantDbContext
            var tenantInfo = new TenantInfo
            {
                TenantId = tenant.TenantId,
                TenantCode = tenant.TenantCode,
                ConnectionString = connectionString
            };
            _httpContextAccessor.HttpContext!.Items[HttpContextKeys.TenantInfo] = tenantInfo;

            // Step 4: Validate user credentials in tenant database
            var isValid = await _tenantDbService.ValidateUserCredentialsAsync(username, password);
            if (!isValid)
            {
                return Result<LoginResponse>.Failure("Invalid username or password");
            }

            // Step 5: Get user details and roles
            var user = await _tenantDbService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("User not found");
            }

            var roles = await _tenantDbService.GetUserRolesAsync(user.UserId);

            // Step 6: Generate tokens
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(tenant, user, roles);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(tenant.TenantId, user.UserId);

            // Step 7: Update last login
            await _tenantDbService.UpdateLastLoginAsync(user.UserId);

            var response = new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(60), // From JwtSettings
                User: new UserInfoDto(
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.FullName,
                    roles));

            _logger.LogInformation("User {Username} logged in successfully to tenant {TenantCode}",
                username, tenantCode);

            return Result<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username} in tenant {TenantCode}",
                username, tenantCode);
            return Result<LoginResponse>.Failure("An error occurred during authentication");
        }
    }

    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenService.GetRefreshTokenAsync(refreshToken);

        if (token == null || !token.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure("Invalid or expired refresh token");
        }

        // Get tenant and user info
        var tenant = await _masterDbService.GetTenantByIdAsync(token.TenantId);
        if (tenant == null)
        {
            return Result<RefreshTokenResponse>.Failure("Tenant not found");
        }

        // Set tenant context
        var connectionString = await _masterDbService.GetTenantConnectionStringAsync(token.TenantId);
        var tenantInfo = new TenantInfo
        {
            TenantId = tenant.TenantId,
            TenantCode = tenant.TenantCode,
            ConnectionString = connectionString!
        };
        _httpContextAccessor.HttpContext!.Items[HttpContextKeys.TenantInfo] = tenantInfo;

        var user = await _tenantDbService.GetUserByIdAsync(token.UserId);
        if (user == null)
        {
            return Result<RefreshTokenResponse>.Failure("User not found");
        }

        var roles = await _tenantDbService.GetUserRolesAsync(user.UserId);

        // Generate new tokens
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(tenant, user, roles);
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(tenant.TenantId, user.UserId);

        // Revoke old refresh token
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, newRefreshToken);

        var response = new RefreshTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(60));

        return Result<RefreshTokenResponse>.Success(response);
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);
        return Result.Success();
    }

    public async Task<Result<UserInfoDto>> GetUserInfoAsync(int userId)
    {
        var user = await _tenantDbService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Result<UserInfoDto>.Failure("User not found");
        }

        var roles = await _tenantDbService.GetUserRolesAsync(userId);

        var userInfo = new UserInfoDto(
            user.UserId,
            user.Username,
            user.Email,
            user.FullName,
            roles);

        return Result<UserInfoDto>.Success(userInfo);
    }
}
