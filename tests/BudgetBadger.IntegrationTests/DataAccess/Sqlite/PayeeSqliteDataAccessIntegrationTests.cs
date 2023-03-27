using System;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.TestData;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.DataAccess.Sqlite
{
    [TestFixture]
    public class PayeeSqliteDataAccessIntegrationTests
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
        public void CreatePayee_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testPayee = TestGen.PayeeDto() with
            {
                Description = null
            };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreatePayeeDtoAsync(testPayee));
        }

        [Test]
        public async Task CreatePayee_ValidRequest_CreatesPayeeDto()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();

            // act
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);

            // assert
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync();
            var payee = payees.First(p => p.Id == testPayee.Id);
            Assert.AreEqual(testPayee, payee);
        }

        [Test]
        public async Task UpdatePayee_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();

            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);

            var updatedPayee = testPayee with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdatePayeeDtoAsync(updatedPayee));
        }

        [Test]
        public async Task UpdatePayee_ValidRequest_UpdatesPayeeDto()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();

            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);

            var updatedPayee = testPayee with
            {
                Description = TestGen.RndString(),
                ModifiedDateTime = DateTime.Now,
                Deleted = false,
                Hidden = false
            };

            // act
            await sqliteDataAccess.UpdatePayeeDtoAsync(updatedPayee);

            // assert
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync();
            var payee = payees.First(p => p.Id == updatedPayee.Id);
            Assert.AreEqual(updatedPayee, payee);
        }

        [Test]
        public async Task ReadPayees_NullPayeeIds_ReturnsAll()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);
            var testPayee2 = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee2);

            // act
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync();

            // assert
            CollectionAssert.Contains(payees, testPayee);
            CollectionAssert.Contains(payees, testPayee2);
        }

        [Test]
        public async Task ReadPayees_EmptyPayeeIds_ReturnsNone()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);
            var testPayee2 = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee2);

            // act
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(payees);
        }

        [Test]
        public async Task ReadPayees_PayeeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);
            var testPayee2 = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee2);

            // act
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync(new List<Guid>() { testPayee.Id });

            // assert
            CollectionAssert.Contains(payees, testPayee);
            CollectionAssert.DoesNotContain(payees, testPayee2);
        }

        [Test]
        public async Task ReadPayees_NonExistentPayeeIds_ReturnsNoPayees()
        {
            // assemble
            var testPayee = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee);
            var testPayee2 = TestGen.PayeeDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testPayee2);

            // act
            var payees = await sqliteDataAccess.ReadPayeeDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(payees);
        }
    }
}

