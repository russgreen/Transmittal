using CommunityToolkit.Mvvm.DependencyInjection;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for ArchiveView.xaml
/// </summary>
public partial class ArchiveView : Window
{
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    private readonly ViewModels.ArchiveViewModel _viewModel;

    public ArchiveView()
    {
        InitializeComponent();

        _viewModel = (ViewModels.ArchiveViewModel)this.DataContext;
    }

    private void Button_MergeTransmittals_Click(object sender, RoutedEventArgs e)
    {
        //Command="{Binding MergeTransmittalsCommand}"
        TaskDialogButton mergeButton = new("Merge the selected transmittal records into a single transmittal record. This action cannot be undone.");
        TaskDialogButton noMergeButton = new("Do not merge the selected transmittal records.");
        TaskDialogButton cancelButton = new(ButtonType.Cancel);

        TaskDialog taskDialog = new()
        {
            WindowTitle = "Merge Transmittals",
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { mergeButton, noMergeButton, cancelButton }
        };

        TaskDialogButton button = taskDialog.ShowDialog(this);
        if (button == mergeButton)
        {
            _viewModel.MergeTransmittalsCommand.Execute(null);
        }
    }

    private void sfDataGridTransmittalItems_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
    {
        var projectIdentifier = string.Empty;

        //check if we're using the project identifier on this project
        if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectNumber;
        }
        else
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier;
        }

        TransmittalItemModel itemModel = e.NewObject as TransmittalItemModel;
        itemModel.DrgProj = projectIdentifier;
        itemModel.DrgOriginator = _settingsService.GlobalSettings.Originator;
        itemModel.DrgRole = _settingsService.GlobalSettings.Role;
        itemModel.DrgNumber = " ";
    }

    private void sfDataGridTransmittals_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        if (_viewModel.SelectedTransmittals.Count == 1)
        {
            TransmittalModel transmittal = _viewModel.SelectedTransmittals.FirstOrDefault() as TransmittalModel; //   .Cast<TransmittalModel>();   //.Cast<TransmittalModel>().ToList();

            if(transmittal.Items.Count == 0 &&
                transmittal.Distribution.Count == 0) 
            { 
                _viewModel.DeleteTransmittalCommand.Execute(null);
            }
        }

        e.Cancel = true;
    }

    private void sfDataGridTransmittalItems_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        TaskDialogButton deleteButton = new($"Delete the selected transmittal item(s). This action cannot be undone.");
        TaskDialogButton cancelButton = new(ButtonType.Cancel);

        TaskDialog taskDialog = new()
        {
            WindowTitle = "Delete item(s) from transmittal",
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { deleteButton, cancelButton }
        };

        TaskDialogButton button = taskDialog.ShowDialog(this);
        if (button == deleteButton)
        {
            if(_viewModel.SelectedTransmittalItems.Count > 0)
            {
                _viewModel.DeleteSelectedTransmittalItemCommand.Execute(null);
                return;
            }
        }

        e.Cancel = true;
    }

    private void sfDataGridTransmittalDistribution_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        TaskDialogButton deleteButton = new($"Remove the selected contact(s) from the transmittal. This action cannot be undone.");
        TaskDialogButton cancelButton = new(ButtonType.Cancel);

        TaskDialog taskDialog = new()
        {
            WindowTitle = "Delete contact(s) from transmittal",
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { deleteButton, cancelButton }
        };

        TaskDialogButton button = taskDialog.ShowDialog(this);
        if (button == deleteButton)
        {
            if(_viewModel.SelectedTransmittalDistributions.Count > 0)
            {
                _viewModel.DeleteSelectedDistributionCommand.Execute(null);
                return;
            }
        }

        e.Cancel = true;
    }


}
