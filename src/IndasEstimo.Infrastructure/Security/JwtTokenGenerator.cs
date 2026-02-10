using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IndasEstimo.Domain.Entities.Master;
using IndasEstimo.Domain.Entities.Tenant;
using IndasEstimo.Infrastructure.Configuration;
using IndasEstimo.Shared.Constants;

namespace IndasEstimo.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(Tenant tenant, User user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(CustomClaimTypes.TenantId, tenant.TenantId.ToString()),
            new(CustomClaimTypes.TenantCode, tenant.TenantCode),
            new(CustomClaimTypes.UserId, user.UserId.ToString()),
            new(CustomClaimTypes.Username, user.Username),
            new(CustomClaimTypes.Email, user.Email ?? string.Empty),
            new(CustomClaimTypes.Roles, string.Join(",", roles)),
            new(CustomClaimTypes.CompanyId, user.CompanyId.ToString()),
            new(CustomClaimTypes.ProductionUnitId, user.ProductionUnitId.ToString()),
            new(CustomClaimTypes.FYear, user.FYear),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public int? ValidateTokenAndExtractTenantId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantId);

            return tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId)
                ? tenantId
                : null;
        }
        catch
        {
            return null;
        }
    }
}
