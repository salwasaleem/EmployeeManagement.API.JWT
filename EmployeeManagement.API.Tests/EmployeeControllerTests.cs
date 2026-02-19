using Xunit;
using Moq;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Data;

namespace EmployeeManagement.API.Tests
{
    public class EmployeeControllerTests
    {
        [Fact]
        public void Controller_Creation_Succeeds()
        {
            // Arrange
            var mockDb = new Mock<DbHelper>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());

            // Act
            var controller = new EmployeeController(mockDb.Object);

            // Assert
            Assert.NotNull(controller);
        }
    }
}
