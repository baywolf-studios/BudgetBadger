using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Models;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic;
using BudgetBadger.Logic.Models;
using BudgetBadger.TestData;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Logic;

[TestFixture]
public class AccountLogicUnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private IDataAccess DataAccess { get; set; }
    private IAccountLogic AccountLogic { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SetUp]
    public void Setup()
    {
        DataAccess = A.Fake<IDataAccess>();
        AccountLogic = new AccountLogic(DataAccess);

        var rngSeed = Environment.TickCount;
        Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
        TestGen.SetRandomGeneratorSeed(rngSeed);
    }

    [Test]
    public async Task SearchAccountsAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(null)).Throws(new Exception());

        // act
        var result = await AccountLogic.SearchAccountsAsync();

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task SearchAccountsAsync_AccountDto_ReturnedWhenHiddenNullFalse([Values] bool? hidden)
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.SearchAccountsAsync(hidden);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == false);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == accountDto.Id));
    }

    [Test]
    public async Task SearchAccountsAsync_HiddenAccountDto_ReturnedWhenHiddenNullTrue([Values] bool? hidden)
    {
        // arrange
        var hiddenAccountDto = TestGen.HiddenAccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto> { hiddenAccountDto });

        // act
        var result = await AccountLogic.SearchAccountsAsync(hidden);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == true);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == hiddenAccountDto.Id));
    }

    [Test]
    public async Task SearchAccountsAsync_DeletedAccountDto_NotReturned([Values] bool? hidden)
    {
        // arrange
        var deletedAccountDto = TestGen.DeletedAccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto> { deletedAccountDto });

        // act
        var result = await AccountLogic.SearchAccountsAsync(hidden);

        // assert 
        Assert.IsTrue(result);
        Assert.That(!result.Items.Any(p => (Guid)p.Id == deletedAccountDto.Id));
    }

    [Test]
    public async Task SearchAccountsAsync_TransactionsAndDebtBudgets_CalculatesValues()
    {
        // arrange
        var accountDto = TestGen.AccountDto() with { OnBudget = true };
        var transactionDtos = new List<TransactionDto>()
        {
            TestGen.PendingTransactionDto().transactionDto with
            {
                Amount = 10.5m, AccountId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = -50m, AccountId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = 0.5m, PayeeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = 3m, PayeeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = -1m, AccountId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = -0.1m, AccountId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = 0.1m, PayeeId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            }
        };

        var (budgetDto1, budgetPeriodDto1, envelopeDto1, envelopeGroupDto1) = TestGen.BudgetDto();
        var (budgetDto2, budgetPeriodDto2, envelopeDto2, envelopeGroupDto2) = TestGen.BudgetDto();

        var debtBudgets = new List<BudgetDto>()
        {
            budgetDto1 with { Amount = 1m, EnvelopeId = accountDto.Id },
            budgetDto2 with { Amount = 0.1m, EnvelopeId = accountDto.Id }
        };

        var debtBudgetPeriods = new List<BudgetPeriodDto>()
        {
            budgetPeriodDto1 with { BeginDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30) },
            budgetPeriodDto2 with { BeginDate = DateTime.Today.AddDays(100), EndDate = DateTime.Today.AddDays(130) },
        };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(null)).Returns(new List<AccountDto> { accountDto });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.PayeeId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.AccountId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadBudgetDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(debtBudgets);
        A.CallTo(() => DataAccess.ReadBudgetPeriodDtosAsync(
            A<IEnumerable<Guid>>.Ignored)).Returns(debtBudgetPeriods);

        // act
        var result = await AccountLogic.SearchAccountsAsync();

        // assert
        Assert.IsTrue(result);
        Assert.AreEqual(10.5m, result.Items.Single().Pending);
        Assert.AreEqual(-54.7m, result.Items.Single().Posted);
        Assert.AreEqual(-44.2m, result.Items.Single().Balance);
        Assert.AreEqual(44m, result.Items.Single().Payment);
    }

    [Test]
    public async Task SearchAccountsAsync_NoTransactionsAndDebtBudgets_CalculatesNone()
    {
        // arrange
        var accountDto = TestGen.AccountDto() with { OnBudget = true };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(null)).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.SearchAccountsAsync();

        // assert
        Assert.IsTrue(result);
        Assert.AreEqual(0m, result.Items.Single().Pending);
        Assert.AreEqual(0m, result.Items.Single().Posted);
        Assert.AreEqual(0m, result.Items.Single().Balance);
        Assert.AreEqual(0m, result.Items.Single().Payment);
    }

    [Test]
    public async Task CreateAccountAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(A<AccountDto>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task CreateAccountAsync_NullDescription_BadRequest()
    {
        // arrange
        string? description = null;
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateAccountAsync_EmptyDescription_BadRequest()
    {
        // arrange
        var description = string.Empty;
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateAccountAsync_Valid_Successful()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task CreateAccountAsync_Valid_ReturnsNonEmptyAccountId()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        Assert.AreNotEqual(new AccountId(Guid.Empty), result.Data);
    }

    [Test]
    public async Task CreateAccountAsync_CallsDataAccessCreateAccount()
    {
        // arrange
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Description == description
                && p.Hidden == hidden
                && p.Notes == notes))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync__AccountTypeBudget_CallsDataAccessCreateAccountWithOnBudget()
    {
        // arrange
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Description == description
                && p.Hidden == hidden
                && p.Notes == notes
                && p.OnBudget))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync__AccountTypeReporting_CallsDataAccessCreateAccountWithOnBudgetFalse()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Description == description
                && p.Hidden == hidden
                && p.Notes == notes
                && !p.OnBudget))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_EmptyStringNotes_CallsDataAccessCreatAccountWithNullNotes()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = string.Empty;
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Description == description
                && p.Hidden == hidden
                && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_WhiteSpaceNotes_CallsDataAccessCreatAccountWithNullNotes()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.WhiteSpaceString;
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Description == description
                && p.Hidden == hidden
                && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_CallsDataAccessCreatePayee()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(
            A<PayeeDto>.That.Matches(p =>
            p.Description == description
            && p.Notes == null
            && p.Hidden == hidden))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_CallsDataAccessCreateEnvelope()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateEnvelopeDtoAsync(
            A<EnvelopeDto>.That.Matches(p =>
            p.Description == description
            && p.Notes == null
            && p.EnvelopGroupId == Constants.DebtEnvelopeGroupId
            && p.IgnoreOverspend == true
            && p.Hidden == hidden))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_CallsDataAccessCreateTransaction()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateTransactionDtoAsync(
            A<TransactionDto>.That.Matches(p =>
                p.AccountId == result.Data.Id
                && p.Amount == balance
                && p.PayeeId == Constants.StartingBalancePayeeId
                && p.Notes == null
                && p.Posted
                && p.Reconciled))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_ReportingAccountType_CallsDataAccessCreateTransactionWithIgnoredEnvelopeId()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Reporting;
        var balance = TestGen.RndDecimal();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateTransactionDtoAsync(
            A<TransactionDto>.That.Matches(p =>
                p.AccountId == result.Data.Id
                && p.Amount == balance
                && p.PayeeId == Constants.StartingBalancePayeeId
                && p.Notes == null
                && p.Posted
                && p.Reconciled
                && p.EnvelopeId == Constants.IgnoredEnvelopeId))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_BudgetAccountTypeAndPositiveBalance_CallsDataAccessCreateTransactionWithIncomeEnvelopeId()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = 5000;
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateTransactionDtoAsync(
            A<TransactionDto>.That.Matches(p =>
                p.AccountId == result.Data.Id
                && p.Amount == balance
                && p.PayeeId == Constants.StartingBalancePayeeId
                && p.Notes == null
                && p.Posted
                && p.Reconciled
                && p.EnvelopeId == Constants.IncomeEnvelopeId))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreateAccountAsync_BudgetAccountTypeAndNegativeBalance_CallsDataAccessCreateTransactionWithAccountDebtEnvelopeId()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var accountType = AccountType.Budget;
        var balance = -5000;
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.CreateAccountAsync(description, notes, accountType, balance, hidden);

        // assert
        A.CallTo(() => DataAccess.CreateTransactionDtoAsync(
            A<TransactionDto>.That.Matches(p =>
                p.AccountId == result.Data.Id
                && p.Amount == balance
                && p.PayeeId == Constants.StartingBalancePayeeId
                && p.Notes == null
                && p.Posted
                && p.Reconciled
                && p.EnvelopeId == result.Data.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ReadAccountAsync_NonExistingAccount_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto>());

        // act
        var result = await AccountLogic.ReadAccountAsync(new AccountId(Guid.Empty));

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task ReadAccountAsync_DeletedAccount_Gone()
    {
        // arrange
        var accountDto = TestGen.DeletedAccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.ReadAccountAsync(new AccountId(accountDto.Id));

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task ReadAccountAsync_Account_Success()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.ReadAccountAsync(new AccountId(accountDto.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReadAccountAsync_TransactionsAndDebtBudgets_CalculatesValues()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var transactionDtos = new List<TransactionDto>()
        {
            TestGen.PendingTransactionDto().transactionDto with
            {
                Amount = 10.5m, AccountId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = -50m, AccountId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = 0.5m, PayeeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = 3m, PayeeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.PostedTransactionDto().transactionDto with
            {
                Amount = -1m, AccountId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = -0.1m, AccountId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            },
            TestGen.ReconciledTransactionDto().transactionDto with
            {
                Amount = 0.1m, PayeeId = accountDto.Id, EnvelopeId = accountDto.Id, ServiceDate = DateTime.Today
            }
        };

        var (budgetDto1, budgetPeriodDto1, envelopeDto1, envelopeGroupDto1) = TestGen.BudgetDto();
        var (budgetDto2, budgetPeriodDto2, envelopeDto2, envelopeGroupDto2) = TestGen.BudgetDto();

        var debtBudgets = new List<BudgetDto>()
        {
            budgetDto1 with { Amount = 1m, EnvelopeId = accountDto.Id },
            budgetDto2 with { Amount = 0.1m, EnvelopeId = accountDto.Id }
        };

        var debtBudgetPeriods = new List<BudgetPeriodDto>()
        {
            budgetPeriodDto1 with { BeginDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30) },
            budgetPeriodDto2 with { BeginDate = DateTime.Today.AddDays(100), EndDate = DateTime.Today.AddDays(130) },
        };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.PayeeId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.AccountId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadBudgetDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(debtBudgets);
        A.CallTo(() => DataAccess.ReadBudgetPeriodDtosAsync(
            A<IEnumerable<Guid>>.Ignored)).Returns(debtBudgetPeriods);

        // act
        var result = await AccountLogic.ReadAccountAsync(new AccountId(accountDto.Id));

        // assert
        Assert.IsTrue(result);
        Assert.AreEqual(10.5m, result.Data.Pending);
        Assert.AreEqual(-54.7m, result.Data.Posted);
        Assert.AreEqual(-44.2m, result.Data.Balance);
        Assert.AreEqual(44m, result.Data.Payment);
    }

    [Test]
    public async Task ReadAccountAsync_NoTransactionsAndDebtBudgets_CalculatesNone()
    {
        // arrange
        var accountDto = TestGen.AccountDto() with { OnBudget = true };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.ReadAccountAsync(new AccountId(accountDto.Id));

        // assert
        Assert.IsTrue(result);
        Assert.AreEqual(0m, result.Data.Pending);
        Assert.AreEqual(0m, result.Data.Posted);
        Assert.AreEqual(0m, result.Data.Balance);
        Assert.AreEqual(0m, result.Data.Payment);
    }

    [Test]
    public async Task UpdateAccountAsync_EmptyId_BadRequest()
    {
        // arrange
        var accountId = new AccountId(Guid.Empty);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdateAccountAsync_NullDescription_BadRequest()
    {
        // arrange
        var accountId = new AccountId();
        string? description = null;
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdateAccountAsync_EmptyDescription_BadRequest()
    {
        // arrange
        var accountId = new AccountId();
        var description = TestGen.EmptyString;
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdateAccountAsync_NonExistingAcount_NotFound()
    {
        // arrange
        var accountId = new AccountId();
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto>());

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task UpdateAccountAsync_DeletedAccount_Gone()
    {
        // arrange
        var accountDto = TestGen.DeletedAccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task UpdateAccountAsync_ReturnsSuccessful()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task UpdateAccountAsync_CallsDataAccessUpdateAccountDto()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Id == accountId
                && p.Description == description
                && p.Hidden == hidden
                && p.Notes == notes))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAccountAsync_EmptyStringNotes_CallsDataAccessCreatAccountWithNullNotes()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.EmptyString;
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Id == accountId
                && p.Description == description
                && p.Hidden == hidden
                && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAccountAsync_WhiteSpaceNotes_CallsDataAccessCreatAccountWithNullNotes()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.WhiteSpaceString;
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdateAccountDtoAsync(
            A<AccountDto>.That.Matches(p =>
                p.Id == accountId
                && p.Description == description
                && p.Hidden == hidden
                && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAccountAsync_CallsDataAccessUpdatePayeeDto()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(
            A<PayeeDto>.That.Matches(p =>
                p.Id == accountId
                && p.Description == description
                && p.Notes == null
                && p.Hidden == hidden))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAccountAsync_CallsDataAccessUpdateEnvelopeDto()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var accountId = new AccountId(accountDto.Id);
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdateEnvelopeDtoAsync(
            A<EnvelopeDto>.That.Matches(p =>
            p.Id == accountId
            && p.Description == description
            && p.Notes == null
            && p.EnvelopGroupId == Constants.DebtEnvelopeGroupId
            && p.IgnoreOverspend == true
            && p.Hidden == hidden))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateAccountAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        var accountId = new AccountId();
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.CreateAccountDtoAsync(A<AccountDto>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.UpdateAccountDtoAsync(A<AccountDto>.Ignored)).Throws(new Exception());

        // act
        var result = await AccountLogic.UpdateAccountAsync(accountId, description, notes, hidden);

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_EmptyId_BadRequest()
    {
        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(Guid.Empty));

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_HiddenAccount_Successful()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeleteAccountAsync_HiddenAccount_UpdatesAccount()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        A.CallTo(() => DataAccess.UpdateAccountDtoAsync(A<AccountDto>.That.Matches(p => p.Deleted && p.Id == account.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteAccountAsync_HiddenAccount_UpdatesPayee()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Deleted && p.Id == account.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteAccountAsync_HiddenAccount_UpdatesEnvelope()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        A.CallTo(() => DataAccess.UpdateEnvelopeDtoAsync(A<EnvelopeDto>.That.Matches(p => p.Deleted && p.Id == account.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteAccountAsync_NonExistingAccount_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto>());

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(Guid.NewGuid()));

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_DeletedAccount_Gone()
    {
        // arrange
        var account = TestGen.DeletedAccountDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_Account_Conflict() // will eventually allow this
    {
        // arrange
        var account = TestGen.AccountDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountWithTransactions_Conflict()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountWithDeletedTransactions_Successful()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.SoftDeletedTransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var result = await AccountLogic.DeleteAccountAsync(new AccountId(account.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountPayeeWithTransactions_Conflict()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountPayeeWithDeletedTransactions_Successful()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.SoftDeletedTransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountEnvelopeWithTransactions_Conflict()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountEnvelopeWithDeletedTransactions_Successful()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.SoftDeletedTransactionDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountEnvelopeWithNonZeroAmountBudgets_Conflict()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();
        var (budgetDto, _, _, _) = TestGen.BudgetDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadBudgetDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<BudgetDto>() { budgetDto with { Amount = 50m } });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeleteAccountAsync_AccountEnvelopeWithZeroAmountBudgets_Successful()
    {
        // arrange
        var account = TestGen.HiddenAccountDto();
        var payeeDto = TestGen.HiddenPayeeDto() with { Id = account.Id };
        var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();
        var (budgetDto, _, _, _) = TestGen.BudgetDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(account.Id),
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeDto> { envelopeDto with { Id = account.Id } });
        A.CallTo(() => DataAccess.ReadBudgetDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<BudgetDto>() { budgetDto with { Amount = 0m } });

        // act
        var request = new AccountId(account.Id);
        var result = await AccountLogic.DeleteAccountAsync(request);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReconcileAccountAsync_NonExistingAccount_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto>());

        // act
        var result = await AccountLogic.ReconcileAccountAsync(new AccountId(Guid.NewGuid()), default, default);

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task ReconcileAccountAsync_DeletedAccount_Gone()
    {
        // arrange
        var account = TestGen.DeletedAccountDto();

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await AccountLogic.ReconcileAccountAsync(new AccountId(account.Id), default, default);

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task ReconcileAccountAsync_AccountWithPostedTransactionsEqualToPostedBalance_Success()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var date = DateTime.Today;
        var transactionDtos = new List<TransactionDto>()
        {
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 50m, AccountId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 10m, PayeeId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = false, Reconciled = false },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = false, Reconciled = false },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date.AddDays(50), Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date.AddDays(50), Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.SoftDeletedTransactionDto().softDeletedTransactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = true, Reconciled = false },
            TestGen.SoftDeletedTransactionDto().softDeletedTransactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = true, Reconciled = false }
        };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.PayeeId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.AccountId == accountDto.Id).ToList());

        // act
        var result = await AccountLogic.ReconcileAccountAsync(new AccountId(accountDto.Id), date, 40m);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReconcileAccountAsync_AccountWithPostedTransactionsEqualToPostedBalance_UpdatesTransactions()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var date = DateTime.Today;
        var transactionDtos = new List<TransactionDto>()
        {
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 50m, AccountId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 10m, PayeeId = accountDto.Id, Posted = true, Reconciled = true },
        };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.PayeeId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.AccountId == accountDto.Id).ToList());

        // act
        var result = await AccountLogic.ReconcileAccountAsync(new AccountId(accountDto.Id), date, 40m);

        // assert
        foreach (var transactionDto in transactionDtos)
        {
            A.CallTo(() => DataAccess.UpdateTransactionDtoAsync(A<TransactionDto>.That.Matches(t => t.Reconciled && t.Id == transactionDto.Id))).MustHaveHappenedOnceExactly();
        }
    }

    [Test]
    public async Task ReconcileAccountAsync_AccountWithPostedTransactionsNotEqualToPostedBalance_Conflict()
    {
        // arrange
        var accountDto = TestGen.AccountDto();
        var date = DateTime.Today;
        var transactionDtos = new List<TransactionDto>()
        {
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 50m, AccountId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = 10m, PayeeId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = false, Reconciled = false },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = false, Reconciled = false },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date.AddDays(50), Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.TransactionDto().transactionDto with { ServiceDate = date.AddDays(50), Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = true, Reconciled = true },
            TestGen.SoftDeletedTransactionDto().softDeletedTransactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), AccountId = accountDto.Id, Posted = true, Reconciled = false },
            TestGen.SoftDeletedTransactionDto().softDeletedTransactionDto with { ServiceDate = date, Amount = TestGen.RndDecimal(), PayeeId = accountDto.Id, Posted = true, Reconciled = false }
        };

        A.CallTo(() => DataAccess.ReadAccountDtosAsync(
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id))).Returns(new List<AccountDto> { accountDto });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.PayeeId == accountDto.Id).ToList());
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(accountDto.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(transactionDtos.Where(t => t.AccountId == accountDto.Id).ToList());

        // act
        var result = await AccountLogic.ReconcileAccountAsync(new AccountId(accountDto.Id), date, 30m);

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }
}

