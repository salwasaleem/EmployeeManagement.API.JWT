using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _context;

        public EmployeeController(EmployeeDbContext context)
        {
            _context = context;
        }

        // ✅ ADMIN — GET ALL
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _context.Employees
                .Select(e => new EmployeeResponse
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.Name,
                    Designation = e.Designation,
                    Address = e.Address,
                    Department = e.Department,
                    JoiningDate = e.JoiningDate,
                    Skillset = e.Skillset,
                    Username = e.Username,
                    Role = e.Role,
                    Status = e.Status,
                    ProfileImageBase64 = e.ProfileImage != null
                        ? Convert.ToBase64String(e.ProfileImage)
                        : null
                })
                .ToListAsync();

            return Ok(employees);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound("Employee not found");

            var response = new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Designation = employee.Designation,
                Address = employee.Address,
                Department = employee.Department,
                JoiningDate = employee.JoiningDate,
                Skillset = employee.Skillset,
                Username = employee.Username,
                Role = employee.Role,
                Status = employee.Status,
                ProfileImageBase64 = employee.ProfileImage != null
                    ? Convert.ToBase64String(employee.ProfileImage)
                    : null
            };

            return Ok(response);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromForm] UpdateEmployeeRequest request)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            employee.Name = request.Name;
            employee.Designation = request.Designation;
            employee.Address = request.Address;
            employee.Department = request.Department;
            employee.Skillset = request.Skillset;

            if (!string.IsNullOrWhiteSpace(request.Username))
                employee.Username = request.Username;

            if (!string.IsNullOrWhiteSpace(request.Password))
                employee.Password = request.Password;

            if (request.ProfileImage != null)
            {
                using var ms = new MemoryStream();
                request.ProfileImage.CopyTo(ms);
                employee.ProfileImage = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return Ok("Employee updated successfully");
        }

        // ✅ SOFT DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            employee.Status = false;
            await _context.SaveChangesAsync();

            return Ok("Employee disabled");
        }

        // ✅ TOGGLE STATUS
        [Authorize(Roles = "Admin")]
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            employee.Status = !employee.Status;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
