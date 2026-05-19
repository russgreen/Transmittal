using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;

namespace Transmittal.Browser;

public partial class App : Application
{
    private ILogger<App>? _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += AppDispatcherUnhandledException;

        await Host.StartHostAsync();
        _logger = Host.GetService<ILogger<App>>();
        _logger.LogInformation("Transmittal.Browser startup complete.");

        var mainWindow = Host.GetService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("Transmittal.Browser shutting down.");
        await Host.StopHostAsync();
        base.OnExit(e);
    }

    private void AppDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Unhandled UI exception in browser app.");
    }
}

