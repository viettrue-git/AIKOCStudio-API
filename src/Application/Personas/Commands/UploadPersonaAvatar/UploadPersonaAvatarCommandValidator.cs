using AiKocStudio.Application.Common.Models;
using FluentValidation;

namespace AiKocStudio.Application.Personas.Commands.UploadPersonaAvatar;

public class UploadPersonaAvatarCommandValidator : AbstractValidator<UploadPersonaAvatarCommand>
{
    public UploadPersonaAvatarCommandValidator()
    {
        RuleFor(x => x.PersonaId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedImageContentTypes.Values.Contains(ct))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedImageContentTypes.Values)}");

        RuleFor(x => x)
            .Must(x => ImageSignatureValidator.MatchesContentType(x.Content, x.ContentType))
            .WithMessage("File content does not match the declared content type.")
            .WithName("Content");
    }
}
