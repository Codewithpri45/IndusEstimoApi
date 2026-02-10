using Microsoft.AspNetCore.Http;
using IndasEstimo.Shared.Extensions;
using IndasEstimo.Application.Interfaces.Services;

namespace IndasEstimo.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.GetUserId();
    }

    public int? GetCompanyId()
    {
        return _httpContextAccessor.HttpContext?.User?.GetCompanyId();
    }

    public int? GetProductionUnitId()
    {
        return _httpContextAccessor.HttpContext?.User?.GetProductionUnitId();
    }

    public string? GetProductionUnitIdStr()
    {
        // For now, return the single ProductionUnitId as a string
        // If multiple production units are needed, this can be enhanced
        var productionUnitId = GetProductionUnitId();
        return productionUnitId?.ToString() ?? "0";
    }

    public string? GetFYear()
    {
        return _httpContextAccessor.HttpContext?.User?.GetFYear();
    }

    public int? GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User?.GetTenantId();
    }

    public string? GetUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.GetUsername();
    }
}
