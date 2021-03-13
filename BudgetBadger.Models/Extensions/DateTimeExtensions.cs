//'\]
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
            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth);
        }

        public static int WeekOfMonth(this DateTime date)
        {
            var test = (int)Math.Ceiling(date.Day / 7f);
            return test;
        }

        public static int WeeksInMonth(this DateTime date)
        {
            var lastDayInMonth = date.LastDayOfMonth();
            var daysInMonth = lastDayInMonth.Day;
            return (int)Math.Ceiling(daysInMonth / 7f);
        }
    }
}
