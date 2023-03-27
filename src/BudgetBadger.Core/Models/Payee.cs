using System;
namespace BudgetBadger.Logic.Models
{
    public readonly record struct PayeeId(Guid Id)
    {
        public PayeeId() : this(Guid.NewGuid()) { }
        public override string ToString() => Id.ToString();
        public static implicit operator Guid(PayeeId payeeId) => payeeId.Id;
    }

    public record Payee()
    {
        public PayeeId Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public bool Hidden { get; init; }
        public string Group { get; init; }
    }
}

