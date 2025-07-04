﻿namespace HRSystem.API.Models
{
    public class EmployeeDocument
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } // Full URL
        public string CloudinaryPublicId { get; set; } // Needed for deletion
        public DateTime UploadedAt { get; set; }
        public string FileType { get; set; }

        public Employee Employee { get; set; }
    }
}
