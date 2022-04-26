using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BudgetBadger.Models.Interfaces;
using System.Linq;

namespace BudgetBadger.Models.Extensions
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
