using System;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using Moq;
using Xunit;

namespace BudgetBadger.UnitTests.Logic
{
    public class AccountLogicTest
    {
        [Fact]         public async Task DeleteAccountAsync_GivenANonDeletedAccount_UpdatesDeletedDateTime()         {             var accountDataAccessMock = new Mock<IAccountDataAccess>();
            var transactionDataAccessMock = new Mock<ITransactionDataAccess>();
             accountDataAccessMock                 .Setup(x => x.UpdateAccountAsync(It.IsAny<Account>()))                 .Returns(Task.CompletedTask);              var accountLogic = new AccountLogic(accountDataAccessMock.Object, transactionDataAccessMock.Object);              var account = new Account             {                 Id = Guid.NewGuid(),                 DeletedDateTime = null             };              var result = await accountLogic.DeleteAccountAsync(account);              accountDataAccessMock.Verify(ms => ms.UpdateAccountAsync(It.Is<Account>(mo => mo.DeletedDateTime != null)), Times.Once());         }

        [Fact]
        public async Task DeleteAccountAsync_GivenADataAccessException_ReturnsFalseResult()
        {
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            var transactionDataAccessMock = new Mock<ITransactionDataAccess>();

            accountDataAccessMock
                .Setup(x => x.UpdateAccountAsync(It.IsAny<Account>()))
                .Throws(new Exception());

            var accountLogic = new AccountLogic(accountDataAccessMock.Object, transactionDataAccessMock.Object);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                DeletedDateTime = null
            };

            var result = await accountLogic.DeleteAccountAsync(account);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpsertAccountAsync_GivenAnAccountWithoutACreatedDateTime_CallsCreateAccountOnDataAccess()
        {
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            var transactionDataAccessMock = new Mock<ITransactionDataAccess>();

            accountDataAccessMock
                .Setup(x => x.CreateAccountAsync(It.IsAny<Account>()))
                .Returns(Task.CompletedTask);

            var accountLogic = new AccountLogic(accountDataAccessMock.Object, transactionDataAccessMock.Object);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = null
            };

            var result = await accountLogic.UpsertAccountAsync(account);

            accountDataAccessMock.Verify(x => x.CreateAccountAsync(It.IsAny<Account>()));
        }

        [Fact]
        public async Task UpsertAccountAsync_GivenAnAccountWithACreatedDateTime_CallsUpdateAccountOnDataAccess()
        {
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            var transactionDataAccessMock = new Mock<ITransactionDataAccess>();

            accountDataAccessMock
                .Setup(x => x.CreateAccountAsync(It.IsAny<Account>()))
                .Returns(Task.CompletedTask);

            var accountLogic = new AccountLogic(accountDataAccessMock.Object, transactionDataAccessMock.Object);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = DateTime.Now.AddDays(-1)
            };

            var result = await accountLogic.UpsertAccountAsync(account);

            accountDataAccessMock.Verify(x => x.UpdateAccountAsync(It.IsAny<Account>()));
        }
    }
}
