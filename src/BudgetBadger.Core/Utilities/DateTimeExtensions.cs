using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace BudgetBadger.Core.Utilities
{
    public static class DateTimeExtensions
    {
        public static bool IsEmpty(this DateTime dateTime)
        {
            return dateTime == DateTime.MinValue;
        }

        public static Guid ToGuid(this DateTime dateTime)
        {
            using var hash = new SHA1CryptoServiceProvider();
            var dateBytes = BitConverter.GetBytes(dateTime.Ticks);
            var hashedBytes = hash.ComputeHash(dateBytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }

        public static DateTime AddWeeks(this DateTime dateTime, int weeks)
        {
            return dateTime.AddDays(weeks * 7);
        }

        public static int WeekOfMonth(this DateTime date)
        {
            return (int)Math.Ceiling(date.Day / 7f);
        }

        public static int DaysSinceEpoch(this DateTime date)
        {
            return date.Date.Subtract(DateTime.MinValue.Date).Days;
        }

        public static int WeeksSinceEpoch(this DateTime date)
        {
            var daysSinceEpoch = date.DaysSinceEpoch();
            return (int)Math.Floor(daysSinceEpoch / 7f);
        }

        public static int MonthsSinceEpoch(this DateTime date)
        {
            var months = (date.Year - 1) * 12;
            months += date.Month;
            return months - 1;
        }

        public static int YearsSinceEpoch(this DateTime date)
        {
            return date.Year - 1;
        }
    }
}
