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
            return input.ValidationResult().Success;
        }

        public static string ValidationMessage(this IValidatable input)
        {
            return input.ValidationResult().Message;
        }

        public static Result ValidationResult(this IValidatable input)
        {
            return input == null ? new Result { Success = false, Message = "Null input is invalid." } : input.Validate();
        }
    }
}
