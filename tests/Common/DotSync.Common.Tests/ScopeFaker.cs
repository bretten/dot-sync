using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace com.brettnamba.DotSync.Common.Tests;

public static class ScopeFaker
{
    public static (Mock<IServiceScopeFactory> stubServiceScopeFactory, Mock<IServiceScope> stubServiceScope) MockScope()
    {
        var mockServiceScope = new Mock<IServiceScope>();

        var stubServiceScopeFactory = new Mock<IServiceScopeFactory>();
        stubServiceScopeFactory.Setup(x => x.CreateScope())
            .Returns(mockServiceScope.Object);

        return (stubServiceScopeFactory, mockServiceScope);
    }
}