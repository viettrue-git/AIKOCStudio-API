using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;

namespace AiKocStudio.Application.Personas.Commands.CreatePersona;

public class CreatePersonaCommandHandler(IApplicationDbContext context) : IRequestHandler<CreatePersonaCommand, Guid>
{
    public async Task<Guid> Handle(CreatePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ToneOfVoice = request.ToneOfVoice,
            TargetAudience = request.TargetAudience,
            Platform = request.Platform,
            DefaultAiProvider = request.DefaultAiProvider,
            SystemPromptTemplate = request.SystemPromptTemplate,
            IsActive = true,
        };

        context.Personas.Add(persona);
        await context.SaveChangesAsync(cancellationToken);

        return persona.Id;
    }
}
