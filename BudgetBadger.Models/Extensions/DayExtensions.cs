using System;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Models.Extensions
{
    public static class DayExtensions
    {
        public static DaysOfWeek ToDay(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    return DaysOfWeek.Friday;
                case DayOfWeek.Monday:
                    return DaysOfWeek.Monday;
                case DayOfWeek.Saturday:
                    return DaysOfWeek.Saturday;
                case DayOfWeek.Sunday:
                    return DaysOfWeek.Sunday;
                case DayOfWeek.Thursday:
                    return DaysOfWeek.Thursday;
                case DayOfWeek.Tuesday:
                    return DaysOfWeek.Tuesday;
                case DayOfWeek.Wednesday:
                    return DaysOfWeek.Wednesday;
                default:
                    break;
            }

            return DaysOfWeek.None;
        }
    }
}
