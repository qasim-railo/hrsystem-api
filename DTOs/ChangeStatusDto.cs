using System;

namespace HRSystem.API.DTOs
{
    public class ChangeStatusDto
    {
        public int NewStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
            public DateTime? LastWorkingDate { get; set; }
            public string Reason { get; set; }
            public int? ChangedByUserId { get; set; }
            public int? SupportingDocumentId { get; set; }
        }
}
