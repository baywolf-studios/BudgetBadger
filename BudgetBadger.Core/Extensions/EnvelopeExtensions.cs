using System;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Extensions
{
    public static class EnvelopeExtensions
    {
        public static bool IsIncome(this Envelope envelope)
        {
            return envelope.Id == Constants.IncomeEnvelope.Id;
        }

        public static bool IsBuffer(this Envelope envelope)
        {
            return envelope.Id == Constants.BufferEnvelope.Id;
        }

        public static bool IsTransfer(this Envelope envelope)
        {
            return envelope.Id == Constants.TransferEnvelope.Id;
        }
    }
}
