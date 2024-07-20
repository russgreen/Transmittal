using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
internal class CommandSettings : ExternalCommand
{

    public override void Execute()
    {
        App.CachedUiApp = Context.UiApplication;
        App.RevitDocument = Context.Document;

        var newView = new Views.SettingsView();
        newView.ShowDialog();
    }
}
