using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Personas.Queries.GetPersonaById;

public class GetPersonaByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPersonaByIdQuery, PersonaDto>
{
    public async Task<PersonaDto> Handle(GetPersonaByIdQuery request, CancellationToken cancellationToken)
    {
        var persona = await context.Personas.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Persona), request.Id);

        return PersonaDto.FromEntity(persona);
    }
}
