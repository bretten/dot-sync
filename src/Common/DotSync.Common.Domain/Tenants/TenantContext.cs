namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// <inheritdoc cref="ITenantContext"/>
/// </summary>
public sealed class TenantContext : ITenantContext
{
    /// <summary>
    /// The underlying current tenant
    /// </summary>
    private Tenant? _currentTenant;

    /// <summary>
    /// <inheritdoc cref="ITenantContext.SwitchTenant"/>
    /// </summary>
    public void SwitchTenant(Tenant tenant)
    {
        _currentTenant = tenant;
    }

    /// <summary>
    /// <inheritdoc cref="ITenantContext.GetCurrentTenant"/>
    /// </summary>
    public Tenant GetCurrentTenant()
    {
        if (_currentTenant == null)
        {
            throw new NoActiveTenantException($"There is no active tenant");
        }

        return _currentTenant;
    }

    private sealed class NoActiveTenantException(string? message) : Exception(message);
}