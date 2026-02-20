using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(EmployeeDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Employees.FirstOrDefault(x => x.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return Unauthorized("Invalid credentials");

            if (!user.Status)
                return BadRequest("Account disabled");

            var token = GenerateJwtToken(user.Username, user.Role);

            return Ok(new
            {
                token,
                employeeId = user.EmployeeId,
                name = user.Name,
                role = user.Role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (_context.Employees.Any(x => x.Username == request.Username))
                return BadRequest("Username already exists");

            var employee = new Employee
            {
                Name = request.Name,
                Designation = request.Designation,
                Address = request.Address,
                Department = request.Department,
                JoiningDate = request.JoiningDate,
                Skillset = request.Skillset,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Employee",
                Status = true
            };

            if (request.ProfileImage != null)
            {
                using var ms = new MemoryStream();
                await request.ProfileImage.CopyToAsync(ms);
                employee.ProfileImage = ms.ToArray();
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee registered successfully");
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwtSettings["DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
