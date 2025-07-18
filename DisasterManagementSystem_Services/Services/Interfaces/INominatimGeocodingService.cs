using DisasterManagementSystem_Services.Models.LocationDtos;

public interface INominatimGeocodingService
{
    Task<GeoInfo?> ReverseGeocodeAsync(double latitude, double longitude);
}