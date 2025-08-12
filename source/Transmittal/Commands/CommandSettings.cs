using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External;
using Serilog.Context;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandSettings : ExternalCommand
{
    private readonly ILogger<CommandSettings> _logger = Host.GetService<ILogger<CommandSettings>>();

    public override void Execute()
    {
        using (LogContext.PushProperty("UsageTracking", true))
        using (LogContext.PushProperty("RevitVersion", App.CtrApp.VersionNumber))
        {
            _logger.LogInformation("{command}", nameof(CommandSettings));
        }

        App.CachedUiApp = Context.UiApplication;
        App.RevitDocument = Context.ActiveDocument;

        var newView = new Views.SettingsView();
        newView.ShowDialog();
    }
}
