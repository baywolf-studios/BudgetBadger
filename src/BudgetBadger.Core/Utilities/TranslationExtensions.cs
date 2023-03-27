using System;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Utilities
{
    public static class TranslationExtensions
    {
        public static void TranslateAccount(this AccountModel account, IResourceContainer resourceContainer)
        {
            if (account is null)
            {
                return;
            }

            if (account.IsHidden)
            {
                account.Group = resourceContainer.GetResourceString("Hidden");
            }
            else
            {
                account.Group = resourceContainer.GetResourceString(Enum.GetName(typeof(AccountModelType), account.Type));
            }
        }

        public static void TranslatePayee(this PayeeModel payee, IResourceContainer resourceContainer)
        {
            if (payee is null)
            {
                return;
            }

            if (payee.IsStartingBalance)
            {
                payee.Description = resourceContainer.GetResourceString(nameof(Constants.StartingBalancePayee));
                payee.Group = string.Empty;
            }
            else if (string.IsNullOrEmpty(payee.Description))
            {
                payee.Group = string.Empty;
            }
            else if (payee.IsAccount)
            {
                payee.Group = resourceContainer.GetResourceString("PayeeTransferGroup");
            }
            else
            {
                payee.Group = payee.Description[0].ToString().ToUpper();
            }
        }

        public static void TranslateEnvelopeGroup(this EnvelopeGroupModel envelopeGroup, IResourceContainer resourceContainer)
        {
            if (envelopeGroup is null)
            {
                return;
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
