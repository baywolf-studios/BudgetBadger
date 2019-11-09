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
        public async Task SoftDeleteAccount_UpdatesDeletedDateTime()
        {
            var id = Guid.NewGuid();
            await AccountDataAccess.CreateAccountAsync(new Account() { Id = id, CreatedDateTime = DateTime.Now, ModifiedDateTime = DateTime.Now, Description = "Test" });

            await AccountDataAccess.SoftDeleteAccountAsync(id);

            var test = await AccountDataAccess.ReadAccountAsync(id);
            Assert.IsNotNull(test.DeletedDateTime);
        }
    }
}
