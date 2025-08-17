using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Transmittal.Library.Enums;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class MainViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ISoftwareUpdateService _softwareUpdateService;

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
    private List<string> _mostRecentlyUsedFiles;

    public MainViewModel()
    {
        // design time constructor
        _settingsService = null;
        _softwareUpdateService = null;
    }

    public MainViewModel(ISettingsService settingsService,
        ISoftwareUpdateService softwareUpdateService)
    {
        _settingsService = settingsService;
        _softwareUpdateService = softwareUpdateService;

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion}";

        SetParameterValues();

        MostRecentlyUsedFiles = GetMostRecentlyUsedFiles();
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

        var path = Environment.GetFolderPath(Environment.SpecialFolder.Recent);

        var directory = new DirectoryInfo(path);
        var shortcutFiles = directory.GetFiles("*.tdb.lnk")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Take(10)
            .ToList();

        if (shortcutFiles.Count < 1)
        {
            return recentFiles;
        }

        dynamic script = CreateComInstance("Wscript.Shell");

        foreach (var file in shortcutFiles)
        {
            dynamic sc = script.CreateShortcut(file.FullName);
            recentFiles.Add(sc.TargetPath);
            Marshal.FinalReleaseComObject(sc);
        }
        Marshal.FinalReleaseComObject(script);

        return recentFiles;
    }

    private object CreateComInstance(string progId)
    {
        Type type = Type.GetTypeFromProgID(progId);
        if (type == null)
        {
            return null;
        }

        return Activator.CreateInstance(type);
    }

}
