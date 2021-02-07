using System;
namespace BudgetBadger.Models.Schedule
{
    [Flags]
    public enum Quarter
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4,
        Fourth = 8,
        All = First | Second | Third | Fourth
    }
}
