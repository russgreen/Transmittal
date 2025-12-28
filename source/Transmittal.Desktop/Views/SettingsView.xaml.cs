using System.Diagnostics;
using System.IO;
using System.Windows;
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

        _viewModel = Host.GetService<ViewModels.SettingsViewModel>();

        DataContext = _viewModel;

        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }

    private void buttonFolderBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "Please select a folder to save the Transmittal files.",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.DrawingIssueStore = dialog.FolderName;
        }
    }

    private void buttonReportPathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "Please select the folder where report templates are stored.",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.ReportTemplatePath = dialog.FolderName;
        }
    }

    private void buttonIssueSheetStorePathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "Please select the folder where transmittal sheets are stored.",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.IssueSheetStorePath = dialog.FolderName;
        }
    }

    private void buttonDirectoryStorePathBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = "Please select the folder where directory reports are stored.",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if ((bool)dialog.ShowDialog(this))
        {
            _viewModel.DirectoryStorePath = dialog.FolderName;
        }
    }

    private void buttonCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void buttonHelp_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://russgreen.github.io/Transmittal/settings/",
            UseShellExecute = true
        });
    }
}
