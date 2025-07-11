using System.Net.Http;
using System.Text.Json;
using DisasterManagementSystem_Services.Models.LocationDtos;

public class NominatimGeocodingService : INominatimGeocodingService
{
    private readonly HttpClient _httpClient;

    public NominatimGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeoInfo?> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "DisasterApp");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        using var content = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(content);

        var address = doc.RootElement.GetProperty("address");

        return new GeoInfo
        {
            Address = doc.RootElement.GetProperty("display_name").GetString(),
            Country = address.GetProperty("country").GetString(),
            Region = address.TryGetProperty("state", out var stateProp) ? stateProp.GetString() : null
        };
    }
}
