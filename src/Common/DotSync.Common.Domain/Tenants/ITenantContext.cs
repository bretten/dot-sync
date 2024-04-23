namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// Represents a tenant's context which an application can run within, so all actions belong to that tenant
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The current tenant
    /// </summary>
    Tenant CurrentTenant { get; }
}