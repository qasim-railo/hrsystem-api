namespace HRSystem.API.DTOs
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string AssetCode { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
