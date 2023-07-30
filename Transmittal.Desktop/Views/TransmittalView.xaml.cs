using CommunityToolkit.Mvvm.DependencyInjection;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for TransmittalView.xaml
/// </summary>
public partial class TransmittalView : Window
{
    private readonly ViewModels.TransmittalViewModel _viewModel;
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public TransmittalView()
    {
        InitializeComponent();

        var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);

        _viewModel = (ViewModels.TransmittalViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }


    private void WizardControl_Help(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("https://russgreen.github.io/Transmittal/standalonetransmittal/");
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
            //_viewModel.AbortFlag = true;
            //if (_viewModel.Processingsheets == false)
            //{
            //    this.Close();
            //}
        }
        //TODO stop the main window closing if the no button is clicked
    }

    private void Button_AddToDirectory_Click(object sender, RoutedEventArgs e)
    {
        Views.NewPersonView dialog = new Views.NewPersonView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void sfDataGridDocuments_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
    }

    private void sfDataGridDocuments_Drop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var file in files)
        {
            _viewModel.AddFileToDocumentsList(file);
        }
    }

    private void sfDataGridDocuments_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
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

        var itemModel = e.NewObject as DocumentModel;
        itemModel.DrgProj = projectIdentifier;
        itemModel.DrgOriginator = _settingsService.GlobalSettings.Originator;
        itemModel.DrgRole = _settingsService.GlobalSettings.Role;
    }
}
