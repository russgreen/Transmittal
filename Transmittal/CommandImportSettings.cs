using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Messaging;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text.Json;
using Transmittal.Library.Models;
using Transmittal.Messages;
using Transmittal.Models;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
internal class CommandImportSettings : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;
        App.CachedUiApp = commandData.Application;

        string jsonFilePath;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Transmittal Settings File (*.json)|*.json",
            Title = "Select the Transmittal settings file",
            InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.CheckFileExists)
            {
                jsonFilePath = dialog.FileName;

                string jsonString = File.ReadAllText(jsonFilePath);
                var settings = JsonSerializer.Deserialize<ImportSettingsModel>(jsonString);

                //TODO : perform some checks here to make sure we have a valid JSON.

                var newView = new Views.SettingsView();

                WeakReferenceMessenger.Default.Send(new ImportSettingsMessage(settings));

                newView.ShowDialog();



                return Result.Succeeded;
            }
        }

        return Result.Cancelled;
    }
}
