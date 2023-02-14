using System;
using BudgetBadger.Core.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestPayees
    {
        public static readonly Payee NewPayee = new Payee
        {
            Description = nameof(NewPayee)
        };

        public static readonly Payee ActivePayee = new Payee
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActivePayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly Payee HiddenPayee = new Payee
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenPayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly Payee SoftDeletedPayee = new Payee
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedPayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };
    }
}
