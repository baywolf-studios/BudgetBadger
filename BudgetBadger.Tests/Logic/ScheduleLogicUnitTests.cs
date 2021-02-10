using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models.Schedule;
using BudgetBadger.Models.Extensions;
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
        public void GetWeeklyOccurrences_ZeroInterval_ReturnsZero()
        {
            // arrange
            var startDate = DateTime.Now;
            var interval = 0;

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval,startDate: startDate);

            // assert
            Assert.Zero(result.Count());
        }

        [TestCase(-1)]
        [TestCase(-101)]
        public void GetWeeklyOccurrences_NegativeInterval_ReturnsZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval);

            // assert
            Assert.AreEqual(0, result.Count());
        }

        [TestCase(1)]
        [TestCase(101)]
        public void GetWeeklyOccurrences_PositiveInterval_ReturnsNotZero(int interval)
        {
            // arrange

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval);

            // assert
            Assert.NotZero(result.Count());
        }

        [TestCase(1, 5)]
        [TestCase(101, 5)]
        [TestCase(1, 98)]
        [TestCase(101, 98)]
        public void GetWeeklyOccurrences_PositiveInterval_ReturnsWeeksBetweenDividedByInterval(int interval, int weeksBetween)
        {
            // arrange
            var startDate = DateTime.Now;
            var endDate = startDate.AddWeeks(weeksBetween);

            // act
            var result = scheduleLogic.GetWeeklyOccurrences(interval, startDate: startDate, endDate: endDate);

            // assert
            var expectedResult = weeksBetween / interval;
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
    }
}
