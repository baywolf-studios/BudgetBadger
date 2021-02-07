using System;
namespace BudgetBadger.Models.Schedule
{
    [Flags]
    public enum Day
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
    }
}
