using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows.Forms;
using Transmittal.Analytics.TrayApp.Services;

namespace Transmittal.Analytics.TrayApp;

internal static class Program
{
    private static Mutex? _mutex;
    private static bool _mutexOwned = false;
    private const string _mutexName = "TransmittalAnalyticsTrayApp_SingleInstance";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main(string[] args)
    {
        // Check for single instance using Mutex
        _mutex = new Mutex(true, _mutexName, out bool createdNew);
        _mutexOwned = createdNew;

        if (!createdNew)
        {
            // Another instance is already running
            MessageBox.Show("Transmittal Analytics Tray App is already running.", 
                "Application Already Running", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configure Serilog
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                "Transmittal", "Analytics_TrayApp_Log.json");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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
        finally
        {
            // Only release mutex if we own it
            if (_mutexOwned && _mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                }
                catch (ApplicationException)
                {
                    // Mutex was already released or not owned by this thread
                    // This can happen if the application was closed from the tray
                }
            }
            _mutex?.Dispose();
        }
    }
}