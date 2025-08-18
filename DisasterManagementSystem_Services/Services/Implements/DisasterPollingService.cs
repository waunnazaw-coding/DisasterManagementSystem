using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class DisasterPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<DisasterNotificationHub> _hubContext;
        private readonly ILogger<DisasterPollingService> _logger;
        private const int DelayMinutes = 5;

        // In-memory tracking: EventId -> Last Sent EventDate
        private readonly Dictionary<string, DateTime> _sentEventTimestamps = new();

        public DisasterPollingService(IServiceScopeFactory scopeFactory,
                                     IHubContext<DisasterNotificationHub> hubContext,
                                     ILogger<DisasterPollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
           {
                _logger.LogInformation("Polling GDACS disasters feed at {time}", DateTime.UtcNow);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var disasterService = scope.ServiceProvider.GetRequiredService<IGdacsDisasterService>();

                    var events = await disasterService.FetchFromFeedAsync();

                    // Find the newest event not sent before
                    GdacsdisasterEvent? newestEventNotSent = null;

                    foreach (var disasterEvent in events)
                    {
                        bool isNewOrUpdated = false;

                        if (!_sentEventTimestamps.TryGetValue(disasterEvent.EventId, out var lastSent))
                        {
                            isNewOrUpdated = true;
                        }
                        else if (disasterEvent.EventDate.HasValue && disasterEvent.EventDate.Value > lastSent)
                        {
                            isNewOrUpdated = true;
                        }

                        if (isNewOrUpdated)
                        {
                            // Keep track of the newest event to notify clients (only send one event per poll)
                            if (newestEventNotSent == null ||
                                (disasterEvent.EventDate.HasValue && disasterEvent.EventDate > newestEventNotSent.EventDate))
                            {
                                newestEventNotSent = disasterEvent;
                            }

                            // Upsert in DB
                            await disasterService.UpsertAsync(disasterEvent);

                            // Update tracking dictionary to prevent duplicates in next polls
                            _sentEventTimestamps[disasterEvent.EventId] = disasterEvent.EventDate ?? DateTime.UtcNow;
                        }
                    }

                    if (newestEventNotSent != null)
                    {
                        // Notify clients only about the single newest unsent/updated disaster event
                        await _hubContext.Clients.All.SendAsync("ReceiveDisasterUpdate", newestEventNotSent);
                        _logger.LogInformation("Sent 1 new or updated disaster event with EventId: {eventId}", newestEventNotSent.EventId);
                    }
                    else
                    {
                        _logger.LogInformation("No new disaster events to send at this poll.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while polling GDACS data.");
                }

                await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
            }
        }
    }
}
