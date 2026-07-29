using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Personas.Commands.UpdatePersona;

public class UpdatePersonaCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdatePersonaCommand>
{
    public async Task Handle(UpdatePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = await context.Personas.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Persona), request.Id);

        persona.Name = request.Name;
        persona.Description = request.Description;
        persona.ToneOfVoice = request.ToneOfVoice;
        persona.TargetAudience = request.TargetAudience;
        persona.Platform = request.Platform;
        persona.DefaultAiProvider = request.DefaultAiProvider;
        persona.SystemPromptTemplate = request.SystemPromptTemplate;
        persona.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);
    }
}
