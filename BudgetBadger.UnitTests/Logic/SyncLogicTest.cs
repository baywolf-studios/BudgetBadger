using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using Moq;
using Xunit;

namespace BudgetBadger.UnitTests.Logic
{
    public class SyncLogicTest
    {
        [Fact]
        public void Test()
        {
            var sharedAccountType = new AccountType { Id = Guid.NewGuid(), Description = "Shared", CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };

            var sourceAccountTypes = new List<AccountType>();
            sourceAccountTypes.Add(new AccountType { Id = Guid.NewGuid(), Description = "Test", CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now });
            sourceAccountTypes.Add(sharedAccountType);

            var targetAccountTypes = new List<AccountType>();
            targetAccountTypes.Add(new AccountType { Id = Guid.NewGuid(), Description = "Test", CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now });
            targetAccountTypes.Add(sharedAccountType);

            var sourceAccountDataAccessMock = new Mock<IAccountDataAccess>();
            sourceAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)sourceAccountTypes));

            var targetAccountDataAccessMock = new Mock<IAccountDataAccess>();
            targetAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)targetAccountTypes));

            var syncLogic = new SyncLogic(sourceAccountDataAccessMock.Object, targetAccountDataAccessMock.Object);

            var result = syncLogic.SyncAccounts(sourceAccountDataAccessMock.Object, targetAccountDataAccessMock.Object);

            Assert.True(true);
        }
    }
}
