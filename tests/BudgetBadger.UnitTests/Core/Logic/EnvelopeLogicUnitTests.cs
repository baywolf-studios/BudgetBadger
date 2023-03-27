using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.DataAccess;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Models;
using BudgetBadger.UnitTests.TestModels;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Logic;

public class EnvelopeLogicUnitTests
{
    IResourceContainer resourceContainer { get; set; }
    IDataAccess dataAccess { get; set; }
    EnvelopeLogic EnvelopeLogic { get; set; }

    [SetUp]
    public void Setup()
    {
        dataAccess = A.Fake<IDataAccess>();
        resourceContainer = A.Fake<IResourceContainer>();
        EnvelopeLogic = new EnvelopeLogic(dataAccess, resourceContainer);
    }

    [Test]
    public async Task SoftDeleteEnvelope_HiddenEnvelope_Successful()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_HiddenEnvelope_UpdatesEnvelope()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task SoftDeleteEnvelope_NewEnvelope_Unsuccessful()
    {
        // arrange
        var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(newEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_DeletedEnvelope_Unsuccessful()
    {
        // arrange
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(deletedEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_ActiveEnvelope_Unsuccessful()
    {
        // arrange
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(activeEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_EnvelopeWithActiveTransactions_Unsuccessful()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);
        A.CallTo(() => dataAccess.ReadEnvelopeTransactionsAsync(hiddenEnvelope.Id)).Returns(new List<Transaction>() { activeTransaction });

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_EnvelopeWithDeletedTransactions_Successful()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);
        A.CallTo(() => dataAccess.ReadEnvelopeTransactionsAsync(hiddenEnvelope.Id)).Returns(new List<Transaction>() { inactiveTransaction });

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_GenericDebtEnvelope_Unsuccessful()
    {
        // arrange
        var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(debtEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_EnvelopeWithDebtEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();
        debtEnvelope.Id = Guid.NewGuid();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(debtEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_IncomeEnvelope_Unsuccessful()
    {
        // arrange
        var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(incomeEnvelope.Id)).Returns(incomeEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(incomeEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_BufferEnvelope_Unsuccessful()
    {
        // arrange
        var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(bufferEnvelope.Id)).Returns(bufferEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(bufferEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_IgnoredEnvelope_Unsuccessful()
    {
        // arrange
        var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(ignoredEnvelope.Id)).Returns(ignoredEnvelope);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(ignoredEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelope_EnvelopeHasNonZeroBudgets_Unsuccessful()
    {
        // arrange
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);
        A.CallTo(() => dataAccess.ReadBudgetsFromEnvelopeAsync(activeEnvelope.Id)).Returns(new List<Budget> { new Budget() { Amount = 100 } });

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(activeEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_ActiveEnvelope_Successful()
    {
        // arrange
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(activeEnvelope.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task HideEnvelope_ActiveEnvelope_UpdatesEnvelope()
    {
        // arrange
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(activeEnvelope.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task HideEnvelope_NewEnvelope_Unsuccessful()
    {
        // arrange
        var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(newEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_DeletedEnvelope_Unsuccessful()
    {
        // arrange
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(deletedEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_HiddenEnvelope_Unsuccessful()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_GenericDebtEnvelope_Unsuccessful()
    {
        // arrange
        var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(debtEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_EnvelopeWithDebtEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();
        debtEnvelope.Id = Guid.NewGuid();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(debtEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_IncomeEnvelope_Unsuccessful()
    {
        // arrange
        var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(incomeEnvelope.Id)).Returns(incomeEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(incomeEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_BufferEnvelope_Unsuccessful()
    {
        // arrange
        var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(bufferEnvelope.Id)).Returns(bufferEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(bufferEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelope_IgnoredEnvelope_Unsuccessful()
    {
        // arrange
        var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeAsync(ignoredEnvelope.Id)).Returns(ignoredEnvelope);

        // act
        var result = await EnvelopeLogic.HideEnvelopeAsync(ignoredEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelope_HiddenEnvelope_Successful()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task UnhideEnvelope_HiddenEnvelope_UpdatesEnvelope()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeAsync(hiddenEnvelope.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task UnhideEnvelope_DeletedEnvelope_Unsuccessful()
    {
        // arrange
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeAsync(deletedEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelope_NewEnvelope_Unsuccessful()
    {
        // arrange
        var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeAsync(newEnvelope.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelope_ActiveEnvelope_Unsuccessful()
    {
        // arrange
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeAsync(activeEnvelope.Id);

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetBudgets_HiddenEnvelope_HiddenEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetBudgetsAsync(TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now));

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Envelope.Id == hiddenEnvelope.Id));
    }

    [Test]
    public async Task GetEnvelopesForSelection_HiddenEnvelope_HiddenEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetEnvelopesForSelectionAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenEnvelope.Id));
    }

    [Test]
    public async Task GetBudgetsForSelection_HiddenEnvelope_HiddenEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetBudgetsForSelectionAsync(TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now));

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Envelope.Id == hiddenEnvelope.Id));
    }

    [Test]
    public async Task GetEnvelopesForReport_HiddenEnvelope_HiddenEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetEnvelopesForReportAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenEnvelope.Id));
    }

    [Test]
    public async Task GetHiddenBudgets_ActiveEnvelope_ActiveEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetHiddenBudgetsAsync(TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now));

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Envelope.IsActive));
    }

    [Test]
    public async Task GetHiddenBudgets_DeletedEnvelope_DeletedEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, deletedEnvelope });

        // act
        var result = await EnvelopeLogic.GetHiddenBudgetsAsync(TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now));

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Envelope.IsActive));
    }

    [Test]
    public async Task GetHiddenEnvelopes_ActiveEnvelope_ActiveEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, activeEnvelope });

        // act
        var result = await EnvelopeLogic.GetHiddenEnvelopesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.IsActive));
    }

    [Test]
    public async Task GetHiddenEnvelopes_DeletedEnvelope_DeletedEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, deletedEnvelope });

        // act
        var result = await EnvelopeLogic.GetHiddenEnvelopesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.IsDeleted));
    }

    [Test]
    public async Task GetHiddenEnvelopes_SystemEnvelope_SystemEnvelopeNotReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        var systemEnvelope = Constants.IgnoredEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope, systemEnvelope });

        // act
        var result = await EnvelopeLogic.GetHiddenEnvelopesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == systemEnvelope.Id));
    }
}