# Transmittal Analytics Client

## Overview

The `Transmittal.Analytics.Client` library provides a robust analytics solution for the Transmittal application using named pipes for inter-process communication. This approach solves the singleton service limitations in Revit environments.

## Architecture

- **NamedPipeAnalyticsClient**: Main client that communicates with the analytics service via named pipes
- **IAnalyticsClient**: Interface defining the analytics contract
- **AnalyticsEventModel**: Data model for analytics events

## Features

- Non-blocking analytics calls
- Timeout handling for service unavailability
- Thread-safe implementation using SemaphoreSlim
- Structured logging support
- JSON serialization for event data
- Exception tracking with stack traces
- Page view tracking
- Custom event properties

## Usage

### Basic Setup

```csharp
// In your Host.cs or DI container setup
services.AddSingleton<IAnalyticsClient, NamedPipeAnalyticsClient>();
```

### Tracking Events

```csharp
var analyticsClient = Host.GetService<IAnalyticsClient>();

// Track a simple event
await analyticsClient.TrackEventAsync("TransmittalStarted");

// Track an event with properties
await analyticsClient.TrackEventAsync("TransmittalCompleted", new Dictionary<string, string>
{
    ["SheetCount"] = "25",
    ["ExportFormat"] = "PDF",
    ["Duration"] = "120.5"
});
```

### Tracking Exceptions

```csharp
try
{
    // Your code here
}
catch (Exception ex)
{
    await analyticsClient.TrackExceptionAsync(ex, new Dictionary<string, string>
    {
        ["Operation"] = "ExportPDF",
        ["SheetNumber"] = "A101"
    });
}
```

### Tracking Page Views

```csharp
await analyticsClient.TrackPageViewAsync("ProgressView", new Dictionary<string, string>
{
    ["TotalSheets"] = "25",
    ["CurrentStep"] = "Export"
});
```

## Integration Example

Here's how to integrate the analytics client into your ViewModels:

```csharp
public class ProgressViewModel : BaseViewModel
{
    private readonly IAnalyticsClient _analyticsClient;
    
    public ProgressViewModel()
    {
        _analyticsClient = Host.GetService<IAnalyticsClient>();
        
        // Track when progress view is opened
        _ = Task.Run(async () => await _analyticsClient.TrackPageViewAsync("ProgressView"));
    }
    
    [RelayCommand]
    public async void CancelTransmittal()
    {
        await _analyticsClient.TrackEventAsync("TransmittalCancelled", new Dictionary<string, string>
        {
            ["SheetsProcessed"] = DrawingSheetsProcessed.ToString(),
            ["TotalSheets"] = DrawingSheetsToProcess.ToString()
        });
        
        // Rest of cancel logic...
    }
}
```

## Configuration

The client uses the named pipe `"TransmittalAnalyticsPipe"` by default. The connection timeout is set to 5 seconds to prevent blocking the UI.

## Error Handling

The client is designed to fail gracefully:
- Connection timeouts are logged but don't throw exceptions
- JSON serialization errors are caught and logged
- Named pipe communication failures are handled silently

## Thread Safety

The implementation is thread-safe using:
- `SemaphoreSlim` for controlling concurrent access
- Proper disposal patterns
- Safe async/await patterns

## Performance Considerations

- Events are sent asynchronously to avoid blocking the UI
- JSON serialization is lightweight
- Named pipes provide efficient local IPC
- Connection pooling is handled by the .NET framework

## Dependencies

- `Microsoft.Extensions.Logging.Abstractions` - For structured logging
- `System.Text.Json` - For JSON serialization
- `System.IO.Pipes` - For named pipe communication
