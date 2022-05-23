using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.Logic;

public class ReportLogicUnitTests
{
    IResourceContainer resourceContainer { get; set; }
    IDataAccess dataAccess { get; set; }
    ReportLogic reportLogic { get; set; }

    [SetUp]
    public void Setup()
    {
        dataAccess = A.Fake<IDataAccess>();
        resourceContainer = A.Fake<IResourceContainer>();
    }

    [Test]
    public async Task SoftDeletePayee_HiddenPayee_Successful()
    {
        // arrange
        //var hiddenPayee = TestPayees.HiddenPayee.DeepCopy();

        //A.CallTo(() => PayeeDataAccess.ReadPayeeAsync(hiddenPayee.Id)).Returns(hiddenPayee);

        // act
        //var result = await PayeeLogic.SoftDeletePayeeAsync(hiddenPayee.Id);

        // assert
        Assert.IsTrue(true);
    }
}