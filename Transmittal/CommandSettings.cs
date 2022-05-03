using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
internal class CommandSettings : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        var form = new Forms.FormSettings(commandData);
        form.ShowDialog(new WindowHandle(commandData.Application.MainWindowHandle));

        return Result.Succeeded;
    }
}
