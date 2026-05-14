using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External;
using Serilog.Context;
using Transmittal.Exceptions;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandSettings : ExternalCommand
{
    private readonly ILogger<CommandSettings> _logger = Host.GetService<ILogger<CommandSettings>>();

    public override void Execute()
    {
        using (LogContext.PushProperty("UsageTracking", true))
        {
            _logger.LogInformation("{command}", nameof(CommandSettings));
        }

        App.CachedUiApp = RevitContext.UiApplication;
        App.RevitDocument = RevitContext.ActiveDocument;

        try
        {
            var newView = new Views.SettingsView();
            newView.ShowDialog();
        }
        catch (SchemaVersionTooNewException ex)
        {
            // Newer schema detected - user needs to upgrade app, don't show settings
            _logger.LogError(ex, "Document schema version too new");
            return;
        }
    }
}
