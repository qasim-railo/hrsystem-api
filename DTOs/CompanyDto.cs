namespace HRSystem.API.DTOs
{
      public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}