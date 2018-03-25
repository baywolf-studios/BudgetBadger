using System;
namespace BudgetBadger.Models.Extensions
{
    public static class PayeeExtensions
    {
        public static bool IsStartingBalance(this Payee payee)
        {
            return payee.Id == Constants.StartingBalancePayee.Id;
        }
    }
}