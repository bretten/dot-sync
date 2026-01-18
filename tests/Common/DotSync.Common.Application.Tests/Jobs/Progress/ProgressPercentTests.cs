using com.brettnamba.DotSync.Common.Application.Jobs.Progress;

namespace com.brettnamba.DotSync.Common.Application.Tests.Jobs.Progress;

public class ProgressPercentTests
{
    [Fact]
    public void AsPercent_ValidProgress_ReturnsPercent()
    {
        // Arrange
        var jobId = Guid.Empty;
        var currentProgressUnits = 39;
        var totalProgressUnits = 80;
        var progress = new ProgressPercent(jobId, currentProgressUnits, totalProgressUnits);

        // Act
        var actual = progress.AsPercent();

        // Assert
        Assert.Equal(((double)39 / 80) * 100, actual);
    }
}