using System;
using BudgetBadger.Models;

namespace BudgetBadger.Tests.TestModels
{
    public static class TestTransactions
    {
        public static readonly Transaction NewTransaction = new Transaction();

        public static readonly Transaction ActiveTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly Transaction DeletedTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };
    }
}
