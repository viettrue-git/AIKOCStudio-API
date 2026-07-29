using MediatR;

namespace AiKocStudio.Application.Personas.Queries.GetPersonaById;

public record GetPersonaByIdQuery(Guid Id) : IRequest<PersonaDto>;
