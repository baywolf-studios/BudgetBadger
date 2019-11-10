using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Tests.Logic
{
    public class AccountLogicUnitTests
    {
        IResourceContainer resourceContainer { get; set; }
        IPayeeDataAccess payeeDataAccess { get; set; }
        IEnvelopeDataAccess envelopeDataAccess { get; set; }
        ITransactionDataAccess transactionDataAccess { get; set; }
        IAccountDataAccess accountDataAccess { get; set; }
        AccountLogic accountLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            accountDataAccess = A.Fake<IAccountDataAccess>();
            transactionDataAccess = A.Fake<ITransactionDataAccess>();
            envelopeDataAccess = A.Fake<IEnvelopeDataAccess>();
            payeeDataAccess = A.Fake<IPayeeDataAccess>();
            resourceContainer = A.Fake<IResourceContainer>();
            accountLogic = new AccountLogic(accountDataAccess, transactionDataAccess, payeeDataAccess, envelopeDataAccess, resourceContainer);
        }

        [Test]
        public async Task SoftDeleteAccount_ActiveAccount_Successful()
        {
            // arrange
            var activeAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(activeAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(activeAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_ActiveAccount_UpdatesAccount()
        {
            // arrange
            var activeAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(activeAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(activeAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task SoftDeleteAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var inactiveAccount = new Account();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(inactiveAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(inactiveAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, DeletedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(deletedAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }


        [Test]
        public async Task SoftDeleteAccount_HiddenAccount_Successful()
        {
            // arrange
            var hiddenAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, HiddenDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountWithActiveTransactions_Unsuccessful()
        {
            // arrange
            var accountId = Guid.NewGuid();
            var activeTransaction = new Transaction() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => transactionDataAccess.ReadAccountTransactionsAsync(accountId)).Returns(new List<Transaction>() { activeTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(accountId);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountPayeeWithActiveTransactions_Unsuccessful()
        {
            // arrange
            var activeTransaction = new Transaction() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => transactionDataAccess.ReadPayeeTransactionsAsync(A<Guid>.Ignored)).Returns(new List<Transaction>() { activeTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(Guid.NewGuid());

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideAccount_ActiveAccount_Successful()
        {
            // arrange
            var activeAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task HideAccount_ActiveAccount_UpdatesAccount()
        {
            // arrange
            var activeAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task HideAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var inactiveAccount = new Account();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(inactiveAccount);

            // act
            var result = await accountLogic.HideAccountAsync(inactiveAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, DeletedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(deletedAccount);

            // act
            var result = await accountLogic.HideAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideAccount_HiddenAccount_Unsuccessful()
        {
            // arrange
            var hiddenAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, HiddenDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.HideAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_HiddenAccount_Successful()
        {
            // arrange
            var hiddenAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, HiddenDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.HideAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task UnhideAccount_HiddenAccount_UpdatesAccount()
        {
            // arrange
            var hiddenAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, HiddenDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task UnhideAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, DeletedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(deletedAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var inactiveAccount = new Account();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(inactiveAccount);

            // act
            var result = await accountLogic.HideAccountAsync(inactiveAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_ActiveAccount_Unsuccessful()
        {
            // arrange
            var activeAccount = new Account() { Id = Guid.NewGuid(), CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            A.CallTo(() => accountDataAccess.ReadAccountAsync(A<Guid>.Ignored)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

    }
}
