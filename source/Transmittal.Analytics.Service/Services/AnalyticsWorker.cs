using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;
using System.Text.Json;
using Transmittal.Analytics.Client;

namespace Transmittal.Analytics.Service.Services;

/// <summary>
/// Background worker service that listens for analytics events via named pipes and forwards them to App Center
/// </summary>
public class AnalyticsWorker : BackgroundService
{
    private readonly ILogger<AnalyticsWorker> _logger;
    private readonly IConfiguration _configuration;
    private const string PipeName = "TransmittalAnalyticsPipe";

    public AnalyticsWorker(ILogger<AnalyticsWorker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        InitializeAppCenter();
    }

    private void InitializeAppCenter()
    {
        // Get App Center secret from configuration (appsettings.json, user secrets, or environment variables)
        var appCenterSecret = _configuration["AppCenter:Secret"] ?? "YOUR_APP_CENTER_SECRET_HERE";
        
        if (!AppCenter.Configured && appCenterSecret != "YOUR_APP_CENTER_SECRET_HERE")
        {
            AppCenter.Start(appCenterSecret, typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Crashes));
            _logger.LogInformation("App Center initialized successfully");
        }
        else
        {
            _logger.LogWarning("App Center not configured - secret not provided or invalid");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics worker service starting...");

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
                _logger.LogError(ex, "Error in analytics worker service");
                
                // Wait before retrying to avoid tight error loops
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("Analytics worker service stopped");
    }

    private async Task ListenForAnalyticsEvents(CancellationToken cancellationToken)
    {
        using var server = new NamedPipeServerStream(
            PipeName, 
            PipeDirection.In, 
            10, // Max concurrent connections
            PipeTransmissionMode.Byte, 
            PipeOptions.Asynchronous);
        
        _logger.LogDebug("Waiting for analytics client connection on pipe: {PipeName}", PipeName);
        
        try
        {
            await server.WaitForConnectionAsync(cancellationToken);
            _logger.LogDebug("Analytics client connected");

            using var reader = new StreamReader(server);
            
            while (server.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await reader.ReadLineAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(message))
                    {
                        await ProcessAnalyticsEvent(message);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading from named pipe");
                    break; // Break the inner loop to allow reconnection
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with named pipe server");
        }
        finally
        {
            _logger.LogDebug("Analytics client disconnected");
        }
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

            // Validate the event
            if (string.IsNullOrEmpty(analyticsEvent.EventName))
            {
                _logger.LogWarning("Received analytics event with empty event name");
                return;
            }

            // Add service metadata
            var properties = analyticsEvent.Properties ?? new Dictionary<string, string>();
            properties["ServiceVersion"] = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown";
            properties["ProcessedAt"] = DateTime.UtcNow.ToString("o");

            // Send to App Center if configured
            if (AppCenter.Configured)
            {
                switch (analyticsEvent.EventName.ToLowerInvariant())
                {
                    case "exception":
                        // For exceptions, use regular TrackEvent since we don't have the actual Exception object
                        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Exception", properties);
                        break;
                    
                    default:
                        Microsoft.AppCenter.Analytics.Analytics.TrackEvent(analyticsEvent.EventName, properties);
                        break;
                }
                
                _logger.LogDebug("Tracked event: {EventName} with {PropertyCount} properties", 
                    analyticsEvent.EventName, properties.Count);
            }
            else
            {
                _logger.LogInformation("Analytics event logged locally (App Center not configured): {EventName}", 
                    analyticsEvent.EventName);
            }
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics worker service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}