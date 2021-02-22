using System;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Models.Extensions
{
    public static class ScheduleExtensions
    {
        public static DaysOfWeek ToDaysOfWeek(this DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

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

        public static DaysOfMonth ToDaysOfMonth(this DateTime date)
        {
            var dayOfMonth = date.Day;
            var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);

            switch (dayOfMonth)
            {
                case 1:
                    return DaysOfMonth.Day01;
                case 2:
                    return DaysOfMonth.Day02;
                case 3:
                    return DaysOfMonth.Day03;
                case 4:
                    return DaysOfMonth.Day04;
                case 5:
                    return DaysOfMonth.Day05;
                case 6:
                    return DaysOfMonth.Day06;
                case 7:
                    return DaysOfMonth.Day07;
                case 8:
                    return DaysOfMonth.Day08;
                case 9:
                    return DaysOfMonth.Day09;
                case 10:
                    return DaysOfMonth.Day10;
                case 11:
                    return DaysOfMonth.Day11;
                case 12:
                    return DaysOfMonth.Day12;
                case 13:
                    return DaysOfMonth.Day13;
                case 14:
                    return DaysOfMonth.Day14;
                case 15:
                    return DaysOfMonth.Day15;
                case 16:
                    return DaysOfMonth.Day16;
                case 17:
                    return DaysOfMonth.Day17;
                case 18:
                    return DaysOfMonth.Day18;
                case 19:
                    return DaysOfMonth.Day19;
                case 20:
                    return DaysOfMonth.Day20;
                case 21:
                    return DaysOfMonth.Day21;
                case 22:
                    return DaysOfMonth.Day22;
                case 23:
                    return DaysOfMonth.Day23;
                case 24:
                    return DaysOfMonth.Day24;
                case 25:
                    return DaysOfMonth.Day25;
                case 26:
                    return DaysOfMonth.Day26;
                case 27:
                    return DaysOfMonth.Day27;
                case 28:
                    if (lastDayOfMonth == 28)
                        return DaysOfMonth.Day28 | DaysOfMonth.DayLast;
                    return DaysOfMonth.Day28;
                case 29:
                    if (lastDayOfMonth == 29)
                        return DaysOfMonth.Day29 | DaysOfMonth.DayLast;
                    return DaysOfMonth.Day29;
                case 30:
                    if (lastDayOfMonth == 30)
                        return DaysOfMonth.Day30 | DaysOfMonth.DayLast;
                    return DaysOfMonth.Day30;
                case 31:
                    if (lastDayOfMonth == 31)
                        return DaysOfMonth.Day31 | DaysOfMonth.DayLast;
                    return DaysOfMonth.Day31;
                default:
                    break;
            }

            return DaysOfMonth.None;
        }

        public static WeeksOfMonth ToWeeksOfMonth(this DateTime dateTime)
        {
            var week = dateTime.WeekOfMonth();
            var lastWeekInMonth = dateTime.WeeksInMonth();

            switch (week)
            {
                case 1:
                    return WeeksOfMonth.First;
                case 2:
                    return WeeksOfMonth.Second;
                case 3:
                    if (lastWeekInMonth == 3)
                        return WeeksOfMonth.Third | WeeksOfMonth.Last;
                    return WeeksOfMonth.Third;
                case 4:
                    if (lastWeekInMonth == 4)
                        return WeeksOfMonth.Fourth | WeeksOfMonth.Last;
                    return WeeksOfMonth.Fourth;
                case 5:
                    if (lastWeekInMonth == 5)
                        return WeeksOfMonth.Fifth | WeeksOfMonth.Last;
                    return WeeksOfMonth.Fifth;
                case 6:
                    if (lastWeekInMonth == 6)
                        return WeeksOfMonth.Sixth | WeeksOfMonth.Last;
                    return WeeksOfMonth.Sixth;
                default:
                    break;
            }

            return WeeksOfMonth.None;
        }
    }
}
