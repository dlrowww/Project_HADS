public class DeparturePreferenceStat
{
    public Guid Id { get; set; }
    public string FromCity { get; set; } = null!;
    public string ToCity { get; set; } = null!;
    public int TotalCount { get; set; } = 1;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
