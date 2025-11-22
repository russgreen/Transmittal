using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.GoogleAnalytics;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Transmittal.Desktop.Services;
using Transmittal.Desktop.ViewModels;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Helpers;
using Transmittal.Library.Services;

namespace Transmittal.Desktop;
internal static class Host
{
    private static IHost _host;

    public static async Task StartHost()
    {
        var logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Transmittal", "Transmittal_Log.json");
        var analyticsSettings = Transmittal.Library.Helpers.AnalyticsSettingsLoader.LoadAnalyticsSettings();

#if DEBUG
        logPath = "log.json";
#endif
        var userName = Environment.UserName.Replace("\\", "_").Replace("/", "_");
        var machineName = Environment.MachineName.Replace("\\", "_").Replace("/", "_");
        var cultureInfo = Thread.CurrentThread.CurrentCulture;
        var regionInfo = new RegionInfo(cultureInfo.LCID);
        var clientId = ClientIdProvider.GetOrCreateClientId();

        var loggerConfigTransmittal = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("UserDomain", userName)
            .Enrich.WithProperty("MachineName", machineName)
            .Enrich.WithProperty("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString())
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

            // Local file - exclude usage tracking logs
            .WriteTo.Logger(l => l
                .Filter.ByExcluding(le => le.Properties.ContainsKey("UsageTracking"))
                .WriteTo.File(new JsonFormatter(), logPath,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7));

#if !DEBUG
            //write to google analytics
            loggerConfigTransmittal = loggerConfigTransmittal
            .WriteTo.GoogleAnalytics(opts =>
            {
                opts.MeasurementId = "##MEASUREMENTID##";
                opts.ApiSecret = "##APISECRET##";
                opts.ClientId = clientId;

                opts.FlushPeriod = TimeSpan.FromSeconds(1);
                opts.BatchSizeLimit = 1;
                opts.MaxEventsPerRequest = 1;
                opts.IncludePredicate = e => e.Properties.ContainsKey("UsageTracking");

                opts.GlobalParams["app_version"] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                opts.GlobalParams["app_country"] = regionInfo.EnglishName;
                opts.GlobalParams["app_type"] = "Desktop";

                opts.CountryId = regionInfo.TwoLetterISORegionName;
            });
#endif

        if (analyticsSettings.EnableAnalytics)
        {
            // Usage tracking enabled, write to specified path
            var usageLogPath = analyticsSettings.AnalyticsPath;

            if (Directory.Exists(usageLogPath))
            {
                var usageLogFilePath = Path.Combine(usageLogPath, $"Transmittal_{userName}_{machineName}_.json");

                loggerConfigTransmittal = loggerConfigTransmittal
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(le => le.Properties.ContainsKey("UsageTracking"))
                    .WriteTo.Async(a => a.File(new JsonFormatter(), usageLogFilePath,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        rollingInterval: RollingInterval.Day,
                        retainedFileTimeLimit: TimeSpan.FromDays(60)), 1000));
            }
        }

        Log.Logger = loggerConfigTransmittal.CreateLogger();

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
            services.AddSingleton<IMessageBoxService, MessageBoxService>();

            services.AddTransient<IDataConnection, SQLiteDataAccess>();
            services.AddTransient<IContactDirectoryService, ContactDirectoryService>();
            services.AddTransient<ITransmittalService, TransmittalService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<TransmittalViewModel>();
            services.AddTransient<ArchiveViewModel>();
            services.AddTransient<DirectoryViewModel>();
            services.AddTransient<AboutViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ICallingViewModelFactory, CallingViewModelFactory>();

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
