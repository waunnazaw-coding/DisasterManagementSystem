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
        if (double.IsNaN(latitude) || double.IsInfinity(latitude) ||
            double.IsNaN(longitude) || double.IsInfinity(longitude))
        {
            return null; // Invalid coordinates
        }

        var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "DisasterApp");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch
        {
            return null; // Network failure
        }

        if (!response.IsSuccessStatusCode)
            return null;

        using var content = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(content);

        var root = doc.RootElement;

        // Use TryGetProperty to safely access fields
        string? displayName = root.TryGetProperty("display_name", out var dn) ? dn.GetString() : null;

        if (!root.TryGetProperty("address", out var address))
        {
            // No address info available
            return new GeoInfo { Address = displayName };
        }

        string? country = address.TryGetProperty("country", out var c) ? c.GetString() : null;
        string? region = address.TryGetProperty("state", out var s) ? s.GetString() : null;

        return new GeoInfo
        {
            Address = displayName,
            Country = country,
            Region = region
        };
    }

}
