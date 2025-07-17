public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? GeoJson { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }

       // Optional: Explicitly store centroid coordinates as nullable doubles
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
