using BudgetBadger.Core.Utilities;
using BudgetBadger.Models.Recurrence;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Utilities;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class FlagExtensionsUnitTests
{
    [Test]
    public void HasAnyFlag_HasFlag_ReturnsTrue()
    {
        // assemble
        const DaysOfMonth flagsToCheck = DaysOfMonth.Day01 | DaysOfMonth.Day02;
        const DaysOfMonth testFlag = DaysOfMonth.Day01 | DaysOfMonth.Day07;

        // act
        var result = flagsToCheck.HasAnyFlag(testFlag);
        var result2 = testFlag.HasAnyFlag(flagsToCheck);

        // assert
        Assert.IsTrue(result);
        Assert.IsTrue(result2);
    }

    [Test]
    public void HasAnyFlag_DoesNotHaveFlag_ReturnsFalse()
    {
        // assemble
        const DaysOfMonth flagsToCheck = DaysOfMonth.Day01 | DaysOfMonth.Day02;
        const DaysOfMonth testFlag = DaysOfMonth.Day08 | DaysOfMonth.Day09;

        // act
        var result = flagsToCheck.HasAnyFlag(testFlag);
        var result2 = testFlag.HasAnyFlag(flagsToCheck);

        // assert
        Assert.IsFalse(result);
        Assert.IsFalse(result2);
    }
}
