using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// A resilient analytics client that handles failures gracefully
/// </summary>
internal class ResilientAnalyticsClient : IAnalyticsClient, IDisposable
{
    private readonly NamedPipeAnalyticsClient _namedPipeClient;
    private readonly ILogger? _logger;
    private bool _disposed = false;
    private bool _serviceAvailable = true;

    public ResilientAnalyticsClient(ILogger? logger = null)
    {
        _logger = logger;
        _namedPipeClient = new NamedPipeAnalyticsClient(logger as ILogger<NamedPipeAnalyticsClient>);
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        if (_serviceAvailable)
        {
            try
            {
                await _namedPipeClient.TrackEventAsync(eventName, properties);
                return;
            }
            catch (Exception ex)
            {
                _serviceAvailable = false;
                LogFallback("Event", eventName, properties, ex);
            }
        }
        else
        {
            LogFallback("Event", eventName, properties);
        }
    }

    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        if (_serviceAvailable)
        {
            try
            {
                await _namedPipeClient.TrackExceptionAsync(exception, properties);
                return;
            }
            catch (Exception ex)
            {
                _serviceAvailable = false;
                LogFallback("Exception", exception.GetType().Name, properties, ex, exception);
            }
        }
        else
        {
            LogFallback("Exception", exception.GetType().Name, properties, null, exception);
        }
    }

    public async Task TrackPageViewAsync(string pageName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        var props = properties ?? new Dictionary<string, string>();
        props["PageName"] = pageName;

        if (_serviceAvailable)
        {
            try
            {
                await _namedPipeClient.TrackPageViewAsync(pageName, properties);
                return;
            }
            catch (Exception ex)
            {
                _serviceAvailable = false;
                LogFallback("PageView", pageName, props, ex);
            }
        }
        else
        {
            LogFallback("PageView", pageName, props);
        }
    }

    private void LogFallback(string eventType, string eventName, 
        Dictionary<string, string>? properties, Exception? sendException = null, Exception? originalException = null)
    {
        var props = properties ?? new Dictionary<string, string>();
        var propsString = string.Join(", ", props.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        if (originalException != null)
        {
            _logger?.LogError(originalException, 
                "Analytics {EventType}: {EventName} | Properties: {Properties} | Service Unavailable: {ServiceError}", 
                eventType, eventName, propsString, sendException?.Message ?? "Service not available");
        }
        else
        {
            _logger?.LogInformation(
                "Analytics {EventType}: {EventName} | Properties: {Properties} | Service Unavailable: {ServiceError}", 
                eventType, eventName, propsString, sendException?.Message ?? "Service not available");
        }
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