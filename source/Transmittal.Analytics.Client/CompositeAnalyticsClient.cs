using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// A composite analytics client that tries named pipe communication first, then falls back to local logging
/// </summary>
public class CompositeAnalyticsClient : IAnalyticsClient, IDisposable
{
    private readonly NamedPipeAnalyticsClient _namedPipeClient;
    private readonly ILogger<CompositeAnalyticsClient>? _logger;
    private bool _disposed = false;

    public CompositeAnalyticsClient(ILogger<CompositeAnalyticsClient>? logger = null)
    {
        _logger = logger;
        _namedPipeClient = new NamedPipeAnalyticsClient(logger as ILogger<NamedPipeAnalyticsClient>);
    }

    /// <summary>
    /// Tracks a custom event, trying named pipe first, then falling back to logging
    /// </summary>
    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await _namedPipeClient.TrackEventAsync(eventName, properties);
        }
        catch (Exception ex)
        {
            // Fallback to local logging
            await LogEventLocally("Event", eventName, properties, ex);
        }
    }

    /// <summary>
    /// Tracks an exception, trying named pipe first, then falling back to logging
    /// </summary>
    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        if (_disposed) return;

        try
        {
            await _namedPipeClient.TrackExceptionAsync(exception, properties);
        }
        catch (Exception ex)
        {
            // Fallback to local logging
            await LogEventLocally("Exception", exception.GetType().Name, properties, ex, exception);
        }
    }

    /// <summary>
    /// Tracks a page view, trying named pipe first, then falling back to logging
    /// </summary>
    public async Task TrackPageViewAsync(string pageName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await _namedPipeClient.TrackPageViewAsync(pageName, properties);
        }
        catch (Exception ex)
        {
            // Fallback to local logging
            var props = properties ?? new Dictionary<string, string>();
            props["PageName"] = pageName;
            await LogEventLocally("PageView", pageName, props, ex);
        }
    }

    private async Task LogEventLocally(string eventType, string eventName, 
        Dictionary<string, string>? properties, Exception? sendException, Exception? originalException = null)
    {
        await Task.Run(() =>
        {
            var props = properties ?? new Dictionary<string, string>();
            var propsString = string.Join(", ", props.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            if (originalException != null)
            {
                _logger?.LogError(originalException, 
                    "Analytics {EventType} (fallback): {EventName} | Properties: {Properties} | Send Error: {SendError}", 
                    eventType, eventName, propsString, sendException?.Message);
            }
            else
            {
                _logger?.LogInformation(
                    "Analytics {EventType} (fallback): {EventName} | Properties: {Properties} | Send Error: {SendError}", 
                    eventType, eventName, propsString, sendException?.Message);
            }
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _namedPipeClient?.Dispose();
            _disposed = true;
        }
    }
}