using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddTransient<IDataConnection, SQLiteDataAccess>()
            .AddTransient<IContactDirectoryService, ContactDirectoryService>()
            .AddTransient<ITransmittalService, TransmittalService>()
            .BuildServiceProvider());
        
        //register the syncfusion license
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");

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
                        var settings = Ioc.Default.GetService<ISettingsService>();
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

                // if the agument is --transmittal then launch the transmittal report view
                if (arg.StartsWith("--transmittal"))
                {
                    TransmittalID = int.Parse(e.Args[0].Substring(e.Args[0].IndexOf("=") + 1));

                    Reports.Reports report = new(Ioc.Default.GetService<ISettingsService>(),
                        Ioc.Default.GetService<IContactDirectoryService>(),
                        Ioc.Default.GetService<ITransmittalService>());

                    report.ShowTransmittalReport(TransmittalID);
                    Current.Shutdown();
                    return;
                }
            }            
        }

        TransmittalView transmittalView = new();
        transmittalView.Show();
        return;
    }



}
