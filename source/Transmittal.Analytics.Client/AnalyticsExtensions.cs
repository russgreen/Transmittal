namespace Transmittal.Analytics.Client;

/// <summary>
/// Extension methods for IAnalyticsClient to provide commonly used tracking scenarios
/// </summary>
public static class AnalyticsExtensions
{
    /// <summary>
    /// Tracks a transmittal started event
    /// </summary>
    public static async Task TrackTransmittalStartedAsync(this IAnalyticsClient client, 
        int sheetCount, string[] exportFormats, bool recordTransmittal = false)
    {
        var properties = new Dictionary<string, string>
        {
            ["SheetCount"] = sheetCount.ToString(),
            ["ExportFormats"] = string.Join(",", exportFormats),
            ["RecordTransmittal"] = recordTransmittal.ToString()
        };

        await client.TrackEventAsync("TransmittalStarted", properties);
    }

    /// <summary>
    /// Tracks a transmittal completed event
    /// </summary>
    public static async Task TrackTransmittalCompletedAsync(this IAnalyticsClient client, 
        int sheetCount, string[] exportFormats, TimeSpan duration, bool cancelled = false)
    {
        var properties = new Dictionary<string, string>
        {
            ["SheetCount"] = sheetCount.ToString(),
            ["ExportFormats"] = string.Join(",", exportFormats),
            ["Duration"] = duration.TotalSeconds.ToString("F2"),
            ["Cancelled"] = cancelled.ToString()
        };

        await client.TrackEventAsync("TransmittalCompleted", properties);
    }

    /// <summary>
    /// Tracks export format usage
    /// </summary>
    public static async Task TrackExportFormatAsync(this IAnalyticsClient client, 
        string format, int sheetCount, bool success = true)
    {
        var properties = new Dictionary<string, string>
        {
            ["Format"] = format,
            ["SheetCount"] = sheetCount.ToString(),
            ["Success"] = success.ToString()
        };

        await client.TrackEventAsync("ExportFormat", properties);
    }

    /// <summary>
    /// Tracks feature usage
    /// </summary>
    public static async Task TrackFeatureUsageAsync(this IAnalyticsClient client, 
        string featureName, Dictionary<string, string>? additionalProperties = null)
    {
        var properties = additionalProperties ?? new Dictionary<string, string>();
        properties["FeatureName"] = featureName;

        await client.TrackEventAsync("FeatureUsage", properties);
    }

    /// <summary>
    /// Tracks user workflow patterns
    /// </summary>
    public static async Task TrackWorkflowStepAsync(this IAnalyticsClient client, 
        string workflowName, string stepName, int stepNumber, int totalSteps)
    {
        var properties = new Dictionary<string, string>
        {
            ["WorkflowName"] = workflowName,
            ["StepName"] = stepName,
            ["StepNumber"] = stepNumber.ToString(),
            ["TotalSteps"] = totalSteps.ToString(),
            ["Progress"] = ((double)stepNumber / totalSteps * 100).ToString("F1")
        };

        await client.TrackEventAsync("WorkflowStep", properties);
    }

    /// <summary>
    /// Tracks performance metrics
    /// </summary>
    public static async Task TrackPerformanceAsync(this IAnalyticsClient client, 
        string operationName, TimeSpan duration, bool success = true, string? errorMessage = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["Operation"] = operationName,
            ["Duration"] = duration.TotalMilliseconds.ToString("F2"),
            ["Success"] = success.ToString()
        };

        if (!string.IsNullOrEmpty(errorMessage))
        {
            properties["ErrorMessage"] = errorMessage;
        }

        await client.TrackEventAsync("Performance", properties);
    }
}