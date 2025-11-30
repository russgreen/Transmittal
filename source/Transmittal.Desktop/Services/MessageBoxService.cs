using System.Windows.Forms; // For TaskDialog
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Services;
internal class MessageBoxService : IMessageBoxService
{
    public bool ShowCancel(string title, string message)
    {
        var page = new TaskDialogPage
        {
            Caption = title,
            Heading = message
        };

        page.Buttons.Add(TaskDialogButton.Cancel);
        var result = TaskDialog.ShowDialog(page);
        return result == TaskDialogButton.Cancel;
    }

    public bool ShowOk(string title, string message)
    {
        var page = new TaskDialogPage
        {
            Caption = title,
            Heading = message
        };
        page.Buttons.Add(TaskDialogButton.OK);
        var result = TaskDialog.ShowDialog(page);
        return result == TaskDialogButton.OK;
    }

    public bool ShowOkCancel(string title, string message)
    {
        var page = new TaskDialogPage
        {
            Caption = title,
            Heading = message
        };
        page.Buttons.Add(TaskDialogButton.OK);
        page.Buttons.Add(TaskDialogButton.Cancel);
        var result = TaskDialog.ShowDialog(page);
        return result == TaskDialogButton.OK;
    }

    public bool ShowYesNo(string title, string message)
    {
        var page = new TaskDialogPage
        {
            Caption = title,
            Heading = message
        };
        page.Buttons.Add(TaskDialogButton.Yes);
        page.Buttons.Add(TaskDialogButton.No);
        var result = TaskDialog.ShowDialog(page);
        return result == TaskDialogButton.Yes;
    }
}
