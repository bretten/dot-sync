using com.brettnamba.DotSync.Apps.Common.Auth;
using com.brettnamba.DotSync.FileSystem.Application.Users;
using Microsoft.AspNetCore.Components.Authorization;

namespace com.brettnamba.DotSync.Apps.Common.Users;

/// <summary>
/// Uses <see cref="AuthenticationStateProvider"/> to verify the state of the user and perform actions
/// </summary>
public sealed class AuthenticationStateProviderUserActions : IUserActions
{
    /// <summary>
    /// Provides authentication state
    /// </summary>
    private readonly AuthenticationStateProvider _provider;

    /// <summary>
    /// For updating/retrieving user settings
    /// </summary>
    private readonly IUserSettings _userSettings;

    public AuthenticationStateProviderUserActions(AuthenticationStateProvider provider, IUserSettings userSettings)
    {
        _provider = provider;
        _userSettings = userSettings;
    }

    /// <inheritdoc/>
    public async Task<bool> GetBoolUserSetting(UserSetting setting)
    {
        return await _userSettings.GetBool(setting, await GetUsername());
    }

    /// <inheritdoc/>
    public async Task UpdateUserSetting(UserSetting setting, bool value)
    {
        await _userSettings.UpdateSetting(setting, await GetUsername(), value);
    }

    private async Task<AuthenticationState> GetAuthenticationState()
    {
        return await _provider.GetAuthenticationStateAsync();
    }

    private async Task<string> GetUsername()
    {
        var authenticationState = await GetAuthenticationState();
        return authenticationState.GetUsername();
    }
}