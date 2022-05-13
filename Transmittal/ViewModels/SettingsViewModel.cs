using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Reflection;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal.ViewModels;

[INotifyPropertyChanged]
internal partial class SettingsViewModel
{
    public string WindowTitle { get; private set; }
    
    private ISettingsServiceRvt _settingsServiceRvt = Ioc.Default.GetRequiredService<ISettingsServiceRvt>();
    private ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
    
    public SettingsViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";
    }
}
