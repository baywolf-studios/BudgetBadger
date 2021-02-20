using System;
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

        [TestCase(DaysOfWeek.Monday)]
        [TestCase(DaysOfWeek.Tuesday)]
        [TestCase(DaysOfWeek.Wednesday)]
        [TestCase(DaysOfWeek.Thursday)]
        [TestCase(DaysOfWeek.Friday)]
        [TestCase(DaysOfWeek.Saturday)]
        [TestCase(DaysOfWeek.Sunday)]
        public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDayOfWeek(DaysOfWeek day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            Assert.That(result.All(r => r.ToDaysOfWeek() == day));
        }

        [TestCase(DaysOfWeek.Monday | DaysOfWeek.Thursday)]
        [TestCase(DaysOfWeek.Tuesday | DaysOfWeek.Sunday | DaysOfWeek.Sunday)]
        [TestCase(DaysOfWeek.Wednesday | DaysOfWeek.Thursday | DaysOfWeek.Tuesday | DaysOfWeek.Monday)]
        [TestCase(DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Sunday | DaysOfWeek.Saturday | DaysOfWeek.Monday)]
        [TestCase(DaysOfWeek.Friday | DaysOfWeek.Saturday | DaysOfWeek.Sunday | DaysOfWeek.Monday | DaysOfWeek.Tuesday | DaysOfWeek.Wednesday)]
        [TestCase(DaysOfWeek.All)]
        public void GetWeeklyOccurrences_MultipleDayOfWeek_ReturnsAllDatesOnTheDaysOfWeekPassedIn(DaysOfWeek day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            var returnedDays = DaysOfWeek.None;
            foreach (var date in result)
            {
                returnedDays |= date.ToDaysOfWeek();
            }
            Assert.AreEqual(day, returnedDays);
            Assert.That(result.All(r => day.HasFlag(r.ToDaysOfWeek())));
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
        public void GetMonthlyOccurrences_NoneDaysOfMonthAndNoneDaysOfWeeksAndNoneWeeksOfMonth_ReturnsZero()
        {
            // arrange
            var daysOfWeek = DaysOfWeek.None;
            var weeksOfMonth = WeeksOfMonth.None;
            var daysOfMonth = DaysOfMonth.None;

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: daysOfMonth, daysOfWeek: daysOfWeek, weeksOfMonth: weeksOfMonth);

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

        [TestCase(DaysOfMonth.Day01)]
        [TestCase(DaysOfMonth.Day04)]
        [TestCase(DaysOfMonth.Day08)]
        [TestCase(DaysOfMonth.Day10)]
        [TestCase(DaysOfMonth.Day15)]
        [TestCase(DaysOfMonth.Day17)]
        [TestCase(DaysOfMonth.Day29)]
        public void GetMonthlyOccurrences_SingleDayOfMonth_ReturnsDates(DaysOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(DaysOfMonth.Day01)]
        [TestCase(DaysOfMonth.Day04)]
        [TestCase(DaysOfMonth.Day08)]
        [TestCase(DaysOfMonth.Day10)]
        [TestCase(DaysOfMonth.Day15)]
        [TestCase(DaysOfMonth.Day17)]
        [TestCase(DaysOfMonth.Day29)]
        public void GetMonthlyOccurrences_SingleDayOfMonth_ReturnsOnlyDatesOnTheDayOfMonth(DaysOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            Assert.That(result.All(r => r.ToDaysOfMonth() == day));
        }

        [TestCase(DaysOfMonth.Day01 | DaysOfMonth.Day02)]
        [TestCase(DaysOfMonth.Day02 | DaysOfMonth.Day03 | DaysOfMonth.Day17)]
        [TestCase(DaysOfMonth.Day03 | DaysOfMonth.Day02 | DaysOfMonth.Day17 | DaysOfMonth.Day22)]
        [TestCase(DaysOfMonth.Day02 | DaysOfMonth.Day10 | DaysOfMonth.Day17 | DaysOfMonth.Day22 | DaysOfMonth.Day27)]
        [TestCase(DaysOfMonth.Day10 | DaysOfMonth.Day17 | DaysOfMonth.Day18 | DaysOfMonth.Day22 | DaysOfMonth.Day27 | DaysOfMonth.Day03)]
        [TestCase(DaysOfMonth.All)]
        public void GetMonthlyOccurrences_MultipleDaysOfMonth_ReturnsAllDatesOnTheDaysOfMonthPassedIn(DaysOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(daysOfMonth: day);

            // assert
            var returnedDays = DaysOfMonth.None;
            foreach (var date in result)
            {
                returnedDays |= date.ToDaysOfMonth();
            }
            Assert.AreEqual(day, returnedDays);
            Assert.That(result.All(r => day.HasFlag(r.ToDaysOfMonth())));
        }

        [TestCase(WeeksOfMonth.First)]
        [TestCase(WeeksOfMonth.Second)]
        [TestCase(WeeksOfMonth.Third)]
        [TestCase(WeeksOfMonth.Fourth)]
        [TestCase(WeeksOfMonth.Last)]
        public void GetMonthlyOccurrences_SingleWeekOfMonth_ReturnsDates(WeeksOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(WeeksOfMonth.First)]
        [TestCase(WeeksOfMonth.Second)]
        [TestCase(WeeksOfMonth.Third)]
        [TestCase(WeeksOfMonth.Fourth)]
        [TestCase(WeeksOfMonth.Last)]
        public void GetMonthlyOccurrences_SingleWeekOfMonth_ReturnsDates(WeeksOfMonth day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetMonthlyOccurrences(weeksOfMonth: day);

            // assert
            Assert.NotZero(result.Count());
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
