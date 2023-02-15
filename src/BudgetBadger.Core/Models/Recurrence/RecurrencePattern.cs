using System;

namespace BudgetBadger.Core.Models.Recurrence
{
    public class RecurrencePattern
    {
        public RecurrencePattern()
        {
            Frequency = Frequency.Daily;
            Interval = 1;
            DaysOfMonth = DaysOfMonth.All;
            DaysOfWeek = DaysOfWeek.All;
            WeeksOfMonth = WeeksOfMonth.All;
            MonthsOfYear = MonthsOfYear.All;
            Count = null;
            Until = null;
        }
        
        public Frequency Frequency { get; set; }

        public int Interval { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public WeeksOfMonth WeeksOfMonth { get; set; }

        public DaysOfMonth DaysOfMonth { get; set; }

        public MonthsOfYear MonthsOfYear { get; set; }

        public int? Count { get; set; }

        public DateTime? Until { get; set; }
    }
}
