using Autodesk.Revit.DB;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using Transmittal.Library.Models;
using Transmittal.ViewModels;

namespace Transmittal.Views;
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

    private void buttonLoadSettingsFromDatabase_Click(object sender, RoutedEventArgs e)
    {
        Ookii.Dialogs.Wpf.TaskDialogButton openButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Load settings from database");
        Ookii.Dialogs.Wpf.TaskDialogButton cancelButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.Cancel);

        Ookii.Dialogs.Wpf.TaskDialog taskDialog = new Ookii.Dialogs.Wpf.TaskDialog()
        {
            WindowTitle = "Transmittal Settings",
            MainInstruction = "Load the settings saved in the database?",
            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.CommandLinks,
            Buttons =  { openButton, cancelButton }
        };

        Ookii.Dialogs.Wpf.TaskDialogButton button = taskDialog.ShowDialog(this);

        if (button == openButton)
        {
            _viewModel.LoadSettingsFromDatabase();
        }
    }

    private void buttonTemplateDatabaseBrowse_Click(object sender, RoutedEventArgs e)
    {
        //not using ookii file dilaog because it doesn't implement initialDirectory correctly
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Transmittal Database File (*.tdb)|*.tdb",
            Multiselect = false,
            Title = "Select a Transmittal Database Template File",
            InitialDirectory = Path.Combine(Path.GetDirectoryName(App.DesktopAssemblyFolder), "Data")
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.CheckFileExists)
            {
                _viewModel.DatabaseTemplateFile = dialog.FileName;
                _viewModel.CheckForDatabaseFile();
            }
        }           
    }

    private void buttonDatabaseBrowse_Click(object sender, RoutedEventArgs e)
    {
        Ookii.Dialogs.Wpf.TaskDialogButton openButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Open existing project transmittal database");
        Ookii.Dialogs.Wpf.TaskDialogButton newButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Create a new project transmittal database");
        Ookii.Dialogs.Wpf.TaskDialogButton cancelButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.Cancel);

        Ookii.Dialogs.Wpf.TaskDialog taskDialog = new Ookii.Dialogs.Wpf.TaskDialog()
        {
            WindowTitle = "Transmittal Database",
            MainInstruction = "Use existing database or create new?",
            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.CommandLinks,
            Buttons = { openButton, newButton, cancelButton }
        };

        Ookii.Dialogs.Wpf.TaskDialogButton button = taskDialog.ShowDialog(this);
        if (button == openButton)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {          
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Select the project Transmittal database file",
                InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialog.CheckFileExists)
                {
                    _viewModel.DatabaseFile = dialog.FileName;
                    _viewModel.UpgradeDatabase();
                }
            }
        }

        if (button == newButton)
        {
            var fileName = $"{(_viewModel.ProjectNumber ?? "PROJECTNUMBER")}-" +
                                $"{(_viewModel.Originator ?? "ORIGINATOR" )}-XX-XX-DB-" +
                                $"{(_viewModel.Role ?? "ROLE")}-0001.tdb";
            
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Create a new project Transmittal database file",
                InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString()),
                FileName = fileName
            };

            if (dialog.ShowDialog() == true)
            {
                //we don't have a file so copy the template to the new file
                File.Copy(_viewModel.DatabaseTemplateFile, dialog.FileName);
                _viewModel.DatabaseFile = dialog.FileName;
            }
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

    private void buttonParamProjId_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
            nameof(SettingsModel.ProjectIdentifierParamGuid), 
            BuiltInCategory.OST_ProjectInformation);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamOriginator_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.OriginatorParamGuid),
    BuiltInCategory.OST_ProjectInformation);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamRole_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.RoleParamGuid),
    BuiltInCategory.OST_ProjectInformation);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamVolume_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
            nameof(SettingsModel.SheetVolumeParamGuid), 
            BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamLevel_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.SheetLevelParamGuid),
    BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamType_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.DocumentTypeParamGuid),
    BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamStatusCode_Click(object sender, RoutedEventArgs e)
    {
        Views.ParameterSelectorView dialog = new Views.ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.SheetStatusParamGuid),
    BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamStatus_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.SheetStatusDescriptionParamGuid),
    BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void buttonParamPackage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ParameterSelectorView(
            _viewModel,
    nameof(SettingsModel.SheetPackageParamGuid),
    BuiltInCategory.OST_Sheets);
        dialog.Owner = this;
        dialog.ShowDialog();
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
