namespace DisasterManagementSystem_Services.Models.LocationDtos
{
    public class LocationCreateDto
    {
        public string Name { get; set; } = null!;
        public string GeoJson { get; set; } = null!;
        public string? Address { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
    }
}
