using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.Core.Models;
using System.Linq;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Dtos;
using System.IO;
using Microsoft.Data.Sqlite;
using BudgetBadger.TestData;
using Dropbox.Api.Users;

namespace BudgetBadger.IntegrationTests.DataAccess.Sqlite
{
    [TestFixture]
    public class SqliteDataAccessIntegrationTests
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
        public async Task Init_DoesNotThrowException()
        {
            await sqliteDataAccess.Init();

            Assert.Pass();
        }

        //[Test]
        //public async Task CreateTransaction_ValidRequest_CreatesTransactionDto()
        //{
        //    // assemble
        //    var transaction = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };

        //    // act
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction);

        //    // assert
        //    var transactionDtos = await sqliteDataAccess.ReadPayeeTransactionDtosAsync();
        //    var result = transactionDtos.First(p => p.Id == transaction.Id);
        //    Assert.AreEqual(transaction, result);
        //}

        //[Test]
        //public async Task ReadPayeeTransactionDtos_NullPayeeIds_ReturnsAll()
        //{
        //    // assemble
        //    var transaction = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction);
        //    var transaction2 = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);

        //    // act
        //    var transactions = await sqliteDataAccess.ReadPayeeTransactionDtosAsync();

        //    // assert
        //    CollectionAssert.Contains(transactions.Select(p => p.Id), transaction.Id);
        //    CollectionAssert.Contains(transactions.Select(p => p.Id), transaction2.Id);
        //}

        //[Test]
        //public async Task ReadPayeeTransactionDtos_EmptyPayeeIds_ReturnsNone()
        //{
        //    // assemble
        //    var transaction = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction);
        //    var transaction2 = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);

        //    // act
        //    var transactions = await sqliteDataAccess.ReadPayeeTransactionDtosAsync(Enumerable.Empty<Guid>());

        //    // assert
        //    CollectionAssert.IsEmpty(transactions);
        //}

        //[Test]
        //public async Task ReadPayeeTransactionDtos_PayeeIds_ReturnsOnlyRowsThatMatch()
        //{
        //    // assemble
        //    var transaction = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction);
        //    var transaction2 = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);

        //    // act
        //    var transactions = await sqliteDataAccess.ReadPayeeTransactionDtosAsync(new List<Guid>() { transaction.Id });

        //    // assert
        //    CollectionAssert.Contains(transactions.Select(p => p.Id), transaction.Id);
        //    CollectionAssert.DoesNotContain(transactions.Select(p => p.Id), transaction2.Id);
        //}

        //[Test]
        //public async Task ReadPayeeTransactionDtos_NonExistentPayeeIds_ReturnsNoPayees()
        //{
        //    // assemble
        //    var transaction = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction);
        //    var transaction2 = new TransactionDto()
        //    {
        //        Id = Guid.NewGuid(),
        //        AccountId = Guid.NewGuid(),
        //        Amount = 50,
        //        EnvelopeId = Guid.NewGuid(),
        //        Notes = TestDataGenerator.RandomName(),
        //        PayeeId = Guid.NewGuid(),
        //        Posted = true,
        //        Reconciled = true,
        //        ServiceDate = DateTime.Now,
        //        SplitId = Guid.NewGuid(),
        //        ModifiedDateTime = DateTime.Now,
        //        Deleted = true
        //    };
        //    await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);

        //    // act
        //    var transactions = await sqliteDataAccess.ReadPayeeTransactionDtosAsync(new List<Guid>() { Guid.NewGuid() });

        //    // assert
        //    CollectionAssert.IsEmpty(transactions);
        //}
    }
}

