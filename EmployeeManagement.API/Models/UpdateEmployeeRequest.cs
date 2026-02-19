using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models
{
    public class UpdateEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public string Skillset { get; set; }
        public string ModifiedBy { get; set; }
        
        public string? Username { get; set; }
        public string? Password { get; set; }
        public IFormFile? ProfileImage { get; set; }


    }
}
