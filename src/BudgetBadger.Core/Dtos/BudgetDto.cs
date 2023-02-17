using System;

namespace BudgetBadger.Core.Dtos
{
    public record BudgetDto
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public bool IgnoreOverspend { get; init; }
        public Guid BudgetPeriodId { get; init; }
        public Guid EnvelopeId { get; init; }
        public DateTime ModifiedDateTime { get; init; }
    }
}

