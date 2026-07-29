using AiKocStudio.Domain.Common;
using AiKocStudio.Domain.Enums;

namespace AiKocStudio.Domain.Entities;

public class Persona : BaseAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ToneOfVoice { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public Platform Platform { get; set; }

    /// <summary>Null = use the system-default AI provider (see Phase 4's provider selection).</summary>
    public string? DefaultAiProvider { get; set; }

    public string SystemPromptTemplate { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
