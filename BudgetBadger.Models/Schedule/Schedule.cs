using System;
namespace BudgetBadger.Models.Schedule
{
    public class Schedule
    {
        public Frequency Frequency { get; set; }

        public int Interval { get; set; }

        public Month Months { get; set; }

        public Week Weeks { get; set; }

        public Day Days { get; set; }

        public int? Count { get; set; }

        public DateTime? Until { get; set; }

        public Schedule()
        {
        }
    }
}
