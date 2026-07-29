using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;

namespace AiKocStudio.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = request.Role,
            IsActive = true,
        };

        user.PasswordHash = identityService.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
