using System;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models.Schedule;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.Tests.Logic
{
    public class ScheduleLogicUnitTests
    {
        ScheduleLogic scheduleLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            scheduleLogic = new ScheduleLogic();
        }

        [Test]
        public void GetDailyOccurrences_Interval1AndSetEndDate_ReturnsDayRange()
        {
            var startDate = DateTime.Now;

            // arrange
            var schedule = new Schedule()
            {
                Frequency = Frequency.Daily,
                Interval = 1,
                Until = startDate.AddDays(20)
            };

            // act
            var result = scheduleLogic.GetOccurrences(startDate, schedule);

            // assert
            Assert.AreEqual(result.Count, 20);
        }
    }
}
