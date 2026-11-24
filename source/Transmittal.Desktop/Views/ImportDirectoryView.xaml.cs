using System.Windows;
using Transmittal.Library.Models.DirectoryImport;

namespace Transmittal.Desktop.Views;

/// <summary>
/// Interaction logic for ImportDirectoryView.xaml
/// </summary>
public partial class ImportDirectoryView : Window
{
    private readonly ViewModels.ImportDirectoryViewModel _viewModel;

    internal ImportDirectoryView(DirectoryImportSource source)
    {
        InitializeComponent();

        _viewModel = Host.GetService<ViewModels.ImportDirectoryViewModel>();
        _viewModel.Initialise(source);
        DataContext = _viewModel;

        // DialogResult closes the window; it is true only when an import was performed.
        _viewModel.ClosingRequest += (sender, e) => DialogResult = _viewModel.ImportCompleted;
    }
}
