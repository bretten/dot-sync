namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// <inheritdoc cref="ITenantContext"/>
/// </summary>
public sealed class TenantContext(Tenant currentTenant) : ITenantContext
{
    /// <summary>
    /// <inheritdoc cref="ITenantContext.CurrentTenant"/>
    /// </summary>
    public Tenant CurrentTenant { get; } = currentTenant;
}