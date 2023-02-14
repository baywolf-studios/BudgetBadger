using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models.Recurrence;

namespace BudgetBadger.Core.Recurrence
{
    public class RecurrenceEngine : IRecurrenceEngine
    {
        public IEnumerable<DateTime> GetOccurrences(DateTime startDate, RecurrencePattern recurrencePattern)
        {
            var occurrences = Enumerable.Empty<DateTime>();

            switch (recurrencePattern.Frequency)
            {
                case Frequency.Daily:
                    occurrences = GetDailyOccurrences(recurrencePattern.Interval, startDate, recurrencePattern.Until);
                    break;
                case Frequency.Weekly:
                    occurrences = GetWeeklyOccurrences(recurrencePattern.DaysOfWeek, recurrencePattern.Interval, startDate, recurrencePattern.Until);
                    break;
                case Frequency.Monthly:
                    occurrences = GetMonthlyOccurrences(recurrencePattern.DaysOfMonth,
                        recurrencePattern.DaysOfWeek,
                        recurrencePattern.WeeksOfMonth,
                        recurrencePattern.Interval,
                        startDate,
                        recurrencePattern.Until);
                    break;
                case Frequency.Yearly:
                    occurrences = GetYearlyOccurrences(recurrencePattern.DaysOfMonth,
                        recurrencePattern.DaysOfWeek,
                        recurrencePattern.WeeksOfMonth,
                        recurrencePattern.MonthsOfYear,
                        recurrencePattern.Interval,
                        startDate,
                        recurrencePattern.Until);
                    break;
                case Frequency.None:
                default:
                    break;
            }

            return occurrences.Take(recurrencePattern.Count ?? int.MaxValue);
        }

        public static IEnumerable<DateTime> GetDatesFromDateRange(DateTime startDate, DateTime? endDate = null)
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
                if ((day.DaysSinceEpoch() - sDate.DaysSinceEpoch()) % interval == 0)
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

            foreach (var day in GetDatesFromDateRange(sDate, endDate))
            {
                if (daysOfWeek.HasAnyFlag(day.ToDaysOfWeek()))
                {
                    if ((day.WeeksSinceEpoch() - sDate.WeeksSinceEpoch()) % interval == 0)
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

            foreach (var day in GetDatesFromDateRange(sDate, endDate))
            {
                if (daysOfMonth.HasAnyFlag(day.ToDaysOfMonth())
                    && weeksOfMonth.HasAnyFlag(day.ToWeeksOfMonth())
                    && daysOfWeek.HasAnyFlag(day.ToDaysOfWeek()))
                {
                    
                    if ((day.MonthsSinceEpoch() - sDate.MonthsSinceEpoch()) % interval == 0)
                    {
                        yield return day;
                    }
                }
            }
        }

        public IEnumerable<DateTime> GetYearlyOccurrences(DaysOfMonth daysOfMonth = DaysOfMonth.All,
            DaysOfWeek daysOfWeek = DaysOfWeek.All,
            WeeksOfMonth weeksOfMonth = WeeksOfMonth.All,
            MonthsOfYear monthsOfYear = MonthsOfYear.All,
            int interval = 1,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (interval < 1
                || daysOfWeek == DaysOfWeek.None
                || weeksOfMonth == WeeksOfMonth.None
                || daysOfMonth == DaysOfMonth.None
                || monthsOfYear == MonthsOfYear.None)
            {
                yield break;
            }

            var sDate = startDate ?? DateTime.MinValue;

            foreach (var day in GetDatesFromDateRange(sDate, endDate))
            {
                if (daysOfMonth.HasAnyFlag(day.ToDaysOfMonth())
                    && weeksOfMonth.HasAnyFlag(day.ToWeeksOfMonth())
                    && daysOfWeek.HasAnyFlag(day.ToDaysOfWeek())
                    && monthsOfYear.HasAnyFlag(day.ToMonthsOfYear()))
                {
                    
                    if ((day.YearsSinceEpoch() - sDate.YearsSinceEpoch()) % interval == 0)
                    {
                        yield return day;
                    }
                }
            }
        }
    }
}
