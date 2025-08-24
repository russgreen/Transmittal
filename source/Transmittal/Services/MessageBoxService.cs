using Ookii.Dialogs.Wpf;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class MessageBoxService : IMessageBoxService
{
    public bool ShowCancel(string title, string message)
    {
        var cancelButton = new TaskDialogButton(ButtonType.Cancel);

        var taskDialog = new TaskDialog()
        {
            WindowTitle = title,
            MainInstruction = message,
            ButtonStyle = TaskDialogButtonStyle.Standard,
            Buttons = { cancelButton }
        };

        var button = taskDialog.ShowDialog();
        if (button == cancelButton)
        {
            return true;
        }

        return false;
    }

    public bool ShowOk(string title, string message)
    {
        var okButton = new TaskDialogButton(ButtonType.Ok);

        var taskDialog = new TaskDialog()
        {
            WindowTitle = title,
            MainInstruction = message,
            ButtonStyle = TaskDialogButtonStyle.Standard,
            Buttons = { okButton }
        };

        var button = taskDialog.ShowDialog();
        if (button == okButton)
        {
            return true;
        }

        return false;
    }

    public bool ShowOkCancel(string title, string message)
    {
        var okButton = new TaskDialogButton(ButtonType.Ok);
        var cancelButton = new TaskDialogButton(ButtonType.Cancel);

        var taskDialog = new TaskDialog()
        {
            WindowTitle = title,
            MainInstruction = message,
            ButtonStyle = TaskDialogButtonStyle.Standard,
            Buttons = { okButton, cancelButton }
        };

        var button = taskDialog.ShowDialog();
        if (button == okButton)
        {
            return true;
        }

        return false;
    }

    public bool ShowYesNo(string title, string message)
    {
        var yesButton = new TaskDialogButton(ButtonType.Yes);
        var noButton = new TaskDialogButton(ButtonType.No);

        var taskDialog = new TaskDialog()
        {
            WindowTitle = title,
            MainInstruction = message,
            ButtonStyle = TaskDialogButtonStyle.Standard,
            Buttons = { yesButton, noButton }
        };

        var button = taskDialog.ShowDialog();
        if (button == yesButton)
        {
            return true;
        }

        return false;
    }
}

