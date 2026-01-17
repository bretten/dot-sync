using com.brettnamba.DotSync.Common.Domain.SeedWork;

namespace com.brettnamba.DotSync.Common.Domain.Tests.SeedWork.TestClasses;

public sealed class TestEntity2 : Entity
{
    private TestEntity2(Guid guid, int integerValue, string stringValue) : base(guid)
    {
        IntegerValue = integerValue;
        StringValue = stringValue;
    }

    public int IntegerValue { get; private set; }

    public string StringValue { get; private set; }

    public static TestEntity2 Create(Guid guid, int integerValue, string stringValue)
    {
        return new TestEntity2(guid, integerValue, stringValue);
    }
}