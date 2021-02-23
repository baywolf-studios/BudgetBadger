using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBadger.Models.Extensions
{
    public static class FlagExtensions
    {
        public static IEnumerable<Enum> GetComponents(this Enum value)
        {
            var values = Enum.GetValues(value.GetType()).Cast<Enum>();
            return values.Where(v => value.HasFlag(v));
        }
    }
}
