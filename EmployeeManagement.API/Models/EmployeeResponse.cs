namespace EmployeeManagement.API.Models
{
    public class EmployeeResponse
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string Skillset { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string? ProfileImageBase64 { get; set; }

        public bool Status { get; set; }
    }
}
