using Hangfire.Dashboard;

namespace com.brettnamba.DotSync.Apps.Common.Hangfire;

public class IpAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IpAuthorizationFilterOptions _options;

    public IpAuthorizationFilter(IpAuthorizationFilterOptions options)
    {
        _options = options;
    }


    public bool Authorize(DashboardContext context)
    {
        var remoteIp = context.GetHttpContext().Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        Console.WriteLine($"remoteIp: {remoteIp}");

        return _options.IpAddresses.Contains(remoteIp);
    }
}

public sealed record IpAuthorizationFilterOptions(string[] IpAddresses);