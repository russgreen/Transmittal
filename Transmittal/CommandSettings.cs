using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using Transmittal.Services;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
internal class CommandSettings : IExternalCommand
{
   
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        var newView = new Views.SettingsView();
        newView.ShowDialog();

        return Result.Succeeded;
    }
}
