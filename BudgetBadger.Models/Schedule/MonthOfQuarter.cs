using System;
namespace BudgetBadger.Models.Schedule
{
    [Flags]
    public enum MonthOfQuarter
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4
    }
}
