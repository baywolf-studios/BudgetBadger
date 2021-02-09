using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Logic
{
    public class ScheduleLogic : IScheduleLogic
    {
        public IEnumerable<DateTime> GetOccurrences(DateTime startDate, Schedule schedule)
        {
            switch (schedule.Frequency)
            {
                case Frequency.Daily:
                    return GetDailyOccurrences(schedule.Interval, startDate, schedule.Until).Take(schedule?.Count ?? int.MaxValue);
                case Frequency.Weekly:
                    break;
                case Frequency.Monthly:
                    break;
                case Frequency.Quarterly:
                    break;
                case Frequency.Yearly:
                    break;
                case Frequency.None:
                default:
                    break;
            }

            return Enumerable.Empty<DateTime>();
        }

        public IEnumerable<DateTime> GetDatesFromDateRange(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sDate = startDate ?? DateTime.MinValue;
            var eDate = endDate ?? DateTime.MaxValue;

            for (var day = sDate.Date;
                day.Date < eDate.Date;
                day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public IEnumerable<DateTime> GetDailyOccurrences(int interval = 1,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (interval < 1)
                yield break;


            var sDate = startDate ?? DateTime.MinValue;

            foreach(var day in GetDatesFromDateRange(startDate, endDate))
            {
                if ((day.Date - sDate.Date).TotalDays % interval == 0)
                {
                    yield return day;
                }
            }
        }

        public IEnumerable<DateTime> GetWeeklyOccurrences(int interval = 1,
            Day daysOfWeek = Day.All,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (interval < 1)
                yield break;

            var sDate = startDate ?? DateTime.MinValue;

            foreach (var day in GetDatesFromDateRange(startDate, endDate))
            {
                if ((day.Date - sDate.Date).TotalWeeks() % interval == 0)
                {
                    yield return day;
                }
            }
        }
    }
}
