using AiKocStudio.Domain.Common;

namespace AiKocStudio.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
