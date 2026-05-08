using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transmittal.Browser.Services;
using Transmittal.Browser.ViewModels;

namespace Transmittal.Browser;

internal static class Host
{
    private static IHost? _host;

    public static async Task StartHostAsync()
    {
        _host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
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
