using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models;
using Moq;
using Xunit;

namespace BudgetBadger.UnitTests.Logic
{
    public class SyncLogicTest
    {
        [Fact]
        public void SyncAccountTypes_GivenSourceAccountType_CreatesItOnTarget()
        {
            var sourceAccountTypes = new List<AccountType>();
            var sourceOnlyAccountType = new AccountType { Id = Guid.NewGuid(), Description = "Test", CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };
            sourceAccountTypes.Add(sourceOnlyAccountType);

            var targetAccountTypes = new List<AccountType>();

            var sourceAccountDataAccessMock = new Mock<IAccountDataAccess>();
            sourceAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)sourceAccountTypes));

            var targetAccountDataAccessMock = new Mock<IAccountDataAccess>();
            targetAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)targetAccountTypes));

            var result = SyncHelper.SyncAccountTypes(sourceAccountDataAccessMock.Object, targetAccountDataAccessMock.Object);

            targetAccountDataAccessMock.Verify(ms => ms.CreateAccountTypeAsync(It.Is<AccountType>(mo => mo.Id == sourceOnlyAccountType.Id)), Times.Once());
        }

        [Fact]
        public void SyncAccountTypes_GivenSharedAccountType_UpdatesItOnTarget()
        {
            var sharedAccountType = new AccountType { Id = Guid.NewGuid(), Description = "Shared", CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now };

            var targetAccountTypes = new List<AccountType>();
            targetAccountTypes.Add(sharedAccountType);

            var sourceAccountTypes = new List<AccountType>();
            var sourceSharedAccountType = sharedAccountType.DeepCopy();
            sourceSharedAccountType.ModifiedDateTime = DateTime.Now;
            sourceAccountTypes.Add(sourceSharedAccountType);

            var sourceAccountDataAccessMock = new Mock<IAccountDataAccess>();
            sourceAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)sourceAccountTypes));

            var targetAccountDataAccessMock = new Mock<IAccountDataAccess>();
            targetAccountDataAccessMock
                .Setup(x => x.ReadAccountTypesAsync())
                .Returns(Task.FromResult((IEnumerable<AccountType>)targetAccountTypes));

            var result = SyncHelper.SyncAccountTypes(sourceAccountDataAccessMock.Object, targetAccountDataAccessMock.Object);

            targetAccountDataAccessMock.Verify(ms => ms.UpdateAccountTypeAsync(It.Is<AccountType>(mo => mo.Id == sharedAccountType.Id)), Times.Once());
        }
    }
}
