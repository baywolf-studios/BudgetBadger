using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BudgetBadger.UnitTests.DataAccessSqlite
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
    }
}
