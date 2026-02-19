using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class AuthControllerTests
{
    [Fact]
    public void Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange

        // ✅ Mock interface (NOT DbHelper)
        var dbHelperMock = new Mock<IDbHelper>();

        // Return a fake connection (will not open)
        dbHelperMock
            .Setup(d => d.GetConnection())
             .Throws(new Exception("DB not available"));
              

        // Mock IConfiguration for JWT
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "TestKey123456789012345678901234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:DurationInMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var controller = new AuthController(dbHelperMock.Object, configuration);

        // Act
        var result = controller.Login("wronguser", "wrongpass");

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
