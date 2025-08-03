using Microsoft.Extensions.Logging;

namespace Transmittal.Analytics.Client;

/// <summary>
/// Factory for creating analytics clients with appropriate fallback behavior
/// </summary>
public static class AnalyticsClientFactory
{
    /// <summary>
    /// Creates an analytics client that attempts named pipe communication with fallback to local logging
    /// </summary>
    /// <param name="logger">Optional logger for the analytics client</param>
    /// <returns>An analytics client instance</returns>
    public static IAnalyticsClient CreateClient(ILogger? logger = null)
    {
        return new ResilientAnalyticsClient(logger);
    }

    /// <summary>
    /// Creates a named pipe only analytics client (no fallback)
    /// </summary>
    /// <param name="logger">Optional logger for the analytics client</param>
    /// <returns>A named pipe analytics client instance</returns>
    public static IAnalyticsClient CreateNamedPipeClient(ILogger? logger = null)
    {
        return new NamedPipeAnalyticsClient(logger as ILogger<NamedPipeAnalyticsClient>);
    }
}
