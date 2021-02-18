using System;
namespace BudgetBadger.Models.Schedule
{
    [Flags]
    public enum WeeksOfMonth
    {
        None = 0,
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
        Last = 1 << 4,
        All = First | Second | Third | Fourth | Last
    }
}
