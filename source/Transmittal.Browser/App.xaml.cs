using System.Windows;

namespace Transmittal.Browser;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await Host.StartHostAsync();

        var mainWindow = Host.GetService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await Host.StopHostAsync();
        base.OnExit(e);
    }
}

