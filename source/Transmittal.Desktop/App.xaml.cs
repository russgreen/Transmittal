using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using Transmittal.Desktop.Services;
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

    async void App_Startup(object sender, StartupEventArgs e)
    {
        //build dependency injection system
        await Host.StartHost();

        //register the syncfusion license
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");

        var settings = Host.GetService<ISettingsService>();

        // enable visual styles for Windows Forms components used in WPF
        System.Windows.Forms.Application.EnableVisualStyles();

        if (e.Args.Length == 0)
        {
            new MainView().Show();
            return;
        }

        //look for the database first
        foreach (string arg in e.Args)
        {               
            if (arg.StartsWith("--database"))
            {
                var databaseFilePath = arg.Substring(arg.IndexOf("=") + 1);
                // set the database filepath string to the value after the --database argument
                if (File.Exists(databaseFilePath.ParsePathWithEnvironmentVariables()))
                {
                    settings.GlobalSettings.DatabaseFile = databaseFilePath;
                    settings.GlobalSettings.RecordTransmittals = true;
                    settings.GetSettings(); 
                }
                else
                {
                    System.Windows.Forms.TaskDialogPage page = new System.Windows.Forms.TaskDialogPage
                    {
                        Caption = "Transmittal Database",
                        Heading = $"{databaseFilePath.ParsePathWithEnvironmentVariables()} was not found",
                        Icon = System.Windows.Forms.TaskDialogIcon.Error,
                        Buttons =
                        {
                            System.Windows.Forms.TaskDialogButton.OK
                        }
                    };

                    // Show the TaskDialog. Since this is a WPF app, no owner window is required here.
                    System.Windows.Forms.TaskDialog.ShowDialog(page);
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

        //handle the action arguments
        foreach (string arg in e.Args)
        {
            switch (arg) 
            {
                case "--directory":
                    new DirectoryView().Show();
                    return;

                case "--archive":
                    new ArchiveView().Show();
                    return;

                case "--about":
                    new AboutView().Show();
                    return;

                default:
                    // if the argument is --transmittal then launch the transmittal report view
                    if (arg.StartsWith("--transmittal"))
                    {
                        TransmittalID = int.Parse(arg.Substring(arg.IndexOf("=") + 1));

                        var report = new ReportsFacade(Host.GetService<ISettingsService>(),
        Host.GetService<IContactDirectoryService>(),
        Host.GetService<ITransmittalService>(),
        Host.GetService<ILogger<App>>());

                        report.ShowTransmittalReport(TransmittalID);
                        Current.Shutdown();
                        return;
                    }

                    if (arg.StartsWith("--filetransfer"))
                    {
                        settings.GetSettings();

                        var manifestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Transmittal", "transmittal-manifest.json");
                        List<string> filesList = new();

                        if (File.Exists(manifestPath))
                        {
                            try
                            {
                                var manifestContent = File.ReadAllText(manifestPath);
                                filesList = JsonSerializer.Deserialize<List<string>>(manifestContent) ?? new List<string>();

                                var fileTransferService = Host.GetService<IFileTransferService>();

                                if (filesList.Count > 0)
                                {
                                    await fileTransferService.PrepareFileTransferUploadAsync(manifestPath);
                                }
                            }
                            catch
                            {
                                //filesList = new List<string>();
                            }
                            finally
                            {
                                //File.Delete(manifestPath);
                            }
                        }

                        Current.Shutdown();
                        return;
                    }

                    break;
            }



        }


        new MainView().Show();
        return;
    }



}
