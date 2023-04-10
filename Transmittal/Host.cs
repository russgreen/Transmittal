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

        public static void StartHost()
        {
            _host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyLocation = assembly.Location;
                var addinVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
#if RELEASE
                //var version = addinVersion.Split('.')[0];
                //if (version == "1") version = "Develop";
                //var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //var userDataLocation = Path.Combine(programDataPath, @"Autodesk\Revit\Addins\", version, "RevitLookup");
                var userDataLocation = Path.GetDirectoryName(assemblyLocation)!;
#else
                var userDataLocation = Path.GetDirectoryName(assemblyLocation)!;
#endif
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                var targetFrameworkAttributes = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), true);
                var targetFrameworkAttribute = (TargetFrameworkAttribute)targetFrameworkAttributes.First();
                var targetFramework = targetFrameworkAttribute.FrameworkDisplayName;

                builder.AddInMemoryCollection(new KeyValuePair<string, string>[]
                {
                    new("Assembly", assemblyLocation),
                    new("Framework", targetFramework),
                    new("AddinVersion", addinVersion),
                    new("ConfigFolder", Path.Combine(userDataLocation, "Config")),
                    new("DownloadFolder", Path.Combine(userDataLocation, "Downloads")),
                });
            })
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

            _host.Start();
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
