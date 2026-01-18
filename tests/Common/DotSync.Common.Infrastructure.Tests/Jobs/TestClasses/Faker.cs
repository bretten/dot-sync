using com.brettnamba.DotSync.Common.Infrastructure.Configuration;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;

public static class Faker
{
    public static JobConfiguration FakeJobConfiguration(string? reportPath = null)
    {
        return new JobConfiguration(reportPath ?? "path");
    }
}