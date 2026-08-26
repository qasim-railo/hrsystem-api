using System;
using System.Collections.Generic;

namespace HRSystem.API.DTOs
{
    public class EmployeeFilterDto
    {
        public string? Search { get; set; }
        public List<int>? Statuses { get; set; }
        public List<int>? CompanyIds { get; set; }
        public List<int>? DepartmentIds { get; set; }
        public string? Category { get; set; }
        public DateTime? JoiningDateFrom { get; set; }
        public DateTime? JoiningDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
