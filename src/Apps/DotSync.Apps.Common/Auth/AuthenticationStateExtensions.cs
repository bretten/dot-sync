using Microsoft.AspNetCore.Components.Authorization;

namespace com.brettnamba.DotSync.Apps.Common.Auth;

/// <summary>
/// Extensions for <see cref="AuthenticationState"/>
/// </summary>
public static class AuthenticationStateExtensions
{
    public static string GetUsername(this AuthenticationState state)
    {
        return state.User.Claims.FirstOrDefault(claim => claim.Type == "cognito:username")?.Value ?? string.Empty;
    }
}