using System;

namespace BudgetBadger.Logic.Models
{
    public readonly record struct EnvelopeGroupId(Guid Id)
    {
        public EnvelopeGroupId() : this(Guid.NewGuid()) { }
        public override string ToString() => Id.ToString();
        public static implicit operator Guid(EnvelopeGroupId payeeId) => payeeId.Id;
    }

    public record EnvelopeGroup()
    {
        public EnvelopeGroupId Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public bool Hidden { get; init; }
    }
}

