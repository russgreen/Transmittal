using CommunityToolkit.Mvvm.DependencyInjection;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for ArchiveView.xaml
/// </summary>
public partial class ArchiveView : Window
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

    private readonly ViewModels.ArchiveViewModel _viewModel;

    public ArchiveView()
    {
        InitializeComponent();

        _viewModel = (ViewModels.ArchiveViewModel)this.DataContext;
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

    private void Button_MergeTransmittals_Click(object sender, RoutedEventArgs e)
    {
        //Command="{Binding MergeTransmittalsCommand}"
        Ookii.Dialogs.Wpf.TaskDialogButton mergeButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Merge the selected transmittal records into a single transmittal record. This action cannot be undone.");
        Ookii.Dialogs.Wpf.TaskDialogButton noMergeButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Do not merge the selected transmittal records.");
        Ookii.Dialogs.Wpf.TaskDialogButton cancelButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.Cancel);

        Ookii.Dialogs.Wpf.TaskDialog taskDialog = new Ookii.Dialogs.Wpf.TaskDialog()
        {
            WindowTitle = "Merge Transmittals",
            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.CommandLinks,
            Buttons = { mergeButton, noMergeButton, cancelButton }
        };

        Ookii.Dialogs.Wpf.TaskDialogButton button = taskDialog.ShowDialog(this);
        if (button == mergeButton)
        {
            _viewModel.MergeTransmittalsCommand.Execute(null);
        }
    }
}
