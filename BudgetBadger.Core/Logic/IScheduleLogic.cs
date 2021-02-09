using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;
using BudgetBadger.Models.Schedule;

namespace BudgetBadger.Core.Logic
{
    public interface IScheduleLogic
    {
        IReadOnlyList<DateTime> GetOccurrences(DateTime startDate, Schedule schedule);
    }
}
