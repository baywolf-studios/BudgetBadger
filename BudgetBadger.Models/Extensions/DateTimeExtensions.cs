using System;
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
    }
}
