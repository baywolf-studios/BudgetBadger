using System;
namespace BudgetBadger.Models.Schedule
{
    public class Schedule
    {
        public Frequency Frequency { get; set; }

        public int RepeatEvery { get; set; }

        public Month MonthsOfYear { get; set; }

        public Quarter QuartersOfYear { get; set; }

        public Month MonthsOfQuarter { get; set; }

        public Week WeeksOfMonth { get; set; }

        public Day DaysOfWeek { get; set; }

        public Schedule()
        {
        }
    }
}
