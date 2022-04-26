using System;
using BudgetBadger.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestAccounts
    {
        public static readonly Account NewAccount = new Account
        {
            Description = nameof(NewAccount)
        };

        public static readonly Account ActiveAccount = new Account
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveAccount),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly Account HiddenAccount = new Account
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenAccount),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly Account SoftDeletedAccount = new Account
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
