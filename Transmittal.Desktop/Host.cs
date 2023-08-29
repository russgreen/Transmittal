using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Services;

namespace Transmittal.Desktop;
internal static class Host
{
    private static IHost _host;

    public static async Task StartHost()
    {
        var logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Transmittal", "Transmittal_Log.json");

#if DEBUG
        logPath = "log.json";
#endif

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(new JsonFormatter(), logPath,
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        _host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .UseSerilog()
        .ConfigureAppConfiguration(builder =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyLocation = assembly.Location;
            var softwareVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;

            var downloadFolder = Transmittal.Library.Helpers.GetDownloadsFolder.GetDownloadsPath();

            builder.AddInMemoryCollection(new KeyValuePair<string, string>[]
            {
                    new("Assembly", assemblyLocation),
                    new("SoftwareVersion", softwareVersion),
                    new("DownloadFolder", downloadFolder)
            });
        })
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ISoftwareUpdateService, SoftwareUpdateService>();

            services.AddTransient<IDataConnection, SQLiteDataAccess>();
            services.AddTransient<IContactDirectoryService, ContactDirectoryService>();
            services.AddTransient<ITransmittalService, TransmittalService>();
        })
        .Build();

        await _host.StartAsync();
    }

    public static async Task StartHost(IHost host)
    {
        _host = host;
        await host.StartAsync();
    }

    public static async Task StopHost()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }
}
