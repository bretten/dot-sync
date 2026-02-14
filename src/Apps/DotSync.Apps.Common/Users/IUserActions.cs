using com.brettnamba.DotSync.FileSystem.Application.Users;

namespace com.brettnamba.DotSync.Apps.Common.Users;

/// <summary>
/// Abstracts user actions to a single interface. Implementations should resolve dependencies here that are required for
/// user actions rather than in individual components
/// </summary>
public interface IUserActions
{
    /// <summary>
    /// Gets a bool user setting
    /// </summary>
    /// <param name="setting">The setting to get</param>
    /// <returns>The value of the setting</returns>
    Task<bool> GetBoolUserSetting(UserSetting setting);

    /// <summary>
    /// Updates a user bool setting
    /// </summary>
    /// <param name="setting">The setting to change</param>
    /// <param name="value">The new value</param>
    /// <returns>The completed task</returns>
    Task UpdateUserSetting(UserSetting setting, bool value);
}