namespace Transmittal.Analytics.Client;
public interface IAnalyticsClient
{
    Task TrackEventAsync(string eventName, Dictionary<string, string> properties = null);
    Task TrackExceptionAsync(Exception exception, Dictionary<string, string> properties = null);
    Task TrackPageViewAsync(string pageName, Dictionary<string, string> properties = null);
}
