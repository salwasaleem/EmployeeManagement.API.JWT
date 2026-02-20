using Xunit;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

public class EmployeeControllerTests
{
    private EmployeeController GetController(string dbName, out EmployeeDbContext context)
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        context = new EmployeeDbContext(options);
        return new EmployeeController(context);
    }

    // ✅ GET ALL
    [Fact]
    public async Task GetAllEmployees_ReturnsEmployees()
    {
        var controller = GetController("GetAllDb", out var context);

        context.Employees.Add(new Employee { Name = "John" });
        context.SaveChanges();

        var result = await controller.GetAllEmployees();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var employees = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);

        Assert.Single(employees);
    }

    // ✅ GET BY ID SUCCESS
    [Fact]
    public async Task GetEmployeeById_ReturnsEmployee()
    {
        var controller = GetController("GetByIdDb", out var context);

        var emp = new Employee { Name = "John" };
        context.Employees.Add(emp);
        context.SaveChanges();

        var result = await controller.GetEmployeeById(emp.EmployeeId);

        Assert.IsType<OkObjectResult>(result);
    }

    // ✅ GET BY ID FAIL
    [Fact]
    public async Task GetEmployeeById_ReturnsNotFound()
    {
        var controller = GetController("NotFoundDb", out _);

        var result = await controller.GetEmployeeById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ✅ UPDATE SUCCESS
    [Fact]
    public async Task UpdateEmployee_UpdatesData()
    {
        var controller = GetController("UpdateDb", out var context);

        var emp = new Employee { Name = "Old" };
        context.Employees.Add(emp);
        context.SaveChanges();

        var request = new UpdateEmployeeRequest
        {
            Name = "New",
            Designation = "Dev",
            Address = "Kerala",
            Department = "IT",
            Skillset = "React"
        };

        var result = await controller.UpdateEmployee(emp.EmployeeId, request);

        Assert.Equal("New", context.Employees.First().Name);
        Assert.IsType<OkObjectResult>(result);
    }

    // ✅ DELETE (SOFT)
    [Fact]
    public async Task SoftDeleteEmployee_DisablesEmployee()
    {
        var controller = GetController("DeleteDb", out var context);

        var emp = new Employee { Status = true };
        context.Employees.Add(emp);
        context.SaveChanges();

        var result = await controller.SoftDeleteEmployee(emp.EmployeeId);

        Assert.False(context.Employees.First().Status);
        Assert.IsType<OkObjectResult>(result);
    }

    // ✅ TOGGLE STATUS
    [Fact]
    public async Task ToggleStatus_TogglesEmployeeStatus()
    {
        var controller = GetController("ToggleDb", out var context);

        var emp = new Employee { Status = true };
        context.Employees.Add(emp);
        context.SaveChanges();

        var result = await controller.ToggleStatus(emp.EmployeeId);

        Assert.False(context.Employees.First().Status);
        Assert.IsType<OkResult>(result);
    }
}