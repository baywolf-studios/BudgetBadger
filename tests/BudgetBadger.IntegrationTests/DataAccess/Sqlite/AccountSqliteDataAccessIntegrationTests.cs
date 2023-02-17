using System;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.DataAccess.Sqlite;
using NUnit.Framework;
using System.Threading.Tasks;
using BudgetBadger.TestData;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBadger.IntegrationTests.DataAccess.Sqlite
{
    [TestFixture]
	public class AccountSqliteDataAccessIntegrationTests
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IDataAccess sqliteDataAccess;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void Setup()
        {
            var tempFac = new TempSqliteDataAccessFactory();
            var test = tempFac.Create();
            sqliteDataAccess = test.sqliteDataAccess;
        }

        [TearDown]
        public void Teardown()
        {
        }

        [Test]
        public void CreateAccount_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto() with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateAccountDtoAsync(testAccount));
        }

        [Test]
        public async Task CreateAccount_ValidRequest_CreatesAccountDto()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();

            // act
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);

            // assert
            var Accounts = await sqliteDataAccess.ReadAccountDtosAsync();
            CollectionAssert.Contains(Accounts, testAccount);
        }

        [Test]
        public async Task UpdateAccount_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();

            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);

            var updatedAccount = testAccount with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateAccountDtoAsync(updatedAccount));
        }

        [Test]
        public async Task UpdateAccount_ValidRequest_UpdatesAccountDto()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();

            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);

            var updatedAccount = testAccount with
            {
                Description = TestDataGenerator.RandomName(),
                ModifiedDateTime = DateTime.Now,
                Deleted = false,
                Hidden = false
            };

            // act
            await sqliteDataAccess.UpdateAccountDtoAsync(updatedAccount);

            // assert
            var accountDtos = await sqliteDataAccess.ReadAccountDtosAsync();
            var account = accountDtos.First(p => p.Id == updatedAccount.Id);
            Assert.AreEqual(updatedAccount, account);
        }

        [Test]
        public async Task ReadAccounts_NullAccountIds_ReturnsAll()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);
            var testAccount2 = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount2);

            // act
            var accounts = await sqliteDataAccess.ReadAccountDtosAsync();

            // assert
            CollectionAssert.Contains(accounts, testAccount);
            CollectionAssert.Contains(accounts, testAccount2);
        }

        [Test]
        public async Task ReadAccounts_EmptyAccountIds_ReturnsNone()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);
            var testAccount2 = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount2);

            // act
            var accounts = await sqliteDataAccess.ReadAccountDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(accounts);
        }

        [Test]
        public async Task ReadAccounts_AccountIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);
            var testAccount2 = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount2);
            var testAccount3 = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount3);

            // act
            var accounts = await sqliteDataAccess.ReadAccountDtosAsync(new List<Guid>() { testAccount.Id, testAccount3.Id });

            // assert
            CollectionAssert.Contains(accounts, testAccount);
            CollectionAssert.Contains(accounts, testAccount3);
            CollectionAssert.DoesNotContain(accounts, testAccount2);
        }

        [Test]
        public async Task ReadAccounts_NonExistentAccountIds_ReturnsNoAccounts()
        {
            // assemble
            var testAccount = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount);
            var testAccount2 = TestDataGenerator.AccountDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testAccount2);

            // act
            var accounts = await sqliteDataAccess.ReadAccountDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(accounts, testAccount);
            CollectionAssert.DoesNotContain(accounts, testAccount2);
        }
    }
}

