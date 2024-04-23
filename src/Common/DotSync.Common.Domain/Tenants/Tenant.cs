namespace com.brettnamba.DotSync.Common.Domain.Tenants;

/// <summary>
/// Represents a tenant
/// </summary>
/// <param name="Name">The name of the tenant</param>
public readonly record struct Tenant(string Name)
{
    public override string ToString()
    {
        return Name;
    }
};