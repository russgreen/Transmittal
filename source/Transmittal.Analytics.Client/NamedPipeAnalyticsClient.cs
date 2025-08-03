using System.IO.Pipes;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// Analytics client that sends events to an analytics service via named pipes
/// </summary>
public class NamedPipeAnalyticsClient : IAnalyticsClient, IDisposable
{
    private readonly ILogger<NamedPipeAnalyticsClient>? _logger;
    private const string PipeName = "TransmittalAnalyticsPipe";
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed = false;

    public NamedPipeAnalyticsClient(ILogger<NamedPipeAnalyticsClient>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tracks a custom event with optional properties
    /// </summary>
    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        await SendAnalyticsEvent(eventName, properties ?? new Dictionary<string, string>());
    }

    /// <summary>
    /// Tracks an exception with optional additional properties
    /// </summary>
    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, string>? properties = null)
    {
        var props = properties ?? new Dictionary<string, string>();
        props["ExceptionType"] = exception.GetType().Name;
        props["ExceptionMessage"] = exception.Message;
        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            props["StackTrace"] = exception.StackTrace;
        }

        await SendAnalyticsEvent("Exception", props);
    }

    /// <summary>
    /// Tracks a page view with optional additional properties
    /// </summary>
    public async Task TrackPageViewAsync(string pageName, Dictionary<string, string>? properties = null)
    {
        var props = properties ?? new Dictionary<string, string>();
        props["PageName"] = pageName;

        await SendAnalyticsEvent("PageView", props);
    }

    private async Task SendAnalyticsEvent(string eventName, Dictionary<string, string> properties)
    {
        if (_disposed)
        {
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            var analyticsEvent = new AnalyticsEventModel
            {
                EventName = eventName,
                Properties = properties,
                Timestamp = DateTime.UtcNow,
                Source = "Transmittal"
            };

            var message = JsonSerializer.Serialize(analyticsEvent);

            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

            // Try to connect with timeout
            var connectTask = client.ConnectAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                _logger?.LogWarning("Analytics service connection timeout for event: {EventName}", eventName);
                return;
            }

            using var writer = new StreamWriter(client);
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();

            _logger?.LogDebug("Sent analytics event: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send analytics event: {EventName}", eventName);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore?.Dispose();
            _disposed = true;
        }
    }
}
