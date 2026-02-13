using com.brettnamba.DotSync.FileSystem.Application.Users;
using com.brettnamba.DotSync.FileSystem.Infrastructure.Users;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.Tests.Users;

public class InMemoryUserSettingsTests
{
    [Fact]
    public async Task GetBool_MissingSetting_ReturnsFalse()
    {
        // Arrange
        var settings = new InMemoryUserSettings();

        // Act
        var actual = await settings.GetBool(UserSetting.ShowTutorials, "user1");

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task GetBool_TrueSetting_ReturnsTrue()
    {
        // Arrange
        var settings = new InMemoryUserSettings();
        await settings.UpdateSetting(UserSetting.ShowTutorials, "user1", true);

        // Act
        var actual = await settings.GetBool(UserSetting.ShowTutorials, "user1");

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task UpdateSetting_SetToTrue_SettingIsNowTrue()
    {
        // Arrange
        var settings = new InMemoryUserSettings();
        await settings.UpdateSetting(UserSetting.ShowTutorials, "user1", false); // Set key

        // Act
        await settings.UpdateSetting(UserSetting.ShowTutorials, "user1", true);
        var actual = await settings.GetBool(UserSetting.ShowTutorials, "user1");

        // Assert
        Assert.True(actual);
    }
}