using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Transmittal.Browser.Models;
using Transmittal.Browser.Services;

namespace Transmittal.Browser.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = "Transmittal Browser";

    [ObservableProperty]
    private bool _isFilePanelExpanded = true;

    [ObservableProperty]
    private bool _showRedropHint;

    [ObservableProperty]
    private string _redropHint = string.Empty;

    [ObservableProperty]
    private string _currentAddress = "about:blank";

    public ObservableCollection<TransferFileItem> TransferFiles { get; } = new();

    public MainWindowViewModel(IBrowserLaunchOptionsProvider launchOptionsProvider)
    {
        var launchOptions = launchOptionsProvider.GetLaunchOptions();
        ShowRedropHint = launchOptions.ShowRedropHint;
        RedropHint = "If upload resets after login, drag files from this panel back into the page.";

        foreach (var filePath in launchOptions.TransferFilePaths
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            TransferFiles.Add(new TransferFileItem { FilePath = filePath });
        }
    }
}
