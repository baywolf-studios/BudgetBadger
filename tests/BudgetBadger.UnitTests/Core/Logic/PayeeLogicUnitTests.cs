using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Converters;
using BudgetBadger.Core.Models;
using BudgetBadger.TestData;
using BudgetBadger.UnitTests.TestModels;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Logic;

[TestFixture]
public class PayeeLogicUnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    IResourceContainer resourceContainer { get; set; }
    IDataAccess dataAccess { get; set; }
    IPayeeLogic PayeeLogic { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
    public async Task GetPayees_HiddenPayee_GenericHiddenPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee });

        // act
        var result = await PayeeLogic.GetPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.IsGenericHiddenPayee));
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

    [Test]
    public async Task GetHiddenPayees_AccountPayee_AccountPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
        var accountPayee = TestPayees.AccountPayee.DeepCopy();
        A.CallTo(() => dataAccess.ReadPayeesAsync()).Returns(new List<Payee> { hiddenPayee, accountPayee });

        // act
        var result = await PayeeLogic.GetHiddenPayeesAsync();

        // assert 
        Assert.IsTrue(result.Success);
        Assert.That(!result.Data.Any(p => p.Id == accountPayee.Id));
    }







    [Test]
    public async Task GetPayees2_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetPayees2_HiddenPayeeDto_HiddenPayeeNotReturned()
    {
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { hiddenPayee, TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), hiddenPayee.Id);
    }

    [Test]
    public async Task GetPayees2_ActivePayeeDto_ActivePayeeReturned()
    {
        var payee = TestDataGenerator.PayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), payee, TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetPayees2_DeletedPayeeDto_DeletedPayeeNotReturned()
    {
        var deletedPayee = TestDataGenerator.SoftDeletedPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), deletedPayee, TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), deletedPayee.Id);
    }

    [Test]
    public async Task GetPayees2_StartingBalancePayeeDto_StartingBalancePayeeNotReturned()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), Constants.StartingBalancePayeeId);
    }

    [Test]
    public async Task GetPayees2_AccountPayeeDto_AccountPayeeNotReturned()
    {
        // arrange
        var payee = TestDataGenerator.PayeeDto() with { Id = Guid.NewGuid() };
        var accountForPayee = TestDataGenerator.AccountDto() with { Id = payee.Id };
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto(), payee });
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<AccountDto> { accountForPayee });

        // act
        var result = await PayeeLogic.GetPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetPayeesForSelection2_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetPayeesForSelection2_HiddenPayeeDto_HiddenPayeeNotReturned()
    {
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { hiddenPayee, TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), hiddenPayee.Id);
    }

    [Test]
    public async Task GetPayeesForSelection2_ActivePayeeDto_ActivePayeeReturned()
    {
        var payee = TestDataGenerator.PayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), payee, TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetPayeesForSelection2_DeletedPayeeDto_DeletedPayeeNotReturned()
    {
        var deletedPayee = TestDataGenerator.SoftDeletedPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), deletedPayee, TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), deletedPayee.Id);
    }

    [Test]
    public async Task GetPayeesForSelection2_StartingBalancePayeeDto_StartingBalancePayeeNotReturned()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), Constants.StartingBalancePayeeId);
    }

    [Test]
    public async Task GetPayeesForSelection2_AccountPayeeDto_AccountPayeeReturned()
    {
        // arrange
        var payee = TestDataGenerator.PayeeDto() with { Id = Guid.NewGuid() };
        var accountForPayee = TestDataGenerator.AccountDto() with { Id = payee.Id };
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto(), payee });
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<AccountDto> { accountForPayee });

        // act
        var result = await PayeeLogic.GetPayeesForSelection2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(result.Data.Select(p => p.Id), payee.Id);
    }


    [Test]
    public async Task GetPayeesForReport2_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetPayeesForReport2_HiddenPayeeDto_HiddenPayeeNotReturned()
    {
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { hiddenPayee, TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), hiddenPayee.Id);
    }

    [Test]
    public async Task GetPayeesForReport2_ActivePayeeDto_ActivePayeeReturned()
    {
        var payee = TestDataGenerator.PayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), payee, TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetPayeesForReport2_DeletedPayeeDto_DeletedPayeeNotReturned()
    {
        var deletedPayee = TestDataGenerator.SoftDeletedPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), deletedPayee, TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), deletedPayee.Id);
    }

    [Test]
    public async Task GetPayeesForReport2_StartingBalancePayeeDto_StartingBalancePayeeNotReturned()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), Constants.StartingBalancePayeeId);
    }

    [Test]
    public async Task GetPayeesForReport2_AccountPayeeDto_AccountPayeeNotReturned()
    {
        // arrange
        var payee = TestDataGenerator.PayeeDto() with { Id = Guid.NewGuid() };
        var accountForPayee = TestDataGenerator.AccountDto() with { Id = payee.Id };
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto(), payee });
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<AccountDto> { accountForPayee });

        // act
        var result = await PayeeLogic.GetPayeesForReport2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetHiddenPayees2_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetHiddenPayees2_HiddenPayeeDto_HiddenPayeeReturned()
    {
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { hiddenPayee, TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(result.Data.Select(p => p.Id), hiddenPayee.Id);
    }

    [Test]
    public async Task GetHiddenPayees2_ActivePayeeDto_ActivePayeeNotReturned()
    {
        var payee = TestDataGenerator.PayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), payee, TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), payee.Id);
    }

    [Test]
    public async Task GetHiddenPayees2_DeletedPayeeDto_DeletedPayeeNotReturned()
    {
        var deletedPayee = TestDataGenerator.SoftDeletedPayeeDto();

        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), deletedPayee, TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), deletedPayee.Id);
    }

    [Test]
    public async Task GetHiddenPayees2_StartingBalancePayeeDto_StartingBalancePayeeNotReturned()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), Constants.StartingBalancePayeeId);
    }

    [Test]
    public async Task GetHiddenPayees2_AccountPayeeDto_AccountPayeeNotReturned()
    {
        // arrange
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto() with { Id = Guid.NewGuid() };
        var hiddenAccountForPayee = TestDataGenerator.HiddenAccountDto() with { Id = hiddenPayee.Id };
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto(), hiddenPayee });
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(hiddenPayee.Id))).Returns(new List<AccountDto> { hiddenAccountForPayee });

        // act
        var result = await PayeeLogic.GetHiddenPayees2Async();

        // assert 
        Assert.IsTrue(result.Success);
        CollectionAssert.DoesNotContain(result.Data.Select(p => p.Id), hiddenPayee.Id);
    }

    [Test]
    public async Task GetHiddenPayeesCountAsync_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.GetHiddenPayeesCountAsync();

        // assert 
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task GetHiddenPayeesCountAsync_NoHiddenPayees_ReturnsZero()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.PayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayeesCountAsync();

        // assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data);
    }

    [Test]
    public async Task GetHiddenPayeesCountAsync_HiddenPayees_ReturnsCount()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.PayeeDto(), TestDataGenerator.HiddenPayeeDto(), TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });

        // act
        var result = await PayeeLogic.GetHiddenPayeesCountAsync();

        // assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data);
    }

    [Test]
    public async Task GetHiddenPayeesCountAsync_HiddenAccountPayee_ReturnsZero()
    {
        // arrange
        var hiddenPayee = TestDataGenerator.HiddenPayeeDto();
        var hiddenAccountPayee = TestDataGenerator.HiddenAccountDto() with { Id = hiddenPayee.Id };
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(null)).Returns(new List<PayeeDto> { TestDataGenerator.PayeeDto(), hiddenPayee, TestDataGenerator.SoftDeletedPayeeDto(), TestDataGenerator.StartingBalancePayeeDto() });
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(hiddenPayee.Id))).Returns(new List<AccountDto> { hiddenAccountPayee });

        // act
        var result = await PayeeLogic.GetHiddenPayeesCountAsync();

        // assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data);
    }

    [Test]
    public async Task SavePayeeAsync_PayeeEditModelHasNullDescription_ReturnsUnsuccessful()
    {
        // arrange
        var payee = TestDataGenerator.ActivePayeeEditModel() with { Description = null };

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_PayeeEditModelHasEmptyDescription_ReturnsUnsuccessful()
    {
        // arrange
        var payee = TestDataGenerator.ActivePayeeEditModel() with { Description = string.Empty };

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_PayeeEditModelIsStartingBalancePayee_ReturnsUnsuccessful()
    {
        // arrange
        var payee = PayeeEditModelConverter.Convert(TestDataGenerator.StartingBalancePayeeDto());

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_PayeeEditModelIsAccount_ReturnsUnsuccessful()
    {
        // arrange
        var payee = TestDataGenerator.ActivePayeeEditModel();
        var account = TestDataGenerator.AccountDto() with { Id = payee.Id };
        A.CallTo(() => dataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_NewPayeeEditModel_ReturnsSuccessful()
    {
        // arrange
        var payee = TestDataGenerator.NewPayeeEditModel();

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_NewPayeeEditModel_CallsDataAccessCreatePayee()
    {
        // arrange
        var payee = TestDataGenerator.NewPayeeEditModel();

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        A.CallTo(() => dataAccess.CreatePayeeDtoAsync(A<PayeeDto>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SavePayeeAsync_ExistingPayeeEditModel_ReturnsSuccessful()
    {
        // arrange
        var payee = TestDataGenerator.ActivePayeeEditModel();
        var payeeDto = PayeeEditModelConverter.Convert(payee);
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task SavePayeeAsync_ExistingPayeeEditModel_CallsDataAccessUpdatePayee()
    {
        // arrange
        var payee = TestDataGenerator.ActivePayeeEditModel();
        var payeeDto = PayeeEditModelConverter.Convert(payee);
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.SavePayeeAsync(payee);

        // assert
        A.CallTo(() => dataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SavePayeeAsync_DataAccessThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
        A.CallTo(() => dataAccess.CreatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());
        A.CallTo(() => dataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());

        // act
        var result = await PayeeLogic.SavePayeeAsync(TestDataGenerator.ActivePayeeEditModel());

        // assert 
        Assert.IsFalse(result.Success);
    }

    //[Test]
    //public async Task SoftDeletePayee2_HiddenPayee_Successful()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.HiddenPayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayee2Async(payee.Id);

    //    // assert
    //    Assert.IsTrue(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee2_HiddenPayee_UpdatesPayee()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.HiddenPayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayee2Async(payee.Id);

    //    // assert
    //    A.CallTo(() => dataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).MustHaveHappenedOnceExactly();
    //}

    //[Test]
    //public async Task SoftDeletePayee2_NewPayee_Unsuccessful()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.NewPayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayee2Async(payee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee2_DeletedPayee_Unsuccessful()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.SoftDeletedPayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayee2Async(payee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee2_ActivePayee_Unsuccessful()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.PayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayee2Async(payee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee2_PayeeWithActiveTransactions_Unsuccessful()
    //{
    //    // arrange
    //    var payee = TestDataGenerator.HiddenPayeeDto();
    //    var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(payee.Id))).Returns(new List<PayeeDto> { payee });
    //    A.CallTo(() => dataAccess.ReadPayeeTransactionsAsync(payee.Id)).Returns(new List<Transaction>() { activeTransaction });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayeeAsync(payee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee_PayeeWithDeletedTransactions_Successful()
    //{
    //    // arrange
    //    var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
    //    var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

    //    A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);
    //    A.CallTo(() => dataAccess.ReadPayeeTransactionsAsync(hiddenPayee.Id)).Returns(new List<Transaction>() { inactiveTransaction });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

    //    // assert
    //    Assert.IsTrue(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayees_StartingBalancePayee_Unsuccessful()
    //{
    //    // arrange
    //    var startingPayee = TestDataGenerator.StartingBalancePayeeDto();

    //    A.CallTo(() => dataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains<IEnumerable<Guid>>(startingPayee.Id))).Returns(new List<PayeeDto> { startingPayee });

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayeeAsync(startingPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task SoftDeletePayee_AccountPayee_Unsuccessful()
    //{
    //    // arrange
    //    var accountPayee = TestPayees.ActivePayee.DeepCopy();
    //    var account = TestAccounts.ActiveAccount.DeepCopy();
    //    account.Id = accountPayee.Id;

    //    A.CallTo(() => dataAccess.ReadAccountAsync(account.Id)).Returns(account);
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(accountPayee.Id)).Returns(accountPayee);

    //    // act
    //    var result = await PayeeLogic.SoftDeletePayeeAsync(accountPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task HidePayee_ActivePayee_Successful()
    //{
    //    // arrange
    //    var activePayee = TestPayees.ActivePayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(activePayee.Id);

    //    // assert
    //    Assert.IsTrue(result.Success);
    //}

    //[Test]
    //public async Task HidePayee_ActivePayee_UpdatesPayee()
    //{
    //    // arrange
    //    var activePayee = TestPayees.ActivePayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(activePayee.Id);

    //    // assert
    //    A.CallTo(() => dataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
    //}

    //[Test]
    //public async Task HidePayee_NewPayee_Unsuccessful()
    //{
    //    // arrange
    //    var newPayee = TestPayees.NewPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(newPayee.Id)).Returns(newPayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(newPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task HidePayee_DeletedPayee_Unsuccessful()
    //{
    //    // arrange
    //    var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(deletedPayee.Id)).Returns(deletedPayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(deletedPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task HidePayee_HiddenPayee_Unsuccessful()
    //{
    //    // arrange
    //    var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(hiddenPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task HidePayee_StartingBalancePayee_Unsuccessful()
    //{
    //    // arrange
    //    var startingPayee = Constants.StartingBalancePayee.DeepCopy();

    //    A.CallTo(() => dataAccess.ReadPayeeAsync(startingPayee.Id)).Returns(startingPayee);

    //    // act
    //    var result = await PayeeLogic.HidePayeeAsync(startingPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task UnhidePayee_HiddenPayee_Successful()
    //{
    //    // arrange
    //    var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

    //    // act
    //    var result = await PayeeLogic.UnhidePayeeAsync(hiddenPayee.Id);

    //    // assert
    //    Assert.IsTrue(result.Success);
    //}

    //[Test]
    //public async Task UnhidePayee_HiddenPayee_UpdatesPayee()
    //{
    //    // arrange
    //    var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

    //    // act
    //    var result = await PayeeLogic.UnhidePayeeAsync(hiddenPayee.Id);

    //    // assert
    //    A.CallTo(() => dataAccess.UpdatePayeeAsync(A<Payee>.Ignored)).MustHaveHappened();
    //}

    //[Test]
    //public async Task UnhidePayee_DeletedPayee_Unsuccessful()
    //{
    //    // arrange
    //    var deletedPayee = TestPayees.SoftDeletedPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(deletedPayee.Id)).Returns(deletedPayee);

    //    // act
    //    var result = await PayeeLogic.UnhidePayeeAsync(deletedPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task UnhidePayee_NewPayee_Unsuccessful()
    //{
    //    // arrange
    //    var newPayee = TestPayees.NewPayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(newPayee.Id)).Returns(newPayee);

    //    // act
    //    var result = await PayeeLogic.UnhidePayeeAsync(newPayee.Id);

    //    // assert
    //    Assert.IsFalse(result.Success);
    //}

    //[Test]
    //public async Task UnhidePayee_ActivePayee_Unsuccessful()
    //{
    //    // arrange
    //    var activePayee = TestPayees.ActivePayee.DeepCopy();
    //    A.CallTo(() => dataAccess.ReadPayeeAsync(activePayee.Id)).Returns(activePayee);

    //    // act
    //    var result = await PayeeLogic.UnhidePayeeAsync(activePayee.Id);

    //    // assert 
    //    Assert.IsFalse(result.Success);
    //}
}