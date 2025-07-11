namespace DisasterManagementSystem_Services.Models.LocationDtos
{
    public class LocationCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string GeoJson { get; set; } = string.Empty;
    }
}
