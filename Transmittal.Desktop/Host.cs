using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        _host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
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
