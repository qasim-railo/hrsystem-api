namespace HRSystem.API.Models;

public sealed class NumberingSequence : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string SequenceKey { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LastNumber { get; set; }
}
