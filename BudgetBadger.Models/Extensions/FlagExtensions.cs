using System;
namespace BudgetBadger.Models.Extensions
{
    public static class FlagExtensions
    {
        public static bool HasAnyFlag(this Enum type, Enum value)
        {
            return (((int)(object)type) & ((int)(object)value)) != 0;
        }
    }
}
