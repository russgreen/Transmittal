using System.Windows;
using Transmittal.Desktop.Requesters;
using Transmittal.Desktop.ViewModels;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for NewCompanyView.xaml
/// </summary>
public partial class NewCompanyView : Window
{
    private readonly NewCompanyViewModel _viewModel;
    
    public NewCompanyView(ICompanyRequester caller)
    {
        InitializeComponent();
        
        _viewModel = new NewCompanyViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }
}
