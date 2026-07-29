using AiKocStudio.Application.Common.Interfaces;

namespace AiKocStudio.Infrastructure.IntegrationTests.Common;

public class TestTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;
}
