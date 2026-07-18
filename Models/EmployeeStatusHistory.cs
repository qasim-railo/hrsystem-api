using System;

namespace HRSystem.API.Models
{
    public class EmployeeStatusHistory
    {
        public int EmployeeStatusHistoryId { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public EmployeeStatus PreviousStatus { get; set; }
        public EmployeeStatus NewStatus { get; set; }

        public DateTime EffectiveDate { get; set; }
        public string Reason { get; set; }
        public int? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? SupportingDocumentId { get; set; }
    }
}
