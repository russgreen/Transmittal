using Ookii.Dialogs.Wpf;
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
using Transmittal.Desktop.ViewModels;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
    {
        InitializeComponent();

        var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);

        _viewModel = (ViewModels.SettingsViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }

    private void buttonFolderBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Please select a folder to save the Transmittal files.",
            UseDescriptionForTitle = true, // This applies to the Vista style dialog only, not the old dialog.
            RootFolder = Environment.SpecialFolder.MyComputer
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.DrawingIssueStore = dialog.SelectedPath;
        }
    }

    private void buttonReportPathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Please select the folder where report templates are stored.",
            UseDescriptionForTitle = true, // This applies to the Vista style dialog only, not the old dialog.
            RootFolder = Environment.SpecialFolder.MyComputer
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.ReportTemplatePath = dialog.SelectedPath;
        }
    }

    private void buttonIssueSheetStorePathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Please select the folder where transmittal sheets are stored.",
            UseDescriptionForTitle = true, // This applies to the Vista style dialog only, not the old dialog.
            RootFolder = Environment.SpecialFolder.MyComputer
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.IssueSheetStorePath = dialog.SelectedPath;
        }
    }

    private void buttonDirectoryStorePathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Please select the folder where directory reports are stored.",
            UseDescriptionForTitle = true, // This applies to the Vista style dialog only, not the old dialog.
            RootFolder = Environment.SpecialFolder.MyComputer
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.DirectoryStorePath = dialog.SelectedPath;
        }
    }

    private void buttonCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void buttonHelp_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("https://russgreen.github.io/Transmittal/settings/");
    }
}
