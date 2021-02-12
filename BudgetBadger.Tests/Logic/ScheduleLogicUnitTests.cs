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
            var daysOfWeek = Day.None;

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: daysOfWeek);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(Day.Monday)]
        [TestCase(Day.Tuesday)]
        [TestCase(Day.Wednesday)]
        [TestCase(Day.Thursday)]
        [TestCase(Day.Friday)]
        [TestCase(Day.Saturday)]
        [TestCase(Day.Sunday)]
        public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsDates(Day day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(Day.Monday)]
        [TestCase(Day.Tuesday)]
        [TestCase(Day.Wednesday)]
        [TestCase(Day.Thursday)]
        [TestCase(Day.Friday)]
        [TestCase(Day.Saturday)]
        [TestCase(Day.Sunday)]
        public void GetWeeklyOccurrences_SingleDayOfWeek_ReturnsOnlyDatesOnTheDayOfWeek(Day day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            Assert.That(result.All(r => r.DayOfWeek.ToDay() == day));
        }

        [TestCase(Day.Monday | Day.Thursday)]
        [TestCase(Day.Tuesday | Day.Sunday | Day.Sunday)]
        [TestCase(Day.Wednesday | Day.Thursday | Day.Tuesday | Day.Monday)]
        [TestCase(Day.Thursday | Day.Friday | Day.Sunday | Day.Saturday | Day.Monday)]
        [TestCase(Day.Friday | Day.Saturday | Day.Sunday | Day.Monday | Day.Tuesday | Day.Wednesday)]
        [TestCase(Day.All)]
        public void GetWeeklyOccurrences_MultipleDayOfWeek_ReturnsAllDatesOnTheDaysOfWeekPassedIn(Day day)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: day);

            // assert
            var returnedDays = Day.None;
            foreach (var date in result)
            {
                returnedDays |= date.DayOfWeek.ToDay();
            }
            Assert.AreEqual(day, returnedDays);
            Assert.That(result.All(r => day.HasFlag(r.DayOfWeek.ToDay())));
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
            var result = scheduleLogic.GetWeeklyOccurrences(Day.Monday, interval: interval);

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
            var result = scheduleLogic.GetWeeklyOccurrences(Day.Monday, interval: interval, startDate: startDate, endDate: endDate);

            // assert
            var expectedResult = weeksBetween / interval;
            if (expectedResult == 0)
            {
                expectedResult = 1;
            }
            Assert.AreEqual(expectedResult, result.Count());
        }

        [TestCase(1, Day.Monday | Day.Friday)]
        [TestCase(101, Day.All)]
        public void GetWeeklyOccurrences_MultipleDaysOfWeekAndPositiveInterval_ReturnsNotZero(int interval, Day dayOfWeek)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: dayOfWeek, interval: interval);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(1, 5, Day.Monday | Day.Tuesday, 10)]
        [TestCase(101, 5, Day.All, 7)]
        [TestCase(1, 98, Day.Friday | Day.Tuesday, 196)]
        [TestCase(101, 98, Day.All, 7)]
        public void GetWeeklyOccurrences_MultipleDaysOfWeekAndPositiveInterval_ReturnsNumberOfDaysTimesWeeksBetweenDividedByInterval(int interval, int weeksBetween, Day dayOfWeek, int expectedResult)
        {
            // arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddWeeks(weeksBetween);

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(daysOfWeek: dayOfWeek, interval: interval, startDate: startDate, endDate: endDate);

            // assert
            Assert.AreEqual(expectedResult, result.Count());
        }
    }
}
