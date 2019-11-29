using System;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Logic;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.Tests.Logic
{
    public class ReportLogicUnitTests
    {
        IResourceContainer resourceContainer { get; set; }
        IPayeeDataAccess payeeDataAccess { get; set; }
        ITransactionDataAccess transactionDataAccess { get; set; }
        IAccountDataAccess accountDataAccess { get; set; }
        ReportLogic reportLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            accountDataAccess = A.Fake<IAccountDataAccess>();
            transactionDataAccess = A.Fake<ITransactionDataAccess>();
            PayeeDataAccess = A.Fake<IPayeeDataAccess>();
            resourceContainer = A.Fake<IResourceContainer>();
            reportLogic = new ReportLogic(tran
        }

        [Test]
        public async Task SoftDeletePayee_HiddenPayee_Successful()
        {
            // arrange
            var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();

            A.CallTo(() => PayeeDataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

            // act
            var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

            // assert
            Assert.IsTrue(result.Success);
        }
    }
}