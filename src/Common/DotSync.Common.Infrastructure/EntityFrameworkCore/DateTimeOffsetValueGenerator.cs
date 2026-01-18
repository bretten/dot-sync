using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace com.brettnamba.DotSync.Common.Infrastructure.EntityFrameworkCore;

public class DateTimeOffsetValueGenerator : ValueGenerator<DateTimeOffset>
{
    public override DateTimeOffset Next(EntityEntry entry)
    {
        return DateTimeOffset.UtcNow;
    }

    public override bool GeneratesTemporaryValues => false;
}