namespace HRSystem.API.DTOs
{
    public class EmployeeDocumentUploadDto
    {
        public int EmployeeId { get; set; }
        public IFormFile File { get; set; }
        public string FileType { get; set; }
    }

    public class EmployeeDocumentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileType { get; set; }
    }
}
