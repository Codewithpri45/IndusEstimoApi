using IndasEstimo.Domain.Entities.Master;
using IndasEstimo.Domain.Entities.Tenant;

namespace IndasEstimo.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Tenant tenant, User user, List<string> roles);
    string GenerateRefreshToken();
    int? ValidateTokenAndExtractTenantId(string token);
}
