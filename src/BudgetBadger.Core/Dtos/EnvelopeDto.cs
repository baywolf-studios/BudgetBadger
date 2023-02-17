using System;

namespace BudgetBadger.Core.Dtos
{
    public record EnvelopeDto
    {
        public Guid Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public Guid EnvelopGroupId { get; init; }
        public bool IgnoreOverspend { get; init; }
        public bool Hidden { get; init; }
        public bool Deleted { get; init; }
        public DateTime ModifiedDateTime { get; init; }
    }
}

