using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.IO;
using Transmittal.Browser.Services;
using Transmittal.Browser.ViewModels;

namespace Transmittal.Browser;

internal static class Host
{
    private static IHost? _host;

    public static async Task StartHostAsync()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Transmittal",
            "Transmittal.Browser_Log.json");

#if DEBUG
        logPath = "browser-log.json";
#endif

        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var userName = Environment.UserName.Replace("\\", "_").Replace("/", "_");
        var machineName = Environment.MachineName.Replace("\\", "_").Replace("/", "_");

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("UserDomain", userName)
            .Enrich.WithProperty("MachineName", machineName)
            .Enrich.WithProperty("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString())
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
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IBrowserLaunchOptionsProvider, BrowserLaunchOptionsProvider>();
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();
    }

    public static async Task StopHostAsync()
    {
        if (_host is null)
        {
            return;
        }

        await _host.StopAsync();
        _host.Dispose();
        _host = null;
    }

    public static T GetService<T>() where T : class
    {
        return _host?.Services.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} has not been registered.");
    }
}
