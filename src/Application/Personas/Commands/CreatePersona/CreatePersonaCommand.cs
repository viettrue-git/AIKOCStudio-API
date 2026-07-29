using AiKocStudio.Domain.Enums;
using MediatR;

namespace AiKocStudio.Application.Personas.Commands.CreatePersona;

public record CreatePersonaCommand(
    string Name,
    string Description,
    string ToneOfVoice,
    string TargetAudience,
    Platform Platform,
    string? DefaultAiProvider,
    string SystemPromptTemplate) : IRequest<Guid>;
