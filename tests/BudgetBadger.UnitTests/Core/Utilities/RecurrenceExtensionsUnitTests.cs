using System;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models.Recurrence;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Utilities;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RecurrenceExtensionsUnitTests
{
    [TestCase("06/06/2022", DaysOfWeek.Monday)]
    [TestCase("06/07/2022", DaysOfWeek.Tuesday)]
    [TestCase("06/08/2022", DaysOfWeek.Wednesday)]
    [TestCase("06/09/2022", DaysOfWeek.Thursday)]
    [TestCase("06/10/2022", DaysOfWeek.Friday)]
    [TestCase("06/11/2022", DaysOfWeek.Saturday)]
    [TestCase("06/12/2022", DaysOfWeek.Sunday)]
    public void ToDaysOfWeek_GivenDateOnDaysOfWeek_ReturnsCorrectDaysOfWeek(DateTime date, DaysOfWeek daysOfWeek)
    {
        // act
        var result = date.ToDaysOfWeek();

        // assert
        Assert.AreEqual(daysOfWeek, result);
    }

    [TestCase("1/1/2022", DaysOfMonth.Day01)]
    [TestCase("1/4/2022", DaysOfMonth.Day04)]
    [TestCase("1/8/2022", DaysOfMonth.Day08)]
    [TestCase("1/10/2022", DaysOfMonth.Day10)]
    [TestCase("1/15/2022", DaysOfMonth.Day15)]
    [TestCase("1/17/2022", DaysOfMonth.Day17)]
    [TestCase("1/29/2022", DaysOfMonth.Day29)]
    [TestCase("1/31/2022", DaysOfMonth.Day31 | DaysOfMonth.DayLast)]
    public void ToDaysOfMonth_GivenDateOnDaysOfMonth_ReturnsCorrectDaysOfMonth(DateTime date, DaysOfMonth daysOfMonth)
    {
        // act 
        var result = date.ToDaysOfMonth();

        // assert
        Assert.AreEqual(daysOfMonth, result);
    }

    [TestCase("01/01/2022", WeeksOfMonth.First)]
    [TestCase("01/08/2022", WeeksOfMonth.Second)]
    [TestCase("01/16/2022", WeeksOfMonth.Third)]
    [TestCase("01/23/2022", WeeksOfMonth.Fourth)]
    [TestCase("01/30/2022", WeeksOfMonth.Fifth | WeeksOfMonth.Last)]
    [TestCase("02/28/2002", WeeksOfMonth.Fourth | WeeksOfMonth.Last)]
    public void ToWeeksOfMonth_GivenDateOnWeeksOfMonth_ReturnsCorrectWeeksOfMonth(DateTime date,
        WeeksOfMonth weeksOfMonth)
    {
        // act 
        var result = date.ToWeeksOfMonth();

        // assert
        Assert.AreEqual(weeksOfMonth, result);
    }
    
    [TestCase("1/1/2022", MonthsOfYear.January)]
    [TestCase("4/4/2022", MonthsOfYear.April)]
    [TestCase("8/8/2022", MonthsOfYear.August)]
    [TestCase("10/10/2022", MonthsOfYear.October)]
    [TestCase("12/15/2022", MonthsOfYear.December)]
    [TestCase("2/17/2022", MonthsOfYear.February)]
    [TestCase("6/29/2022", MonthsOfYear.June)]
    public void ToMonthsOfYear_GivenDateOnDaysOfMonth_ReturnsCorrectDaysOfMonth(DateTime date, MonthsOfYear monthsOfYear)
    {
        // act 
        var result = date.ToMonthsOfYear();

        // assert
        Assert.AreEqual(monthsOfYear, result);
    }
}
