namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// <inheritdoc cref="ITenantAware"/>
/// </summary>
public class TenantAware(ITenantContext tenantContext) : ITenantAware
{
    /// <summary>
    /// <inheritdoc cref="ITenantAware.TenantContext"/>
    /// </summary>
    public ITenantContext TenantContext { get; } = tenantContext;
}