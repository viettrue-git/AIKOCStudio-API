using AiKocStudio.Application.Common.Security;
using AiKocStudio.Domain.Constants;
using MediatR;

namespace AiKocStudio.Application.Users.Queries.GetUsers;

[Authorize(Roles = Roles.Admin)]
public record GetUsersQuery : IRequest<List<UserDto>>;
