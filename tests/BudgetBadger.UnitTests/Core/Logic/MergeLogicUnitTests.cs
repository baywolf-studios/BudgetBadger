using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using BudgetBadger.UnitTests.TestModels;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Logic;

[TestFixture]
public class MergeLogicUnitTests
{
    private MergeLogic _mergeLogic { get; set; }
    private IDataAccess _sourceDataAccess { get; set; }
    private IDataAccess _targetDataAccess { get; set; }

    [SetUp]
    public void Setup()
    {
        _sourceDataAccess = A.Fake<IDataAccess>();
        _targetDataAccess = A.Fake<IDataAccess>();
        _mergeLogic = new MergeLogic();
    }
    
    [Test]
    public async Task MergeAccounts_SourceDataAccessAccountNotInTargetDataAccess_AccountCreatedInTargetDataAccess()
    {
        // arrange
        var sourceAccount = TestAccounts.ActiveAccount.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).Returns(new List<Account> { sourceAccount });
        A.CallTo(() => _targetDataAccess.ReadAccountsAsync()).Returns(new List<Account>());

        // act
        await _mergeLogic.MergeAccountsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateAccountAsync(sourceAccount)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeAccounts_SourceDataAccessAccountNewerThanTargetDataAccessAccount_AccountUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceAccount = TestAccounts.ActiveAccount.DeepCopy();
        var targetAccount = TestAccounts.ActiveAccount.DeepCopy();
        targetAccount.ModifiedDateTime = sourceAccount.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).Returns(new List<Account> { sourceAccount });
        A.CallTo(() => _targetDataAccess.ReadAccountsAsync()).Returns(new List<Account> { targetAccount });

        // act
        await _mergeLogic.MergeAccountsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateAccountAsync(sourceAccount)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeAccounts_SourceDataAccessAccountOlderThanTargetDataAccessAccount_AccountNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceAccount = TestAccounts.ActiveAccount.DeepCopy();
        var targetAccount = TestAccounts.ActiveAccount.DeepCopy();
        targetAccount.ModifiedDateTime = sourceAccount.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).Returns(new List<Account> { sourceAccount });
        A.CallTo(() => _targetDataAccess.ReadAccountsAsync()).Returns(new List<Account> { targetAccount });

        // act
        await _mergeLogic.MergeAccountsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateAccountAsync(sourceAccount)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergeAccounts_SourceDataAccessAccountMissingId_ThrowsException()
    {
        // arrange
        var sourceAccount = TestAccounts.ActiveAccount.DeepCopy();
        sourceAccount.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).Returns(new List<Account> { sourceAccount });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeAccountsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public async Task MergePayees_SourceDataAccessPayeeNotInTargetDataAccess_PayeeCreatedInTargetDataAccess()
    {
        // arrange
        var sourcePayee = TestPayees.ActivePayee.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { sourcePayee });
        A.CallTo(() => _targetDataAccess.ReadPayeesAsync()).Returns(new List<Payee>());

        // act
        await _mergeLogic.MergePayeesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreatePayeeAsync(sourcePayee)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergePayees_SourceDataAccessPayeeNewerThanTargetDataAccessPayee_PayeeUpdatedInTargetDataAccess()
    {
        // arrange
        var sourcePayee = TestPayees.ActivePayee.DeepCopy();
        var targetPayee = TestPayees.ActivePayee.DeepCopy();
        targetPayee.ModifiedDateTime = sourcePayee.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { sourcePayee });
        A.CallTo(() => _targetDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { targetPayee });

        // act
        await _mergeLogic.MergePayeesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdatePayeeAsync(sourcePayee)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergePayees_SourceDataAccessPayeeOlderThanTargetDataAccessPayee_PayeeNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourcePayee = TestPayees.ActivePayee.DeepCopy();
        var targetPayee = TestPayees.ActivePayee.DeepCopy();
        targetPayee.ModifiedDateTime = sourcePayee.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { sourcePayee });
        A.CallTo(() => _targetDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { targetPayee });

        // act
        await _mergeLogic.MergePayeesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdatePayeeAsync(sourcePayee)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergePayees_SourceDataAccessPayeeMissingId_ThrowsException()
    {
        // arrange
        var sourcePayee = TestPayees.ActivePayee.DeepCopy();
        sourcePayee.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).Returns(new List<Payee> { sourcePayee });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergePayeesAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public async Task MergeEnvelopeGroups_SourceDataAccessEnvelopeGroupNotInTargetDataAccess_EnvelopeGroupCreatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { sourceEnvelopeGroup });
        A.CallTo(() => _targetDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup>());

        // act
        await _mergeLogic.MergeEnvelopeGroupsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateEnvelopeGroupAsync(sourceEnvelopeGroup)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeEnvelopeGroups_SourceDataAccessEnvelopeGroupNewerThanTargetDataAccessEnvelopeGroup_EnvelopeGroupUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        var targetEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        targetEnvelopeGroup.ModifiedDateTime = sourceEnvelopeGroup.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { sourceEnvelopeGroup });
        A.CallTo(() => _targetDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { targetEnvelopeGroup });

        // act
        await _mergeLogic.MergeEnvelopeGroupsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateEnvelopeGroupAsync(sourceEnvelopeGroup)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeEnvelopeGroups_SourceDataAccessEnvelopeGroupOlderThanTargetDataAccessEnvelopeGroup_EnvelopeGroupNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        var targetEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        targetEnvelopeGroup.ModifiedDateTime = sourceEnvelopeGroup.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { sourceEnvelopeGroup });
        A.CallTo(() => _targetDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { targetEnvelopeGroup });

        // act
        await _mergeLogic.MergeEnvelopeGroupsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateEnvelopeGroupAsync(sourceEnvelopeGroup)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergeEnvelopeGroups_SourceDataAccessEnvelopeGroupMissingId_ThrowsException()
    {
        // arrange
        var sourceEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        sourceEnvelopeGroup.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { sourceEnvelopeGroup });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeEnvelopeGroupsAsync(_sourceDataAccess, _targetDataAccess));
    }

    [Test]
    public async Task MergeEnvelopes_SourceDataAccessEnvelopeNotInTargetDataAccess_EnvelopeCreatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
        A.CallTo(() => _targetDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope>());

        // act
        await _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateEnvelopeAsync(sourceEnvelope)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeEnvelopes_SourceDataAccessEnvelopeNewerThanTargetDataAccessEnvelope_EnvelopeUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        var targetEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        targetEnvelope.ModifiedDateTime = sourceEnvelope.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
        A.CallTo(() => _targetDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { targetEnvelope });

        // act
        await _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateEnvelopeAsync(sourceEnvelope)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeEnvelopes_SourceDataAccessEnvelopeOlderThanTargetDataAccessEnvelope_EnvelopeNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        var targetEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        targetEnvelope.ModifiedDateTime = sourceEnvelope.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
        A.CallTo(() => _targetDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { targetEnvelope });

        // act
        await _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateEnvelopeAsync(sourceEnvelope)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergeEnvelopes_SourceDataAccessEnvelopeMissingId_ThrowsException()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        sourceEnvelope.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeEnvelopes_SourceDataAccessEnvelopesEnvelopeGroupMissingId_ThrowsException()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        sourceEnvelope.Group.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeEnvelopes_SourceDataAccessEnvelopeMissingEnvelopeGroup_ThrowsException()
    {
        // arrange
        var sourceEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        sourceEnvelope.Group = null;

        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { sourceEnvelope });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeEnvelopesAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public async Task MergeBudgetSchedules_SourceDataAccessBudgetScheduleNotInTargetDataAccess_BudgetScheduleCreatedInTargetDataAccess()
    {
        // arrange
        var sourceBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule> { sourceBudgetSchedule });
        A.CallTo(() => _targetDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule>());

        // act
        await _mergeLogic.MergeBudgetSchedulesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateBudgetScheduleAsync(sourceBudgetSchedule)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeBudgetSchedules_SourceDataAccessBudgetScheduleNewerThanTargetDataAccessBudgetSchedule_BudgetScheduleUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();
        var targetBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();
        targetBudgetSchedule.ModifiedDateTime = sourceBudgetSchedule.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule> { sourceBudgetSchedule });
        A.CallTo(() => _targetDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule> { targetBudgetSchedule });

        // act
        await _mergeLogic.MergeBudgetSchedulesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateBudgetScheduleAsync(sourceBudgetSchedule)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeBudgetSchedules_SourceDataAccessBudgetScheduleOlderThanTargetDataAccessBudgetSchedule_BudgetScheduleNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();
        var targetBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();
        targetBudgetSchedule.ModifiedDateTime = sourceBudgetSchedule.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule> { sourceBudgetSchedule });
        A.CallTo(() => _targetDataAccess.ReadBudgetSchedulesAsync()).Returns(new List<BudgetSchedule> { targetBudgetSchedule });

        // act
        await _mergeLogic.MergeBudgetSchedulesAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateBudgetScheduleAsync(sourceBudgetSchedule)).MustNotHaveHappened();
    }

    [Test]
    public void MergeBudgetSchedules_SourceDataAccessBudgetScheduleMissingId_ThrowsException()
    {
        // arrange
        var sourceBudgetSchedule = TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now).DeepCopy();
        sourceBudgetSchedule.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadBudgetSchedulesAsync())
            .Returns(new List<BudgetSchedule> { sourceBudgetSchedule });

        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetSchedulesAsync(_sourceDataAccess, _targetDataAccess));
    }

    [Test]
    public async Task MergeBudgets_SourceDataAccessBudgetNotInTargetDataAccess_BudgetCreatedInTargetDataAccess()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
        A.CallTo(() => _targetDataAccess.ReadBudgetsAsync()).Returns(new List<Budget>());

        // act
        await _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateBudgetAsync(sourceBudget)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeBudgets_SourceDataAccessBudgetNewerThanTargetDataAccessBudget_BudgetUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        var targetBudget = TestBudgets.ActiveBudget.DeepCopy();
        targetBudget.ModifiedDateTime = sourceBudget.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
        A.CallTo(() => _targetDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { targetBudget });

        // act
        await _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateBudgetAsync(sourceBudget)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeBudgets_SourceDataAccessBudgetOlderThanTargetDataAccessBudget_BudgetNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        var targetBudget = TestBudgets.ActiveBudget.DeepCopy();
        targetBudget.ModifiedDateTime = sourceBudget.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
        A.CallTo(() => _targetDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { targetBudget });

        // act
        await _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateBudgetAsync(sourceBudget)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergeBudgets_SourceDataAccessBudgetMissingId_ThrowsException()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        sourceBudget.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeBudgets_SourceDataAccessBudgetBudgetScheduleMissing_ThrowsException()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        sourceBudget.Schedule = null;

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeBudgets_SourceDataAccessBudgetBudgetScheduleMissingId_ThrowsException()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        sourceBudget.Schedule.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeBudgets_SourceDataAccessBudgetEnvelopeMissing_ThrowsException()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        sourceBudget.Envelope = null;

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeBudgets_SourceDataAccessBudgetEnvelopeMissingId_ThrowsException()
    {
        // arrange
        var sourceBudget = TestBudgets.ActiveBudget.DeepCopy();
        sourceBudget.Envelope.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).Returns(new List<Budget> { sourceBudget });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeBudgetsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public async Task MergeTransactions_SourceDataAccessTransactionNotInTargetDataAccess_TransactionCreatedInTargetDataAccess()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
        A.CallTo(() => _targetDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction>());

        // act
        await _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.CreateTransactionAsync(sourceTransaction)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeTransactions_SourceDataAccessTransactionNewerThanTargetDataAccessTransaction_TransactionUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        var targetTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        targetTransaction.ModifiedDateTime = sourceTransaction.ModifiedDateTime.GetValueOrDefault().AddDays(-10);

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
        A.CallTo(() => _targetDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { targetTransaction });

        // act
        await _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateTransactionAsync(sourceTransaction)).MustHaveHappened();
    }
    
    [Test]
    public async Task MergeTransactions_SourceDataAccessTransactionOlderThanTargetDataAccessTransaction_TransactionNotUpdatedInTargetDataAccess()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        var targetTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        targetTransaction.ModifiedDateTime = sourceTransaction.ModifiedDateTime.GetValueOrDefault().AddDays(10);

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
        A.CallTo(() => _targetDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { targetTransaction });

        // act
        await _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        A.CallTo(() => _targetDataAccess.UpdateTransactionAsync(sourceTransaction)).MustNotHaveHappened();
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionMissingId_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsEnvelopeMissing_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Envelope = null;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsEnvelopeMissingId_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Envelope.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsAccountMissing_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Account = null;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsAccountMissingId_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Account.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsPayeeMissing_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Payee = null;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }
    
    [Test]
    public void MergeTransactions_SourceDataAccessTransactionsPayeeMissingId_ThrowsException()
    {
        // arrange
        var sourceTransaction = TestTransactions.ActiveTransaction.DeepCopy();
        sourceTransaction.Payee.Id = Guid.Empty;

        A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).Returns(new List<Transaction> { sourceTransaction });
      
        // act and assert
        Assert.ThrowsAsync<Exception>(() => _mergeLogic.MergeTransactionsAsync(_sourceDataAccess, _targetDataAccess));
    }

    [Test]
    public void MergeAll_MergesAccountsBeforePayees()
    {
        // Act
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesAccountsBeforeEnvelopes()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesAccountsBeforeTransactions()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadAccountsAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesPayeesBeforeTransactions()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadPayeesAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesEnvelopeGroupsBeforeEnvelopes()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadEnvelopeGroupsAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesEnvelopesBeforeBudgets()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesEnvelopesBeforeTransactions()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadEnvelopesAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadTransactionsAsync()).MustHaveHappened());
    }
    
    [Test]
    public void MergeAll_MergesBudgetSchedulesBeforeBudgets()
    {
        _mergeLogic.MergeAllAsync(_sourceDataAccess, _targetDataAccess);
        
        // Assert
        A.CallTo(() => _sourceDataAccess.ReadBudgetSchedulesAsync()).MustHaveHappened()
            .Then(A.CallTo(() => _sourceDataAccess.ReadBudgetsAsync()).MustHaveHappened());
    }
}