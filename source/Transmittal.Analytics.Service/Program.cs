using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Transmittal.Analytics.Service.Services;

namespace Transmittal.Analytics.Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure Serilog for Windows Service
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "Transmittal", "Analytics_Service_Log.json");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .WriteTo.EventLog("Transmittal Analytics Service", manageEventSource: true)
            .CreateLogger();

        try
        {
            Log.Information("Starting Transmittal Analytics Windows Service");

            var builder = Host.CreateApplicationBuilder(args);
            
            // Add Windows Service support
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "TransmittalAnalyticsService";
            });

            // Add Serilog
            builder.Services.AddSerilog();

            // Add the analytics worker service
            builder.Services.AddHostedService<AnalyticsWorker>();

            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Analytics service terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}