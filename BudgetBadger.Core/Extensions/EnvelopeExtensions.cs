using System;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Extensions
{
    public static class EnvelopeExtensions
    {
        public static bool IsIncome(this EnvelopeGroup envelopeGroup)
        {
            return envelopeGroup.Id == Constants.IncomeEnvelopeGroup.Id;
        }

        public static bool IsIncome(this Envelope envelope)
        {
            return envelope.Id == Constants.IncomeEnvelope.Id;
        }

        public static bool IsBuffer(this Envelope envelope)
        {
            return envelope.Id == Constants.BufferEnvelope.Id;
        }

        public static bool IsSystem(this EnvelopeGroup envelopeGroup)
        {
            return envelopeGroup.Id == Constants.SystemEnvelopeGroup.Id;
        }

        public static bool IsSystem(this Envelope envelope)
        {
            return envelope.Id == Constants.IgnoredEnvelope.Id;
        }

        public static bool IsDebt(this EnvelopeGroup envelopeGroup)
        {
            return envelopeGroup.Id == Constants.DebtEnvelopeGroup.Id;
        }

        public static bool IsGenericDebtEnvelope(this Envelope envelope)
        {
            return envelope.Id == Constants.GenericDebtEnvelope.Id;
        }
    }
}
