using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Localization;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic;
using BudgetBadger.Logic.Models;
using BudgetBadger.TestData;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Logic;

[TestFixture]
public class PayeeLogicUnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    IDataAccess DataAccess { get; set; }
    IPayeeLogic PayeeLogic { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [SetUp]
    public void Setup()
    {
        DataAccess = A.Fake<IDataAccess>();
        PayeeLogic = new PayeeLogic(DataAccess);

        var rngSeed = Environment.TickCount;
        Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
        TestGen.SetRandomGeneratorSeed(rngSeed);
    }

    [Test]
    public async Task SearchPayeesAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(null)).Throws(new Exception());

        // act
        var result = await PayeeLogic.SearchPayeesAsync();

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task SearchPayeesAsync_HiddenPayeeDto_ReturnedWhenHiddenNullTrueAndIsAccountNullFalseAndIsStartingBalanceNullFalse([Values] bool? hidden, [Values] bool? isAccount, [Values] bool? isStartingBalance)
    {
        // arrange
        var hiddenPayee = TestGen.HiddenPayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto> { hiddenPayee });

        // act
        var result = await PayeeLogic.SearchPayeesAsync(hidden: hidden, isAccount: isAccount, isStartingBalance: isStartingBalance);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == true)
            && (isAccount == null || isAccount == false)
            && (isStartingBalance == null || isStartingBalance == false);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == hiddenPayee.Id));
    }

    [Test]
    public async Task SearchPayeesAsync_PayeeDto_ReturnedWhenHiddenNullFalseAndIsAccountNullFalseAndIsStartingBalanceNullFalse([Values] bool? hidden, [Values] bool? isAccount, [Values] bool? isStartingBalance)
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.SearchPayeesAsync(hidden: hidden, isAccount: isAccount, isStartingBalance: isStartingBalance);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == false)
            && (isAccount == null || isAccount == false)
            && (isStartingBalance == null || isStartingBalance == false);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == payeeDto.Id));
    }

    [Test]
    public async Task SearchPayeesAsync_StartingBalancePayeeDto_ReturnedWhenHiddenNullFalseAndIsAccountNullFalseAndIsStartingBalanceNullTrue([Values] bool? hidden, [Values] bool? isAccount, [Values] bool? isStartingBalance)
    {
        // arrange
        var startingBalancePayee = TestGen.StartingBalancePayeeDto;
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto> { startingBalancePayee });

        // act
        var result = await PayeeLogic.SearchPayeesAsync(hidden: hidden, isAccount: isAccount, isStartingBalance: isStartingBalance);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == false)
            && (isAccount == null || isAccount == false)
            && (isStartingBalance == null || isStartingBalance == true);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == startingBalancePayee.Id));
    }

    [Test]
    public async Task SearchPayeesAsync_AccountPayeeDto_ReturnedWhenHiddenNullFalseAndIsAccountNullTrueAndIsStartingBalanceNullFalse([Values] bool? hidden, [Values] bool? isAccount, [Values] bool? isStartingBalance)
    {
        // arrange
        var accountPayee = TestGen.PayeeDto();
        var account = TestGen.AccountDto() with { Id = accountPayee.Id };

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto> { accountPayee });
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<AccountDto> { account });

        // act
        var result = await PayeeLogic.SearchPayeesAsync(hidden: hidden, isAccount: isAccount, isStartingBalance: isStartingBalance);

        // assert 
        Assert.IsTrue(result);
        var shouldReturn = (hidden == null || hidden == false)
            && (isAccount == null || isAccount == true)
            && (isStartingBalance == null || isStartingBalance == false);
        Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == accountPayee.Id));
    }

    [Test]
    public async Task SearchPayeesAsync_DeletedPayeeDto_NotReturned([Values] bool? hidden, [Values] bool? isAccount, [Values] bool? isStartingBalance)
    {
        // arrange
        var deletedPayeeDto = TestGen.DeletedPayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto> { deletedPayeeDto });

        // act
        var result = await PayeeLogic.SearchPayeesAsync(hidden: hidden, isAccount: isAccount, isStartingBalance: isStartingBalance);

        // assert 
        Assert.IsTrue(result);
        Assert.IsFalse(result.Items.Any(p => (Guid)p.Id == deletedPayeeDto.Id));
    }

    [Test]
    public async Task CreatePayeeAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());

        // act
        var result = await PayeeLogic.CreatePayeeAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task CreatePayeeAsync_NullDescription_BadRequest()
    {
        // act
        var result = await PayeeLogic.CreatePayeeAsync(null, TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreatePayeeAsync_EmptyDescription_BadRequest()
    {
        // act
        var result = await PayeeLogic.CreatePayeeAsync(TestGen.EmptyString, TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreatePayeeAsync_Valid_Successful()
    {
        // act
        var result = await PayeeLogic.CreatePayeeAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task CreatePayeeAsync_Valid_ReturnsNonEmptyPayeeId()
    {
        // act
        var result = await PayeeLogic.CreatePayeeAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreNotEqual(new PayeeId(Guid.Empty), result.Data);
    }

    [Test]
    public async Task CreatePayeeAsync_Valid_CallsDataAccessCreatePayee()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        // act
        var result = await PayeeLogic.CreatePayeeAsync(description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == notes))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreatePayeeAsync_EmptyStringNotes_CallsDataAccessCreatePayeeWithNullNotes()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.EmptyString;
        var hidden = TestGen.RndBool();

        // act
        var result = await PayeeLogic.CreatePayeeAsync(description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task CreatePayeeAsync_WhiteSpaceStringNotes_CallsDataAccessCreatePayeeWithNullNotes()
    {
        // arrange
        var description = TestGen.RndString();
        var notes = TestGen.WhiteSpaceString;
        var hidden = TestGen.RndBool();

        // act
        var result = await PayeeLogic.CreatePayeeAsync(description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == null))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ReadPayeeAsync_NonExistingPayee_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto>());

        // act
        var result = await PayeeLogic.ReadPayeeAsync(new PayeeId(Guid.Empty));

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task ReadPayeeAsync_DeletedPayee_Gone()
    {
        // arrange
        var payeeDto = TestGen.DeletedPayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.ReadPayeeAsync(new PayeeId(payeeDto.Id));

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task ReadPayeeAsync_Payee_Success()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.ReadPayeeAsync(new PayeeId(payeeDto.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReadPayeeAsync_Account_Success()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        var accountDto = TestGen.AccountDto() with { Id = payeeDto.Id };

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await PayeeLogic.ReadPayeeAsync(new PayeeId(payeeDto.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReadPayeeAsync_Account_GroupIsPayeeTransferGroup()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        var accountDto = TestGen.AccountDto() with { Id = payeeDto.Id };

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<AccountDto> { accountDto });

        // act
        var result = await PayeeLogic.ReadPayeeAsync(new PayeeId(payeeDto.Id));

        // assert
        Assert.AreEqual(AppResources.PayeeTransferGroup, result.Data.Group);
    }

    [Test]
    public async Task UpdatePayeeAsync_EmptyId_BadRequest()
    {
        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(Guid.Empty), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_UpdatePayeeRequestHasNullDescription_BadRequest()
    {
        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(), null, TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_UpdatePayeeRequestHasEmptyDescription_BadRequest()
    {
        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(), TestGen.EmptyString, TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_UpdatePayeeRequestIsStartingBalancePayee_Forbidden()
    {
        // arrange
        var payeeDto = TestGen.StartingBalancePayeeDto;
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(payeeDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_UpdatePayeeRequestIsAccount_Conflict()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });
        var account = TestGen.AccountDto() with { Id = payeeDto.Id };
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(payeeDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_NonExistingPayee_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto>());

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(Guid.NewGuid()), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_DeletedPayee_Gone()
    {
        // arrange
        var payeeDto = TestGen.DeletedPayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(payeeDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task UpdatePayeeAsync_ValidUpdatePayeeRequest_ReturnsSuccessful()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(payeeDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task UpdatePayeeAsync_ValidUpdatePayeeRequest_CallsDataAccessUpdatePayeeDto()
    {
        // arrange
        var payeeDto = TestGen.PayeeDto();
        var description = TestGen.RndString();
        var notes = TestGen.RndString();
        var hidden = TestGen.RndBool();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payeeDto.Id))).Returns(new List<PayeeDto> { payeeDto });

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(payeeDto.Id), description, notes, hidden);

        // assert
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Id == payeeDto.Id && p.Description == description && p.Hidden == hidden && p.Notes == notes))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdatePayeeAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.CreatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.Ignored)).Throws(new Exception());

        // act
        var result = await PayeeLogic.UpdatePayeeAsync(new PayeeId(Guid.NewGuid()), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_DataAccessThrowsException_InternalError()
    {
        // arrange
        var payee = TestGen.HiddenPayeeDto();
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert 
        Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_EmptyId_BadRequest()
    {
        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(Guid.Empty));

        // assert
        Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_HiddenPayee_Successful()
    {
        // arrange
        var payee = TestGen.HiddenPayeeDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeletePayeeAsync_HiddenPayee_UpdatesPayee()
    {
        // arrange
        var payee = TestGen.HiddenPayeeDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        A.CallTo(() => DataAccess.UpdatePayeeDtoAsync(A<PayeeDto>.That.Matches(p => p.Deleted && p.Id == payee.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeletePayeeAsync_NonExistingPayee_NotFound()
    {
        // arrange
        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<PayeeDto>());

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(Guid.NewGuid()));

        // assert
        Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_DeletedPayee_Gone()
    {
        // arrange
        var payee = TestGen.DeletedPayeeDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        Assert.AreEqual(StatusCode.Gone, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_Payee_Conflict() // will eventually allow this
    {
        // arrange
        var payee = TestGen.PayeeDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_PayeeWithTransactions_Conflict()
    {
        // arrange
        var payee = TestGen.HiddenPayeeDto();
        var (transactionDto, _, _, _, _) = TestGen.TransactionDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(payee.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_PayeeWithDeletedTransactions_Successful()
    {
        // arrange
        var payee = TestGen.HiddenPayeeDto();
        var (transactionDto, _, _, _, _) = TestGen.SoftDeletedTransactionDto();

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(payee.Id))).Returns(new List<PayeeDto> { payee });
        A.CallTo(() => DataAccess.ReadTransactionDtosAsync(
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.That.Contains(payee.Id),
            A<IEnumerable<Guid>>.Ignored,
            A<IEnumerable<Guid>>.Ignored)).Returns(new List<TransactionDto>() { transactionDto });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(payee.Id));

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task DeletePayeeAsync_StartingBalancePayee_Forbidden()
    {
        // arrange
        var startingPayee = TestGen.StartingBalancePayeeDto;

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(startingPayee.Id))).Returns(new List<PayeeDto> { startingPayee });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(startingPayee.Id));

        // assert
        Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
    }

    [Test]
    public async Task DeletePayeeAsync_AccountPayee_Conflict()
    {
        // arrange
        var accountPayee = TestGen.PayeeDto();
        var account = TestGen.AccountDto() with { Id = accountPayee.Id };

        A.CallTo(() => DataAccess.ReadPayeeDtosAsync(A<IEnumerable<Guid>>.That.Contains(accountPayee.Id))).Returns(new List<PayeeDto> { accountPayee });
        A.CallTo(() => DataAccess.ReadAccountDtosAsync(A<IEnumerable<Guid>>.That.Contains(account.Id))).Returns(new List<AccountDto> { account });

        // act
        var result = await PayeeLogic.DeletePayeeAsync(new PayeeId(accountPayee.Id));

        // assert
        Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
    }
}