using AiKocStudio.Application.Common.Security;
using AiKocStudio.Domain.Constants;
using AiKocStudio.Domain.Enums;
using MediatR;

namespace AiKocStudio.Application.Users.Commands.CreateUser;

[Authorize(Roles = Roles.Admin)]
public record CreateUserCommand(string Email, string Password, string DisplayName, UserRole Role) : IRequest<Guid>;
