using AiKocStudio.Domain.Common;

namespace AiKocStudio.Domain.Entities;

public class Product : BaseAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> KeyFeatures { get; set; } = [];
    public Guid? TargetPersonaId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
