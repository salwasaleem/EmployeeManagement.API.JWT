namespace EmployeeManagement.API.Models
{
    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Designation { get; set; }
        public required string Address { get; set; }
        public required string Department { get; set; }
        public DateTime JoiningDate { get; set; }
        public required string Skillset { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }

        public IFormFile? ProfileImage { get; set; }

    }
}
