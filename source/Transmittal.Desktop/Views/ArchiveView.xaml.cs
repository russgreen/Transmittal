using CommunityToolkit.Mvvm.DependencyInjection;
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
    private readonly ISettingsService _settingsService;

    private readonly ViewModels.ArchiveViewModel _viewModel;

    public ArchiveView()
    {
        InitializeComponent();

        _settingsService = Host.GetService<ISettingsService>();
        _viewModel = Host.GetService<ViewModels.ArchiveViewModel>();

        DataContext = _viewModel;
    }

    private void Button_MergeTransmittals_Click(object sender, RoutedEventArgs e)
    {
        var mergeButton = new System.Windows.Forms.TaskDialogCommandLinkButton("Merge the selected transmittal records into a single transmittal record. This action cannot be undone.");

        var page = new System.Windows.Forms.TaskDialogPage
        {
            Caption = "Merge Transmittals",
            Buttons = { mergeButton, System.Windows.Forms.TaskDialogButton.Cancel }
        };

        var button = System.Windows.Forms.TaskDialog.ShowDialog(page);
        if (button == mergeButton)
        {
            _viewModel.MergeTransmittalsCommand.Execute(null);
        }
    }

    private void Button_AddPackage_Click(object sender, RoutedEventArgs e)
    {
        Views.NewPackageView dialog = new Views.NewPackageView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
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
        var deleteButton = new System.Windows.Forms.TaskDialogCommandLinkButton($"Delete the selected transmittal item(s). This action cannot be undone.");

        var page = new System.Windows.Forms.TaskDialogPage
        {
            Caption = "Delete item(s) from transmittal",
            Buttons = { deleteButton, System.Windows.Forms.TaskDialogButton.Cancel }
        };

        var button = System.Windows.Forms.TaskDialog.ShowDialog(page);
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
        var deleteButton = new System.Windows.Forms.TaskDialogCommandLinkButton($"Remove the selected contact(s) from the transmittal. This action cannot be undone.");

        var page = new System.Windows.Forms.TaskDialogPage
        {
            Caption = "Delete contact(s) from transmittal",
            Buttons = { deleteButton, System.Windows.Forms.TaskDialogButton.Cancel }
        };

        var button = System.Windows.Forms.TaskDialog.ShowDialog(page);
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
