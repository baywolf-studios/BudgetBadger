using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
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
    public async Task SoftDeleteEnvelopeGroup_HiddenEnvelopeGroup_Successful()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_HiddenEnvelopeGroup_UpdatesEnvelopeGroup()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeGroupAsync(A<EnvelopeGroup>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_NewEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var newEnvelopeGroup = TestEnvelopeGroups.NewEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(newEnvelopeGroup.Id)).Returns(newEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(newEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_DeletedEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var deletedEnvelopeGroup = TestEnvelopeGroups.SoftDeletedEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(deletedEnvelopeGroup.Id)).Returns(deletedEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(deletedEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_ActiveEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(activeEnvelopeGroup.Id)).Returns(activeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(activeEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_EnvelopeGroupWithActiveEnvelopes_Unsuccessful()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var activeEnvelopes = TestEnvelopes.ActiveEnvelope.DeepCopy();
        activeEnvelopes.Group = hiddenEnvelopeGroup;

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope>() { activeEnvelopes });

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_EnvelopeGroupWithDeletedEnvelopes_Successful()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
        deletedEnvelope.Group = hiddenEnvelopeGroup;

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope>() { deletedEnvelope });

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_IncomeEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var incomeEnvelopeGroup = Constants.IncomeEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(incomeEnvelopeGroup.Id)).Returns(incomeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(incomeEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_DebtEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var debtEnvelopeGroup = Constants.DebtEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(debtEnvelopeGroup.Id)).Returns(debtEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(debtEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeleteEnvelopeGroup_SystemEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var systemEnvelopeGroup = Constants.SystemEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(systemEnvelopeGroup.Id)).Returns(systemEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.SoftDeleteEnvelopeGroupAsync(systemEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_ActiveEnvelopeGroup_Successful()
    {
        // arrange
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(activeEnvelopeGroup.Id)).Returns(activeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(activeEnvelopeGroup.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_ActiveEnvelopeGroup_UpdatesEnvelopeGroup()
    {
        // arrange
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(activeEnvelopeGroup.Id)).Returns(activeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(activeEnvelopeGroup.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeGroupAsync(A<EnvelopeGroup>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task HideEnvelopeGroup_NewEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var newEnvelopeGroup = TestEnvelopeGroups.NewEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(newEnvelopeGroup.Id)).Returns(newEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(newEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_DeletedEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var deletedEnvelopeGroup = TestEnvelopeGroups.SoftDeletedEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(deletedEnvelopeGroup.Id)).Returns(deletedEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(deletedEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_HiddenEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_IncomeEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var incomeEnvelopeGroup = Constants.IncomeEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(incomeEnvelopeGroup.Id)).Returns(incomeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(incomeEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_DebtEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var debtEnvelopeGroup = Constants.DebtEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(debtEnvelopeGroup.Id)).Returns(debtEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(debtEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HideEnvelopeGroup_SystemEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var systemEnvelopeGroup = Constants.SystemEnvelopeGroup.DeepCopy();

        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(systemEnvelopeGroup.Id)).Returns(systemEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.HideEnvelopeGroupAsync(systemEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelopeGroup_HiddenEnvelopeGroup_Successful()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task UnhideEnvelopeGroup_HiddenEnvelopeGroup_UpdatesEnvelopeGroup()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(hiddenEnvelopeGroup.Id)).Returns(hiddenEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeGroupAsync(hiddenEnvelopeGroup.Id);

        // assert
        A.CallTo(() => dataAccess.UpdateEnvelopeGroupAsync(A<EnvelopeGroup>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task UnhideEnvelopeGroup_DeletedEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var deletedEnvelopeGroup = TestEnvelopeGroups.SoftDeletedEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(deletedEnvelopeGroup.Id)).Returns(deletedEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeGroupAsync(deletedEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelopeGroup_NewEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var newEnvelopeGroup = TestEnvelopeGroups.NewEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(newEnvelopeGroup.Id)).Returns(newEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeGroupAsync(newEnvelopeGroup.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhideEnvelopeGroup_ActiveEnvelopeGroup_Unsuccessful()
    {
        // arrange
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupAsync(activeEnvelopeGroup.Id)).Returns(activeEnvelopeGroup);

        // act
        var result = await EnvelopeLogic.UnhideEnvelopeGroupAsync(activeEnvelopeGroup.Id);

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetEnvelopeGroups_HiddenEnvelopeGroup_HiddenEnvelopeGroupNotReturned()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { hiddenEnvelopeGroup, activeEnvelopeGroup });

        // act
        var result = await EnvelopeLogic.GetEnvelopeGroupsAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenEnvelopeGroup.Id));
    }

    [Test]
    public async Task GetEnvelopeGroups_HiddenEnvelopeGroup_GenericHiddenEnvelopeGroupReturned()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { hiddenEnvelopeGroup, activeEnvelopeGroup });

        // act
        var result = await EnvelopeLogic.GetEnvelopeGroupsAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.Any(p => p.IsGenericHiddenEnvelopeGroup));
    }

    [Test]
    public async Task GetEnvelopeGroupsForSelection_HiddenEnvelopeGroup_HiddenEnvelopeGroupNotReturned()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { hiddenEnvelopeGroup, activeEnvelopeGroup });

        // act
        var result = await EnvelopeLogic.GetEnvelopeGroupsForSelectionAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenEnvelopeGroup.Id));
    }

    [Test]
    public async Task GetHiddenEnvelopeGroups_ActiveEnvelopeGroup_ActiveEnvelopeGroupNotReturned()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var activeEnvelopeGroup = TestEnvelopeGroups.ActiveEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { hiddenEnvelopeGroup, activeEnvelopeGroup });

        // act
        var result = await EnvelopeLogic.GetHiddenEnvelopeGroupsAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.IsActive));
    }

    [Test]
    public async Task GetHiddenEnvelopeGroups_DeletedEnvelopeGroup_DeletedEnvelopeGroupNotReturned()
    {
        // arrange
        var hiddenEnvelopeGroup = TestEnvelopeGroups.HiddenEnvelopeGroup.DeepCopy();
        var deletedEnvelopeGroup = TestEnvelopeGroups.SoftDeletedEnvelopeGroup.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopeGroupsAsync()).Returns(new List<EnvelopeGroup> { hiddenEnvelopeGroup, deletedEnvelopeGroup });

        // act
        var result = await EnvelopeLogic.GetHiddenEnvelopeGroupsAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.IsActive));
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
    public async Task GetBudgets_HiddenEnvelope_GenericHiddenEnvelopeReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope });

        // act
        var result = await EnvelopeLogic.GetBudgetsAsync(TestBudgetSchedules.GetBudgetScheduleFromDate(DateTime.Now));

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.Any(e => e.Envelope.IsGenericHiddenEnvelope));
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
    public async Task GetEnvelopesForReport_HiddenEnvelope_GenericHiddenEnvelopeReturned()
    {
        // arrange
        var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
        A.CallTo(() => dataAccess.ReadEnvelopesAsync()).Returns(new List<Envelope> { hiddenEnvelope });

        // act
        var result = await EnvelopeLogic.GetEnvelopesForReportAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.Any(p => p.IsGenericHiddenEnvelope));
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