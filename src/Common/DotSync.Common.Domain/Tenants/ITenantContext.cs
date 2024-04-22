namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// Represents a tenant's context which an application can run within, so all actions belong to that tenant
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Switches the current tenant
    /// </summary>
    void SwitchTenant(Tenant tenant);

    /// <summary>
    /// Gets the current tenant
    /// </summary>
    Tenant GetCurrentTenant();
}