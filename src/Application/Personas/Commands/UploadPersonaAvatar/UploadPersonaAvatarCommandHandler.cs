using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Personas.Commands.UploadPersonaAvatar;

public class UploadPersonaAvatarCommandHandler(
    IApplicationDbContext context,
    IFileStorageService fileStorageService) : IRequestHandler<UploadPersonaAvatarCommand, string>
{
    public async Task<string> Handle(UploadPersonaAvatarCommand request, CancellationToken cancellationToken)
    {
        var persona = await context.Personas.FirstOrDefaultAsync(p => p.Id == request.PersonaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Persona), request.PersonaId);

        var url = await fileStorageService.UploadAsync(request.Content, request.FileName, request.ContentType, cancellationToken);

        persona.AvatarUrl = url;
        await context.SaveChangesAsync(cancellationToken);

        return url;
    }
}
