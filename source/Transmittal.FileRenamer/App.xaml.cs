using Microsoft.Extensions.Hosting;
using System.Windows;
using Transmittal.FileRenamer.Views;

namespace Transmittal.FileRenamer;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string[] Args;
    async void App_Startup(object sender, StartupEventArgs e)
    {
        //build dependency injection system
        await Host.StartHost();

        // If no command line arguments were provided, don't process them if (e.Args.Length == 0) return;  
        if (e.Args.Length > 0)
        {
            Args = e.Args;

            MainView mainView = new();
            mainView.Show();
        }

        return;
    }
}

