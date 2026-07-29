using AiKocStudio.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return context.Users
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserDto(u.Id, u.Email, u.DisplayName, u.Role, u.IsActive))
            .ToListAsync(cancellationToken);
    }
}
