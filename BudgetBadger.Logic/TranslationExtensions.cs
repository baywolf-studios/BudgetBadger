using System;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public static class TranslationExtensions
    {
        public static void TranslateAccount(this Account account, IResourceContainer resourceContainer)
        {
            if (account.IsGenericHiddenAccount)
            {
                account.Description = resourceContainer.GetResourceString("Hidden");
                account.Group = resourceContainer.GetResourceString("Hidden");
            }

            if (account.IsHidden)
            {
                account.Group = resourceContainer.GetResourceString("Hidden");
            }
            else
            {
                account.Group = resourceContainer.GetResourceString(Enum.GetName(typeof(AccountType), account.Type));
            }
        }

        public static void TranslatePayee(this Payee payee, IResourceContainer resourceContainer)
        {
            if (payee.IsGenericHiddenPayee)
            {
                payee.Description = resourceContainer.GetResourceString("Hidden");
                payee.Group = resourceContainer.GetResourceString("Hidden");
            }
            else if (payee.IsStartingBalance)
            {
                payee.Description = resourceContainer.GetResourceString(nameof(Constants.StartingBalancePayee));
                payee.Group = String.Empty;
            }
            else if (string.IsNullOrEmpty(payee.Description))
            {
                payee.Group = String.Empty;
            }
            else if (payee.IsAccount)
            {
                payee.Group = resourceContainer.GetResourceString("PayeeTransferGroup");
            }
            else if (payee.IsHidden)
            {
                payee.Group = resourceContainer.GetResourceString("Hidden");
            }
            else
            {
                payee.Group = payee.Description[0].ToString().ToUpper();
            }
        }

        public static void TranslateEnvelope(this Envelope envelope, IResourceContainer resourceContainer)
		{
            if (envelope.IsGenericDebtEnvelope)
            {
                envelope.Description = resourceContainer.GetResourceString(nameof(Constants.GenericDebtEnvelope));
            }
		}
    }
}
