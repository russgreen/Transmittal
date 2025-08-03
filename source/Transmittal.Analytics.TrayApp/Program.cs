using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows.Forms;
using Transmittal.Analytics.TrayApp.Services;

namespace Transmittal.Analytics.TrayApp;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Configure Serilog
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "Transmittal", "Analytics_TrayApp_Log.json");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            Log.Information("Starting Transmittal Analytics Tray Application");

            // Create the host
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<TrayApplicationContext>();
                    services.AddHostedService<AnalyticsService>();
                })
                .Build();

            // Start the host
            await host.StartAsync();

            // Get the tray context and run the application
            var trayContext = host.Services.GetRequiredService<TrayApplicationContext>();
            Application.Run(trayContext);

            // Stop the host when the application exits
            await host.StopAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}