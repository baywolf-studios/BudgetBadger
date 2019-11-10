using BudgetBadger.Logic;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace BudgetBadger.Tests.Logic
{
    public class PayeeLogicUnitTests
    {
        PayeeLogic PayeeLogic { get; set; }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public async Task SoftDeletePayee_PayeeWithActiveTransactions_Unsuccessful()
        {
            Assert.Pass();
        }
    }
}
