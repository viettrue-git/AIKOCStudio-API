using AiKocStudio.Domain.Entities;
using AiKocStudio.Domain.Enums;

namespace AiKocStudio.Application.Personas;

public record PersonaDto(
    Guid Id,
    string Name,
    string Description,
    string ToneOfVoice,
    string TargetAudience,
    Platform Platform,
    string? DefaultAiProvider,
    string SystemPromptTemplate,
    string? AvatarUrl,
    bool IsActive)
{
    public static PersonaDto FromEntity(Persona persona) => new(
        persona.Id,
        persona.Name,
        persona.Description,
        persona.ToneOfVoice,
        persona.TargetAudience,
        persona.Platform,
        persona.DefaultAiProvider,
        persona.SystemPromptTemplate,
        persona.AvatarUrl,
        persona.IsActive);
}
