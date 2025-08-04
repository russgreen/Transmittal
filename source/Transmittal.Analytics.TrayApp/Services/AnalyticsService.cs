using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;
using System.Text.Json;
using Transmittal.Analytics.Client;

namespace Transmittal.Analytics.TrayApp.Services;

/// <summary>
/// Background service that listens for analytics events via named pipes and forwards them to App Center
/// </summary>
public class AnalyticsService : BackgroundService
{
    private readonly ILogger<AnalyticsService> _logger;
    private const string _pipeName = "TransmittalAnalyticsPipe";
    private const string _appCenterSecret = "YOUR_APP_CENTER_SECRET"; // Replace with actual secret

    public AnalyticsService(ILogger<AnalyticsService> logger)
    {
        _logger = logger;

        // Initialize App Center
        if (!AppCenter.Configured)
        {
            AppCenter.Start(_appCenterSecret, typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Crashes));
            _logger.LogDebug("App Center initialized with secret: {Secret}", _appCenterSecret.Substring(0, 8) + "...");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenForAnalyticsEvents(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics service");
                await Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Analytics service stopped");
    }

    private async Task ListenForAnalyticsEvents(CancellationToken cancellationToken)
    {
        using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        
        _logger.LogDebug("Waiting for analytics client connection...");
        await server.WaitForConnectionAsync(cancellationToken);
        
        _logger.LogDebug("Analytics client connected");

        using var reader = new StreamReader(server);
        
        while (server.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(message))
                {
                    await ProcessAnalyticsEvent(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing analytics event");
                break; // Break the inner loop to allow reconnection
            }
        }

        _logger.LogDebug("Analytics client disconnected");
    }

    private async Task ProcessAnalyticsEvent(string message)
    {
        try
        {
            var analyticsEvent = JsonSerializer.Deserialize<AnalyticsEventModel>(message);
            
            if (analyticsEvent == null)
            {
                _logger.LogWarning("Failed to deserialize analytics event: {Message}", message);
                return;
            }

            // Send to App Center based on event type
            switch (analyticsEvent.EventName)
            {
                case "Exception":
                    // For exceptions, we could also use Crashes.TrackError if we had the actual exception
                    Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Exception", analyticsEvent.Properties);
                    break;
                
                default:
                    Microsoft.AppCenter.Analytics.Analytics.TrackEvent(analyticsEvent.EventName, analyticsEvent.Properties);
                    break;
            }
            
            _logger.LogDebug("Tracked event: {EventName} with {PropertyCount} properties", 
                analyticsEvent.EventName, analyticsEvent.Properties.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize analytics event: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process analytics event: {Message}", message);
        }
    }
}