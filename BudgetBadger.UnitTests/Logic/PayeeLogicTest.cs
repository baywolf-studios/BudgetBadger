using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Logic;
using BudgetBadger.Models;
using Moq;
using Xunit;

namespace BudgetBadger.UnitTests.Logic
{
    public class PayeeLogicTest
    {
        [Fact]
        public async Task UpsertPayeeAsync_GivenAPayeeWithoutACreatedDateTime_CallsCreatePayeeOnDataAccess()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.CreatePayeeAsync(It.IsAny<Payee>()))
                .Returns(Task.CompletedTask);

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var payee = new Payee
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = null
            };

            var result = await payeeLogic.SavePayeeAsync(payee);

            payeeDataAccessMock.Verify(x => x.CreatePayeeAsync(It.IsAny<Payee>()));
        }

        [Fact]
        public async Task UpsertPayeeAsync_GivenAPayeeWithoutACreatedDateTimeOrModifiedDateTime_ReturnsAPayeeWithACreatedDateTimeAndModifiedDateTime()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.CreatePayeeAsync(It.IsAny<Payee>()))
                .Returns(Task.CompletedTask);

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var payee = new Payee
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = null,
                ModifiedDateTime = null
            };

            var result = await payeeLogic.SavePayeeAsync(payee);

            Assert.True(result.Success);
            Assert.NotNull(result.Data.CreatedDateTime);
            Assert.NotNull(result.Data.ModifiedDateTime);
        }

        [Fact]
        public async Task UpsertPayeeAsync_GivenAPayeeWithACreatedDateTime_CallsUpdatePayeeOnDataAccess()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.CreatePayeeAsync(It.IsAny<Payee>()))
                .Returns(Task.CompletedTask);

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var payee = new Payee
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = DateTime.Now.AddDays(-1)
            };

            var result = await payeeLogic.SavePayeeAsync(payee);

            payeeDataAccessMock.Verify(x => x.UpdatePayeeAsync(It.IsAny<Payee>()));
        }

        [Fact]
        public async Task UpsertPayeeAsync_GivenAPayeeWithACreatedDateTimeAndModifiedDateTime_ReturnsAPayeeWithAnUpdatedModifiedDateTime()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.CreatePayeeAsync(It.IsAny<Payee>()))
                .Returns(Task.CompletedTask);

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var dateTime = DateTime.Now;

            var payee = new Payee
            {
                Id = Guid.NewGuid(),
                CreatedDateTime = dateTime,
                ModifiedDateTime = dateTime
            };

            var result = await payeeLogic.SavePayeeAsync(payee);

            Assert.True(result.Success);
            Assert.NotEqual(dateTime, result.Data.ModifiedDateTime);
        }

        [Fact]
        public async Task UpsertPayeeAsync_GivenADataAccessException_ReturnsFalseResult()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.CreatePayeeAsync(It.IsAny<Payee>()))
                .Throws(new Exception());

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var payee = new Payee
            {
                Id = Guid.NewGuid()
            };

            var result = await payeeLogic.SavePayeeAsync(payee);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPayeesAsync_GivenADataAccessException_ReturnsFalseResult()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.ReadPayeesAsync())
                .Throws(new Exception());

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var result = await payeeLogic.GetPayeesAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeletePayeeAsync_GivenAPayeeWithoutADeletedDateTime_ReturnsAPayeeWithADeletedDateTime()
        {
            var payeeDataAccessMock = new Mock<IPayeeDataAccess>();
            var accountDataAccessMock = new Mock<IAccountDataAccess>();
            payeeDataAccessMock
                .Setup(x => x.UpdatePayeeAsync(It.IsAny<Payee>()))
                .Returns(Task.CompletedTask);

            var payeeLogic = new PayeeLogic(payeeDataAccessMock.Object, accountDataAccessMock.Object);

            var payee = new Payee
            {
                Id = Guid.NewGuid(),
                DeletedDateTime = null
            };

            var result = await payeeLogic.DeletePayeeAsync(payee);

            Assert.True(result.Success);
            Assert.NotNull(payee.DeletedDateTime);
        }
    }
}
