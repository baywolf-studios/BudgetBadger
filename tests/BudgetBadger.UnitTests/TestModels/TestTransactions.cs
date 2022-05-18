using System;
using System.Collections.Generic;
using BudgetBadger.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestTransactions
    {
        public static readonly Transaction NewTransaction = new Transaction();

        public static readonly Transaction ActiveTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            Envelope = TestEnvelopes.ActiveEnvelope,
            Account = TestAccounts.ActiveAccount,
            Payee = TestPayees.ActivePayee
        };

        public static readonly Transaction DeletedTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now,
            Envelope = TestEnvelopes.ActiveEnvelope,
            Account = TestAccounts.ActiveAccount,
            Payee = TestPayees.ActivePayee
        };

        public static List<Transaction> GetSplitTransactions(int numberOfTransactions)
        {
            var splitID = Guid.NewGuid();
            var splitTransactions = new List<Transaction>();

            for (int i = 0; i < numberOfTransactions; i++)
            {
                var newTransaction = TestTransactions.ActiveSplitTransaction.DeepCopy();
                newTransaction.Id = Guid.NewGuid();
                newTransaction.SplitId = splitID;

                splitTransactions.Add(newTransaction);
            }

            return splitTransactions;
        }

        public static readonly Transaction ActiveSplitTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            SplitId = Guid.NewGuid()
        };
    }
}
