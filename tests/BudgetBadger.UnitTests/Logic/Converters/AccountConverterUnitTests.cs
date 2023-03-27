using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Localization;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Logic.Models;
using BudgetBadger.TestData;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Logic.Converters
{

    [TestFixture]
    public class AccountConverterUnitTests
    {
        [SetUp]
        public void Setup()
        {
            var rngSeed = Environment.TickCount;
            Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
            TestGen.SetRandomGeneratorSeed(rngSeed);
        }

        [Test]
        public void Convert_FromAccountDto_ToAccount()
        {
            // arrange
            var accountDto = TestGen.AccountDto();

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: Enumerable.Empty<TransactionDto>(),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(accountDto.Id, (Guid)account.Id);
            Assert.AreEqual(accountDto.Description, account.Description);
            Assert.AreEqual(accountDto.Notes, account.Notes);
            Assert.AreEqual(accountDto.Hidden, account.Hidden);
        }

        [Test]
        public void Convert_FromAccountDtoWithNullNotes_ToAccountWithEmptyNotes()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { Notes = null };

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: Enumerable.Empty<TransactionDto>(),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(string.Empty, account.Notes);
        }

        [Test]
        public void Convert_FromAccountDtoWithOnBudget_ToAccountWithBudgetType()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = true };

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: Enumerable.Empty<TransactionDto>(),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(AccountType.Budget, account.Type);
            Assert.AreEqual(AppResources.AccountTypeBudget, account.Group);
        }

        [Test]
        public void Convert_FromAccountDtoWithOnBudgetFalse_ToAccountWithReportingType()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = false };

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: Enumerable.Empty<TransactionDto>(),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(AccountType.Reporting, account.Type);
            Assert.AreEqual(AppResources.AccountTypeReporting, account.Group);
        }

        [Test]
        public void Convert_FromAccountAndPayeePendingTransactions_ToAccountWithPending()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = true };
            var accountTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = false, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = false, Reconciled = false }
            };

            var payeeTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = false, Reconciled = false }
            };

            var expectedPending = accountTransactions.Where(t => t.Pending).Sum(t => t.Amount)
                - payeeTransactions.Where(t => t.Pending).Sum(t => t.Amount);

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: accountTransactions.Concat(payeeTransactions),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(expectedPending, account.Pending);
        }

        [Test]
        public void Convert_FromAccountAndPayeePostedTransactions_ToAccountWithPosted()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = true };
            var accountTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = false, Reconciled = false }
            };

            var payeeTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = true }
            };

            var expectedPosted = accountTransactions.Where(t => t.Posted).Sum(t => t.Amount)
                - payeeTransactions.Where(t => t.Posted).Sum(t => t.Amount);

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: accountTransactions.Concat(payeeTransactions),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(expectedPosted, account.Posted);
        }

        [Test]
        public void Convert_FromDebtTransactionsAndDebtBudgets_ToAccountWithBalance()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = true };
            var accountTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = false, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = true, Reconciled = false }
            };

            var payeeTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = true }
            };

            var expectedBalance = accountTransactions.Sum(t => t.Amount) - payeeTransactions.Sum(t => t.Amount);

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: accountTransactions.Concat(payeeTransactions),
                budgets: Enumerable.Empty<BudgetDto>(),
                budgetPeriods: Enumerable.Empty<BudgetPeriodDto>());

            // assert
            Assert.AreEqual(expectedBalance, account.Balance);
        }

        [Test]
        public void Convert_FromAccountAndPayeeTransactionsAndDebtEnvelopeBudgets_ToAccountWithPayment()
        {
            // arrange
            var accountDto = TestGen.AccountDto() with { OnBudget = true };
            var accountTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = false, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, Posted = true, Reconciled = false }
            };

            var payeeTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, Posted = true, Reconciled = true }
            };

            var debtAccountTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, EnvelopeId = accountDto.Id, Posted = true, Reconciled = false },
                TestGen.TransactionDto().transactionDto with { AccountId = accountDto.Id, EnvelopeId = accountDto.Id, Posted = true, Reconciled = true }
            };

            var debtPayeeTransactions = new List<TransactionDto>()
            {
                TestGen.TransactionDto().transactionDto with { PayeeId = accountDto.Id, EnvelopeId = accountDto.Id, Posted = true, Reconciled = true }
            };

            var (budgetDto1, budgetPeriodDto1, envelopeDto1, envelopeGroupDto1) = TestGen.BudgetDto();
            var (budgetDto2, budgetPeriodDto2, envelopeDto2, envelopeGroupDto2) = TestGen.BudgetDto();

            var debtBudgets = new List<BudgetDto>()
            {
                budgetDto1 with {EnvelopeId = accountDto.Id },
                budgetDto2 with {EnvelopeId = accountDto.Id }
            };

            var debtBudgetPeriods = new List<BudgetPeriodDto>()
            {
                budgetPeriodDto1,
                budgetPeriodDto2
            };

            var expectedBalance = accountTransactions.Sum(t => t.Amount)
                + debtAccountTransactions.Sum(t => t.Amount)
                - payeeTransactions.Sum(t => t.Amount)
                - debtPayeeTransactions.Sum(t => t.Amount);

            var dateTimeToday = DateTime.Today;

            var amountBudgetedToPayDownDebt = debtBudgets
                .Where(a => debtBudgetPeriods.FirstOrDefault(b => b.Id == a.BudgetPeriodId)?.BeginDate.Date <= dateTimeToday)
                .Sum(a => a.Amount);

            var debtTransactionAmount = debtAccountTransactions.Where(d => d.ServiceDate.Date <= dateTimeToday).Sum(d => d.Amount)
                - debtPayeeTransactions.Where(d => d.ServiceDate.Date <= dateTimeToday).Sum(d => d.Amount);

            var expectedPayment = amountBudgetedToPayDownDebt + debtTransactionAmount - expectedBalance;
            expectedPayment = (expectedPayment >= 0) ? expectedPayment : 0;

            // act
            var account = AccountConverter.Convert(accountDto,
                transactions: accountTransactions.Concat(payeeTransactions).Concat(debtAccountTransactions).Concat(debtPayeeTransactions),
                budgets: debtBudgets,
                budgetPeriods: debtBudgetPeriods);

            // assert
            Assert.AreEqual(expectedPayment, account.Payment);
        }
    }
}

