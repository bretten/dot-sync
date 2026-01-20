using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;
using com.brettnamba.DotSync.Common.Infrastructure.Jobs.Execution;
using com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;
using Moq;
using Faker = com.brettnamba.DotSync.Common.Application.Tests.Jobs.TestClasses.Faker;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.Execution;

public class SingleInstanceJobValidatorTests
{
    [Fact]
    public async Task IsValid_NoActiveJobs_ReturnsTrue()
    {
        // Arrange
        var activeJobs = Array.Empty<IJob>().ToList();
        var stubJobManager = new Mock<IJobManager>();
        stubJobManager.Setup(x => x.Jobs).Returns(activeJobs);
        var jobToValidate = new Test1Parameters("b", 0);

        var jobValidator = new SingleInstanceJobValidator(stubJobManager.Object);

        // Act
        var actual = await jobValidator.IsValid(jobToValidate);

        // Assert
        Assert.True(actual.IsValid);
        Assert.Empty(actual.Message);
    }

    [Fact]
    public async Task IsValid_HasActiveJobs_ReturnsFalse()
    {
        // Arrange
        var activeJobs = new List<IJob>()
        {
            Faker.FakeJob(new Test1Parameters("a", 0)),
        };
        var stubJobManager = new Mock<IJobManager>();
        stubJobManager.Setup(x => x.Jobs).Returns(activeJobs);
        var jobToValidate = new Test1Parameters("b", 0);

        var jobValidator = new SingleInstanceJobValidator(stubJobManager.Object);

        // Act
        var actual = await jobValidator.IsValid(jobToValidate);

        // Assert
        Assert.False(actual.IsValid);
        Assert.NotEmpty(actual.Message);
    }
}