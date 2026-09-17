using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Enums;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class MainViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ISoftwareUpdateService _softwareUpdateService;
    private readonly IDataConnection _dataConnection;

    public string WindowTitle { get; private set; }

    [ObservableProperty]
    private string _projectNo;

    [ObservableProperty]
    private string _projectName;

    [ObservableProperty]
    private string _database;

    [ObservableProperty]
    private bool _hasDatabase = false;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private List<string> _mostRecentlyUsedFiles = new();

    public MainViewModel()
    {
        // design time constructor
        _settingsService = null;
        _softwareUpdateService = null;
        _dataConnection = null;
    }

    public MainViewModel(ISettingsService settingsService,
        ISoftwareUpdateService softwareUpdateService,
        IDataConnection dataConnection)
    {
        _settingsService = settingsService;
        _softwareUpdateService = softwareUpdateService;
        _dataConnection = dataConnection;

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion}";

        SetParameterValues();

        //MostRecentlyUsedFiles = GetMostRecentlyUsedFiles();
    }

    //don't want this to run every time the app launches
    private async Task CheckForUpdates()
    {    
        await _softwareUpdateService.CheckUpdates();
        if (_softwareUpdateService.State == SoftwareUpdateState.ReadyToInstall)
        {
            Message = $"New version {_softwareUpdateService.NewVersion} is ready to install.";
        }
    }

    [RelayCommand]
    private void SetParameterValues()
    {
        ProjectNo = _settingsService.GlobalSettings.ProjectNumber;
        ProjectName = _settingsService.GlobalSettings.ProjectName;
        Database = System.IO.Path.GetFileName(_settingsService.GlobalSettings.DatabaseFile);

        HasDatabase = true;

        if (Database == "[NONE]")
        {
            HasDatabase = false;
        }
    }

    [RelayCommand]
    private void UpdateMRU()
    {
        MostRecentlyUsedFiles.Clear();

        MostRecentlyUsedFiles = GetMostRecentlyUsedFiles();
    }

    private List<string> GetMostRecentlyUsedFiles()
    {
        var recentFiles = new List<string>();

        if (_dataConnection != null)
        {
            recentFiles.AddRange(_dataConnection.GetMostRecentlyUsedFiles()
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }


        return recentFiles.Take(10).ToList();
    }

    private static string GetShortcutTargetPath(string shortcutPath)
    {
        try
        {
            dynamic shell = CreateComInstance("Wscript.Shell");
            if (shell == null)
            {
                return string.Empty;
            }

            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            var targetPath = shortcut.TargetPath as string;
            Marshal.FinalReleaseComObject(shortcut);
            Marshal.FinalReleaseComObject(shell);
            return targetPath ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static object CreateComInstance(string progId)
    {
        Type type = Type.GetTypeFromProgID(progId);
        if (type == null)
        {
            return null;
        }

        return Activator.CreateInstance(type);
    }

}
