using System;
using BudgetBadger.Core.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestAccounts
    {
        public static readonly AccountModel NewAccount = new AccountModel
        {
            Description = nameof(NewAccount)
        };

        public static readonly AccountModel ActiveAccount = new AccountModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveAccount),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly AccountModel HiddenAccount = new AccountModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenAccount),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly AccountModel SoftDeletedAccount = new AccountModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedAccount),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };
    }
}
