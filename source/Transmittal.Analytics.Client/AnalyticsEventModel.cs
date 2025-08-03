namespace Transmittal.Analytics.Client;

/// <summary>
/// Represents an analytics event to be sent to the analytics service
/// </summary>
public class AnalyticsEventModel
{
    /// <summary>
    /// The name of the event to track
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties to include with the event
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The source application that generated the event
    /// </summary>
    public string Source { get; set; } = "Transmittal";
}