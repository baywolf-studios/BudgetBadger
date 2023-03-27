using System;
using NUnit.Framework;
using BudgetBadger.TestData;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.UnitTests.Logic.Converters
{
    [TestFixture]
    public class PayeeConverterUnitTests
    {
        [SetUp]
        public void Setup()
        {
            var rngSeed = Environment.TickCount;
            Console.WriteLine($"{TestContext.CurrentContext.Test.Name} RNG Seed: {rngSeed}");
            TestGen.SetRandomGeneratorSeed(rngSeed);
        }

        [Test]
        public void Convert_FromStartingBalancePayeeDto_ToPayee()
        {
            // arrange
            var startingBalancePayeeDto = TestGen.StartingBalancePayeeDto;

            // act
            var payee = PayeeConverter.Convert(startingBalancePayeeDto);

            // assert
            Assert.AreEqual(startingBalancePayeeDto.Id, (Guid)payee.Id);
            Assert.AreEqual(AppResources.StartingBalancePayee, payee.Description);
            Assert.AreEqual(string.Empty, payee.Notes);
            Assert.IsFalse(payee.Hidden);
            Assert.AreEqual(AppResources.StartingBalancePayee, payee.Group);
        }

        [Test]
        public void Convert_FromPayeeDto_ToPayee()
        {
            // arrange
            var payeeDto = TestGen.PayeeDto();

            // act
            var payee = PayeeConverter.Convert(payeeDto);

            // assert
            Assert.AreEqual(payeeDto.Id, (Guid)payee.Id);
            Assert.AreEqual(payeeDto.Description, payee.Description);
            Assert.AreEqual(payeeDto.Notes, payee.Notes);
            Assert.AreEqual(payeeDto.Hidden, payee.Hidden);
            Assert.AreEqual(payeeDto.Description[0].ToString().ToUpper(), payee.Group);
        }

        [Test]
        public void Convert_FromPayeeDtoWithLowercaseDescription_ToPayeeWithUppercaseGroup()
        {
            // arrange
            var payeeDto = TestGen.PayeeDto() with { Description = "j" };

            // act
            var payee = PayeeConverter.Convert(payeeDto);

            // assert
            Assert.AreEqual("J", payee.Group);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNullDescription_ToPayeeWithEmptyGroup()
        {
            // arrange
            var payeeDto = TestGen.PayeeDto() with { Description = null };

            // act
            var payee = PayeeConverter.Convert(payeeDto);

            // assert
            Assert.AreEqual(string.Empty, payee.Group);
        }

        [Test]
        public void Convert_FromPayeeDtoWithEmptyDescription_ToPayeeWithEmptyGroup()
        {
            // arrange
            var payeeDto = TestGen.PayeeDto() with { Description = string.Empty };

            // act
            var payee = PayeeConverter.Convert(payeeDto);

            // assert
            Assert.AreEqual(string.Empty, payee.Group);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNullNotes_ToPayeeWithEmptyNotes()
        {
            // arrange
            var payeeDto = TestGen.PayeeDto() with { Notes = null };

            // act
            var payee = PayeeConverter.Convert(payeeDto);

            // assert
            Assert.AreEqual(string.Empty, payee.Notes);
        }
    }
}
