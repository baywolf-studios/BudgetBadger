using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Logic;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Schedule;
using NUnit.Framework;

namespace BudgetBadger.Tests.Logic
{
    public class ScheduleLogicUnitTests
    {
        private ScheduleLogic scheduleLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            scheduleLogic = new ScheduleLogic();
        }

        [Test]
        public void GetDatesFromDateRange_StartDateNotNull_ReturnsOnlyDatesGreaterThanOrEqualToStartDate()
        {
            // arrange
            var startDate = DateTime.Now;

            // act
            var result = scheduleLogic.GetDatesFromDateRange(startDate: startDate);

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
            var result = scheduleLogic.GetDatesFromDateRange(startDate, endDate);

            // assert
            Assert.That(result.All(r => r <= endDate.Date));
        }

        [Test]
        public void GetDatesFromDateRange_EndDateBeforeStartDate_Returns0()
        {
            // arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(-5);

            // act
            var result = scheduleLogic.GetDatesFromDateRange(startDate, endDate);

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
            var result = scheduleLogic.GetDailyOccurrences(interval, startDate, null);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(-1)]
        [TestCase(-101)]
        public void GetDailyOccurrences_NegativeInterval_ReturnsZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetDailyOccurrences(interval);

            // assert
            Assert.AreEqual(0, result.Count());
        }

        [TestCase(1)]
        [TestCase(101)]
        public void GetDailyOccurrences_PositiveInterval_ReturnsNotZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetDailyOccurrences(interval);

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
            var result = scheduleLogic.GetDailyOccurrences(interval, startDate, endDate);

            // assert
            var expectedResult = (daysBetween / interval);
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
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: daysOfWeek);

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
        public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsDates(DaysOfWeek day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(DaysOfWeek.Monday, DayOfWeek.Monday)]
        [TestCase(DaysOfWeek.Tuesday, DayOfWeek.Tuesday)]
        [TestCase(DaysOfWeek.Wednesday, DayOfWeek.Wednesday)]
        [TestCase(DaysOfWeek.Thursday, DayOfWeek.Thursday)]
        [TestCase(DaysOfWeek.Friday, DayOfWeek.Friday)]
        [TestCase(DaysOfWeek.Saturday, DayOfWeek.Saturday)]
        [TestCase(DaysOfWeek.Sunday, DayOfWeek.Sunday)]
        public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDayOfWeek(DaysOfWeek day, DayOfWeek expectedResult)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            Assert.That(result.All(r => r.DayOfWeek == expectedResult));
        }

        public void GetWeeklyOccurrences_MultipleDayOfWeek_ReturnsAllDatesOnTheDaysOfWeekPassedIn()
        {
            // arrange
            var daysOfWeek = DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Tuesday | DaysOfWeek.Monday;

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: daysOfWeek);

            // assert
            var returnedDays = DaysOfWeek.None;
            foreach (var date in result)
            {
                returnedDays |= date.ToDaysOfWeek();
            }
            Assert.AreEqual(daysOfWeek, returnedDays);
            Assert.That(result.All(r => r.DayOfWeek == DayOfWeek.Wednesday
                                     || r.DayOfWeek == DayOfWeek.Thursday
                                     || r.DayOfWeek == DayOfWeek.Tuesday
                                     || r.DayOfWeek == DayOfWeek.Monday));
        }

        public void GetWeeklyOccurrences_AllDaysOfWeek_ReturnsAllDates()
        {
            // arrange
            var daysOfWeek = DaysOfWeek.All;

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: daysOfWeek);

            // assert
            var allDates = scheduleLogic.GetDatesFromDateRange(DateTime.MinValue, DateTime.MaxValue);
            Assert.That(result.All(allDates.Contains) && result.Count() == allDates.Count());
        }


        [Test]
        public void GetWeeklyOccurrences_ZeroInterval_ReturnsZero()
        {
            // arrange
            var startDate = DateTime.Now;
            var interval = 0;

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval: interval, startDate: startDate);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(-1)]
        [TestCase(-101)]
        public void GetWeeklyOccurrences_NegativeInterval_ReturnsZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval: interval);

            // assert
            Assert.AreEqual(0, result.Count());
        }

        [TestCase(1)]
        [TestCase(101)]
        public void GetWeeklyOccurrences_SingleDayOfWeekAndPositiveInterval_ReturnsNotZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(DaysOfWeek.Monday, interval: interval);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(1, 5)]
        [TestCase(101, 5)]
        [TestCase(1, 98)]
        [TestCase(101, 98)]
        public void GetWeeklyOccurrences_SingleDayOfWeekAndPositiveInterval_ReturnsWeeksBetweenDividedByInterval(int interval, int weeksBetween)
        {
            // arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddWeeks(weeksBetween);

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(DaysOfWeek.Monday, interval: interval, startDate: startDate, endDate: endDate);

            // assert
            var expectedResult = weeksBetween / interval;
            if (expectedResult == 0)
            {
                expectedResult = 1;
            }
            Assert.AreEqual(expectedResult, result.Count());
        }

        [TestCase(1, DaysOfWeek.Monday | DaysOfWeek.Friday)]
        [TestCase(101, DaysOfWeek.All)]
        public void GetWeeklyOccurrences_MultipleDaysOfWeekAndPositiveInterval_ReturnsNotZero(int interval, DaysOfWeek dayOfWeek)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: dayOfWeek, interval: interval);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(1, 5, DaysOfWeek.Monday | DaysOfWeek.Tuesday, 10)]
        [TestCase(101, 5, DaysOfWeek.All, 7)]
        [TestCase(1, 98, DaysOfWeek.Friday | DaysOfWeek.Tuesday, 196)]
        [TestCase(101, 98, DaysOfWeek.All, 7)]
        public void GetWeeklyOccurrences_MultipleDaysOfWeekAndPositiveInterval_ReturnsNumberOfDaysTimesWeeksBetweenDividedByInterval(int interval, int weeksBetween, DaysOfWeek dayOfWeek, int expectedResult)
        {
            // arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddWeeks(weeksBetween);

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: dayOfWeek, interval: interval, startDate: startDate, endDate: endDate);

            // assert
            Assert.AreEqual(expectedResult, result.Count());
        }

        [Test]
        public void GetMonthlyOccurrences_NoneDaysOfWeeks_ReturnsZero()
        {
            // arrange
            var daysOfWeek = DaysOfWeek.None;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfWeek: daysOfWeek);

            // assert
            Assert.Zero(result.Count());
        }

        [Test]
        public void GetMonthlyOccurrences_NoneDaysOfMonth_ReturnsZero()
        {
            // arrange
            var daysOfMonth = DaysOfMonth.None;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: daysOfMonth);

            // assert
            Assert.Zero(result.Count());
        }

        [Test]
        public void GetMonthlyOccurrences_NoneWeeksOfMonth_ReturnsZero()
        {
            // arrange
            var weeksOfMonth = WeeksOfMonth.None;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: weeksOfMonth);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(DaysOfMonth.Day01)]
        [TestCase(DaysOfMonth.Day04)]
        [TestCase(DaysOfMonth.Day08)]
        [TestCase(DaysOfMonth.Day10)]
        [TestCase(DaysOfMonth.Day15)]
        [TestCase(DaysOfMonth.Day17)]
        [TestCase(DaysOfMonth.Day29)]
        public void GetMonthlyOccurrences_SingleDaysOfMonth_ReturnsDates(DaysOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(DaysOfMonth.Day01, 1)]
        [TestCase(DaysOfMonth.Day04, 4)]
        [TestCase(DaysOfMonth.Day08, 8)]
        [TestCase(DaysOfMonth.Day10, 10)]
        [TestCase(DaysOfMonth.Day15, 15)]
        [TestCase(DaysOfMonth.Day17, 17)]
        [TestCase(DaysOfMonth.Day29, 29)]
        public void GetMonthlyOccurrences_SingleDaysOfMonth_ReturnsOnlyDatesOnTheDayOfMonth(DaysOfMonth day, int expectedResult)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            Assert.That(result.All(r => r.Day == expectedResult));
        }

        public void GetMonthlyOccurrences_MultipleDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
        {
            // arrange
            var day = DaysOfMonth.Day02 | DaysOfMonth.Day10 | DaysOfMonth.Day17 | DaysOfMonth.Day22 | DaysOfMonth.Day27;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            var returnedDays = DaysOfMonth.None;
            foreach (var date in result)
            {
                returnedDays |= date.ToDaysOfMonth();
            }
            Assert.AreEqual(day, returnedDays);
            Assert.That(result.All(r => r.Day == 2
                                     || r.Day == 10
                                     || r.Day == 17
                                     || r.Day == 22
                                     || r.Day == 27));
        }

        public void GetMonthlyOccurrences_AllDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn()
        {
            // arrange
            var day = DaysOfMonth.All;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            var allDates = scheduleLogic.GetDatesFromDateRange(DateTime.MinValue, DateTime.MaxValue);
            Assert.That(result.All(allDates.Contains) && result.Count() == allDates.Count());
        }

        [TestCase(WeeksOfMonth.First)]
        [TestCase(WeeksOfMonth.Second)]
        [TestCase(WeeksOfMonth.Third)]
        [TestCase(WeeksOfMonth.Fourth)]
        [TestCase(WeeksOfMonth.Fifth)]
        public void GetMonthlyOccurrences_SingleWeeksOfMonth_ReturnsDates(WeeksOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(WeeksOfMonth.First, 1)]
        [TestCase(WeeksOfMonth.Second, 2)]
        [TestCase(WeeksOfMonth.Third, 3)]
        [TestCase(WeeksOfMonth.Fourth, 4)]
        [TestCase(WeeksOfMonth.Fifth, 5)]
        public void GetMonthlyOccurrences_SingleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth( WeeksOfMonth day, int expectedResult)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: day);

            // assert
            Assert.That(result.All(r => r.WeekOfMonth() == expectedResult));
        }

        [Test]
        public void GetMonthlyOccurrences_MultipleWeeksOfMonth_ReturnsOnlyDatesOnTheWeekOfMonth()
        {
            // arrange
            var weeksOfMonth = WeeksOfMonth.First | WeeksOfMonth.Second;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: weeksOfMonth);

            // assert
            Assert.That(result.All(r => r.WeekOfMonth() == 1 || r.WeekOfMonth() == 2));
        }

        [TestCase(DaysOfWeek.Monday, DayOfWeek.Monday)]
        [TestCase(DaysOfWeek.Tuesday, DayOfWeek.Tuesday)]
        [TestCase(DaysOfWeek.Wednesday, DayOfWeek.Wednesday)]
        [TestCase(DaysOfWeek.Thursday, DayOfWeek.Thursday)]
        [TestCase(DaysOfWeek.Friday, DayOfWeek.Friday)]
        [TestCase(DaysOfWeek.Saturday, DayOfWeek.Saturday)]
        [TestCase(DaysOfWeek.Sunday, DayOfWeek.Sunday)]
        public void GetMonthlyOccurrences_SingleDayOfWeek_ReturnsDates(DaysOfWeek day, DayOfWeek expectedResult)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfWeek: day);

            // assert
            Assert.That(result.All(r => r.DayOfWeek == expectedResult));
        }

        [TestCase(DaysOfWeek.Monday)]
        [TestCase(DaysOfWeek.Tuesday)]
        [TestCase(DaysOfWeek.Wednesday)]
        [TestCase(DaysOfWeek.Thursday)]
        [TestCase(DaysOfWeek.Friday)]
        [TestCase(DaysOfWeek.Saturday)]
        [TestCase(DaysOfWeek.Sunday)]
        public void GetMonthlyOccurrences_SingleDayOfWeek_ReturnsDates(DaysOfWeek day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfWeek: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [Test]
        public void GetMonthlyOccurrences_MultipleDaysOfWeek_ReturnsOnlyDatesOnTheDayOfWeek()
        {
            // arrange
            var daysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Sunday;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfWeek: daysOfWeek);

            // assert
            Assert.That(result.All(r => r.DayOfWeek == DayOfWeek.Monday || r.DayOfWeek == DayOfWeek.Sunday));
        }

        [Test]
        public void GetMonthlyOccurrences_ZeroInterval_ReturnsZero()
        {
            // arrange
            var startDate = DateTime.Now;
            var interval = 0;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(interval: interval, startDate: startDate);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(-1)]
        [TestCase(-101)]
        public void GetMonthlyOccurrences_NegativeInterval_ReturnsZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(interval: interval);

            // assert
            Assert.AreEqual(0, result.Count());
        }

    }
}
