using AiKocStudio.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AiKocStudio.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private System.Security.Claims.ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(JwtClaimTypes.UserId)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = User?.FindFirst(JwtClaimTypes.TenantId)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IEnumerable<string> Roles =>
        User?.FindAll(JwtClaimTypes.Role).Select(c => c.Value) ?? [];
}
