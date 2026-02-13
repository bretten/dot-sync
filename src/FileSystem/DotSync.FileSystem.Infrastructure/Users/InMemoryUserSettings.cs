using System.Collections.Concurrent;
using com.brettnamba.DotSync.FileSystem.Application.Users;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Users;

/// <summary>
/// Ephemeral user settings that are lost on application resets. Mainly to be used for demos or for non-critical settings
/// </summary>
public sealed class InMemoryUserSettings : IUserSettings
{
    private readonly ConcurrentDictionary<string, bool> _userBoolSettings = new ConcurrentDictionary<string, bool>();

    /// <inheritdoc/>
    public Task<bool> GetBool(UserSetting setting, string userId)
    {
        var exists = _userBoolSettings.TryGetValue(GetKey(setting, userId), out var value);
        return exists ? Task.FromResult(value) : Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task UpdateSetting(UserSetting setting, string userId, bool value)
    {
        if (!_userBoolSettings.ContainsKey(GetKey(setting, userId)))
        {
            _userBoolSettings[GetKey(setting, userId)] = value;
            return Task.CompletedTask;
        }

        // TryUpdate so it will update only if it is the opposite
        _userBoolSettings.TryUpdate(GetKey(setting, userId), value, !value);
        return Task.CompletedTask;
    }

    private string GetKey(UserSetting setting, string userId)
    {
        return $"{setting.ToString()}_{userId}";
    }
}