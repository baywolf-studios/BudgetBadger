using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Models;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic;
using BudgetBadger.Logic.Models;
using BudgetBadger.TestData;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Logic
{
    [TestFixture]
    public class EnvelopeLogicUnitTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        IDataAccess DataAccess { get; set; }
        IEnvelopeLogic EnvelopeLogic { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void Setup()
        {
            DataAccess = A.Fake<IDataAccess>();
            EnvelopeLogic = new EnvelopeLogic(DataAccess, A.Fake<IResourceContainer>());

            var rngSeed = Environment.TickCount;
            Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
            TestGen.SetRandomGeneratorSeed(rngSeed);
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_DataAccessThrowsException_InternalError()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(null)).Throws(new Exception());

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync();

            // assert 
            Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_HiddenEnvelopeGroupDto_ReturnedWhenHiddenNullTrueAndIsSystemNullFalseAndIsIncomeNullFalseAndIsDebtNullFalse([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var hiddenEnvelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { hiddenEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            var shouldReturn = (hidden == null || hidden == true)
                && (isSystem == null || isSystem == false)
                && (isIncome == null || isIncome == false)
                && (isDebt == null || isDebt == false);
            Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == hiddenEnvelopeGroup.Id));
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_EnvelopeGroupDto_ReturnedWhenHiddenNullFalseAndIsSystemNullFalseAndIsIncomeNullFalseAndIsDebtNullFalse([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var hiddenEnvelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            var envelopeGroupDto = TestGen.EnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { hiddenEnvelopeGroup, envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            var shouldReturn = (hidden == null || hidden == false)
                && (isSystem == null || isSystem == false)
                && (isIncome == null || isIncome == false)
                && (isDebt == null || isDebt == false);
            Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == envelopeGroupDto.Id));
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_DeletedEnvelopeGroupDto_NotReturned([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var deletedEnvelopeGroup = TestGen.DeletedEnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { deletedEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            Assert.That(!result.Items.Any(p => (Guid)p.Id == deletedEnvelopeGroup.Id));
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_SystemEnvelopeGroupDto_ReturnedWhenHiddenNullFalseAndIsSystemNullTrueAndIsIncomeNullFalseAndIsDebtNullFalse([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var systemEnvelopeGroup = TestGen.SystemEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { systemEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            var shouldReturn = (hidden == null || hidden == false)
                && (isSystem == null || isSystem == true)
                && (isIncome == null || isIncome == false)
                && (isDebt == null || isDebt == false);
            Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == systemEnvelopeGroup.Id));
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_IncomeEnvelopeGroupDto_ReturnedWhenHiddenNullFalseAndIsSystemNullFalseAndIsIncomeNullTrueAndIsDebtNullFalse([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var incomeEnvelopeGroup = TestGen.IncomeEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { incomeEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            var shouldReturn = (hidden == null || hidden == false)
                && (isSystem == null || isSystem == false)
                && (isIncome == null || isIncome == true)
                && (isDebt == null || isDebt == false);
            Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == incomeEnvelopeGroup.Id));
        }

        [Test]
        public async Task SearchEnvelopeGroupsAsync_DebtEnvelopeGroupDtoExists_ReturnedWhenHiddenNullFalseAndIsSystemNullFalseAndIsIncomeNullFalseAndIsDebtNullTrue([Values] bool? hidden, [Values] bool? isSystem, [Values] bool? isIncome, [Values] bool? isDebt)
        {
            // arrange
            var debtEnvelopeGroup = TestGen.DebtEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto> { debtEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.SearchEnvelopeGroupsAsync(hidden: hidden, isSystem: isSystem, isIncome: isIncome, isDebt: isDebt);

            // assert 
            Assert.IsTrue(result);
            var shouldReturn = (hidden == null || hidden == false)
                && (isSystem == null || isSystem == false)
                && (isIncome == null || isIncome == false)
                && (isDebt == null || isDebt == true);
            Assert.AreEqual(shouldReturn, result.Items.Any(p => (Guid)p.Id == debtEnvelopeGroup.Id));
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_DataAccessThrowsException_InternalError()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
            A.CallTo(() => DataAccess.CreateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.Ignored)).Throws(new Exception());
            A.CallTo(() => DataAccess.UpdateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.Ignored)).Throws(new Exception());

            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_NullDescription_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(null, TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_EmptyDescription_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(TestGen.EmptyString, TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_Valid_Successful()
        {
            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_Valid_ReturnsNonEmptyEnvelopeGroupId()
        {
            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreNotEqual(new EnvelopeGroupId(Guid.Empty), result.Data);
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_Valid_CallsDataAccessCreateEnvelopeGroup()
        {
            // arrange
            var description = TestGen.RndString();
            var notes = TestGen.RndString();
            var hidden = TestGen.RndBool();

            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(description, notes, hidden);

            // assert
            A.CallTo(() => DataAccess.CreateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == notes))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_EmptyStringNotes_CallsDataAccessCreateEnvelopeGroupWithNullNotes()
        {
            // arrange
            var description = TestGen.RndString();
            var notes = TestGen.EmptyString;
            var hidden = TestGen.RndBool();

            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(description, notes, hidden);

            // assert
            A.CallTo(() => DataAccess.CreateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == null))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task CreateEnvelopeGroupAsync_WhiteSpaceStringNotes_CallsDataAccessCreateEnvelopeGroupWithNullNotes()
        {
            // arrange
            var description = TestGen.RndString();
            var notes = TestGen.WhiteSpaceString;
            var hidden = TestGen.RndBool();

            // act
            var result = await EnvelopeLogic.CreateEnvelopeGroupAsync(description, notes, hidden);

            // assert
            A.CallTo(() => DataAccess.CreateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.That.Matches(p => p.Description == description && p.Hidden == hidden && p.Notes == null))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ReadEnvelopeGroupAsync_NonExistingEnvelopeGroup_NotFound()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto>());

            // act
            var result = await EnvelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(Guid.Empty));

            // assert
            Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
        }

        [Test]
        public async Task ReadEnvelopeGroupAsync_DeletedEnvelopeGroup_Gone()
        {
            // arrange
            var envelopeGroupDto = TestGen.DeletedEnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id));

            // assert
            Assert.AreEqual(StatusCode.Gone, result.StatusCode);
        }

        [Test]
        public async Task ReadEnvelopeGroupAsync_EnvelopeGroup_Success()
        {
            // arrange
            var envelopeGroupDto = TestGen.EnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id));

            // assert
            Assert.IsTrue(result);
        }


        [Test]
        public async Task UpdateEnvelopeGroupAsync_EmptyId_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(Guid.Empty), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_NullDescription_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(), null, TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_EmptyDescription_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(), TestGen.EmptyString, TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_NonExistingEnvelopeGroup_NotFound()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto>());

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(Guid.NewGuid()), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_DeletedEnvelopeGroup_Gone()
        {
            // arrange
            var envelopeGroupDto = TestGen.DeletedEnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.Gone, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_UpdateSystemEnvelopeGroupDto_Forbidden()
        {
            // arrange
            var envelopeGroupDto = TestGen.SystemEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_UpdateIncomeEnvelopeGroup_Forbidden()
        {
            // arrange
            var envelopeGroupDto = TestGen.IncomeEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_UpdateDebtEnvelopeGroupDto_Forbidden()
        {
            // arrange
            var envelopeGroupDto = TestGen.DebtEnvelopeGroupDto;
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_Valid_ReturnsSuccessful()
        {
            // arrange
            var envelopeGroupDto = TestGen.EnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_Valid_CallsDataAccessUpdateEnvelopeGroupDto()
        {
            // arrange
            var envelopeGroupDto = TestGen.EnvelopeGroupDto();
            var description = TestGen.RndString();
            var notes = TestGen.RndString();
            var hidden = TestGen.RndBool();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroupDto.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroupDto });

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroupDto.Id), description, notes, hidden);

            // assert
            A.CallTo(() => DataAccess.UpdateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.That.Matches(p => p.Id == envelopeGroupDto.Id && p.Description == description && p.Hidden == hidden && p.Notes == notes))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task UpdateEnvelopeGroupAsync_DataAccessThrowsException_InternalError()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());
            A.CallTo(() => DataAccess.CreateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.Ignored)).Throws(new Exception());
            A.CallTo(() => DataAccess.UpdateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.Ignored)).Throws(new Exception());

            // act
            var result = await EnvelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(Guid.NewGuid()), TestGen.RndString(), TestGen.RndString(), TestGen.RndBool());

            // assert 
            Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
        }


        [Test]
        public async Task DeleteEnvelopeGroupAsync_DataAccessThrowsException_InternalError()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Throws(new Exception());

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert 
            Assert.AreEqual(StatusCode.InternalError, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_EmptyId_BadRequest()
        {
            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(Guid.Empty));

            // assert
            Assert.AreEqual(StatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_HiddenEnvelopeGroup_Successful()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_HiddenEnvelopeGroup_UpdatesEnvelopeGroup()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            A.CallTo(() => DataAccess.UpdateEnvelopeGroupDtoAsync(A<EnvelopeGroupDto>.That.Matches(p => p.Deleted && p.Id == envelopeGroup.Id))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_NonExistingEnvelopeGroup_NotFound()
        {
            // arrange
            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.Ignored)).Returns(new List<EnvelopeGroupDto>());

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(Guid.NewGuid()));

            // assert
            Assert.AreEqual(StatusCode.NotFound, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_DeletedEnvelopeGroup_Gone()
        {
            // arrange
            var envelopeGroup = TestGen.DeletedEnvelopeGroupDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Gone, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_EnvelopeGroup_Conflict() // will eventually allow this
        {
            // arrange
            var envelopeGroup = TestGen.EnvelopeGroupDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_EnvelopeGroupWithEnvelopes_Conflict()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            var (envelopeDto, _) = TestGen.EnvelopeDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });
            A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
                A<IEnumerable<Guid>>.Ignored,
                A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeDto>() { envelopeDto });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_EnvelopeGroupWithHiddenEnvelopes_Conflict()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            var (envelopeDto, _) = TestGen.HiddenEnvelopeDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });
            A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
                A<IEnumerable<Guid>>.Ignored,
                A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeDto>() { envelopeDto });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_EnvelopeGroupWithDeletedEnvelopes_Successful()
        {
            // arrange
            var envelopeGroup = TestGen.HiddenEnvelopeGroupDto();
            var (envelopeDto, _) = TestGen.SoftDeletedEnvelopeDto();

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { envelopeGroup });
            A.CallTo(() => DataAccess.ReadEnvelopeDtosAsync(
                A<IEnumerable<Guid>>.Ignored,
                A<IEnumerable<Guid>>.That.Contains(envelopeGroup.Id))).Returns(new List<EnvelopeDto>() { envelopeDto });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));

            // assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_SystemEnvelopeGroup_Forbidden()
        {
            // arrange
            var startingEnvelopeGroup = TestGen.SystemEnvelopeGroupDto;

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(startingEnvelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { startingEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(startingEnvelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_IncomeEnvelopeGroup_Forbidden()
        {
            // arrange
            var startingEnvelopeGroup = TestGen.IncomeEnvelopeGroupDto;

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(startingEnvelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { startingEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(startingEnvelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task DeleteEnvelopeGroupAsync_DebtEnvelopeGroup_Forbidden()
        {
            // arrange
            var startingEnvelopeGroup = TestGen.DebtEnvelopeGroupDto;

            A.CallTo(() => DataAccess.ReadEnvelopeGroupDtosAsync(A<IEnumerable<Guid>>.That.Contains(startingEnvelopeGroup.Id))).Returns(new List<EnvelopeGroupDto> { startingEnvelopeGroup });

            // act
            var result = await EnvelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(startingEnvelopeGroup.Id));

            // assert
            Assert.AreEqual(StatusCode.Forbidden, result.StatusCode);
        }
    }
}

