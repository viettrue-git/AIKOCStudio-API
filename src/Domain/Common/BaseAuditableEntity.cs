namespace AiKocStudio.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public DateTimeOffset Created { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public Guid? LastModifiedBy { get; set; }
}
