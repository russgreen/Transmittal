using Ookii.Dialogs.Wpf;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Transmittal.Models;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for TransmittalView.xaml
/// </summary>
public partial class TransmittalView : Window
{
    private readonly ViewModels.TransmittalViewModel _viewModel;
        
    public TransmittalView()
    {
        InitializeComponent();

        var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);

        _viewModel = Host.GetService<ViewModels.TransmittalViewModel>();
        DataContext = _viewModel;

        _viewModel.ClosingRequest += (sender, e) => this.Close();

#if REVIT2025_OR_GREATER
        this.sfDataGridSheets.GroupColumnDescriptions.Add(new GroupColumnDescription() { ColumnName = "DrgSheetCollection" });
        this.sfDataGridSheets.Columns["DrgSheetCollection"].GroupMode = DataReflectionMode.Display;
        this.sfDataGridSheets.AutoExpandGroups = true;
        this.sfDataGridSheets.AllowFrozenGroupHeaders = true;
#endif
    }

    private void WizardControl_Help(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://russgreen.github.io/Transmittal/revit-addin/",
            UseShellExecute = true
        });
    }

    private void WizardControl_Cancel(object sender, RoutedEventArgs e)
    {
       
        Ookii.Dialogs.Wpf.TaskDialogButton yesButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.Yes);
        Ookii.Dialogs.Wpf.TaskDialogButton noButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.No);

        Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog()
        {
            WindowTitle = "Cancel Transmittal",
            MainInstruction = "Are you sure you want to cancel?",
            MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Information,
            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.Standard,
            Buttons = { yesButton, noButton }
        };
        
        Ookii.Dialogs.Wpf.TaskDialogButton button = dialog.ShowDialog(this);
        if (button == yesButton)
        {
            _viewModel.AbortFlag = true;
            if (_viewModel.Processingsheets == false)
            {
                this.Close();
            }
        }
        //TODO stop the main window closing if the no button is clicked
    }

    private void ButtonRevise_Click(object sender, RoutedEventArgs e)
    {
        Views.RevisionsView dialog = new Views.RevisionsView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void ButtonStatus_Click(object sender, RoutedEventArgs e)
    {
        Views.StatusView dialog = new Views.StatusView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();

        this.sfDataGridSheets.View.Refresh();
    }

    private void Button_AddToDirectory_Click(object sender, RoutedEventArgs e)
    {
        Views.NewPersonView dialog = new Views.NewPersonView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void RecordIssue_Unchecked(object sender, RoutedEventArgs e)
    {
        if(this.RecordIssue.IsChecked == false)
        {
            _viewModel.SelectedProjectDirectory.Clear();
        }
    }

    private void sfDataGridSheets_CurrentCellValidated(object sender, CurrentCellValidatedEventArgs e)
    {
        if (e.NewValue != e.OldValue)
        {
            DrawingSheetModel sheet = e.RowData as DrawingSheetModel;
            if (sheet != null)
            {
                _viewModel.UpdateSheet(sheet);
            }
        }
    }

    private void CopiesTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !IsPositiveInt((TextBox)sender, e.Text);
    }

    private void CopiesTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
        {
            e.CancelCommand();
            return;
        }

        var pasteText = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
        if (!IsPositiveInt((TextBox)sender, pasteText))
        {
            e.CancelCommand();
        }
    }

    private bool IsPositiveInt(TextBox textBox, string newText)
    {
        var positiveIntRegex = new Regex(@"^[1-9]\d*$");

        var proposed = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, newText);

        return string.IsNullOrEmpty(proposed) || positiveIntRegex.IsMatch(proposed);
    }
}
