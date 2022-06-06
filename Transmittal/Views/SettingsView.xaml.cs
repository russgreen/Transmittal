using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
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

    private void buttonTemplateDatabaseBrowse_Click(object sender, RoutedEventArgs e)
    {
        //not using ookii file dilaog because it doesn't implement initialDirectory correctly
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Transmittal Database File (*.tdb)|*.tdb",
            Multiselect = false,
            Title = "Select a Transmittal Database Template File",
            InitialDirectory = Path.Combine(Path.GetDirectoryName(App.ExecutingAssemblyPath), "Data")
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.CheckFileExists)
            {
                _viewModel.DatabaseTemplateFile = dialog.FileName;
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
                Title = "Select the Project Transmittal Database  File",
                InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialog.CheckFileExists)
                {
                    _viewModel.DatabaseFile = dialog.FileName;
                }
            }
        }

        if (button == newButton)
        {
            var fileName = $"{_viewModel.ProjectNumber}-" +
                                $"{_viewModel.Originator}-XX-XX-DB-" +
                                $"{_viewModel.Role}-0001.tdb";
            
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Create a new Project Transmittal Database  File",
                InitialDirectory = Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString()),
                FileName = fileName
            };

            if (dialog.ShowDialog() == true)
            {
                //we don't have a file so copy the template to the new file
                _viewModel.DatabaseFile = dialog.FileName;
                File.Copy(_viewModel.DatabaseTemplateFile, _viewModel.DatabaseFile);
            }
        }


    }
}
