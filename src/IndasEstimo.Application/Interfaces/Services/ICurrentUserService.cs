namespace IndasEstimo.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int? GetUserId();
    int? GetCompanyId();
    int? GetProductionUnitId();
    string? GetProductionUnitIdStr();
    string? GetFYear();
    int? GetTenantId();
    string? GetUsername();
}
