using System;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Utilities
{
    public static class TranslationExtensions
    {
        public static void TranslateAccount(this Account account, IResourceContainer resourceContainer)
        {
            if (account is null)
            {
                return;
            }

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
            if (payee is null)
            {
                return;
            }

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

        public static void TranslateEnvelopeGroup(this EnvelopeGroup envelopeGroup, IResourceContainer resourceContainer)
        {
            if (envelopeGroup is null)
            {
                return;
            }

            if (envelopeGroup.IsGenericHiddenEnvelopeGroup)
            {
                envelopeGroup.Description = resourceContainer.GetResourceString("Hidden");
            }

            if (envelopeGroup.IsSystem)
            {
                envelopeGroup.Description = resourceContainer.GetResourceString(nameof(Constants.SystemEnvelopeGroup));
            }

            if (envelopeGroup.IsIncome)
            {
                envelopeGroup.Description = resourceContainer.GetResourceString(nameof(Constants.IncomeEnvelopeGroup));
            }

            if (envelopeGroup.IsDebt)
            {
                envelopeGroup.Description = resourceContainer.GetResourceString(nameof(Constants.DebtEnvelopeGroup));
            }
        }

        public static void TranslateEnvelope(this Envelope envelope, IResourceContainer resourceContainer)
		{
            if (envelope is null)
            {
                return;
            }

            if (envelope.IsGenericHiddenEnvelope)
            {
                envelope.Description = resourceContainer.GetResourceString("Hidden");
            }

            if (envelope.IsGenericDebtEnvelope)
            {
                envelope.Description = resourceContainer.GetResourceString(nameof(Constants.GenericDebtEnvelope));
            }

            if (envelope.IsIncome)
            {
                envelope.Description = resourceContainer.GetResourceString(nameof(Constants.IncomeEnvelope));
            }

            if (envelope.IsBuffer)
            {
                envelope.Description = resourceContainer.GetResourceString(nameof(Constants.BufferEnvelope));
            }

            if (envelope.IsSystem)
            {
                envelope.Description = resourceContainer.GetResourceString(nameof(Constants.IgnoredEnvelope));
            }

            envelope.Group.TranslateEnvelopeGroup(resourceContainer);
        }
    }
}
