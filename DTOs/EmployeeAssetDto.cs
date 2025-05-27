namespace HRSystem.API.DTOs
{
    public class EmployeeAssetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int AssetId { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public string Status { get; set; }
    }
}
