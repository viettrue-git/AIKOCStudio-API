namespace AiKocStudio.Application.Common.Exceptions;

/// <summary>
/// Thrown for invalid credentials or an invalid/expired/revoked refresh token.
/// Deliberately uses one generic message for all causes to avoid leaking
/// which part of a login attempt was wrong (user enumeration protection).
/// </summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException()
        : base("Invalid credentials.")
    {
    }
}
