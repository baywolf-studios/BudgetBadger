using System.IO;
using System.Linq;

namespace BudgetBadger.Core.Utilities
{
    public static class StringExtensions
    {
        public static bool IsInvalidPath(this string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            return !string.IsNullOrEmpty(path) && path.Any(c => invalidChars.Contains(c));
        }
    }
}
