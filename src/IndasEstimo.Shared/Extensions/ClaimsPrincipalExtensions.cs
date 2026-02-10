using System.Security.Claims;
using IndasEstimo.Shared.Constants;

namespace IndasEstimo.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetTenantId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(CustomClaimTypes.TenantId);
        return claim != null && int.TryParse(claim.Value, out var tenantId) ? tenantId : null;
    }

    public static string? GetTenantCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(CustomClaimTypes.TenantCode)?.Value;
    }

    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(CustomClaimTypes.UserId);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(CustomClaimTypes.Username)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(CustomClaimTypes.Email)?.Value;
    }

    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        var rolesClaim = principal.FindFirst(CustomClaimTypes.Roles)?.Value;
        return string.IsNullOrEmpty(rolesClaim)
            ? new List<string>()
            : rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static int? GetCompanyId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(CustomClaimTypes.CompanyId);
        return claim != null && int.TryParse(claim.Value, out var companyId) ? companyId : null;
    }

    public static int? GetProductionUnitId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(CustomClaimTypes.ProductionUnitId);
        return claim != null && int.TryParse(claim.Value, out var productionUnitId) ? productionUnitId : null;
    }

    public static string? GetFYear(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(CustomClaimTypes.FYear)?.Value;
    }
}
