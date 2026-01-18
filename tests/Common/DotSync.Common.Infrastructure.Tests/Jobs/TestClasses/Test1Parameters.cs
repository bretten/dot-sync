using com.brettnamba.DotSync.Common.Application.Jobs;
using com.brettnamba.DotSync.Common.Application.Jobs.Contracts;

namespace com.brettnamba.DotSync.Common.Infrastructure.Tests.Jobs.TestClasses;

public sealed record Test1Parameters(string Param1, int Delay, bool ThrowException = false) : IJobParameters
{
    public JobType Type => JobType.Verify;

    public IReadOnlyDictionary<string, string> AsKeyValuePairs()
    {
        return new Dictionary<string, string>()
        {
            { "Param1", Param1 },
            { "Delay", Delay.ToString() },
        }.AsReadOnly();
    }
}