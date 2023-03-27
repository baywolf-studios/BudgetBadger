using System;

namespace BudgetBadger.DataAccess.Dtos
{
    public record TransactionDto
    {
        public Guid Id { get; init; }
        public decimal Amount { get; init; }
        public bool Posted { get; init; }
        public bool Reconciled { get; init; }
        public bool Pending => !Posted && !Reconciled;
        public Guid AccountId { get; init; }
        public Guid PayeeId { get; init; }
        public Guid EnvelopeId { get; init; }
        public Guid? SplitId { get; init; }
        public DateTime ServiceDate { get; init; }
        public string Notes { get; init; }
        public bool Deleted { get; init; }
        public DateTime ModifiedDateTime { get; init; }
    }
}

