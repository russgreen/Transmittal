using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Reflection;
using System.Windows.Input;

namespace Transmittal.ViewModels;
internal class MainViewModel : ObservableValidator
{
    public string WindowTitle { get; private set; }

    public bool IsCommandEnabled { get; private set; } = true;

    public ICommand RunCommand { get; }

    public MainViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        RunCommand = new RelayCommand(RunCommandMethod);
    }

    private void RunCommandMethod()
    {
        //DO STUFF HERE
        IsCommandEnabled = false;
    }
}
