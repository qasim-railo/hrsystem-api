using System.Collections.Generic;

namespace HRSystem.API.DTOs
{
    public class DuplicateCheckResultDto
    {
        public bool HasPotentialDuplicates { get; set; }
        public int MatchScore { get; set; }
        public List<string> MatchedFields { get; set; } = new List<string>();
        public List<EmployeeDto> Candidates { get; set; } = new List<EmployeeDto>();
    }
}
