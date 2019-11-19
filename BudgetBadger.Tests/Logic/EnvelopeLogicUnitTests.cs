using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using BudgetBadger.Tests.TestModels;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Tests.Logic
{
    public class EnvelopeLogicUnitTests
    {
        IResourceContainer resourceContainer { get; set; }
        IEnvelopeDataAccess envelopeDataAccess { get; set; }
        ITransactionDataAccess transactionDataAccess { get; set; }
        IAccountDataAccess accountDataAccess { get; set; }
        EnvelopeLogic EnvelopeLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            accountDataAccess = A.Fake<IAccountDataAccess>();
            transactionDataAccess = A.Fake<ITransactionDataAccess>();
            envelopeDataAccess = A.Fake<IEnvelopeDataAccess>();
            resourceContainer = A.Fake<IResourceContainer>();
            EnvelopeLogic = new EnvelopeLogic(envelopeDataAccess, transactionDataAccess, accountDataAccess, resourceContainer);
        }

        [Test]
        public async Task SoftDeleteEnvelope_HiddenEnvelope_Successful()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_HiddenEnvelope_UpdatesEnvelope()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            A.CallTo(() => envelopeDataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task SoftDeleteEnvelope_NewEnvelope_Unsuccessful()
        {
            // arrange
            var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(newEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_DeletedEnvelope_Unsuccessful()
        {
            // arrange
            var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(deletedEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_ActiveEnvelope_Unsuccessful()
        {
            // arrange
            var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(activeEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_EnvelopeWithActiveTransactions_Unsuccessful()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
            var activeTransaction = TestTransactions.ActiveTransaction.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);
            A.CallTo(() => transactionDataAccess.ReadEnvelopeTransactionsAsync(hiddenEnvelope.Id)).Returns(new List<Transaction>() { activeTransaction });

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_EnvelopeWithDeletedTransactions_Successful()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
            var inactiveTransaction = TestTransactions.DeletedTransaction.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);
            A.CallTo(() => transactionDataAccess.ReadEnvelopeTransactionsAsync(hiddenEnvelope.Id)).Returns(new List<Transaction>() { inactiveTransaction });

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_GenericDebtEnvelope_Unsuccessful()
        {
            // arrange
            var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(debtEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_IncomeEnvelope_Unsuccessful()
        {
            // arrange
            var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(incomeEnvelope.Id)).Returns(incomeEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(incomeEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_BufferEnvelope_Unsuccessful()
        {
            // arrange
            var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(bufferEnvelope.Id)).Returns(bufferEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(bufferEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task SoftDeleteEnvelope_IgnoredEnvelope_Unsuccessful()
        {
            // arrange
            var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(ignoredEnvelope.Id)).Returns(ignoredEnvelope);

            // act
            var result = await EnvelopeLogic.SoftDeleteEnvelopeAsync(ignoredEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_ActiveEnvelope_Successful()
        {
            // arrange
            var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(activeEnvelope.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task HideEnvelope_ActiveEnvelope_UpdatesEnvelope()
        {
            // arrange
            var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(activeEnvelope.Id);

            // assert
            A.CallTo(() => envelopeDataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task HideEnvelope_NewEnvelope_Unsuccessful()
        {
            // arrange
            var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(newEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_DeletedEnvelope_Unsuccessful()
        {
            // arrange
            var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(deletedEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_HiddenEnvelope_Unsuccessful()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_GenericDebtEnvelope_Unsuccessful()
        {
            // arrange
            var debtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(debtEnvelope.Id)).Returns(debtEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(debtEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_IncomeEnvelope_Unsuccessful()
        {
            // arrange
            var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(incomeEnvelope.Id)).Returns(incomeEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(incomeEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_BufferEnvelope_Unsuccessful()
        {
            // arrange
            var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(bufferEnvelope.Id)).Returns(bufferEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(bufferEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task HideEnvelope_IgnoredEnvelope_Unsuccessful()
        {
            // arrange
            var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();

            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(ignoredEnvelope.Id)).Returns(ignoredEnvelope);

            // act
            var result = await EnvelopeLogic.HideEnvelopeAsync(ignoredEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideEnvelope_HiddenEnvelope_Successful()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

            // act
            var result = await EnvelopeLogic.UnhideEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task UnhideEnvelope_HiddenEnvelope_UpdatesEnvelope()
        {
            // arrange
            var hiddenEnvelope = TestEnvelopes.HiddenEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(hiddenEnvelope.Id)).Returns(hiddenEnvelope);

            // act
            var result = await EnvelopeLogic.UnhideEnvelopeAsync(hiddenEnvelope.Id);

            // assert
            A.CallTo(() => envelopeDataAccess.UpdateEnvelopeAsync(A<Envelope>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task UnhideEnvelope_DeletedEnvelope_Unsuccessful()
        {
            // arrange
            var deletedEnvelope = TestEnvelopes.SoftDeletedEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(deletedEnvelope.Id)).Returns(deletedEnvelope);

            // act
            var result = await EnvelopeLogic.UnhideEnvelopeAsync(deletedEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideEnvelope_NewEnvelope_Unsuccessful()
        {
            // arrange
            var newEnvelope = TestEnvelopes.NewEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(newEnvelope.Id)).Returns(newEnvelope);

            // act
            var result = await EnvelopeLogic.UnhideEnvelopeAsync(newEnvelope.Id);

            // assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task UnhideEnvelope_ActiveEnvelope_Unsuccessful()
        {
            // arrange
            var activeEnvelope = TestEnvelopes.ActiveEnvelope.DeepCopy();
            A.CallTo(() => envelopeDataAccess.ReadEnvelopeAsync(activeEnvelope.Id)).Returns(activeEnvelope);

            // act
            var result = await EnvelopeLogic.UnhideEnvelopeAsync(activeEnvelope.Id);

            // assert 
            Assert.IsFalse(result.Success);
        }

    }
}
