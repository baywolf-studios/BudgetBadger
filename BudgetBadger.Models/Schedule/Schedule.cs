using System;
namespace BudgetBadger.Models.Schedule
{
    public class Schedule
    {
        public Frequency Frequency { get; set; }

        public int Interval { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public WeeksOfMonth WeeksOfMonth { get; set; }

        public DaysOfMonth DaysOfMonth { get; set; }

        public MonthsOfYear MonthsOfYear { get; set; }

        public int? Count { get; set; }

        public DateTime? Until { get; set; }

        public Schedule()
        {
        }
    }
}
