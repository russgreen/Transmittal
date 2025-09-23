﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External;
using Serilog.Context;
using System.Diagnostics;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandTransmittalsArchive : ExternalCommand
{
    private readonly ISettingsServiceRvt _settingsServiceRvt = Host.GetService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();
    private readonly ILogger<CommandTransmittalsArchive> _logger = Host.GetService<ILogger<CommandTransmittalsArchive>>();

    public override void Execute()
    {
        using (LogContext.PushProperty("UsageTracking", true))
        {
            _logger.LogInformation("{command}", nameof(CommandTransmittalsArchive));
        }

        App.CachedUiApp = Context.UiApplication;
        App.RevitDocument = Context.ActiveDocument;

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);

         //get the database file from the current model
        var dbFile = _settingsService.GlobalSettings.DatabaseFile;

        //if database is found the launch the UI
        if (_settingsService.GlobalSettings.RecordTransmittals == false)
        {
            var td = new TaskDialog("Transmittal")
            {
                MainContent = "This project is not configured to store transmittals in a database.  Update settings and try again.",
                CommonButtons = TaskDialogCommonButtons.Close
            };
            td.Show();

            return;
        }

#if DEBUG
        var currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPath, @"..\..\..\"));

        var pathToExe = System.IO.Path.Combine(newPath, @$"Transmittal.Desktop\bin\Debug", "Transmittal.Desktop.exe");
#else
        var pathToExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal", "Transmittal.Desktop.exe");
#endif

        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = pathToExe,
            Arguments = $"--archive \"--database={dbFile}\""
        };

        Process.Start(processStartInfo);
    }
}
