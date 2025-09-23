using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External;
using Serilog.Context;
using System.Diagnostics;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandAbout : ExternalCommand
{
    private readonly ILogger<CommandAbout> _logger = Host.GetService<ILogger<CommandAbout>>();
    public override void Execute()
    {
        using (LogContext.PushProperty("UsageTracking", true))
        {
            _logger.LogInformation("{command}", nameof(CommandAbout));
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
        processStartInfo.Arguments = $"--about";

        Process.Start(processStartInfo);
    }
}
