using System;
using NUnit.Framework;
using BudgetBadger.TestData;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.UnitTests.Logic.Converters
{
    [TestFixture]
    public class EnvelopeGroupConverterUnitTests
    {
        [SetUp]
        public void Setup()
        {
            var rngSeed = Environment.TickCount;
            Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
            TestGen.SetRandomGeneratorSeed(rngSeed);
        }

        [Test]
        public void Convert_FromSystemEnvelopeGroupDto_ToEnvelopeGroup()
        {
            // arrange
            var systemEnvelopeGroup = TestGen.SystemEnvelopeGroupDto;

            // act
            var envelopeGroup = EnvelopeGroupConverter.Convert(systemEnvelopeGroup);

            // assert
            Assert.AreEqual(systemEnvelopeGroup.Id, (Guid)envelopeGroup.Id);
            Assert.AreEqual(AppResources.SystemEnvelopeGroup, envelopeGroup.Description);
            Assert.AreEqual(string.Empty, envelopeGroup.Notes);
            Assert.IsFalse(envelopeGroup.Hidden);
        }

        [Test]
        public void Convert_FromIncomeEnvelopeGroupDto_ToEnvelopeGroup()
        {
            // arrange
            var incomeEnvelopeGroup = TestGen.IncomeEnvelopeGroupDto;

            // act
            var envelopeGroup = EnvelopeGroupConverter.Convert(incomeEnvelopeGroup);

            // assert
            Assert.AreEqual(incomeEnvelopeGroup.Id, (Guid)envelopeGroup.Id);
            Assert.AreEqual(AppResources.IncomeEnvelopeGroup, envelopeGroup.Description);
            Assert.AreEqual(string.Empty, envelopeGroup.Notes);
            Assert.IsFalse(envelopeGroup.Hidden);
        }

        [Test]
        public void Convert_FromDebtEnvelopeGroupDto_ToEnvelopeGroup()
        {
            // arrange
            var debtEnvelopeGroup = TestGen.DebtEnvelopeGroupDto;

            // act
            var envelopeGroup = EnvelopeGroupConverter.Convert(debtEnvelopeGroup);

            // assert
            Assert.AreEqual(debtEnvelopeGroup.Id, (Guid)envelopeGroup.Id);
            Assert.AreEqual(AppResources.DebtEnvelopeGroup, envelopeGroup.Description);
            Assert.AreEqual(string.Empty, envelopeGroup.Notes);
            Assert.IsFalse(envelopeGroup.Hidden);
        }

        [Test]
        public void Convert_FromEnvelopeGroupDto_ToEnvelopeGroup()
        {
            // arrange
            var envelopeGroupDto = TestGen.EnvelopeGroupDto();

            // act
            var envelopeGroup = EnvelopeGroupConverter.Convert(envelopeGroupDto);

            // assert
            Assert.AreEqual(envelopeGroupDto.Id, (Guid)envelopeGroup.Id);
            Assert.AreEqual(envelopeGroupDto.Description, envelopeGroup.Description);
            Assert.AreEqual(envelopeGroupDto.Notes, envelopeGroup.Notes);
            Assert.AreEqual(envelopeGroupDto.Hidden, envelopeGroup.Hidden);
        }

        [Test]
        public void Convert_FromEnvelopeGroupDtoWithNullNotes_ToEnvelopeGroupWithEmptyNotes()
        {
            // arrange
            var envelopeGroupDto = TestGen.EnvelopeGroupDto() with { Notes = null };

            // act
            var envelopeGroup = EnvelopeGroupConverter.Convert(envelopeGroupDto);

            // assert
            Assert.AreEqual(string.Empty, envelopeGroup.Notes);
        }
    }
}
