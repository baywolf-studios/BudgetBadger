using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using BudgetBadger.Tests.TestModels;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Tests.Logic
{
    public class TransactionLogicUnitTests
    {
        IResourceContainer resourceContainer { get; set; }
        IPayeeDataAccess payeeDataAccess { get; set; }
        IEnvelopeDataAccess envelopeDataAccess { get; set; }
        ITransactionDataAccess transactionDataAccess { get; set; }
        IAccountDataAccess accountDataAccess { get; set; }
        TransactionLogic transactionLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            accountDataAccess = A.Fake<IAccountDataAccess>();
            transactionDataAccess = A.Fake<ITransactionDataAccess>();
            envelopeDataAccess = A.Fake<IEnvelopeDataAccess>();
            payeeDataAccess = A.Fake<IPayeeDataAccess>();
            resourceContainer = A.Fake<IResourceContainer>();
            transactionLogic = new TransactionLogic(transactionDataAccess, accountDataAccess, payeeDataAccess, envelopeDataAccess, resourceContainer);
        }

        [Test]
        public async Task SoftDeleteAccount_ActiveTransaction_Successful()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_ActiveTransaction_UpdatesTransaction()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_DeletedTransaction_Unsuccessful()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_NewTransaction_Unsuccessful()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_SplitTransactionTwoSplits_Successful()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_SplitTransactionTwoSplits_ConvertsRemainingTransactionToNoLongerBeSplit()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_SplitTransactionMoreThanTwoSplits_Successful()
        {
            Assert.Fail();
        }

        [Test]
        public async Task SoftDeleteAccount_SplitTransactionMoreThanTwoSplits_LeavesRemainingSplitTransactions()
        {
            Assert.Fail();
        }
    }
}
