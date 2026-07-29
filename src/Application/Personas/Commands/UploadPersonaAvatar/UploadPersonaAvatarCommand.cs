using MediatR;

namespace AiKocStudio.Application.Personas.Commands.UploadPersonaAvatar;

public record UploadPersonaAvatarCommand(Guid PersonaId, Stream Content, string FileName, string ContentType) : IRequest<string>;
