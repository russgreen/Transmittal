using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
internal class CommandDirectory : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        //var form = new Forms.FormDirectory(commandData);
        //form.ShowDialog(new WindowHandle(commandData.Application.MainWindowHandle));

        var newView = new Views.DirectoryView();
        newView.ShowDialog();

        return Result.Succeeded;
    }
}
