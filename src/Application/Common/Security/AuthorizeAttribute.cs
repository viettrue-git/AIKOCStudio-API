namespace AiKocStudio.Application.Common.Security;

/// <summary>
/// MediatR request-level authorization, checked by AuthorizationBehaviour.
/// Distinct from ASP.NET Core's [Authorize] (which still gates the controller
/// action itself) — this is a second, defense-in-depth check at the command/query
/// handler level so a handler is never reachable without its declared role,
/// regardless of which controller ends up calling it.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
    public string Roles { get; set; } = string.Empty;
}
