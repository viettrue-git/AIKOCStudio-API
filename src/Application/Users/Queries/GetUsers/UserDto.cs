using AiKocStudio.Domain.Enums;

namespace AiKocStudio.Application.Users.Queries.GetUsers;

public record UserDto(Guid Id, string Email, string DisplayName, UserRole Role, bool IsActive);
