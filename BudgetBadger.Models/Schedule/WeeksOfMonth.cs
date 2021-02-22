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
        Fifth = 1 << 4,
        Sixth = 1 << 5,
        Last = 1 << 6,
        All = First | Second | Third | Fourth | Fifth | Sixth | Last
    }
}
