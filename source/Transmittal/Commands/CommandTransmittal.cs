using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External;
using System.Diagnostics;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal.Commands;

[Transaction(TransactionMode.Manual)]
public class CommandTransmittal : ExternalCommand
{
    private ISettingsServiceRvt _settingsServiceRvt;
    private ILogger<CommandTransmittal> _logger;

    public override void Execute()
    {       
        _settingsServiceRvt = Host.GetService<ISettingsServiceRvt>();
        _logger = Host.GetService<ILogger<CommandTransmittal>>();

        App.CachedUiApp = Context.UiApplication;
        App.RevitDocument = Context.ActiveDocument;

        try
        {
            //first check if the document is saved as a large publish job might crash revit
            var td = new TaskDialog("Save Project")
            {
                MainContent = "Are all your recent changes to the model saved?  Recommended in case a crash occurs during transmittal exports",
                CommonButtons = TaskDialogCommonButtons.Cancel
            };
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Save the project then continue to launch Transmittal");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "No need to save the project. Just launch Transmittal");

            var taskDialogResult = td.Show();
            if (taskDialogResult == TaskDialogResult.CommandLink1)
            {
                App.CachedUiApp.ActiveUIDocument.Document.Save();
            }
            else if (taskDialogResult == TaskDialogResult.CommandLink2)
            {
            }
            // do nothing
            else if (taskDialogResult == TaskDialogResult.Cancel)
            {
                // cancel clicked
                return;
            }

            // add a showdialog watcher
            App.CachedUiApp.DialogBoxShowing += AppDialogShowing;

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

            return;            
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
            //message = ex.Message;
            return;
        }
        finally
        {
            App.CachedUiApp.DialogBoxShowing -= AppDialogShowing;
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
