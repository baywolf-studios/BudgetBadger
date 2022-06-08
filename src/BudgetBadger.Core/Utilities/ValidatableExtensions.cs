using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Core.Utilities
{
    public static class ValidatableExtensions
    {
        public static bool IsValid(this IValidatable input)
        {
            return input.Validate().Success;
        }

        public static string ValidationMessage(this IValidatable input)
        {
            return input.Validate().Message;
        }
    }
}
