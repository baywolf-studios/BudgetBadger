using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Recurrence;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models.Recurrence;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Recurrence;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RecurrenceEngineUnitTests
{
    [SetUp]
    public void Setup()
    {
        recurrenceEngine = new RecurrenceEngine();
    }

    private RecurrenceEngine recurrenceEngine { get; set; }

    [Test]
    public void GetDatesFromDateRange_StartDateNotNull_ReturnsOnlyDatesGreaterThanOrEqualToStartDate()
    {
        // arrange
        var startDate = DateTime.Now;

        // act
        var result = RecurrenceEngine.GetDatesFromDateRange(startDate);

        // assert
        Assert.That(result.All(r => r >= startDate.Date));
    }

    [Test]
    public void GetDatesFromDateRange_EndDateNotNull_ReturnsOnlyDateLessThanOrEqualToEndDate()
    {
        // arrange
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(5);

        // act
        var result = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);

        // assert
        Assert.That(result.All(r => r <= endDate.Date));
    }

    [Test]
    public void GetDatesFromDateRange_EndDateBeforeStartDate_ReturnsZero()
    {
        // arrange
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddDays(-5);

        // act
        var result = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetDailyOccurrences_ZeroInterval_ReturnsZero()
    {
        // arrange
        var startDate = DateTime.Now;
        var interval = 0;

        // act
        var result = recurrenceEngine.GetDailyOccurrences(interval, startDate);

        // assert
        Assert.Zero(result.Count());
    }

    [TestCase(-1)]
    [TestCase(-101)]
    public void GetDailyOccurrences_NegativeInterval_ReturnsZero(int interval)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetDailyOccurrences(interval);

        // assert
        Assert.AreEqual(0, result.Count());
    }

    [TestCase(1)]
    [TestCase(101)]
    public void GetDailyOccurrences_PositiveInterval_ReturnsNotZero(int interval)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetDailyOccurrences(interval);

        // assert
        Assert.NotZero(result.Count());
    }


    [TestCase(1, 5)]
    [TestCase(101, 5)]
    [TestCase(1, 98)]
    [TestCase(101, 98)]
    public void GetDailyOccurrences_PositiveInterval_ReturnsDaysBetweenDividedByInterval(int interval, int daysBetween)
    {
        // arrange
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(daysBetween);

        // act
        var result = recurrenceEngine.GetDailyOccurrences(interval, startDate, endDate);

        // assert
        var expectedResult = daysBetween / interval;
        if (expectedResult == 0)
        {
            expectedResult = 1;
        }

        Assert.AreEqual(expectedResult, result.Count());
    }

    [Test]
    public void GetWeeklyOccurrences_NoneDayOfWeek_ReturnsZero()
    {
        // arrange
        var daysOfWeek = DaysOfWeek.None;

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(daysOfWeek);

        // assert
        Assert.Zero(result.Count());
    }

    [TestCase(DaysOfWeek.Monday)]
    [TestCase(DaysOfWeek.Tuesday)]
    [TestCase(DaysOfWeek.Wednesday)]
    [TestCase(DaysOfWeek.Thursday)]
    [TestCase(DaysOfWeek.Friday)]
    [TestCase(DaysOfWeek.Saturday)]
    [TestCase(DaysOfWeek.Sunday)]
    public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDaysOfWeek(DaysOfWeek day)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(day);

        // assert
        Assert.That(result.All(r => r.ToDaysOfWeek() == day));
    }

    [Test]
    public void GetWeeklyOccurrences_MultipleDayOfWeek_ReturnsAllDatesOnTheDaysOfWeekPassedIn()
    {
        // arrange
        const DaysOfWeek daysOfWeek =
            DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Tuesday | DaysOfWeek.Monday;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(50);

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(startDate: DateTime.Now, endDate: endDate, daysOfWeek: daysOfWeek);

        // assert
        Assert.That(result.All(r =>
            r.ToDaysOfWeek() is DaysOfWeek.Wednesday
                or DaysOfWeek.Thursday
                or DaysOfWeek.Tuesday
                or DaysOfWeek.Monday));
    }

    [Test]
    public void GetWeeklyOccurrences_AllDaysOfWeek_ReturnsAllDates()
    {
        // arrange
        const DaysOfWeek daysOfWeek = DaysOfWeek.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(50);

        // act
        var result =
            recurrenceEngine.GetWeeklyOccurrences(startDate: startDate, endDate: endDate, daysOfWeek: daysOfWeek);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }


    [Test]
    public void GetWeeklyOccurrences_ZeroInterval_ReturnsZero()
    {
        // arrange
        var startDate = DateTime.Now;
        const int interval = 0;

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(interval: interval, startDate: startDate);

        // assert
        Assert.Zero(result.Count());
    }

    [TestCase(-1)]
    [TestCase(-101)]
    public void GetWeeklyOccurrences_NegativeInterval_ReturnsZero(int interval)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(interval: interval);

        // assert
        Assert.AreEqual(0, result.Count());
    }

    [TestCase(5)]
    [TestCase(98)]
    public void GetWeeklyOccurrences_IntervalIsOne_ReturnsEveryDayBetween(int weeksBetween)
    {
        // arrange
        const int interval = 1;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddWeeks(weeksBetween);

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetWeeklyOccurrences_IntervalIsHundredOneAndWeeksBetweenIsNinetyEight_ReturnsFirstWeek()
    {
        // arrange
        const int interval = 101;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddWeeks(98);

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = new List<DateTime>
        {
            new DateTime(2022, 6, 9),
            new DateTime(2022, 6, 10),
            new DateTime(2022, 6, 11),
            new DateTime(2022, 6, 12)
        };
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetWeeklyOccurrences_IntervalIsTwoAndWeeksBetweenIs5_ReturnsFirstThirdAndFifthWeek()
    {
        // arrange
        const int interval = 2;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddWeeks(5);

        // act
        var result = recurrenceEngine.GetWeeklyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = new List<DateTime>
        {
            new DateTime(2022, 6, 9),
            new DateTime(2022, 6, 10),
            new DateTime(2022, 6, 11),
            new DateTime(2022, 6, 12),
            new DateTime(2022, 6, 20),
            new DateTime(2022, 6, 21),
            new DateTime(2022, 6, 22),
            new DateTime(2022, 6, 23),
            new DateTime(2022, 6, 24),
            new DateTime(2022, 6, 25),
            new DateTime(2022, 6, 26),
            new DateTime(2022, 7, 4),
            new DateTime(2022, 7, 5),
            new DateTime(2022, 7, 6),
            new DateTime(2022, 7, 7),
            new DateTime(2022, 7, 8),
            new DateTime(2022, 7, 9),
            new DateTime(2022, 7, 10)
        };
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetMonthlyOccurrences_NoneDaysOfWeeks_ReturnsZero()
    {
        // arrange
        const DaysOfWeek daysOfWeek = DaysOfWeek.None;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(daysOfWeek: daysOfWeek);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetMonthlyOccurrences_NoneDaysOfMonth_ReturnsZero()
    {
        // arrange
        const DaysOfMonth daysOfMonth = DaysOfMonth.None;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(daysOfMonth);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetMonthlyOccurrences_NoneWeeksOfMonth_ReturnsZero()
    {
        // arrange
        const WeeksOfMonth weeksOfMonth = WeeksOfMonth.None;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(weeksOfMonth: weeksOfMonth);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetMonthlyOccurrences_AllDaysOfWeek_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfWeek day = DaysOfWeek.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(300);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(daysOfWeek: day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [Test]
    public void GetMonthlyOccurrences_AllDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfMonth day = DaysOfMonth.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(300);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [Test]
    public void GetMonthlyOccurrences_AllWeeksOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const WeeksOfMonth day = WeeksOfMonth.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(300);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(weeksOfMonth: day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [TestCase(DaysOfMonth.Day01)]
    [TestCase(DaysOfMonth.Day04)]
    [TestCase(DaysOfMonth.Day08)]
    [TestCase(DaysOfMonth.Day10)]
    [TestCase(DaysOfMonth.Day15)]
    [TestCase(DaysOfMonth.Day17)]
    [TestCase(DaysOfMonth.Day29)]
    public void GetMonthlyOccurrences_SingleDaysOfMonth_ReturnsOnlyDatesOnTheDayOfMonth(DaysOfMonth day)
    {
        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(day);

        // assert
        Assert.That(result.All(r => r.ToDaysOfMonth().HasAnyFlag(day)));
    }

    [Test]
    public void GetMonthlyOccurrences_MultipleDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfMonth day = DaysOfMonth.Day02 |
                                DaysOfMonth.Day10 |
                                DaysOfMonth.Day17 |
                                DaysOfMonth.Day22 |
                                DaysOfMonth.Day27;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(day);

        // assert
        var returnedDays = result.Aggregate(DaysOfMonth.None, (current, date) => current | date.ToDaysOfMonth());

        Assert.AreEqual(day, returnedDays);
        Assert.That(result.All(r => r.ToDaysOfMonth() is DaysOfMonth.Day02
            or DaysOfMonth.Day10
            or DaysOfMonth.Day17
            or DaysOfMonth.Day22
            or DaysOfMonth.Day27));
    }

    [TestCase(WeeksOfMonth.First)]
    [TestCase(WeeksOfMonth.Second)]
    [TestCase(WeeksOfMonth.Third)]
    [TestCase(WeeksOfMonth.Fourth)]
    [TestCase(WeeksOfMonth.Fifth)]
    public void GetMonthlyOccurrences_SingleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth(WeeksOfMonth day)
    {
        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(weeksOfMonth: day);

        // assert
        Assert.That(result.All(r => r.ToWeeksOfMonth().HasAnyFlag(day)));
    }

    [Test]
    public void GetMonthlyOccurrences_MultipleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth()
    {
        // arrange
        const WeeksOfMonth weeksOfMonth = WeeksOfMonth.First | WeeksOfMonth.Second;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(weeksOfMonth: weeksOfMonth);

        // assert
        Assert.That(result.All(r => r.ToWeeksOfMonth() is WeeksOfMonth.First or WeeksOfMonth.Second));
    }

    [TestCase(DaysOfWeek.Monday)]
    [TestCase(DaysOfWeek.Tuesday)]
    [TestCase(DaysOfWeek.Wednesday)]
    [TestCase(DaysOfWeek.Thursday)]
    [TestCase(DaysOfWeek.Friday)]
    [TestCase(DaysOfWeek.Saturday)]
    [TestCase(DaysOfWeek.Sunday)]
    public void GetMonthlyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDaysOfWeek(DaysOfWeek day)
    {
        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(daysOfWeek: day);

        // assert
        Assert.That(result.All(r => r.ToDaysOfWeek() == day));
    }

    [Test]
    public void GetMonthlyOccurrences_MultipleDaysOfWeek_ReturnsOnlyDatesOnTheDayOfWeek()
    {
        // arrange
        const DaysOfWeek daysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Sunday;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(daysOfWeek: daysOfWeek);

        // assert
        Assert.That(result.All(r => r.ToDaysOfWeek() is DaysOfWeek.Monday or DaysOfWeek.Sunday));
    }

    [Test]
    public void GetMonthlyOccurrences_ZeroInterval_ReturnsZero()
    {
        // arrange
        var startDate = DateTime.Now;
        const int interval = 0;

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(interval: interval, startDate: startDate);

        // assert
        Assert.Zero(result.Count());
    }

    [TestCase(-1)]
    [TestCase(-101)]
    public void GetMonthlyOccurrences_NegativeInterval_ReturnsZero(int interval)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(interval: interval);

        // assert
        Assert.AreEqual(0, result.Count());
    }

    [TestCase(5)]
    [TestCase(98)]
    public void GetMonthlyOccurrences_IntervalIsOne_ReturnsEveryDayBetween(int monthsBetween)
    {
        // arrange
        const int interval = 1;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddMonths(monthsBetween);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetMonthlyOccurrences_IntervalIsHundredOneAndMonthsBetweenIsNinetyEight_ReturnsFirstMonth()
    {
        // arrange
        const int interval = 101;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddMonths(98);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, new DateTime(2022, 7, 1));
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetMonthlyOccurrences_IntervalIsTwoAndMonthsBetweenIs5_ReturnsFirstThirdAndFifthMonth()
    {
        // arrange
        const int interval = 2;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddMonths(5);

        // act
        var result = recurrenceEngine.GetMonthlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, new DateTime(2022, 7, 1)).ToList();
        expectedDates.AddRange(
            RecurrenceEngine.GetDatesFromDateRange(new DateTime(2022, 8, 1), new DateTime(2022, 9, 1)));
        expectedDates.AddRange(
            RecurrenceEngine.GetDatesFromDateRange(new DateTime(2022, 10, 1), new DateTime(2022, 11, 1)));
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetYearlyOccurrences_NoneDaysOfWeeks_ReturnsZero()
    {
        // arrange
        const DaysOfWeek daysOfWeek = DaysOfWeek.None;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(daysOfWeek: daysOfWeek);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetYearlyOccurrences_NoneDaysOfMonth_ReturnsZero()
    {
        // arrange
        const DaysOfMonth daysOfMonth = DaysOfMonth.None;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(daysOfMonth);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetYearlyOccurrences_NoneWeeksOfMonth_ReturnsZero()
    {
        // arrange
        const WeeksOfMonth weeksOfMonth = WeeksOfMonth.None;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(weeksOfMonth: weeksOfMonth);

        // assert
        Assert.Zero(result.Count());
    }
    
    [Test]
    public void GetYearlyOccurrences_NoneMonthsOfYear_ReturnsZero()
    {
        // arrange
        const MonthsOfYear monthsOfYear = MonthsOfYear.None;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(monthsOfYear: monthsOfYear);

        // assert
        Assert.Zero(result.Count());
    }

    [Test]
    public void GetYearlyOccurrences_AllDaysOfWeek_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfWeek day = DaysOfWeek.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(300);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(daysOfWeek: day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [Test]
    public void GetYearlyOccurrences_AllDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfMonth day = DaysOfMonth.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(300);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [Test]
    public void GetYearlyOccurrences_AllWeeksOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const WeeksOfMonth day = WeeksOfMonth.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(300);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(weeksOfMonth: day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }
    
    [Test]
    public void GetYearlyOccurrences_AllMonthsOfYear_ReturnsAllDatesOnTheMonthsOfYearPassedIn()
    {
        // arrange
        const WeeksOfMonth day = WeeksOfMonth.All;
        var startDate = DateTime.Now;
        var endDate = startDate.AddYears(300);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(weeksOfMonth: day, startDate: startDate, endDate: endDate);

        // assert
        var allDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(allDates, result);
    }

    [TestCase(DaysOfMonth.Day01)]
    [TestCase(DaysOfMonth.Day04)]
    [TestCase(DaysOfMonth.Day08)]
    [TestCase(DaysOfMonth.Day10)]
    [TestCase(DaysOfMonth.Day15)]
    [TestCase(DaysOfMonth.Day17)]
    [TestCase(DaysOfMonth.Day29)]
    public void GetYearlyOccurrences_SingleDaysOfMonth_ReturnsOnlyDatesOnTheDayOfMonth(DaysOfMonth day)
    {
        // act
        var result = recurrenceEngine.GetYearlyOccurrences(day);

        // assert
        Assert.That(result.All(r => r.ToDaysOfMonth().HasAnyFlag(day)));
    }

    [Test]
    public void GetYearlyOccurrences_MultipleDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
    {
        // arrange
        const DaysOfMonth day = DaysOfMonth.Day02 |
                                DaysOfMonth.Day10 |
                                DaysOfMonth.Day17 |
                                DaysOfMonth.Day22 |
                                DaysOfMonth.Day27;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(day);

        // assert
        var returnedDays = result.Aggregate(DaysOfMonth.None, (current, date) => current | date.ToDaysOfMonth());

        Assert.AreEqual(day, returnedDays);
        Assert.That(result.All(r => r.ToDaysOfMonth() is DaysOfMonth.Day02
            or DaysOfMonth.Day10
            or DaysOfMonth.Day17
            or DaysOfMonth.Day22
            or DaysOfMonth.Day27));
    }

    [TestCase(WeeksOfMonth.First)]
    [TestCase(WeeksOfMonth.Second)]
    [TestCase(WeeksOfMonth.Third)]
    [TestCase(WeeksOfMonth.Fourth)]
    [TestCase(WeeksOfMonth.Fifth)]
    public void GetYearlyOccurrences_SingleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth(WeeksOfMonth day)
    {
        // act
        var result = recurrenceEngine.GetYearlyOccurrences(weeksOfMonth: day);

        // assert
        Assert.That(result.All(r => r.ToWeeksOfMonth().HasAnyFlag(day)));
    }

    [Test]
    public void GetYearlyOccurrences_MultipleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth()
    {
        // arrange
        const WeeksOfMonth weeksOfMonth = WeeksOfMonth.First | WeeksOfMonth.Second;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(weeksOfMonth: weeksOfMonth);

        // assert
        Assert.That(result.All(r => r.ToWeeksOfMonth() is WeeksOfMonth.First or WeeksOfMonth.Second));
    }

    [TestCase(DaysOfWeek.Monday)]
    [TestCase(DaysOfWeek.Tuesday)]
    [TestCase(DaysOfWeek.Wednesday)]
    [TestCase(DaysOfWeek.Thursday)]
    [TestCase(DaysOfWeek.Friday)]
    [TestCase(DaysOfWeek.Saturday)]
    [TestCase(DaysOfWeek.Sunday)]
    public void GetYearlyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDaysOfWeek(DaysOfWeek day)
    {
        // act
        var result = recurrenceEngine.GetYearlyOccurrences(daysOfWeek: day);

        // assert
        Assert.That(result.All(r => r.ToDaysOfWeek() == day));
    }

    [Test]
    public void GetYearlyOccurrences_MultipleDaysOfWeek_ReturnsOnlyDatesOnTheDayOfWeek()
    {
        // arrange
        const DaysOfWeek daysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Sunday;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(daysOfWeek: daysOfWeek);

        // assert
        Assert.That(result.All(r => r.ToDaysOfWeek() is DaysOfWeek.Monday or DaysOfWeek.Sunday));
    }
    
    [TestCase(MonthsOfYear.January)]
    [TestCase(MonthsOfYear.February)]
    [TestCase(MonthsOfYear.March)]
    [TestCase(MonthsOfYear.April)]
    [TestCase(MonthsOfYear.May)]
    [TestCase(MonthsOfYear.June)]
    [TestCase(MonthsOfYear.July)]
    public void GetYearlyOccurrences_SingleMonthOfYear_ReturnsOnlyDatesOnTheMonthOfYear(MonthsOfYear month)
    {
        // act
        var result = recurrenceEngine.GetYearlyOccurrences(monthsOfYear: month);

        // assert
        Assert.That(result.All(r => r.ToMonthsOfYear() == month));
    }

    [Test]
    public void GetYearlyOccurrences_MultipleMonthsOfYear_ReturnsOnlyDatesOnTheMonthsOfYear()
    {
        // arrange
        const MonthsOfYear monthsOfYear = MonthsOfYear.January | MonthsOfYear.February;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(monthsOfYear: monthsOfYear);

        // assert
        Assert.That(result.All(r => r.ToMonthsOfYear() is MonthsOfYear.January or MonthsOfYear.February));
    }

    [Test]
    public void GetYearlyOccurrences_ZeroInterval_ReturnsZero()
    {
        // arrange
        var startDate = DateTime.Now;
        const int interval = 0;

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(interval: interval, startDate: startDate);

        // assert
        Assert.Zero(result.Count());
    }

    [TestCase(-1)]
    [TestCase(-101)]
    public void GetYearlyOccurrences_NegativeInterval_ReturnsZero(int interval)
    {
        // arrange

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(interval: interval);

        // assert
        Assert.AreEqual(0, result.Count());
    }

    [TestCase(5)]
    [TestCase(98)]
    public void GetYearlyOccurrences_IntervalIsOne_ReturnsEveryDayBetween(int yearsBetween)
    {
        // arrange
        const int interval = 1;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddYears(yearsBetween);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, endDate);
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetYearlyOccurrences_IntervalIsHundredOneAndYearsBetweenIsNinetyEight_ReturnsFirstYear()
    {
        // arrange
        const int interval = 101;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddYears(98);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, new DateTime(2023, 1, 1));
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetYearlyOccurrences_IntervalIsTwoAndYearsBetweenIs5_ReturnsFirstThirdAndFifthYear()
    {
        // arrange
        const int interval = 2;
        var startDate = new DateTime(2022, 6, 9);
        var endDate = startDate.AddYears(5);

        // act
        var result = recurrenceEngine.GetYearlyOccurrences(interval: interval, startDate: startDate, endDate: endDate);

        // assert
        var expectedDates = RecurrenceEngine.GetDatesFromDateRange(startDate, new DateTime(2023, 1, 1)).ToList();
        expectedDates.AddRange(
            RecurrenceEngine.GetDatesFromDateRange(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1)));
        expectedDates.AddRange(
            RecurrenceEngine.GetDatesFromDateRange(new DateTime(2026, 1, 1), new DateTime(2027, 1, 1)));
        CollectionAssert.AreEquivalent(expectedDates, result);
    }

    [Test]
    public void GetOccurrences_CountProvided_LimitsResults()
    {
        // assemble
        var pattern = new RecurrencePattern { Count = 100 };
        
        // act
        var result = recurrenceEngine.GetOccurrences(DateTime.Now, pattern);
        
        // assert
        Assert.AreEqual(100, result.Count());
    }
}
