using System;
using System.Collections.Generic;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Logic
{
    public class ScheduleLogic : IScheduleLogic
    {

        public IReadOnlyList<DateTime> GetOccurrences(DateTime startDate, Schedule schedule)
        {
            var result = new List<DateTime>();

            switch (schedule.Frequency)
            {
                case Frequency.Daily:
                    result.AddRange(GetDailyOccurrences(schedule.Interval, startDate, schedule.Until, schedule.Count));
                    break;
            }

            return result;
        }

        protected IReadOnlyList<DateTime> GetDailyOccurrences(int interval, DateTime startDate, DateTime? endDate, int? count)
        {
            var result = new List<DateTime>();

            for (var day = startDate.Date; day.Date <= (endDate?.Date ?? DateTime.MaxValue); day = day.AddDays(1))
            {
                if (day > startDate)
                {
                    if ((day - startDate).Days % interval == 0)
                    {
                        result.Add(day);
                    }
                }
                
            }

            return result;
        }
    }
}
