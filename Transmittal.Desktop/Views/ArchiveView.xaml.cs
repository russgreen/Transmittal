using CommunityToolkit.Mvvm.DependencyInjection;
using Syncfusion.UI.Xaml.Grid;
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
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for ArchiveView.xaml
/// </summary>
public partial class ArchiveView : Window
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

    private readonly ViewModels.ArchiveViewModel _viewModel;

    public ArchiveView()
    {
        InitializeComponent();

        _viewModel = (ViewModels.ArchiveViewModel)this.DataContext;
    }

    private void sfDataGridTransmittalItems_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
    {
        TransmittalItemModel itemModel = e.NewObject as TransmittalItemModel;
        itemModel.DrgProj = _settingsService.GlobalSettings.ProjectNumber;
        itemModel.DrgOriginator = _settingsService.GlobalSettings.Originator;
        itemModel.DrgRole = _settingsService.GlobalSettings.Role;
    }
}
