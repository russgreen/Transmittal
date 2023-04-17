using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Extensions;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class MainViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public string WindowTitle { get; private set; }

    public string ProjectNo {  get; private set; }
    public string ProjectName { get; private set; }
    public string Database {  get; private set; }

    [ObservableProperty]
    private bool _hasDatabase = true;

    public MainViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion}";

        ProjectNo = _settingsService.GlobalSettings.ProjectNumber;
        ProjectName = _settingsService.GlobalSettings.ProjectName;
        Database = System.IO.Path.GetFileName(_settingsService.GlobalSettings.DatabaseFile);

        if (Database == "[NONE]")
        {
            HasDatabase = false;
        }
    }


}
