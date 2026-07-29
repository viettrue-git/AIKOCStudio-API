using AiKocStudio.Domain.Entities;

namespace AiKocStudio.Application.Common.Interfaces;

public interface IIdentityService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
