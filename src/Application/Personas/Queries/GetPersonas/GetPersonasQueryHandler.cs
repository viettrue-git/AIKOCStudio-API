using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Personas.Queries.GetPersonas;

public class GetPersonasQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPersonasQuery, PagedResult<PersonaDto>>
{
    public async Task<PagedResult<PersonaDto>> Handle(GetPersonasQuery request, CancellationToken cancellationToken)
    {
        var query = context.Personas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (request.PlatformFilter is not null)
        {
            query = query.Where(p => p.Platform == request.PlatformFilter);
        }

        if (request.IsActiveFilter is not null)
        {
            query = query.Where(p => p.IsActive == request.IsActiveFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Materialize entities first, then map to DTOs in-memory — PersonaDto.FromEntity
        // can't be translated to SQL by a real provider (only appears to work under
        // EF Core InMemory, which falls back to LINQ-to-Objects).
        var entities = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(PersonaDto.FromEntity).ToList();

        return new PagedResult<PersonaDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
