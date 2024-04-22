namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// Defines something that is aware of multi-tenancy and has access to the tenant's context
/// </summary>
public interface ITenantAware
{
    /// <summary>
    /// The tenant context
    /// </summary>
    ITenantContext TenantContext { get; }
}