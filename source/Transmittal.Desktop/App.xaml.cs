using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using Transmittal.Desktop.Views;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Extensions;
using Transmittal.Library.Services;

namespace Transmittal.Desktop;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static int TransmittalID;
        
    void App_Startup(object sender, StartupEventArgs e)
    {
        //build dependency injection system
        Host.StartHost().Wait();

        //register the syncfusion license
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");

        var settings = Host.GetService<ISettingsService>();

        // If no command line arguments were provided, don't process them if (e.Args.Length == 0) return;  
        if (e.Args.Length > 0)
        {
            //look for the database first
            foreach (string arg in e.Args)
            {               
                if (arg.StartsWith("--database"))
                {
                    string databaseFilePath = arg.Substring(arg.IndexOf("=") + 1);
                    // set the database filepath string to the value after the --database argument
                    if (File.Exists(databaseFilePath.ParsePathWithEnvironmentVariables()))
                    {
                        settings.GlobalSettings.DatabaseFile = databaseFilePath;
                        settings.GlobalSettings.RecordTransmittals = true;
                        settings.GetSettings(); 
                    }
                    else
                    {
                        TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);

                        TaskDialog dialog = new TaskDialog()
                        {
                            WindowTitle = "Transmittal Database",
                            MainInstruction = @$"{databaseFilePath.ParsePathWithEnvironmentVariables()} was not found",
                            MainIcon = TaskDialogIcon.Error,
                            ButtonStyle = TaskDialogButtonStyle.Standard,
                            Buttons = { okButton }
                        };

                        dialog.ShowDialog();
                        Current.Shutdown();
                    }
                }
            }

            //look for the database if just the path has been provided
            foreach (string arg in e.Args)
            {
                if (arg.EndsWith(".tdb"))
                {
                    if (File.Exists(arg))
                    {
                        settings.GlobalSettings.DatabaseFile = arg;
                        settings.GlobalSettings.RecordTransmittals = true;
                        settings.GetSettings();
                    }
                }
            }

            foreach (string arg in e.Args)
            {
                // if the argument is --directory then launch the directory view
                if (arg == "--directory")
                {
                    DirectoryView directoryView = new();
                    directoryView.Show();
                    return;
                }

                // if the argument is --archive then launch the archive view 
                if (arg == "--archive")
                {
                    ArchiveView archiveView = new();
                    archiveView.Show();
                    return;
                }

                // if the argument is --about then launch the about view 
                if (arg == "--about")
                {
                    AboutView aboutView = new();
                    aboutView.Show();
                    return;
                }

                // if the argument is --transmittal then launch the transmittal report view
                if (arg.StartsWith("--transmittal"))
                {
                    TransmittalID = int.Parse(e.Args[0].Substring(e.Args[0].IndexOf("=") + 1));

                    Reports.Reports report = new(Host.GetService<ISettingsService>(),
    Host.GetService<IContactDirectoryService>(),
    Host.GetService<ITransmittalService>());

                    report.ShowTransmittalReport(TransmittalID);
                    Current.Shutdown();
                    return;
                }
            }            
        }

        MainView mainView = new();
        mainView.Show();
        return;
    }



}
