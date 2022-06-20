using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
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

        _viewModel = (ViewModels.TransmittalViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }

    private void WizardControl_Help(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("https://russgreen.github.io/Transmittal/transmittal/");
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

    private void sfDataGridSheets_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        // couldn't get the databinding to work on the selecteditems property.
        // TODO investigate why not working
        var items = sfDataGridSheets.SelectedItems;
        _viewModel.SelectedDrawingSheets.Clear();

        if (items.Count > 0)
        {
            //viewModel.SheetsSelected = true;
            foreach (var item in items)
            {
                _viewModel.SelectedDrawingSheets.Add((DrawingSheetModel)item);
            }
        }
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
}
