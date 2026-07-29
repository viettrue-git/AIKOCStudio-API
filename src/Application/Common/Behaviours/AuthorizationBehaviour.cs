using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.Common.Security;
using MediatR;

namespace AiKocStudio.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = typeof(TRequest).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .ToList();

        if (authorizeAttributes.Count != 0)
        {
            if (currentUserService.UserId is null)
            {
                throw new UnauthorizedAccessException();
            }

            var requiredRoles = authorizeAttributes
                .SelectMany(a => a.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .ToList();

            if (requiredRoles.Count != 0 && !requiredRoles.Any(role => currentUserService.Roles.Contains(role)))
            {
                throw new ForbiddenAccessException();
            }
        }

        return await next();
    }
}
