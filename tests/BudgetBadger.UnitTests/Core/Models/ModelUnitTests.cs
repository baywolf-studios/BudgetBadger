using System;
using BudgetBadger.Core.Converters;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.UnitTests.TestModels;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Models
{
    [TestFixture]
    public class ModelUnitTests
	{
        [Test]
        public void AccountPayeeModel_GroupIsAccountPayeeModelGroup()
        {
            // act
            var accountPayeeModel = new AccountPayeeModel();

            // assert
            Assert.AreEqual(AppResources.PayeeTransferGroup, accountPayeeModel.Group);
        }

        [Test]
        public void StartingBalancePayeeModel_GroupIsStartingBalancePayee()
        {
            // act
            var startingBalancePayeeModel = new StartingBalancePayeeModel();

            // assert
            Assert.AreEqual(AppResources.StartingBalancePayee, startingBalancePayeeModel.Group);
        }

        [Test]
        public void PayeeModel_NonEmptyDescription_GroupIsFirstLetterOfDescription()
        {
            // act
            var payeeModel = new PayeeModel { Description = "Test" };

            // assert
            Assert.AreEqual("T", payeeModel.Group);
        }

        [Test]
        public void PayeeModel_EmptyStringDescription_GroupIsEmptyString()
        {
            // assemble
            var payeeModel = new PayeeModel { Description = string.Empty };

            // assert
            Assert.AreEqual(string.Empty, payeeModel.Group);
        }

        [Test]
        public void PayeeModel_NullDescription_GroupIsEmptyString()
        {
            // assemble
            var payeeModel = new PayeeModel { Description = null };

            // assert
            Assert.AreEqual(string.Empty, payeeModel.Group);
        }

        [Test]
        public void PayeeEditModel_NonEmptyDescription_GroupIsFirstLetterOfDescription()
        {
            // act
            var payeeEditModel = new PayeeEditModel { Description = "Test" };

            // assert
            Assert.AreEqual("T", payeeEditModel.Group);
        }

        [Test]
        public void PayeeEditModel_EmptyStringDescription_GroupIsEmptyString()
        {
            // assemble
            var payeeEditModel = new PayeeEditModel { Description = string.Empty };

            // assert
            Assert.AreEqual(string.Empty, payeeEditModel.Group);
        }

        [Test]
        public void PayeeEditModel_NullDescription_GroupIsEmptyString()
        {
            // assemble
            var payeeEditModel = new PayeeEditModel { Description = null };

            // assert
            Assert.AreEqual(string.Empty, payeeEditModel.Group);
        }
    }
}

