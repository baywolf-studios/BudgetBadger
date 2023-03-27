using System;
using BudgetBadger.Core.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestPayees
    {
        public static readonly PayeeModel NewPayee = new PayeeModel
        {
            Description = nameof(NewPayee)
        };

        public static readonly PayeeModel ActivePayee = new PayeeModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActivePayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly PayeeModel HiddenPayee = new PayeeModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenPayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly PayeeModel SoftDeletedPayee = new PayeeModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedPayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };

        public static readonly PayeeModel AccountPayee = new PayeeModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(AccountPayee),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now,
            IsAccount = true
        };
    }
}
