using System;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Converters;
using BudgetBadger.Core.Models;
using BudgetBadger.Core.Utilities;
using BudgetBadger.TestData;
using BudgetBadger.UnitTests.TestModels;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Converters
{
	[TestFixture]
	public class PayeeEditModelConverterUnitTests
	{
		[Test]
        public void Convert_FromPayeeDto_ToPayeeEditModelWithCorrectMappings()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsInstanceOf<PayeeEditModel>(result);
            Assert.AreEqual(payee.Id, result.Id);
            Assert.AreEqual(payee.Description, result.Description);
        }

        [Test]
        public void Convert_FromPayeeDtoWithHiddenDateTime_ToPayeeEditModelWithIsHiddenTrue()
        {
            // assemble
            var payee = TestDataGenerator.HiddenPayeeDto();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsTrue(result.Hidden);
        }

        [Test]
        public void Convert_FromPayeeDtoWithoutHiddenDateTime_ToPayeeEditModelWithIsHiddenFalse()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsFalse(result.Hidden);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNotes_ToPayeeEditModelWithNotes()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto() with { Notes = "lkj" };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(payee.Notes, result.Notes);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNullNotes_ToPayeeEditModelWithEmptyStringNotes()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto() with { Notes = null };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(string.Empty, result.Notes);
        }

        [Test]
        public void Convert_FromPayeeEditModel_ToPayeeDtoWithCorrectMappings()
        {
            // assemble
            var payee = TestDataGenerator.NewPayeeEditModel();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsInstanceOf<PayeeDto>(result);
            Assert.AreEqual(payee.Description, result.Description);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithEmptyGuidId_ToPayeeDtoWithNewGuidId()
        {
            // assemble
            var payee = TestDataGenerator.NewPayeeEditModel();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithNonEmptyGuidId_ToPayeeDtoWithSameId()
        {
            // assemble
            var payee = TestDataGenerator.ActivePayeeEditModel();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(payee.Id, result.Id);
        }

        [Test]
        public void Convert_FromPayeeEditModel_ToPayeeDtoWithModifiedDateTime()
        {
            // assemble
            var payee = TestDataGenerator.NewPayeeEditModel();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsFalse(result.ModifiedDateTime.IsEmpty());
        }

        [Test]
        public void Convert_FromPayeeEditModel_ToPayeeDtoWithDeletedFalse()
        {
            // assemble
            var payee = TestDataGenerator.ActivePayeeEditModel();

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsFalse(result.Deleted);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithIsHiddenTrue_ToPayeeDtoWithHiddenTrue()
        {
            // assemble
            var payee = TestDataGenerator.NewPayeeEditModel() with { Hidden = true };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsTrue(result.Hidden);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithIsHiddenFalse_ToPayeeDtoWithHiddenFalse()
        {
            var payee = TestDataGenerator.NewPayeeEditModel() with { Hidden = false };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsFalse(result.Hidden);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithNotes_ToPayeeDtoWithNotes()
        {
            // assemble
            var payee = TestDataGenerator.ActivePayeeEditModel() with { Notes = "lkj" };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(payee.Notes, result.Notes);
        }

        [Test]
        public void Convert_FromPayeeEditModelWithEmptyNotes_ToPayeeDtoWithNullNotes()
        {
            // assemble
            var payee = TestDataGenerator.ActivePayeeEditModel() with { Notes = string.Empty };

            // act
            var result = PayeeEditModelConverter.Convert(payee);

            // assert
            Assert.IsNull(result.Notes);
        }
    }
}

