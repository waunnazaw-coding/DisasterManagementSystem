using DisasterManagementSystem_Services.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class ReverseGeocodingService : IReverseGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, string?> _cache = new();

        public ReverseGeocodingService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DisasterManagementSystem/1.0"); // Required by Nominatim
        }

        public async Task<string?> GetAddressAsync(double latitude, double longitude)
        {
            string key = $"{latitude},{longitude}";

            if (_cache.TryGetValue(key, out var cachedAddress))
                return cachedAddress;

            // Add zoom and accept-language parameters to improve result
            string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json&zoom=18&accept-language=en";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Reverse geocode request failed with status {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Reverse geocode returned empty response");
                    return null;
                }

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                {
                    Console.WriteLine($"Reverse geocode error: {errorProp.GetString()}");
                    return null;
                }

                if (doc.RootElement.TryGetProperty("display_name", out var displayNameProp))
                {
                    string address = displayNameProp.GetString() ?? string.Empty;
                    _cache[key] = address;
                    return address;
                }
                else
                {
                    Console.WriteLine("Reverse geocode response missing 'display_name'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during reverse geocode: {ex.Message}");
            }

            return null;
        }

    }
}
