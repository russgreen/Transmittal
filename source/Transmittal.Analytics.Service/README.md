# Transmittal Analytics Service

## Overview

The Transmittal Analytics Service is a Windows Service that runs in the background to collect and forward analytics data from the Transmittal application to Microsoft App Center. It uses named pipes for inter-process communication to receive analytics events from the main Transmittal applications.

## Features

- **Windows Service**: Runs as a background service with automatic startup
- **Named Pipe Server**: Listens on `TransmittalAnalyticsPipe` for analytics events
- **App Center Integration**: Forwards events to Microsoft App Center Analytics
- **Robust Error Handling**: Gracefully handles disconnections and errors
- **Structured Logging**: Uses Serilog for comprehensive logging to files and Event Log
- **Configuration Support**: Supports appsettings.json and user secrets for configuration

## Architecture

```
???????????????????    Named Pipe     ???????????????????????    HTTPS     ???????????????????
?  Transmittal    ? ????????????????  ?  Analytics Service  ? ???????????? ?   App Center    ?
?  Applications   ?                   ?  (Windows Service)  ?              ?   Analytics     ?
???????????????????                   ???????????????????????              ???????????????????
```

## Installation

### Prerequisites

- Windows 10/11 or Windows Server 2016+
- .NET 8 Runtime
- Administrator privileges for service installation

### Install the Service

1. Build and publish the service:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

2. Copy the published files to your desired location (e.g., `C:\Program Files\Transmittal\Analytics\`)

3. Run the installation script as Administrator:
   ```cmd
   install-service.bat
   ```

   Or manually install using PowerShell:
   ```powershell
   sc.exe create "TransmittalAnalyticsService" binPath="C:\Path\To\Transmittal.Analytics.Service.exe" start=auto
   sc.exe start "TransmittalAnalyticsService"
   ```

### Uninstall the Service

Run the uninstall script as Administrator:
```cmd
uninstall-service.bat
```

Or manually remove using PowerShell:
```powershell
sc.exe stop "TransmittalAnalyticsService"
sc.exe delete "TransmittalAnalyticsService"
```

## Configuration

### App Center Secret

Configure your App Center secret in one of the following ways:

1. **appsettings.json**:
   ```json
   {
     "AppCenter": {
       "Secret": "your-app-center-secret-here"
     }
   }
   ```

2. **User Secrets** (for development):
   ```bash
   dotnet user-secrets set "AppCenter:Secret" "your-app-center-secret-here"
   ```

3. **Environment Variable**:
   ```cmd
   setx AppCenter__Secret "your-app-center-secret-here" /M
   ```

### Service Configuration

The service can be configured via `appsettings.json`:

```json
{
  "Service": {
    "PipeName": "TransmittalAnalyticsPipe",
    "MaxConcurrentConnections": 10,
    "ReconnectDelaySeconds": 10
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Logging

The service logs to multiple outputs:

- **Console**: During development and debugging
- **File**: `%ProgramData%\Transmittal\Analytics_Service_Log.json`
- **Windows Event Log**: Under "Transmittal Analytics Service"

## Monitoring

### Service Status

Check if the service is running:
```cmd
sc query "TransmittalAnalyticsService"
```

### Event Log

View service events in Windows Event Viewer:
1. Open Event Viewer
2. Navigate to Windows Logs > Application
3. Filter by Source: "Transmittal Analytics Service"

### Log Files

Service logs are written to:
```
%ProgramData%\Transmittal\Analytics_Service_Log.json
```

## Troubleshooting

### Service Won't Start

1. Check Windows Event Log for error details
2. Verify .NET 8 Runtime is installed
3. Ensure service executable exists and has proper permissions
4. Check if port/pipe name conflicts exist

### No Analytics Data

1. Verify App Center secret is configured correctly
2. Check network connectivity to App Center
3. Verify client applications are sending events to the correct pipe name
4. Review service logs for connection issues

### Performance Issues

1. Monitor named pipe connection count
2. Check for memory leaks in long-running operations
3. Review log file size and rotation settings

## Development

### Building

```bash
dotnet build
```

### Running Locally

```bash
dotnet run
```

### Testing

The service can be tested without installing it as a Windows Service by running it directly. It will run as a console application in development mode.

## Security Considerations

- The service runs under the Local System account by default
- Named pipes are local to the machine (no network exposure)
- App Center communication uses HTTPS
- Consider running under a dedicated service account for enhanced security

## Related Projects

- **Transmittal.Analytics.Client**: Client library for sending analytics events
- **Transmittal.Analytics.TrayApp**: Alternative system tray application implementation
- **Transmittal**: Main Revit add-in
- **Transmittal.Desktop**: Standalone desktop application