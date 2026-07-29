using MediatR;

namespace AiKocStudio.Application.Personas.Commands.DeletePersona;

public record DeletePersonaCommand(Guid Id) : IRequest;
