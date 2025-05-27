namespace HRSystem.API.Models
{
    public class EmployeeAsset
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public string Status { get; set; } // e.g. Assigned, Returned, Lost
    }
}
