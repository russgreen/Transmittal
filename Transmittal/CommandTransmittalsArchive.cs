using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
internal class CommandTransmittalsArchive : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        var form = new Forms.FormArchive(commandData);
        form.ShowDialog(new WindowHandle(commandData.Application.MainWindowHandle));

        return Result.Succeeded;
    }
}
