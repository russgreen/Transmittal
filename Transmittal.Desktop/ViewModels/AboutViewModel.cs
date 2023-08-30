using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Transmittal.Library.Enums;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class AboutViewModel : BaseViewModel
{
    private ISoftwareUpdateService _softwareUpdateService = Host.GetService<ISoftwareUpdateService>();
    private IConfiguration _configuration = Host.GetService<IConfiguration>();

    public SoftwareUpdateState State => _softwareUpdateService.State;
    public string CurrentVersion => _softwareUpdateService.CurrentVersion;
    public string NewVersion => _softwareUpdateService.NewVersion;
    public string ErrorMessage => _softwareUpdateService.ErrorMessage;
    public string ReleaseNotesUrl => _softwareUpdateService.ReleaseNotesUrl;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _copyright;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    [NotifyPropertyChangedFor(nameof(NewVersion))]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyPropertyChangedFor(nameof(ReleaseNotesUrl))]
    private bool _isUpdateChecked = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    private bool _isDownloadAttempted = false;

    [ObservableProperty]
    private bool _canCheckForUpdates = true;

    [ObservableProperty]
    private bool _downloadAvailable = false;

    [ObservableProperty]
    private bool _installAvailable = false;

    [ObservableProperty]
    private string _updateMessage;

    [ObservableProperty]
    private List<OpenSourceSoftwareModel> _openSourceSoftwareModels = new List<OpenSourceSoftwareModel>();

    public AboutViewModel()
    {
        //get the description from the assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        
        var descriptionAttribute = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false).FirstOrDefault() as System.Reflection.AssemblyDescriptionAttribute;
        Description = descriptionAttribute?.Description;

        var copyRightAttribute = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false).FirstOrDefault() as System.Reflection.AssemblyCopyrightAttribute; 
        Copyright = copyRightAttribute?.Copyright;

        UpdateMessage = $"Running version {CurrentVersion}";

        BuildOpenSourceSoftwareList();
    }

    private void StateChanged()
    {
        switch (State)
        {
            case SoftwareUpdateState.ErrorDownloading:
                UpdateMessage = ErrorMessage;
                break;
            case SoftwareUpdateState.ErrorChecking:
                UpdateMessage = ErrorMessage;
                break;
            case SoftwareUpdateState.ReadyToDownload:
                DownloadAvailable = true;
                CanCheckForUpdates = false;
                UpdateMessage = $"Version {NewVersion} ready to download";
                break;
            case SoftwareUpdateState.ReadyToInstall:
                InstallAvailable = true;
                CanCheckForUpdates = false;
                UpdateMessage = $"Version {NewVersion} ready to install";
                break;
            default: 
                //Applies to SoftwareUpdateState.UpToDate and any other states
                UpdateMessage = $"Running version {CurrentVersion}. No updates available";
                break;
        }
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await _softwareUpdateService.CheckUpdates();
        IsUpdateChecked = true;

        StateChanged();
    }

    [RelayCommand]
    private async Task DownloadUpdate()
    {
        await _softwareUpdateService.DownloadUpdate();
        IsDownloadAttempted = true;

        StateChanged();
    }

    [RelayCommand]
    private void InstallUpdate()
    {
        if(File.Exists(_softwareUpdateService.LocalFilePath))
        {
            Process.Start(_softwareUpdateService.LocalFilePath);
        }
    }

    private void BuildOpenSourceSoftwareList()
    {
        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "AutoMapper",
            SoftwareUri = "https://github.com/AutoMapper/AutoMapper",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "CommunityToolkit.MVVM",
            SoftwareUri = "https://github.com/CommunityToolkit/dotnet",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/CommunityToolkit/dotnet/blob/main/License.md"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Dapper",
            SoftwareUri = "https://github.com/DapperLib/Dapper",
            LicenseName = "Apache 2.0 License",
            LicenseUri = "https://github.com/DapperLib/Dapper/blob/main/License.txt"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Humanizer",
            SoftwareUri = "https://github.com/Humanizr/Humanizer",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/Humanizr/Humanizer/blob/main/LICENSE"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Microsoft.Extensions.Hosting",
            SoftwareUri = "https://github.com/dotnet/runtime",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Serilog",
            SoftwareUri = "https://serilog.net/",
            LicenseName = "Apache 2.0 License",
            LicenseUri = "https://github.com/serilog/serilog/blob/dev/LICENSE"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Microsoft.Data.Sqlite",
            SoftwareUri = "https://github.com/dotnet/efcore",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/dotnet/efcore/blob/main/LICENSE.txt"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Nice3point.Revit.Api",
            SoftwareUri = "https://github.com/Nice3point/RevitApi",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/Nice3point/RevitApi/blob/main/License.md"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Nice3point.Revit.Toolkit",
            SoftwareUri = "https://github.com/Nice3point/RevitToolkit",
            LicenseName = "MIT License",
            LicenseUri = "https://github.com/Nice3point/RevitToolkit/blob/develop/License.md"
        });

        OpenSourceSoftwareModels.Add(new OpenSourceSoftwareModel()
        {
            SoftwareName = "Ookii.Dialogs.Wpf",
            SoftwareUri = "https://github.com/ookii-dialogs/ookii-dialogs-wpf",
            LicenseName = "BSD 3-Clause License",
            LicenseUri = "https://github.com/ookii-dialogs/ookii-dialogs-wpf/blob/master/LICENSE.md"
        });

    }
}
