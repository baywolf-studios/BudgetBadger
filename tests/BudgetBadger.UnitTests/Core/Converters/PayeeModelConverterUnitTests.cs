using BudgetBadger.Core.Converters;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.TestData;
using BudgetBadger.UnitTests.TestModels;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Converters
{
    [TestFixture]
    public class PayeeModelConverterUnitTests
    {
        [Test]
        public void Convert_FromAccountDto_ToAccountPayeeModelWithCorrectMappings()
        {
            // assemble
            var account = TestDataGenerator.AccountDto();

            // act
            var result = PayeeModelConverter.Convert(account);

            // assert
            Assert.IsInstanceOf<AccountPayeeModel>(result);
            Assert.AreEqual(account.Id, result.Id);
            Assert.AreEqual(account.Description, result.Description);
        }

        [Test]
        public void Convert_FromAccountDto_ToAccountPayeeModelWithEmptyStringNotes()
        {
            // assemble
            var account = TestDataGenerator.AccountDto() with { Notes = string.Empty };

            // act
            var result = PayeeModelConverter.Convert(account);

            // assert
            Assert.AreEqual(string.Empty, result.Notes);
        }

        [Test]
        public void Convert_FromStartingBalancePayeeDto_ToStartingBalancePayeeModelWithCorrectMappings()
        {
            // act
            var result = PayeeModelConverter.Convert(TestDataGenerator.StartingBalancePayeeDto());

            // assert
            Assert.IsInstanceOf<StartingBalancePayeeModel>(result);
            Assert.AreEqual(Constants.StartingBalancePayeeId, result.Id);
            Assert.AreEqual(AppResources.StartingBalancePayee, result.Description);
        }

        [Test]
        public void Convert_FromStartingBalancePayeeDto_ToStartingBalancePayeeModelWithEmptyStringNotes()
        {
            // act
            var result = PayeeModelConverter.Convert(TestDataGenerator.StartingBalancePayeeDto());

            // assert
            Assert.AreEqual(string.Empty, result.Notes);
        }

        [Test]
        public void Convert_FromPayeeDto_ToPayeeModelWithCorrectMappings()
        {
            // assemble
            var account = TestDataGenerator.AccountDto();

            // act
            var result = PayeeModelConverter.Convert(account);

            // assert
            Assert.IsInstanceOf<PayeeModel>(result);
            Assert.AreEqual(account.Id, result.Id);
            Assert.AreEqual(account.Description, result.Description);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNotes_ToPayeeModelWithNotes()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto() with { Notes = "lkj" };

            // act
            var result = PayeeModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(payee.Notes, result.Notes);
        }

        [Test]
        public void Convert_FromPayeeDtoWithNullNotes_ToPayeeModelWithEmptyStringNotes()
        {
            // assemble
            var payee = TestDataGenerator.PayeeDto() with { Notes = null };

            // act
            var result = PayeeModelConverter.Convert(payee);

            // assert
            Assert.AreEqual(string.Empty, result.Notes);
        }
    }
}

