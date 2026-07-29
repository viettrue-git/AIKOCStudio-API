namespace AiKocStudio.Infrastructure.Identity;

/// <summary>
/// Shared between JwtTokenService (issuing) and CurrentUserService (reading).
/// Program.cs sets MapInboundClaims = false so these short names survive
/// untouched instead of being remapped to long ClaimTypes.* URIs.
/// </summary>
public static class JwtClaimTypes
{
    public const string UserId = "sub";
    public const string TenantId = "tenant";
    public const string Role = "role";
}
