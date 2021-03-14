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
            IEnumerable<DateTime> occurrences = Enumerable.Empty<DateTime>();

            switch (schedule.Frequency)
            {
                case Frequency.Daily:
                    occurrences = GetDailyOccurrences(schedule.Interval, startDate, schedule.Until);
                    break;
                case Frequency.Weekly:
                    occurrences = GetWeeklyOccurrences(schedule.DaysOfWeek, schedule.Interval, startDate, schedule.Until);
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

            return occurrences.Take(schedule?.Count ?? int.MaxValue);
        }

        public IEnumerable<DateTime> GetDatesFromDateRange(DateTime startDate, DateTime? endDate = null)
        {
            var eDate = endDate ?? DateTime.MaxValue;

            for (var day = startDate.Date;
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

            foreach(var day in GetDatesFromDateRange(sDate, endDate))
            {
                if ((day.Date - sDate.Date).TotalDays % interval == 0)
                {
                    yield return day;
                }
            }
        }

        public IEnumerable<DateTime> GetWeeklyOccurrences(DaysOfWeek daysOfWeek = DaysOfWeek.All,
            int interval = 1,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (interval < 1 || daysOfWeek == DaysOfWeek.None)
                yield break;

            var sDate = startDate ?? DateTime.MinValue;
            List<DateTime> firstDateTimes = new List<DateTime>();

            foreach (var day in GetDatesFromDateRange(sDate, endDate))
            {
                if (daysOfWeek.HasAnyFlag(day.ToDaysOfWeek()))
                {
                    if (!firstDateTimes.Any(d => d.DayOfWeek == day.DayOfWeek))
                        firstDateTimes.Add(day);

                    var firstDateTime = firstDateTimes.First(d => d.DayOfWeek == day.DayOfWeek);

                    if ((day.Date - firstDateTime.Date).TotalWeeks() % interval == 0)
                    {
                        yield return day;
                    }
                }
            }
        }

        public IEnumerable<DateTime> GetMonthlyOccurrences(DaysOfMonth daysOfMonth = DaysOfMonth.All,
            DaysOfWeek daysOfWeek = DaysOfWeek.All,
            WeeksOfMonth weeksOfMonth = WeeksOfMonth.All,
            int interval = 1,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (interval < 1
                || daysOfWeek == DaysOfWeek.None
                || weeksOfMonth == WeeksOfMonth.None
                || daysOfMonth == DaysOfMonth.None)
            {
                yield break;
            }

            var sDate = startDate ?? DateTime.MinValue;
            List<DateTime> firstDateTimes = new List<DateTime>();

            foreach (var day in GetDatesFromDateRange(sDate, endDate))
            {
                if (daysOfMonth.HasAnyFlag(day.ToDaysOfMonth())
                    && weeksOfMonth.HasAnyFlag(day.ToWeeksOfMonth())
                    && daysOfWeek.HasAnyFlag(day.ToDaysOfWeek()))
                {
                    yield return day;
                }
            }
        }
    }
}
