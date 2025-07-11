public class LocationUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? GeoJson { get; set; } // optional for updates
}
