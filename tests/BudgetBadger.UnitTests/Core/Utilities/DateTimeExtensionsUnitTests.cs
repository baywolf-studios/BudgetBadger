using System;
using BudgetBadger.Core.Utilities;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Utilities;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class DateTimeExtensionsUnitTests
{
    [TestCase("6/10/2022", 738315)]
    [TestCase("1/2/0001", 1)]
    [TestCase("1/8/2570", 938315)]
    [TestCase("1/1/0001", 0)]
    [TestCase("1/1/0005", 1461)]
    public void DaysSinceEpoch_ReturnsExpected(DateTime date, int expected)
    {
        // act
        var actual = date.DaysSinceEpoch();

        // assert
        Assert.AreEqual(expected, actual);
    }

    [TestCase("6/10/2022", 105473)]
    [TestCase("1/8/0001", 1)]
    [TestCase("1/8/2570", 134045)]
    [TestCase("1/1/0001", 0)]
    [TestCase("1/1/1970", 102737)]
    public void WeeksSinceEpoch_ReturnsExpected(DateTime date, int expected)
    {
        // act
        var actual = date.WeeksSinceEpoch();

        // assert
        Assert.AreEqual(expected, actual);
    }

    [TestCase("6/10/2022", 24257)]
    [TestCase("2/8/0001", 1)]
    [TestCase("1/8/2570", 30828)]
    [TestCase("1/1/0001", 0)]
    public void MonthsSinceEpoch_ReturnsExpected(DateTime date, int expected)
    {
        // act
        var actual = date.MonthsSinceEpoch();

        // assert
        Assert.AreEqual(expected, actual);
    }

    [TestCase("6/10/2022", 2021)]
    [TestCase("2/8/0001", 0)]
    [TestCase("1/8/2570", 2569)]
    [TestCase("1/1/0001", 0)]
    public void YearsSinceEpoch_ReturnsExpected(DateTime date, int expected)
    {
        // act
        var actual = date.YearsSinceEpoch();

        // assert
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ToGuid_SameDates_ReturnsSameGuid()
    {
        // assemble
        var testDate = DateTime.Now;

        // act
        var guid1 = testDate.ToGuid();
        var guid2 = testDate.ToGuid();

        // assert
        Assert.AreEqual(guid1, guid2);
    }

    [Test]
    public void ToGuid_DifferentDates_ReturnsDifferentGuid()
    {
        // assemble
        var testDate = DateTime.Now;
        var testDate2 = DateTime.UnixEpoch;

        // act
        var guid1 = testDate.ToGuid();
        var guid2 = testDate2.ToGuid();

        // assert
        Assert.AreNotEqual(guid1, guid2);
    }

    [Test]
    public void AddWeeks_ReturnsGivenDatePlus7TimesWeeks()
    {
        // assemble
        var testDate = DateTime.Now;

        // act
        var result = testDate.AddWeeks(2);

        // assert
        var expected = testDate.AddDays(14);
        Assert.AreEqual(expected, result);
    }

    [TestCase("6/10/2022", 2)]
    [TestCase("2/8/0001", 2)]
    [TestCase("1/30/2570", 5)]
    [TestCase("1/1/0001", 1)]
    public void WeekOfMonth_GivenDate_ReturnsExpected(DateTime date, int expected)
    {
        // act
        var result = date.WeekOfMonth();

        // assert
        Assert.AreEqual(expected, result);
    }
}
