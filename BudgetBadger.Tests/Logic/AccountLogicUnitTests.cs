using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using BudgetBadger.Tests.TestModels;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task SoftDeleteAccount_HiddenAccount_Successful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_HiddenAccount_UpdatesAccount()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task SoftDeleteAccount_HiddenAccount_UpdatesTransferPayee()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            A.CallTo(() => payeeDataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task SoftDeleteAccount_HiddenAccount_UpdatesDebtEnvelope()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            A.CallTo(() => envelopeDataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task SoftDeleteAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var newAccount = TestAccounts.NewAccount.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(newAccount.Id)).Returns(newAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(newAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = TestAccounts.SoftDeletedAccount.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(deletedAccount.Id)).Returns(deletedAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_ActiveAccount_Unsuccessful()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(activeAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountWithActiveTransactions_Unsuccessful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);
            A.CallTo(() => transactionDataAccess.ReadAccountTransactionsAsync(hiddenAccount.Id)).Returns(new List<Transaction>() { activeTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountWithDeletedTransactions_Successful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);
            A.CallTo(() => transactionDataAccess.ReadAccountTransactionsAsync(hiddenAccount.Id)).Returns(new List<Transaction>() { inactiveTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountPayeeWithActiveTransactions_Unsuccessful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);
            A.CallTo(() => transactionDataAccess.ReadPayeeTransactionsAsync(hiddenAccount.Id)).Returns(new List<Transaction>() { activeTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteAccount_AccountPayeeWithDeletedTransactions_Successful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);
            A.CallTo(() => transactionDataAccess.ReadPayeeTransactionsAsync(hiddenAccount.Id)).Returns(new List<Transaction>() { inactiveTransaction });

            // act
            var result = await accountLogic.SoftDeleteAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task HideAccount_ActiveAccount_Successful()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task HideAccount_ActiveAccount_UpdatesAccount()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task HideAccount_ActiveAccount_UpdatesTransferPayee()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            A.CallTo(() => payeeDataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task HideAccount_ActiveAccount_UpdatesDebtEnvelope()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.HideAccountAsync(activeAccount.Id);

            // assert
            A.CallTo(() => envelopeDataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task HideAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var newAccount = TestAccounts.NewAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(newAccount.Id)).Returns(newAccount);

            // act
            var result = await accountLogic.HideAccountAsync(newAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = TestAccounts.SoftDeletedAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(deletedAccount.Id)).Returns(deletedAccount);

            // act
            var result = await accountLogic.HideAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideAccount_HiddenAccount_Unsuccessful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.HideAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_HiddenAccount_Successful()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(hiddenAccount.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task UnhideAccount_HiddenAccount_UpdatesAccount()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(hiddenAccount.Id)).Returns(hiddenAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(hiddenAccount.Id);

            // assert
            A.CallTo(() => accountDataAccess.UpdateAccountAsync(A<Account>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task UnhideAccount_DeletedAccount_Unsuccessful()
        {
            // arrange
            var deletedAccount = TestAccounts.SoftDeletedAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(deletedAccount.Id)).Returns(deletedAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(deletedAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_NewAccount_Unsuccessful()
        {
            // arrange
            var newAccount = TestAccounts.NewAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(newAccount.Id)).Returns(newAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(newAccount.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideAccount_ActiveAccount_Unsuccessful()
        {
            // arrange
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountAsync(activeAccount.Id)).Returns(activeAccount);

            // act
            var result = await accountLogic.UnhideAccountAsync(activeAccount.Id);

            // assert 
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task GetAccounts_HiddenAccount_HiddenAccountNotReturned()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountsAsync()).Returns(new List<Account> { hiddenAccount, activeAccount });

            // act
            var result = await accountLogic.GetAccountsAsync();

            // assert 
            Assert.IsTrue(result.Success);
            Assert.That(!result.Data.Any(p => p.Id == hiddenAccount.Id));
        }

        [Test]
        public async Task GetAccounts_HiddenAccount_GenericHiddenAccountReturned()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountsAsync()).Returns(new List<Account> { hiddenAccount, activeAccount });

            // act
            var result = await accountLogic.GetAccountsAsync();

            // assert 
            Assert.IsTrue(result.Success);
            Assert.That(result.Data.Any(p => p.IsGenericHiddenAccount));
        }

        [Test]
        public async Task GetAccounts_HiddenAccounts_BalanceCombinedOnGenericHiddenAccount()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var hiddenAccount2 = TestAccounts.HiddenAccount.DeepCopy();
            hiddenAccount2.Id = Guid.NewGuid();

            var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();
            activeTransaction.Amount = 50;

            var activeTransaction2 = TestTransactions.ActiveTransaction.DeepCopy();
            activeTransaction2.Amount = 55;

            A.CallTo(() => accountDataAccess.ReadAccountsAsync()).Returns(new List<Account> { hiddenAccount, hiddenAccount2 });
            A.CallTo(() => transactionDataAccess.ReadAccountTransactionsAsync(hiddenAccount.Id)).Returns(new List<Transaction> { activeTransaction });
            A.CallTo(() => transactionDataAccess.ReadAccountTransactionsAsync(hiddenAccount2.Id)).Returns(new List<Transaction> { activeTransaction2 });

            // act
            var result = await accountLogic.GetAccountsAsync();

            // assert 
            Assert.IsTrue(result.Success);
            Assert.That(result.Data.FirstOrDefault(p => p.IsGenericHiddenAccount).Balance == (activeTransaction.Amount + activeTransaction2.Amount));
        }

        [Test]
        public async Task GetAccountsForSelection_HiddenAccount_HiddenAccountNotReturned()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountsAsync()).Returns(new List<Account> { hiddenAccount, activeAccount });

            // act
            var result = await accountLogic.GetAccountsForSelectionAsync();

            // assert 
            Assert.IsTrue(result.Success);
            Assert.That(!result.Data.Any(p => p.Id == hiddenAccount.Id));
        }

        [Test]
        public async Task GetHiddenAccounts_ActiveAccount_ActiveAccountNotReturned()
        {
            // arrange
            var hiddenAccount = TestAccounts.HiddenAccount.DeepCopy();
            var activeAccount = TestAccounts.ActiveAccount.DeepCopy();
            A.CallTo(() => accountDataAccess.ReadAccountsAsync()).Returns(new List<Account> { hiddenAccount, activeAccount });

            // act
            var result = await accountLogic.GetHiddenAccountsAsync();

            // assert 
            Assert.IsTrue(result.Success);
            Assert.That(!result.Data.Any(p => p.Id == activeAccount.Id));
        }

    }
}
