using System;
using System.Collections.Generic;
using BudgetBadger.Core.Models.Recurrence;

namespace BudgetBadger.Core.Recurrence
{
    public interface IRecurrenceEngine
    {
        IEnumerable<DateTime> GetOccurrences(DateTime startDate, RecurrencePattern recurrencePattern);
    }
}
