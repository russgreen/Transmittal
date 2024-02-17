using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandDirectory : IExternalCommand
{
    private readonly ISettingsServiceRvt _settingsServiceRvt = Host.GetService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);

        //get the database file from the current model
        var dbFile = _settingsService.GlobalSettings.DatabaseFile;

        //if database is found the launch the UI
        if(_settingsService.GlobalSettings.RecordTransmittals == false)
        {
            var td = new TaskDialog("Transmittal")
            {
                MainContent = "This project is not configured to store transmittals in a database.  Update settings and try again.",
                CommonButtons = TaskDialogCommonButtons.Close
            };
            td.Show();

            return Result.Cancelled;
        }

#if DEBUG
        var currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPath, @"..\..\..\"));

        var pathToExe = System.IO.Path.Combine(newPath, @$"Transmittal.Desktop\bin\Debug", "Transmittal.Desktop.exe");
#else
        var pathToExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal", "Transmittal.Desktop.exe");
#endif

        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = pathToExe;
        processStartInfo.Arguments = $"--directory \"--database={dbFile}\"";

        Process.Start(processStartInfo);

        return Result.Succeeded;
    }
}
