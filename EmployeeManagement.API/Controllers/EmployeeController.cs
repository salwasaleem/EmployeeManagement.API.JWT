using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ All endpoints require login
    public class EmployeeController : ControllerBase
    {
        private readonly IDbHelper _db;

        public EmployeeController(IDbHelper db)
        {
            _db = db;
        }

        // 🔐 ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public IActionResult GetAllEmployees()
        {
            var employees = new List<EmployeeResponse>();

            using SqlConnection conn = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_GetAllEmployees", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new EmployeeResponse
                {
                    EmployeeId = (int)reader["EmployeeId"],
                    Name = reader["Name"].ToString()!,
                    Designation = reader["Designation"].ToString()!,
                    Address = reader["Address"].ToString()!,
                    Department = reader["Department"].ToString()!,
                    JoiningDate = reader["JoiningDate"] == DBNull.Value
                        ? null
                        : (DateTime)reader["JoiningDate"],
                    Skillset = reader["Skillset"].ToString()!,
                    Username = reader["Username"].ToString()!,
                    Role = reader["Role"].ToString()!,
                    Status = (bool)reader["Status"],
                    ProfileImageBase64 = reader["ProfileImage"] == DBNull.Value
                        ? null
                        : Convert.ToBase64String((byte[])reader["ProfileImage"])
                });
            }

            return Ok(employees);
        }

        // 🔐 Logged-in user can view profile
        [HttpGet("{employeeId}")]
        public IActionResult GetEmployeeById(int employeeId)
        {
            using SqlConnection conn = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_GetEmployeeById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

            conn.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound("Employee not found");

            var employee = new EmployeeResponse
            {
                EmployeeId = (int)reader["EmployeeId"],
                Name = reader["Name"].ToString()!,
                Designation = reader["Designation"].ToString()!,
                Address = reader["Address"].ToString()!,
                Department = reader["Department"].ToString()!,
                JoiningDate = reader["JoiningDate"] == DBNull.Value
                    ? null
                    : (DateTime)reader["JoiningDate"],
                Skillset = reader["Skillset"].ToString()!,
                Username = reader["Username"].ToString()!,
                Role = reader["Role"].ToString()!,
                Status = (bool)reader["Status"],
                ProfileImageBase64 = reader["ProfileImage"] == DBNull.Value
                    ? null
                    : Convert.ToBase64String((byte[])reader["ProfileImage"])
            };

            return Ok(employee);
        }

        // 🔐 Admin OR employee (logged-in)
        [HttpPut("{employeeId}")]
        public IActionResult UpdateEmployee(int employeeId, [FromForm] UpdateEmployeeRequest request)
        {
            using SqlConnection conn = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_UpdateEmployee", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
            cmd.Parameters.AddWithValue("@Name", request.Name);
            cmd.Parameters.AddWithValue("@Designation", request.Designation);
            cmd.Parameters.AddWithValue("@Address", request.Address);
            cmd.Parameters.AddWithValue("@Department", request.Department);
            cmd.Parameters.AddWithValue("@Skillset", request.Skillset);
            cmd.Parameters.AddWithValue("@ModifiedBy", request.ModifiedBy);

            cmd.Parameters.AddWithValue("@Username",
                string.IsNullOrWhiteSpace(request.Username)
                    ? (object)DBNull.Value
                    : request.Username);

            cmd.Parameters.AddWithValue("@Password",
                string.IsNullOrWhiteSpace(request.Password)
                    ? (object)DBNull.Value
                    : request.Password);

            byte[]? imageBytes = null;

            if (request.ProfileImage != null)
            {
                using (var ms = new MemoryStream())
                {
                    request.ProfileImage.CopyTo(ms);
                    imageBytes = ms.ToArray();
                }
            }

            var imageParam = new SqlParameter("@ProfileImage", SqlDbType.VarBinary);
            imageParam.Value = imageBytes != null ? imageBytes : DBNull.Value;
            cmd.Parameters.Add(imageParam);

            conn.Open();
            cmd.ExecuteNonQuery();

            return Ok("Employee updated successfully");
        }

        // 🔐 ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpDelete("{employeeId}")]
        public IActionResult SoftDeleteEmployee(int employeeId, [FromQuery] string modifiedBy)
        {
            using SqlConnection conn = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_SoftDeleteEmployee", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            conn.Open();
            cmd.ExecuteNonQuery();

            return Ok("Employee deleted successfully");
        }

        // 🔐 ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpPut("toggle-status/{id}")]
        public IActionResult ToggleStatus(int id, [FromQuery] string modifiedBy)
        {
            using SqlConnection conn = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_ToggleEmployeeStatus", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@EmployeeId", id);
            cmd.Parameters.AddWithValue("@ModifiedBy", modifiedBy);

            conn.Open();
            cmd.ExecuteNonQuery();

            return Ok();
        }
    }
}
