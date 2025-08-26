using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class GdacsDisasterService : IGdacsDisasterService
    {
        private readonly IGdacsDisasterRepository _repository;
        private readonly IHubContext<DisasterNotificationHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IReverseGeocodingService _reverseGeocodingService;
        private const string GdacsRssUrl = "https://www.gdacs.org/xml/rss.xml";

        public GdacsDisasterService(
            IGdacsDisasterRepository repository,
            IHubContext<DisasterNotificationHub> hubContext,
            IHttpClientFactory httpClientFactory,
            IReverseGeocodingService reverseGeocodingService)
        {
            _repository = repository;
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
            _reverseGeocodingService = reverseGeocodingService;
        }

        // Fetch and parse feed, resolve location address asynchronously, returning all disaster events without notification/upsert
        public async Task<IEnumerable<GdacsdisasterEvent>> FetchFromFeedAsync()
        {
            var client = _httpClientFactory.CreateClient();
            using var stream = await client.GetStreamAsync(GdacsRssUrl);
            using var reader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(reader);
            var parsedEvents = new List<GdacsdisasterEvent>();

            var tasks = feed.Items.Select(async item =>
            {
                var eventId = item.ElementExtensions.ReadElementExtensions<string>("eventid", "http://www.gdacs.org").FirstOrDefault();
                if (string.IsNullOrEmpty(eventId))
                    return null;

                var pointStr = item.ElementExtensions.ReadElementExtensions<string>("point", "http://www.georss.org/georss").FirstOrDefault();
                double? latitude = null;
                double? longitude = null;
                if (!string.IsNullOrEmpty(pointStr))
                {
                    var parts = pointStr.Split(' ');
                    if (parts.Length == 2)
                    {
                        latitude = ParseDouble(parts[0]);
                        longitude = ParseDouble(parts[1]);
                    }
                }
                var status = item.ElementExtensions.ReadElementExtensions<string>("status", "http://www.gdacs.org").FirstOrDefault();
                //string? locationAddress = null;
                //if (latitude.HasValue && longitude.HasValue)
                //{
                //    locationAddress = await _reverseGeocodingService.GetAddressAsync(latitude.Value, longitude.Value);
                //}

                return new GdacsdisasterEvent
                {
                    EventId = eventId,
                    EventType = item.ElementExtensions.ReadElementExtensions<string>("eventtype", "http://www.gdacs.org").FirstOrDefault(),
                    Severity = item.ElementExtensions.ReadElementExtensions<string>("alertlevel", "http://www.gdacs.org").FirstOrDefault(),
                    EventDate = item.PublishDate.UtcDateTime,
                    Latitude = latitude,
                    Longitude = longitude,
                   // LocationAddress = locationAddress,
                    Impact = item.Summary?.Text,
                    Status = status
                };
            });

            var eventResults = await Task.WhenAll(tasks);
            parsedEvents.AddRange(eventResults.Where(e => e != null));
            return parsedEvents;
        }


        public async Task UpsertAsync(GdacsdisasterEvent disasterEvent)
        {
            await _repository.UpsertAsync(disasterEvent);
        }

        public async Task<IEnumerable<GdacsdisasterEvent>> GetAllEventsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<List<GdacsdisasterEvent>> GetTodaysEventsAsync()
        {
            var today = DateTime.Today;
            return await _repository.GetEventsByDateAsync(today);
        }

        public async Task<List<GdacsdisasterEvent>> GetEventsForCurrentWeekAsync()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff).Date;     // Monday
            DateTime endOfWeek = startOfWeek.AddDays(6).Date;         // Sunday

            return await _repository.GetEventsByDateRangeAsync(startOfWeek, endOfWeek);
        }

        private double? ParseDouble(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value) && double.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        
    }

}
