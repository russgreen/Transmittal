using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal
{
    internal static class Host
    {
        private static IHost _host;

        public static async Task StartHost()
        {
            _host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ISettingsService, SettingsService>();

                services.AddTransient<ISettingsServiceRvt, SettingsServiceRvt>();
                services.AddTransient<IDataConnection, SQLiteDataAccess>();
                services.AddTransient<IExportPDFService, ExportPDFService>();
                services.AddTransient<IExportDWGService, ExportDWGService>();
                services.AddTransient<IExportDWFService, ExportDWFService>();
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
}
