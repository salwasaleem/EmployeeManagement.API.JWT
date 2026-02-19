using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;



// ✅ JWT using statements
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IDbHelper _db;
        private readonly IConfiguration _configuration; // ✅ Added

        // ✅ Updated constructor
        public AuthController(IDbHelper db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        // 🔐 LOGIN WITH JWT
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(string username, string password)
        {
            try
            {
                using SqlConnection conn = _db.GetConnection();
                using SqlCommand cmd = new SqlCommand("sp_Login", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return Unauthorized("Invalid credentials");

                string storedHash = reader["Password"].ToString();

                if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                    return Unauthorized("Invalid credentials");

                if (Convert.ToInt32(reader["Status"]) == 0)
                    return BadRequest("Your account has been disabled.");

                string role = reader["Role"].ToString();
                string user = username;

                var token = GenerateJwtToken(user, role);

                return Ok(new
                {
                    token,
                    employeeId = reader["EmployeeId"],
                    name = reader["Name"],
                    role = role
                });
            }
            catch
            {
                return Unauthorized("Invalid credentials");
            }
        }

        // 🔐 JWT TOKEN GENERATOR
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

        // 📝 REGISTER (unchanged)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            try
            {
                using SqlConnection conn = _db.GetConnection();
                using SqlCommand cmd = new SqlCommand("sp_RegisterEmployee", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", request.Name);
                cmd.Parameters.AddWithValue("@Designation", request.Designation);
                cmd.Parameters.AddWithValue("@Address", request.Address);
                cmd.Parameters.AddWithValue("@Department", request.Department);
                cmd.Parameters.AddWithValue("@JoiningDate", request.JoiningDate);
                cmd.Parameters.AddWithValue("@Skillset", request.Skillset);
                cmd.Parameters.AddWithValue("@Username", request.Username);

                // 🔐 Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                // 🔽 Image to byte[]
                byte[]? imageBytes = null;
                if (request.ProfileImage != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await request.ProfileImage.CopyToAsync(ms);
                        imageBytes = ms.ToArray();
                    }
                }

                cmd.Parameters.AddWithValue("@ProfileImage",
                    (object?)imageBytes ?? DBNull.Value);

                SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.Int);
                resultParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(resultParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                int result = (int)resultParam.Value;

                if (result == -1)
                {
                    return BadRequest("Username already exists. Please choose another.");
                }

                return Ok("Employee registered successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
