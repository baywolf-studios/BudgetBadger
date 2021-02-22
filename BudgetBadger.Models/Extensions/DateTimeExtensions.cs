using System;
using System.Globalization;
using System.Security.Cryptography;

namespace BudgetBadger.Models.Extensions
{
    public static class DateTimeExtensions
    {
        public static Guid ToGuid(this DateTime dateTime)
        {
            using (var hash = new SHA1CryptoServiceProvider())
            {
                byte[] dateBytes = BitConverter.GetBytes(dateTime.Ticks);
                byte[] hashedBytes = new SHA1CryptoServiceProvider().ComputeHash(dateBytes);
                Array.Resize(ref hashedBytes, 16);
                return new Guid(hashedBytes);
            }
        }

        public static double TotalWeeks(this TimeSpan timeSpan)
        {
            return timeSpan.TotalDays / 7;
        }

        public static DateTime AddWeeks(this DateTime dateTime, int weeks)
        {
            return dateTime.AddDays(weeks * 7);
        }

        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth);
        }

        public static int GetWeekOfYear(this DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.Date,
                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
        }

        public static int WeekOfMonth(this DateTime date)
        {
            var firstDayOfMonth = date.FirstDayOfMonth();
            return (date.GetWeekOfYear() - firstDayOfMonth.GetWeekOfYear()) + 1;
        }

        public static int WeeksInMonth(this DateTime date)
        {
            var firstDayOfMonth = date.FirstDayOfMonth();
            var lastDayOfMonth = date.LastDayOfMonth();
            return (lastDayOfMonth.GetWeekOfYear() - firstDayOfMonth.GetWeekOfYear()) + 1;
        }
    }
}
