using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.TestData;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace BudgetBadger.IntegrationTests.DataAccess.Sqlite
{
    [TestFixture]
	public class TransactionSqliteDataAccessIntegrationTests
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
        public async Task CreateTransaction_EnvelopeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto));
        }

        [Test]
        public async Task CreateTransaction_AccountIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto));
        }

        [Test]
        public async Task CreateTransaction_PayeeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto));
        }

        [Test]
        public async Task CreateTransaction_ValidRequest_CreatesTransactionDto()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            // act
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);

            // assert
            var transactionsDtos = await sqliteDataAccess.ReadTransactionDtosAsync();
            CollectionAssert.Contains(transactionsDtos, testData.transactionDto);
        }

        [Test]
        public async Task UpdateTransaction_EnvelopeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);

            var updatedTransaction = testData.transactionDto with { EnvelopeId = Guid.NewGuid(), ModifiedDateTime = DateTime.Now };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateTransactionDtoAsync(updatedTransaction));
        }

        [Test]
        public async Task UpdateTransaction_AccountIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);

            var updatedTransaction = testData.transactionDto with { AccountId = Guid.NewGuid(), ModifiedDateTime = DateTime.Now };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateTransactionDtoAsync(updatedTransaction));
        }

        [Test]
        public async Task UpdateTransaction_PayeeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);

            var updatedTransaction = testData.transactionDto with { PayeeId = Guid.NewGuid(), ModifiedDateTime = DateTime.Now };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateTransactionDtoAsync(updatedTransaction));
        }

        [Test]
        public async Task UpdateTransaction_ValidRequest_UpdatesTransactionDto()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var updatedTransaction = testData.transactionDto with { Amount = TestDataGenerator.RandomDecimal(), ModifiedDateTime = DateTime.Now };

            // act
            await sqliteDataAccess.UpdateTransactionDtoAsync(updatedTransaction);

            // assert
            var transactionsDtos = await sqliteDataAccess.ReadTransactionDtosAsync();
            CollectionAssert.Contains(transactionsDtos, updatedTransaction);
        }

        [Test]
        public async Task ReadTransactions_NullTransactionIds_ReturnsAll()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(transactionIds: null);

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.Contains(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_EmptyTransactionIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(transactionIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_NonExistentTransactionIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(transactionIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_TransactionIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(transactionIds: new List<Guid>() { testData.transactionDto.Id });

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.DoesNotContain(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_NullPayeeIds_ReturnsAll()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(payeeIds: null);

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.Contains(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_EmptyPayeeIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(payeeIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_NonExistentPayeeIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(payeeIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_PayeeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(payeeIds: new List<Guid>() { testData.transactionDto.PayeeId });

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.DoesNotContain(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_NullAccountIds_ReturnsAll()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(accountIds: null);

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.Contains(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_EmptyAccountIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(accountIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_NonExistentAccountIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(accountIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_AccountIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(accountIds: new List<Guid>() { testData.transactionDto.AccountId });

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.DoesNotContain(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_NullEnvelopeIds_ReturnsAll()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(envelopeIds: null);

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.Contains(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_EmptyEnvelopeIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(envelopeIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_NonExistentEnvelopeIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(envelopeIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_EnvelopeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(envelopeIds: new List<Guid>() { testData.transactionDto.EnvelopeId });

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.DoesNotContain(transactionDtos, testData2.transactionDto);
        }

        [Test]
        public async Task ReadTransactions_NullSplitIds_ReturnsAll()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            var transaction1 = testData.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction1);
            var testData2 = TestDataGenerator.TransactionDto();
            var transaction2 = testData2.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);
            var testData3 = TestDataGenerator.TransactionDto();
            var transaction3 = testData3.transactionDto with { SplitId = transaction2.SplitId };
            await sqliteDataAccess.CreateAccountDtoAsync(testData3.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData3.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction3);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(splitIds: null);

            // assert
            CollectionAssert.Contains(transactionDtos, transaction1);
            CollectionAssert.Contains(transactionDtos, transaction2);
            CollectionAssert.Contains(transactionDtos, transaction3);
        }

        [Test]
        public async Task ReadTransactions_EmptySplitIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            var transaction1 = testData.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction1);
            var testData2 = TestDataGenerator.TransactionDto();
            var transaction2 = testData2.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);
            var testData3 = TestDataGenerator.TransactionDto();
            var transaction3 = testData3.transactionDto with { SplitId = transaction2.SplitId };
            await sqliteDataAccess.CreateAccountDtoAsync(testData3.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData3.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction3);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(splitIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_NonExistentSplitIds_ReturnsNone()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            var transaction1 = testData.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction1);
            var testData2 = TestDataGenerator.TransactionDto();
            var transaction2 = testData2.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);
            var testData3 = TestDataGenerator.TransactionDto();
            var transaction3 = testData3.transactionDto with { SplitId = transaction2.SplitId };
            await sqliteDataAccess.CreateAccountDtoAsync(testData3.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData3.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction3);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(splitIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.IsEmpty(transactionDtos);
        }

        [Test]
        public async Task ReadTransactions_SplitIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            var transaction1 = testData.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction1);
            var testData2 = TestDataGenerator.TransactionDto();
            var transaction2 = testData2.transactionDto with { SplitId = Guid.NewGuid() };
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction2);
            var testData3 = TestDataGenerator.TransactionDto();
            var transaction3 = testData3.transactionDto with { SplitId = transaction2.SplitId };
            await sqliteDataAccess.CreateAccountDtoAsync(testData3.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData3.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(transaction3);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(splitIds: new List<Guid>() { transaction2.SplitId.Value });

            // assert
            CollectionAssert.DoesNotContain(transactionDtos, transaction1);
            CollectionAssert.Contains(transactionDtos, transaction2);
            CollectionAssert.Contains(transactionDtos, transaction3);
        }

        [Test]
        public async Task ReadTransactions_EvelopeIdAndPayeeIdAndAccountId_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData.transactionDto);
            var testData2 = TestDataGenerator.TransactionDto();
            await sqliteDataAccess.CreateAccountDtoAsync(testData2.accountDto);
            await sqliteDataAccess.CreatePayeeDtoAsync(testData2.payeeDto);
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateTransactionDtoAsync(testData2.transactionDto);

            // act
            var transactionDtos = await sqliteDataAccess.ReadTransactionDtosAsync(
                envelopeIds: new List<Guid>() { testData.transactionDto.EnvelopeId },
                payeeIds: new List<Guid>() { testData.transactionDto.PayeeId },
                accountIds: new List<Guid>() { testData.transactionDto.AccountId });

            // assert
            CollectionAssert.Contains(transactionDtos, testData.transactionDto);
            CollectionAssert.DoesNotContain(transactionDtos, testData2.transactionDto);
        }
    }
}

