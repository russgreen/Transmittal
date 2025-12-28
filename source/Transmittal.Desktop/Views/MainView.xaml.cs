using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views
{
    /// <summary>
    /// Interaction logic for TransmittalView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly ViewModels.MainViewModel _viewModel;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<MainView> _logger;

        public MainView()
        {
            InitializeComponent();

            _settingsService = Host.GetService<ISettingsService>();
            _logger = Host.GetService<ILogger<MainView>>();

            _viewModel = Host.GetService<ViewModels.MainViewModel>();

            DataContext = _viewModel;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        private void Button_Transmittal_Click(object sender, RoutedEventArgs e)
        {
            using (LogContext.PushProperty("UsageTracking", true))
            {
                _logger.LogInformation("{command}", nameof(Button_Transmittal_Click));
            }

            TransmittalView view = new();
            view.ShowDialog();
        }

        private void Button_TransmittalArchive_Click(object sender, RoutedEventArgs e)
        {
            using (LogContext.PushProperty("UsageTracking", true))
            {
                _logger.LogInformation("{command}", nameof(Button_TransmittalArchive_Click));
            }

            ArchiveView view = new();
            view.ShowDialog();
        }

        private void Button_Directory_Click(object sender, RoutedEventArgs e)
        {
            using (LogContext.PushProperty("UsageTracking", true))
            {
                _logger.LogInformation("{command}", nameof(Button_Directory_Click));
            }

            DirectoryView view = new();
            view.ShowDialog();
        }

        private void Button_About_Click(object sender, RoutedEventArgs e)
        {
           using (LogContext.PushProperty("UsageTracking", true))
           {
                _logger.LogInformation("{command}", nameof(Button_About_Click));
           }

            AboutView view = new AboutView();
            view.ShowDialog();
        }

        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
           using (LogContext.PushProperty("UsageTracking", true))
           {
                _logger.LogInformation("{command}", nameof(Button_Settings_Click));
           }

            SettingsView view = new();
            view.ShowDialog();
        }

        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Create a new project Transmittal database file",
                InitialDirectory = System.IO.Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
            };

            if (dialog.ShowDialog() == true)
            {
                //we don't have a file so copy the template to the new file
                System.IO.File.Copy(_settingsService.GlobalSettings.DatabaseTemplateFile, dialog.FileName, true);

                _settingsService.GlobalSettings.DatabaseFile = dialog.FileName;
                _settingsService.GlobalSettings.RecordTransmittals = true;
                _settingsService.GetSettings();

                //pop up the settings dialog to set the project details
                SettingsView view = new();
                view.ShowDialog();

                _viewModel.SetParameterValuesCommand.Execute(null);

                _viewModel.UpdateMRUCommand.Execute(null);  
            }
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            //not using ookii file dilaog because it doesn't implement initialDirectory correctly
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Transmittal Database File (*.tdb)|*.tdb",
                Title = "Select the project Transmittal database file",
                InitialDirectory = System.IO.Path.GetDirectoryName(Environment.SpecialFolder.MyComputer.ToString())
            };

            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName);
            }
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            AboutView view = new AboutView();
            view.ShowDialog();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void MenuItem_MRU_Click(object sender, RoutedEventArgs e)
        {
            //get the header value from the clicked submenu item
            string filePath = ((System.Windows.Controls.MenuItem)e.OriginalSource).Header.ToString();

            OpenDatabase(filePath);

        }

        private void OpenDatabase(string filePath)
        {
            if (File.Exists(filePath))
            {
                _settingsService.GlobalSettings.DatabaseFile = filePath;
                _settingsService.GlobalSettings.RecordTransmittals = true;
                _settingsService.GetSettings();

                _viewModel.SetParameterValuesCommand.Execute(null);

                _viewModel.UpdateMRUCommand.Execute(null);
            }
            else
            {
                MessageBox.Show("The file does not exist.  It may have been moved or deleted.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }   
    }
}
