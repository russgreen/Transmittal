using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Transmittal.Library.Services;

namespace Transmittal.FileRenamer.Views;
/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    private readonly ViewModels.MainViewModel? _viewModel;

    private readonly ISettingsService? _settingService;
    private readonly IMessageBoxService? _messageBoxService;

    public MainView()
    {
        InitializeComponent();

        _settingService = Host.GetService<ISettingsService>();
        _messageBoxService = Host.GetService<IMessageBoxService>();
        _viewModel = Host.GetService<ViewModels.MainViewModel>();

        DataContext = _viewModel;
        _viewModel?.ClosingRequest += (sender, e) => this.Close();
         
    }
}
