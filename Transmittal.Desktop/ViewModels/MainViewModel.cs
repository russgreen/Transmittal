using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Extensions;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.ViewModels;
internal class MainViewModel
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

    public string WindowTitle { get; private set; }

    public string ProjectNo {  get; private set; }
    public string ProjectName { get; private set; }
    public string Database {  get; private set; }

    public MainViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion}";

        ProjectNo = _settingsService.GlobalSettings.ProjectNumber;
        ProjectName = _settingsService.GlobalSettings.ProjectName;
        Database = System.IO.Path.GetFileName(_settingsService.GlobalSettings.DatabaseFile);

    }
}
