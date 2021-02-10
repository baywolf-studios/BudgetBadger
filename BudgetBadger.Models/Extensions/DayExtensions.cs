using System;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Models.Extensions
{
    public static class DayExtensions
    {
        public static Day ToDay(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    return Day.Friday;
                case DayOfWeek.Monday:
                    return Day.Monday;
                case DayOfWeek.Saturday:
                    return Day.Saturday;
                case DayOfWeek.Sunday:
                    return Day.Sunday;
                case DayOfWeek.Thursday:
                    return Day.Thursday;
                case DayOfWeek.Tuesday:
                    return Day.Tuesday;
                case DayOfWeek.Wednesday:
                    return Day.Wednesday;
                default:
                    break;
            }

            return Day.None;
        }
    }
}
