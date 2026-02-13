namespace com.brettnamba.DotSync.FileSystem.Application.Users;

/// <summary>
/// Defines an interface for retrieving and updating user settings
/// </summary>
public interface IUserSettings
{
    /// <summary>
    /// Gets the value of a boolean setting
    /// </summary>
    /// <param name="setting">The setting to get</param>
    /// <param name="userId">The user whose setting to get</param>
    /// <returns>The setting value</returns>
    Task<bool> GetBool(UserSetting setting, string userId);

    /// <summary>
    /// Updates the value of a boolean setting
    /// </summary>
    /// <param name="setting">The setting to change</param>
    /// <param name="userId">The user whose setting to update</param>
    /// <param name="value">The new value</param>
    /// <returns>The completed task</returns>
    Task UpdateSetting(UserSetting setting, string userId, bool value);
}