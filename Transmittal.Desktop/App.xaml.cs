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
        //build dependancy injection system
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
                        TaskDialogButton okButon = new TaskDialogButton(ButtonType.Ok);

                        TaskDialog dialog = new TaskDialog()
                        {
                            WindowTitle = "Transmittal Database",
                            MainInstruction = @$"{databaseFilePath.ParsePathWithEnvironmentVariables()} was not found",
                            MainIcon = TaskDialogIcon.Error,
                            ButtonStyle = TaskDialogButtonStyle.Standard,
                            Buttons = { okButon }
                        };

                        dialog.ShowDialog();
                        Current.Shutdown();
                    }
                }
            }
            
            foreach (string arg in e.Args)
            {
                // if the agument is --directory then launch the directory view
                if (arg == "--directory")
                {
                    DirectoryView directoryView = new();
                    directoryView.Show();
                    return;
                }

                // if the agument is --archive then launch the archive view 
                if (arg == "--archive")
                {
                    ArchiveView archiveView = new();
                    archiveView.Show();
                    return;
                }

                // if the agument is --about then launch the about view 
                if (arg == "--about")
                {
                    AboutView aboutView = new();
                    aboutView.Show();
                    return;
                }

                // if the agument is --transmittal then launch the transmittal report view
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

        if(settings.GlobalSettings.DatabaseFile == "[NONE]" || settings.GlobalSettings.DatabaseFile == string.Empty)
        {
            //not using ookii file dilaog because it doesn't implement initialDirectory correctly
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Select the Project Transmittal Database  File",
                InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialog.CheckFileExists)
                {
                    settings.GlobalSettings.DatabaseFile = dialog.FileName;
                    settings.GlobalSettings.RecordTransmittals = true;
                    settings.GetSettings();
                }
            }

        }

        MainView mainView = new();
        mainView.Show();
        return;
    }



}
