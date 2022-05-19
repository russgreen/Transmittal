using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Transmittal.Services;

namespace Transmittal;

[Transaction(TransactionMode.Manual)]
public class CommandTransmittal : IExternalCommand
{
    private ISettingsServiceRvt _settingsServiceRvt = Ioc.Default.GetRequiredService<ISettingsServiceRvt>();
    
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {        
        UIApplication uiapp = commandData.Application;
        App.RevitDocument = commandData.Application.ActiveUIDocument.Document;

        try
        {
            //first check if the document is saved as a large publish job might crash revit
            var td = new TaskDialog("Save Project")
            {
                MainContent = "Are all your recent changes to the model saved?  Recomended in case a crash occurs during transmittal exports",
                CommonButtons = TaskDialogCommonButtons.Cancel
            };
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Save the project then continue to launch Transmittal");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "No need to save the project. Just launch Transmittal");

            var taskDialogResult = td.Show();
            if (taskDialogResult == TaskDialogResult.CommandLink1)
            {
                uiapp.ActiveUIDocument.Document.Save();
            }
            else if (taskDialogResult == TaskDialogResult.CommandLink2)
            {
            }
            // do nothing
            else if (taskDialogResult == TaskDialogResult.Cancel)
            {
                // cancel clicked
                return Result.Failed;
            }

            // add a showdialog watcher
            uiapp.DialogBoxShowing += AppDialogShowing;

            if (_settingsServiceRvt.GetSettingsRvt(App.RevitDocument) == false)
            {
                var settingsView = new Views.SettingsView();
                settingsView.ShowDialog();
            }
            else
            {
                var transmittalView = new Views.TransmittalView();
                transmittalView.ShowDialog();
            }

            return Result.Succeeded;            
            
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
        finally
        {
            uiapp.DialogBoxShowing -= AppDialogShowing;
        }

    }


    private void AppDialogShowing(object sender, Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs args)
    {
        // Get the help id of the showing dialog
        string dialogId = args.DialogId;

        Debug.WriteLine("DialogID : " + dialogId.ToString());
        // Format the prompt information string
        args.OverrideResult(1);
    }
}
