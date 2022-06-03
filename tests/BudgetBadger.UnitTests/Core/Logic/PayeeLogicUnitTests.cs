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

public class PayeeLogicUnitTests
{
    IResourceContainer resourceContainer { get; set; }
    IDataAccess dataAccess { get; set; }
    PayeeLogic PayeeLogic { get; set; }

    [SetUp]
    public void Setup()
    {
        dataAccess = A.Fake<IDataAccess>();
        resourceContainer = A.Fake<IResourceContainer>();
        PayeeLogic = new PayeeLogic(dataAccess, resourceContainer);
    }

    [Test]
    public async Task SoftDeletePayee_HiddenPayee_Successful()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_HiddenPayee_UpdatesPayee()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

        // assert
        A.CallTo(() => dataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task SoftDeletePayee_NewPayee_Unsuccessful()
    {
        // arrange
        var newPayee = TestPayees.NewPayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(newPayee.Id)).Returns(newPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(newPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_DeletedPayee_Unsuccessful()
    {
        // arrange
        var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(deletedPayee.Id)).Returns(deletedPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(deletedPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_ActivePayee_Unsuccessful()
    {
        // arrange
        var activePayee = TestPayees.ActivePayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(activePayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_PayeeWithActiveTransactions_Unsuccessful()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);
        A.CallTo(() => dataAccess.ReadPayeeTransactionsAsync(hiddenPayee.Id)).Returns(new List<Transaction>() { activeTransaction });

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_PayeeWithDeletedTransactions_Successful()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);
        A.CallTo(() => dataAccess.ReadPayeeTransactionsAsync(hiddenPayee.Id)).Returns(new List<Transaction>() { inactiveTransaction });

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_StartingBalancePayee_Unsuccessful()
    {
        // arrange
        var startingPayee = Constants.StartingBalancePayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(startingPayee.Id)).Returns(startingPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(startingPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SoftDeletePayee_AccountPayee_Unsuccessful()
    {
        // arrange
        var accountPayee = TestPayees.ActivePayee.DeepCopy();
        var account = TestAccounts.ActiveAccount.DeepCopy();
        account.Id = accountPayee.Id;

        A.CallTo(() => dataAccess.ReadAccountAsync(account.Id)).Returns(account);
        A.CallTo(() => dataAccess.ReadPayeeAsync(accountPayee.Id)).Returns(accountPayee);

        // act
        var result = await PayeeLogic.SoftDeletePayeeAsync(accountPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HidePayee_ActivePayee_Successful()
    {
        // arrange
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(activePayee.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task HidePayee_ActivePayee_UpdatesPayee()
    {
        // arrange
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(activePayee.Id);

        // assert
        A.CallTo(() => dataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task HidePayee_NewPayee_Unsuccessful()
    {
        // arrange
        var newPayee = TestPayees.NewPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(newPayee.Id)).Returns(newPayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(newPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HidePayee_DeletedPayee_Unsuccessful()
    {
        // arrange
        var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(deletedPayee.Id)).Returns(deletedPayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(deletedPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HidePayee_HiddenPayee_Unsuccessful()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task HidePayee_StartingBalancePayee_Unsuccessful()
    {
        // arrange
        var startingPayee = Constants.StartingBalancePayee.DeepCopy();

        A.CallTo(() => dataAccess.ReadPayeeAsync(startingPayee.Id)).Returns(startingPayee);

        // act
        var result = await PayeeLogic.HidePayeeAsync(startingPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhidePayee_HiddenPayee_Successful()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        var result = await PayeeLogic.UnhidePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task UnhidePayee_HiddenPayee_UpdatesPayee()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        var result = await PayeeLogic.UnhidePayeeAsync(hiddenPayee.Id);

        // assert
        A.CallTo(() => dataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
    }

    [Test]
    public async Task UnhidePayee_DeletedPayee_Unsuccessful()
    {
        // arrange
        var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(deletedPayee.Id)).Returns(deletedPayee);

        // act
        var result = await PayeeLogic.UnhidePayeeAsync(deletedPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhidePayee_NewPayee_Unsuccessful()
    {
        // arrange
        var newPayee = TestPayees.NewPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(newPayee.Id)).Returns(newPayee);

        // act
        var result = await PayeeLogic.UnhidePayeeAsync(newPayee.Id);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task UnhidePayee_ActivePayee_Unsuccessful()
    {
        // arrange
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

        // act
        var result = await PayeeLogic.UnhidePayeeAsync(activePayee.Id);

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetPayees_HiddenPayee_HiddenPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, activePayee });

        // act
        var result = await PayeeLogic.GetPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenPayee.Id));
    }

    [Test]
    public async Task GetPayees_HiddenPayee_GenericHiddenPayeeReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee });

        // act
        var result = await PayeeLogic.GetPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.Any(p => p.IsGenericHiddenPayee));
    }

    [Test]
    public async Task GetPayeesForSelection_HiddenPayee_HiddenPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, activePayee });

        // act
        var result = await PayeeLogic.GetPayeesForSelectionAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenPayee.Id));
    }

    [Test]
    public async Task GetPayeesForReport_HiddenPayee_HiddenPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, activePayee });

        // act
        var result = await PayeeLogic.GetPayeesForReportAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == hiddenPayee.Id));
    }

    [Test]
    public async Task GetPayeesForReport_HiddenPayee_GenericHiddenPayeeReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee });

        // act
        var result = await PayeeLogic.GetPayeesForReportAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(result.Data.Any(p => p.IsGenericHiddenPayee));
    }

    [Test]
    public async Task GetHiddenPayees_ActivePayee_ActivePayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var activePayee = TestPayees.ActivePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, activePayee });

        // act
        var result = await PayeeLogic.GetHiddenPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == activePayee.Id));
    }

    [Test]
    public async Task GetHiddenPayees_DeletedPayee_DeletedPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, deletedPayee });

        // act
        var result = await PayeeLogic.GetHiddenPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == deletedPayee.Id));
    }

    [Test]
    public async Task GetHiddenPayees_StartingBalancePayee_StartingBalancePayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var startingBalancePayee = Constants.StartingBalancePayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, startingBalancePayee });

        // act
        var result = await PayeeLogic.GetHiddenPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == startingBalancePayee.Id));
    }
}