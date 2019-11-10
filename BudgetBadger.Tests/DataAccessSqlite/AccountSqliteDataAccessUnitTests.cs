using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BudgetBadger.Tests.DataAccessSqlite
{
    public class AccountSqliteDataAccessUnitTests
    {
        AccountSqliteDataAccess AccountDataAccess { get; set; }

        [SetUp]
        public void Setup()
        {
            var appDataDirectory = Path.Combine(Path.GetTempPath(), "BudgetBadgerTest");
            Directory.CreateDirectory(appDataDirectory);
            var defaultConnectionString = "Data Source=" + Path.Combine(appDataDirectory, "default.bb");

            AccountDataAccess = new AccountSqliteDataAccess(defaultConnectionString);
        }

        [Test]
        public async Task CreateAccount_UpdatesCreatedDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, CreatedDateTime = new DateTime(), Description = "Test" });

            await AccountDataAccess.SoftDeleteAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.AreNotEqual(test.CreatedDateTime, new DateTime());
        }

        [Test]
        public async Task CreateAccount_UpdatesModifiedDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, ModifiedDateTime = new DateTime(), Description = "Test" });

            await AccountDataAccess.SoftDeleteAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.IsNotNull(test.DeletedDateTime);
        }

        [Test]
        public async Task UpdateAccount_UpdatesModifiedDateTime()
        {
            var id = Guid.NewGuid();
            var account = new Account() { Id = id, Description = "Test" };
            await AccountDataAccess.CreateAccountAsync(account);

            var test1 = await AccountDataAccess.ReadAccountAsync(id);

            await AccountDataAccess.UpdateAccountAsync(account);

            var test2 = await AccountDataAccess.ReadAccountAsync(id);
            Assert.AreNotEqual(test1.ModifiedDateTime, test2.ModifiedDateTime);
        }

        [Test]
        public async Task SoftDeleteAccount_UpdatesDeletedDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, Description = "Test" });

            await AccountDataAccess.SoftDeleteAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.IsNotNull(test.DeletedDateTime);
        }

        [Test]
        public async Task HideAccount_UpdatesHiddenDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, Description = "Test" });

            await AccountDataAccess.HideAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.IsNotNull(test.HiddenDateTime);
        }

        [Test]
        public async Task UnhideAccount_NullsHiddenDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, Description = "Test", HiddenDateTime = DateTime.Now });

            await AccountDataAccess.UnhideAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.IsNull(test.HiddenDateTime);
        }
    }
}
