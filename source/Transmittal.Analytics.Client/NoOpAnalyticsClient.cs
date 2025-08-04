using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// A no-operation analytics client that does nothing. Useful for testing or when analytics is disabled.
/// </summary>
public class NoOpAnalyticsClient : IAnalyticsClient
{
    private readonly ILogger<NoOpAnalyticsClient>? _logger;

    public NoOpAnalyticsClient(ILogger<NoOpAnalyticsClient>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Does nothing and returns a completed task
    /// </summary>
    public Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        _logger?.LogDebug("NoOp Analytics Event: {EventName}", eventName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing and returns a completed task
    /// </summary>
    public Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        _logger?.LogDebug("NoOp Analytics Exception: {ExceptionType}", exception.GetType().Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing and returns a completed task
    /// </summary>
    public Task TrackPageViewAsync(string pageName, Dictionary<string, string>? properties = null)
    {
        _logger?.LogDebug("NoOp Analytics PageView: {PageName}", pageName);
        return Task.CompletedTask;
    }
}