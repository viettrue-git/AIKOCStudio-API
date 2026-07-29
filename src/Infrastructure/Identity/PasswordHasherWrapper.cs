using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AiKocStudio.Infrastructure.Identity;

/// <summary>
/// Thin wrapper over ASP.NET Core Identity's PasswordHasher — reuses its
/// battle-tested PBKDF2 implementation without pulling in the full Identity
/// membership system (UserManager, SignInManager, etc.), which this project
/// doesn't need.
/// </summary>
public class PasswordHasherWrapper : IIdentityService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) => _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // `user` is the same tracked entity the caller (e.g. LoginCommandHandler)
            // holds — mutating it here is picked up by that caller's next
            // SaveChangesAsync without any extra plumbing.
            user.PasswordHash = _hasher.HashPassword(user, password);
        }

        return result != PasswordVerificationResult.Failed;
    }
}
