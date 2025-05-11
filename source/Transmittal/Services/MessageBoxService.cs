using Autodesk.Revit.UI;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class MessageBoxService : IMessageBoxService
{
    public bool ShowCancel(string title, string message)
    {
        var result = TaskDialog.Show(title, message, TaskDialogCommonButtons.Cancel);

        if (result == TaskDialogResult.Cancel)
        {
            return true;
        }

        return false;
    }

    public bool ShowOk(string title, string message)
    {
        var result = TaskDialog.Show(title, message, TaskDialogCommonButtons.Ok);

        if (result == TaskDialogResult.Ok)
        {
            return true;
        }

        return false;
    }

    public bool ShowOkCancel(string title, string message)
    {
        var result = TaskDialog.Show(title, message, TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel);

        if (result == TaskDialogResult.Ok)
        {
            return true;
        }

        return false;
    }

    public bool ShowYesNo(string title, string message)
    {
        var result = TaskDialog.Show(title, message, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

        if (result == TaskDialogResult.Yes)
        {
            return true;
        }

        return false;
    }
}
