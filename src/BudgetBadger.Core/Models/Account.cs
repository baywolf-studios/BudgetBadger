using System;
namespace BudgetBadger.Logic.Models
{
    public readonly record struct AccountId(Guid Id)
    {
        public AccountId() : this(Guid.NewGuid()) { }
        public override string ToString() => Id.ToString();
        public static implicit operator Guid(AccountId accountId) => accountId.Id;
    }

    public enum AccountType
    {
        Budget,
        Reporting,
    }

    public record Account()
    {
        public AccountId Id { get; init; }
        public string Description { get; init; }
        public string Notes { get; init; }
        public bool Hidden { get; init; }
        public string Group { get; init; }
        public AccountType Type { get; init; }
        public decimal Balance { get; init; }
        public decimal Pending { get; init; }
        public decimal Posted { get; init; }
        public decimal Payment { get; init; }
    }
}

