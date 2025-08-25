using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories.Interfaces;  // Needed for IUserRepository
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    // Create a new scope for scoped services
                    using var scope = _scopeFactory.CreateScope();

                    // Resolve scoped services within the scope
                    var disasterService = scope.ServiceProvider.GetRequiredService<IGdacsDisasterService>();
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

                    var events = await disasterService.FetchFromFeedAsync();

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
                            if (newestEventNotSent == null ||
                                (disasterEvent.EventDate.HasValue && disasterEvent.EventDate > newestEventNotSent.EventDate))
                            {
                                newestEventNotSent = disasterEvent;
                            }

                            await disasterService.UpsertAsync(disasterEvent);

                            _sentEventTimestamps[disasterEvent.EventId] = disasterEvent.EventDate ?? DateTime.UtcNow;
                        }
                    }

                    if (newestEventNotSent != null)
                    {
                        var adminConnectionIds = DisasterNotificationHub.GetAdminConnectionIds();

                        if (adminConnectionIds.Count > 0)
                        {
                            await _hubContext.Clients.Clients(adminConnectionIds)
                                .SendAsync("ReceiveDisasterUpdate", newestEventNotSent);
                            _logger.LogInformation("Sent disaster event with EventId: {eventId} to admin clients.", newestEventNotSent.EventId);
                        }
                        else
                        {
                            _logger.LogInformation("No admin clients connected to receive disaster updates.");
                        }

                        var adminEmails = await userRepo.GetAdminEmailsAsync();

                        try
                        {
                            string subject = $"New Disaster Alert: {newestEventNotSent.EventType}";
                            string htmlMessage = $@"
                            <h1>Disaster Notification</h1>
                            <p><strong>Event ID:</strong> {newestEventNotSent.EventId}</p>
                            <p><strong>Type:</strong> {newestEventNotSent.EventType}</p>
                            <p><strong>Severity:</strong> {newestEventNotSent.Severity}</p>
                            <p><strong>Date:</strong> {newestEventNotSent.EventDate}</p>
                            <p><strong>Impact:</strong> {newestEventNotSent.Impact}</p>>";

                            if (adminEmails.Any())
                            {
                                await emailService.SendEmailAsync(adminEmails, subject, htmlMessage);
                                _logger.LogInformation("Sent disaster email notification to all admins.");
                            }
                            else
                            {
                                _logger.LogWarning("No admin emails configured to send disaster notification.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send disaster email notification.");
                        }
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
