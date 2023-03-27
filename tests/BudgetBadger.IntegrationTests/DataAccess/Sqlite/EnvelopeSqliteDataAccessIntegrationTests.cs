using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.TestData;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.DataAccess.Sqlite
{
    [TestFixture]
	public class EnvelopeSqliteDataAccessIntegrationTests
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
        public void CreateEnvelopeGroup_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var EnvelopeGroupDto = TestGen.EnvelopeGroupDto() with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateEnvelopeGroupDtoAsync(EnvelopeGroupDto));
        }

        [Test]
        public async Task CreateEnvelopeGroup_ValidRequest_CreatesEnvelopeGroupDto()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();

            // act
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);

            // assert
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync();
            CollectionAssert.Contains(envelopeGroupDtos, testEnvelopeGroupDto);
        }

        [Test]
        public async Task UpdateEnvelopeGroup_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();

            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);

            var updatedEnvelopeGroup = testEnvelopeGroupDto with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateEnvelopeGroupDtoAsync(updatedEnvelopeGroup));
        }

        [Test]
        public async Task UpdateEnvelopeGroup_ValidRequest_UpdatesEnvelopeGroupDto()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();

            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);

            var updatedEnvelopeGroupDto = testEnvelopeGroupDto with
            {
                Description = TestGen.RndString(),
                ModifiedDateTime = DateTime.Now,
                Deleted = false,
                Hidden = false
            };

            // act
            await sqliteDataAccess.UpdateEnvelopeGroupDtoAsync(updatedEnvelopeGroupDto);

            // assert
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync();
            var envelopeGroupDto = envelopeGroupDtos.First(p => p.Id == updatedEnvelopeGroupDto.Id);
            Assert.AreEqual(updatedEnvelopeGroupDto, envelopeGroupDto);
        }

        [Test]
        public async Task ReadEnvelopeGroups_NullEnvelopeGroupIds_ReturnsAll()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);
            var testEnvelopeGroupDto2 = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto2);

            // act
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync();

            // assert
            CollectionAssert.Contains(envelopeGroupDtos, testEnvelopeGroupDto);
            CollectionAssert.Contains(envelopeGroupDtos, testEnvelopeGroupDto2);
        }

        [Test]
        public async Task ReadEnvelopeGroups_EmptyEnvelopeGroupIds_ReturnsNone()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);
            var testEnvelopeGroupDto2 = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto2);

            // act
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(envelopeGroupDtos);
        }

        [Test]
        public async Task ReadEnvelopeGroups_EnvelopeGroupIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);
            var testEnvelopeGroupDto2 = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto2);
            var testEnvelopeGroupDto3 = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto3);

            // act
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync(new List<Guid>() { testEnvelopeGroupDto.Id, testEnvelopeGroupDto3.Id });

            // assert
            CollectionAssert.Contains(envelopeGroupDtos, testEnvelopeGroupDto);
            CollectionAssert.Contains(envelopeGroupDtos, testEnvelopeGroupDto3);
            CollectionAssert.DoesNotContain(envelopeGroupDtos, testEnvelopeGroupDto2);
        }

        [Test]
        public async Task ReadEnvelopeGroups_NonExistentEnvelopeGroupIds_ReturnsNoEnvelopeGroups()
        {
            // assemble
            var testEnvelopeGroupDto = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto);
            var testEnvelopeGroupDto2 = TestGen.EnvelopeGroupDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testEnvelopeGroupDto2);

            // act
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(envelopeGroupDtos, testEnvelopeGroupDto);
            CollectionAssert.DoesNotContain(envelopeGroupDtos, testEnvelopeGroupDto2);
        }

        [Test]
        public void CreateEnvelope_EnvelopeGroupIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto));
        }

        [Test]
        public async Task CreateEnvelope_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            var envelopeDto = testData.envelopeDto with { Description = null };

            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateEnvelopeDtoAsync(envelopeDto));
        }

        [Test]
        public async Task CreateEnvelope_ValidRequest_CreatesEnvelopeDto()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();

            // act
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            // assert
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync();
            CollectionAssert.Contains(envelopeDtos, testData.envelopeDto);
        }

        [Test]
        public async Task UpdateEnvelope_EnvelopeGroupIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();

            // act
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            var updatedEnvelope = testData.envelopeDto with { EnvelopGroupId = Guid.NewGuid() };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateEnvelopeDtoAsync(updatedEnvelope));
        }

        [Test]
        public async Task UpdateEnvelope_NullDescription_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();

            // act
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            var updatedEnvelope = testData.envelopeDto with { Description = null };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateEnvelopeDtoAsync(updatedEnvelope));
        }

        [Test]
        public async Task UpdateEnvelope_ValidRequest_UpdatesEnvelopeDto()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();

            // act
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            var updatedEnvelopeDto = testData.envelopeDto with
            {
                Description = TestGen.RndString(),
                ModifiedDateTime = DateTime.Now,
                Deleted = false,
                Hidden = false
            };

            // act
            await sqliteDataAccess.UpdateEnvelopeDtoAsync(updatedEnvelopeDto);

            // assert
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync();
            var envelopeDto = envelopeDtos.First(p => p.Id == updatedEnvelopeDto.Id);
            Assert.AreEqual(updatedEnvelopeDto, envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_NullEnvelopeIds_ReturnsAll()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync();

            // assert
            CollectionAssert.Contains(envelopeDtos, testData.envelopeDto);
            CollectionAssert.Contains(envelopeDtos, testData2.envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_EmptyEnvelopeIds_ReturnsNone()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(envelopeDtos);
        }

        [Test]
        public async Task ReadEnvelopes_EnvelopeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            var testData3 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(new List<Guid>() { testData.envelopeDto.Id, testData3.envelopeDto.Id });

            // assert
            CollectionAssert.Contains(envelopeDtos, testData.envelopeDto);
            CollectionAssert.Contains(envelopeDtos, testData3.envelopeDto);
            CollectionAssert.DoesNotContain(envelopeDtos, testData2.envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_NonExistentEnvelopeIds_ReturnsNoEnvelopes()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(envelopeDtos, testData.envelopeDto);
            CollectionAssert.DoesNotContain(envelopeDtos, testData2.envelopeDto);
        }





        [Test]
        public async Task ReadEnvelopes_NullEnvelopeGroupIds_ReturnsAll()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync();

            // assert
            CollectionAssert.Contains(envelopeDtos, testData.envelopeDto);
            CollectionAssert.Contains(envelopeDtos, testData2.envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_EmptyEnvelopeGroupIds_ReturnsNone()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(envelopeGroupIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(envelopeDtos);
        }

        [Test]
        public async Task ReadEnvelopes_EnvelopeGroupIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(envelopeGroupIds: new List<Guid>() { testData.envelopeGroupDto.Id });

            // assert
            CollectionAssert.Contains(envelopeDtos, testData.envelopeDto);
            CollectionAssert.DoesNotContain(envelopeDtos, testData2.envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_NonExistentEnvelopeGroupIds_ReturnsNoEnvelopes()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(envelopeGroupIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(envelopeDtos, testData.envelopeDto);
            CollectionAssert.DoesNotContain(envelopeDtos, testData2.envelopeDto);
        }

        [Test]
        public async Task ReadEnvelopes_EnvelopeIdsAndEnvelopeGroupIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            var testData2 = TestGen.EnvelopeDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            var testData3 = TestGen.EnvelopeDto();
            var testData3envelope = testData3.envelopeDto with { EnvelopGroupId = testData2.envelopeGroupDto.Id };
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3envelope);

            // act
            var envelopeDtos = await sqliteDataAccess.ReadEnvelopeDtosAsync(envelopeIds: new List<Guid>() { testData2.envelopeDto.Id },
                envelopeGroupIds: new List<Guid>() { testData2.envelopeGroupDto.Id });

            // assert
            CollectionAssert.DoesNotContain(envelopeDtos, testData.envelopeDto);
            CollectionAssert.Contains(envelopeDtos, testData2.envelopeDto);
            CollectionAssert.DoesNotContain(envelopeDtos, testData3envelope);
        }

        [Test]
        public async Task CreateBudgetPeriod_ValidRequest_CreatesBudgetPeriodDto()
        {
            // assemble
            var testBudgetPeriod = TestGen.BudgetPeriodDto();

            // act
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriod);

            // assert
            var budgetPeriodDtos = await sqliteDataAccess.ReadBudgetPeriodDtosAsync();
            CollectionAssert.Contains(budgetPeriodDtos, testBudgetPeriod);
        }

        [Test]
        public async Task UpdateBudgetPeriod_ValidRequest_UpdatesBudgetPeriodDto()
        {
            // assemble
            var testBudgetPeriod = TestGen.BudgetPeriodDto();

            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriod);

            var updatedBudgetPeriod = testBudgetPeriod with
            {
                ModifiedDateTime = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(12)
            };

            // act
            await sqliteDataAccess.UpdateBudgetPeriodDtoAsync(updatedBudgetPeriod);

            // assert
            var budgetPeriodDtos = await sqliteDataAccess.ReadBudgetPeriodDtosAsync();
            var budgetPeriodDto = budgetPeriodDtos.First(p => p.Id == updatedBudgetPeriod.Id);
            Assert.AreEqual(updatedBudgetPeriod, budgetPeriodDto);
        }

        [Test]
        public async Task ReadBudgetPeriods_NullBudgetPeriodIds_ReturnsAll()
        {
            // assemble
            var testBudgetPeriodDto = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto);
            var testBudgetPeriodDto2 = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto2);

            // act
            var budgetPeriodDtos = await sqliteDataAccess.ReadBudgetPeriodDtosAsync();

            // assert
            CollectionAssert.Contains(budgetPeriodDtos, testBudgetPeriodDto);
            CollectionAssert.Contains(budgetPeriodDtos, testBudgetPeriodDto2);
        }

        [Test]
        public async Task ReadBudgetPeriods_EmptyBudgetPeriodIds_ReturnsNone()
        {
            // assemble
            var testBudgetPeriodDto = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto);
            var testBudgetPeriodDto2 = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto2);

            // act
            var envelopeGroupDtos = await sqliteDataAccess.ReadEnvelopeGroupDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(envelopeGroupDtos);
        }

        [Test]
        public async Task ReadBudgetPeriods_BudgetPeriodIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testBudgetPeriodDto = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto);
            var testBudgetPeriodDto2 = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto2);

            // act
            var budgetPeriodDtos = await sqliteDataAccess.ReadBudgetPeriodDtosAsync(new List<Guid>() { testBudgetPeriodDto.Id });

            // assert
            CollectionAssert.Contains(budgetPeriodDtos, testBudgetPeriodDto);
            CollectionAssert.DoesNotContain(budgetPeriodDtos, testBudgetPeriodDto2);
        }

        [Test]
        public async Task ReadBudgetPeriods_NonExistentBudgetPeriodIds_ReturnsNoBudgetPeriods()
        {
            // assemble
            var testBudgetPeriodDto = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto);
            var testBudgetPeriodDto2 = TestGen.BudgetPeriodDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testBudgetPeriodDto2);

            // act
            var budgetPeriodDtos = await sqliteDataAccess.ReadBudgetPeriodDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(budgetPeriodDtos, testBudgetPeriodDto);
            CollectionAssert.DoesNotContain(budgetPeriodDtos, testBudgetPeriodDto2);
        }

        [Test]
        public async Task CreateBudget_EnvelopeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto));
        }

        [Test]
        public async Task CreateBudget_BudgetPeriodIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto));
        }

        [Test]
        public async Task CreateBudget_ValidRequest_CreatesBudgetDto()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);

            // act
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);

            // assert
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync();
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
        }

        [Test]
        public async Task CreateBudget_BudgetPeriodIdAndEnvelopeIdAlreadyExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);

            var updatedBudget = testData.budgetDto with { Id = Guid.NewGuid() };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.CreateBudgetDtoAsync(updatedBudget));
        }

        [Test]
        public async Task UpdateBudget_EnvelopeIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);

            var updatedBudget = testData.budgetDto with { EnvelopeId = Guid.NewGuid() };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateBudgetDtoAsync(updatedBudget));
        }

        [Test]
        public async Task UpdateBudget_BudgetPeriodIdDoesNotExist_ThrowsSqliteException()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);

            var updatedBudget = testData.budgetDto with { BudgetPeriodId = Guid.NewGuid() };

            // act and assert
            Assert.ThrowsAsync<SqliteException>(() => sqliteDataAccess.UpdateBudgetDtoAsync(updatedBudget));
        }

        [Test]
        public async Task UpdateBudget_ValidRequest_UpdatesBudgetDto()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);

            var updatedBudget = testData.budgetDto with { Amount = TestGen.RndDecimal(), ModifiedDateTime = DateTime.Now };

            // act
            await sqliteDataAccess.UpdateBudgetDtoAsync(updatedBudget);

            // assert
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync();
            var budgetDto = budgetDtos.First(p => p.Id == updatedBudget.Id);
            Assert.AreEqual(updatedBudget, budgetDto);
        }

        [Test]
        public async Task ReadBudgets_NullBudgetIds_ReturnsAll()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync();

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.Contains(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_EmptyBudgetIds_ReturnsNone()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(budgetDtos);
        }

        [Test]
        public async Task ReadBudgets_BudgetIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(new List<Guid>() { testData.budgetDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_NonExistentBudgetIds_ReturnsNoBudgets()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_NullEnvelopeIds_ReturnsAll()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync();

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.Contains(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_EmptyEnvelopeIds_ReturnsNone()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(envelopeIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(budgetDtos);
        }

        [Test]
        public async Task ReadBudgets_EnvelopeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(envelopeIds: new List<Guid>() { testData.envelopeDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_NonExistentEnvelopeIds_ReturnsNoBudgets()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(envelopeIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_NullBudgetPeriodIdsIds_ReturnsAll()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync();

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.Contains(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_EmptyBudgetPeriodIdsIds_ReturnsNone()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(budgetPeriodIds: Enumerable.Empty<Guid>());

            // assert
            CollectionAssert.IsEmpty(budgetDtos);
        }

        [Test]
        public async Task ReadBudgets_BudgetPeriodIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(budgetPeriodIds: new List<Guid>() { testData.budgetPeriodDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }

        [Test]
        public async Task ReadBudgets_BudgetPeriodIdsAndEvelopeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);
            var testData3 = TestGen.BudgetDto();
            var testData3budget = testData3.budgetDto with { EnvelopeId = testData.budgetDto.EnvelopeId };
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData3.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData3budget);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(
                envelopeIds: new List<Guid>() { testData.budgetDto.EnvelopeId },
                budgetPeriodIds: new List<Guid>() { testData.budgetPeriodDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData3budget);
        }

        [Test]
        public async Task ReadBudgets_BudgetIdsAndEvelopeIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);
            var testData3 = TestGen.BudgetDto();
            var testData3budget = testData3.budgetDto with { EnvelopeId = testData.budgetDto.EnvelopeId };
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData3.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData3budget);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(
                budgetIds: new List<Guid>() { testData.budgetDto.Id },
                budgetPeriodIds: new List<Guid>() { testData.budgetPeriodDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData3budget);
        }

        [Test]
        public async Task ReadBudgets_BudgetIdsAndBudgetPeriodIds_ReturnsOnlyRowsThatMatch()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);
            var testData3 = TestGen.BudgetDto();
            var testData3budget = testData3.budgetDto with { BudgetPeriodId = testData.budgetDto.BudgetPeriodId };
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData3.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData3.envelopeDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData3budget);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(
                budgetIds: new List<Guid>() { testData.budgetDto.Id },
                envelopeIds: new List<Guid>() { testData.envelopeDto.Id });

            // assert
            CollectionAssert.Contains(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData3budget);
        }

        [Test]
        public async Task ReadBudgets_NonExistentBudgetPeriodIds_ReturnsNoBudgets()
        {
            // assemble
            var testData = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData.budgetDto);
            var testData2 = TestGen.BudgetDto();
            await sqliteDataAccess.CreateEnvelopeGroupDtoAsync(testData2.envelopeGroupDto);
            await sqliteDataAccess.CreateEnvelopeDtoAsync(testData2.envelopeDto);
            await sqliteDataAccess.CreateBudgetPeriodDtoAsync(testData2.budgetPeriodDto);
            await sqliteDataAccess.CreateBudgetDtoAsync(testData2.budgetDto);

            // act
            var budgetDtos = await sqliteDataAccess.ReadBudgetDtosAsync(budgetPeriodIds: new List<Guid>() { Guid.NewGuid() });

            // assert
            CollectionAssert.DoesNotContain(budgetDtos, testData.budgetDto);
            CollectionAssert.DoesNotContain(budgetDtos, testData2.budgetDto);
        }
    }
}

