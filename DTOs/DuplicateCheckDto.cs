using System;

namespace HRSystem.API.DTOs
{
    public class DuplicateCheckDto
    {
        public string PassportNumber { get; set; }
        public string NationalId { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
