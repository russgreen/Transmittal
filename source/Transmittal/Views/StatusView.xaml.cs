using Autodesk.Revit.DB;
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
using Transmittal.Requesters;
using Transmittal.Services;
using Transmittal.ViewModels;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for StatusView.xaml
/// </summary>
public partial class StatusView : Window
{
    private readonly StatusViewModel _viewModel;
        
    public StatusView(IStatusRequester caller)
    {
        InitializeComponent();

        var factory = Host.GetService<ICallingViewModelFactory>();
        _viewModel = factory.CreateStatusViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }
}
