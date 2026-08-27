namespace HRSystem.API.Models
{
    public class Asset : ITenantOwned
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string AssetCode { get; set; }
        public DateTime PurchaseDate { get; set; }

        public ICollection<EmployeeAsset> EmployeeAssets { get; set; }
    }
}
