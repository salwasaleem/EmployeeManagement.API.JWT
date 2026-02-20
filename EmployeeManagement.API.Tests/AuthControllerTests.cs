using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AuthControllerTests
{
    private AuthController GetController(string dbName, out EmployeeDbContext context)
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        context = new EmployeeDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Jwt:Key", "TestKey123456789012345678901234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:DurationInMinutes", "60"}
            })
            .Build();

        return new AuthController(context, config);
    }

    // ✅ LOGIN FAIL — USER NOT FOUND
    [Fact]
    public void Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        var controller = GetController("LoginFailDb", out _);

        var result = controller.Login("wrong", "wrong");

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ✅ LOGIN SUCCESS
    [Fact]
    public void Login_ReturnsToken_WhenValidCredentials()
    {
        var controller = GetController("LoginSuccessDb", out var context);

        context.Employees.Add(new Employee
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Admin",
            Status = true,
            Name = "Admin"
        });
        context.SaveChanges();

        var result = controller.Login("admin", "123456");

        Assert.IsType<OkObjectResult>(result);
    }

    // ✅ LOGIN FAIL — ACCOUNT DISABLED
    [Fact]
    public void Login_ReturnsBadRequest_WhenAccountDisabled()
    {
        var controller = GetController("DisabledDb", out var context);

        context.Employees.Add(new Employee
        {
            Username = "user",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            Status = false
        });
        context.SaveChanges();

        var result = controller.Login("user", "123456");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ✅ REGISTER SUCCESS
    [Fact]
    public async Task Register_AddsEmployee_ToDatabase()
    {
        var controller = GetController("RegisterDb", out var context);

        var request = new RegisterRequest
        {
            Name = "Salwa",
            Designation = "Developer",
            Address = "Kerala",
            Department = "IT",
            Skillset = "React",
            Username = "salwa",
            Password = "123456"
        };

        var result = await controller.Register(request);

        Assert.Equal(1, context.Employees.Count());
        Assert.IsType<OkObjectResult>(result);
    }

    // ✅ REGISTER FAIL — DUPLICATE USERNAME
    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameExists()
    {
        var controller = GetController("DuplicateDb", out var context);

        context.Employees.Add(new Employee { Username = "salwa" });
        context.SaveChanges();

        var request = new RegisterRequest
        {
            Name = "Salwa",
            Designation = "Developer",
            Address = "Kerala",
            Department = "IT",
            Skillset = "React",
            Username = "salwa",
            Password = "123"
        };

        var result = await controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}