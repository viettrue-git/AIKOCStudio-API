using AiKocStudio.Domain.Enums;
using MediatR;

namespace AiKocStudio.Application.Personas.Commands.UpdatePersona;

public record UpdatePersonaCommand(
    Guid Id,
    string Name,
    string Description,
    string ToneOfVoice,
    string TargetAudience,
    Platform Platform,
    string? DefaultAiProvider,
    string SystemPromptTemplate,
    bool IsActive) : IRequest;
