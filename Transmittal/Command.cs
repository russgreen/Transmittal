using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Transmittal;
[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        if (commandData.Application.ActiveUIDocument.Document is null)
        {
            throw new ArgumentException("activedoc");
        }
        else
        {
            App.RevitDocument = commandData.Application.ActiveUIDocument.Document;
        }



        var mainWindowView = new Views.MainView();
        mainWindowView.ShowDialog();

        return Result.Succeeded;
    }


}
