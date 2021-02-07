using System;
namespace BudgetBadger.Models.Schedule
{
    [Flags]
    public enum Week
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4,
        Fourth = 8,
        Last = 16,
        All = First | Second | Third | Fourth | Last
    }
}
