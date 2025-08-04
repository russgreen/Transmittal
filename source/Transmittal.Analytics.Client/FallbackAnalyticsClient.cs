using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// A fallback analytics client that logs events locally when the named pipe service is not available
/// </summary>
public class FallbackAnalyticsClient : IAnalyticsClient, IDisposable
{
    private readonly ILogger<FallbackAnalyticsClient>? _logger;
    private bool _disposed = false;

    public FallbackAnalyticsClient(ILogger<FallbackAnalyticsClient>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tracks a custom event by logging it locally
    /// </summary>
    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        await Task.Run(() =>
        {
            var props = properties ?? new Dictionary<string, string>();
            var propsString = string.Join(", ", props.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            
            _logger?.LogInformation("Analytics Event: {EventName} | Properties: {Properties}", 
                eventName, propsString);
        });
    }

    /// <summary>
    /// Tracks an exception by logging it locally
    /// </summary>
    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        await Task.Run(() =>
        {
            var props = properties ?? new Dictionary<string, string>();
            var propsString = string.Join(", ", props.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            
            _logger?.LogError(exception, "Analytics Exception: {ExceptionType} | Properties: {Properties}", 
                exception.GetType().Name, propsString);
        });
    }

    /// <summary>
    /// Tracks a page view by logging it locally
    /// </summary>
    public async Task TrackPageViewAsync(string pageName, Dictionary<string, string>? properties = null)
    {
        if (_disposed)
        {
            return;
        }

        await Task.Run(() =>
        {
            var props = properties ?? new Dictionary<string, string>();
            props["PageName"] = pageName;
            var propsString = string.Join(", ", props.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            
            _logger?.LogInformation("Analytics PageView: {PageName} | Properties: {Properties}", 
                pageName, propsString);
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}