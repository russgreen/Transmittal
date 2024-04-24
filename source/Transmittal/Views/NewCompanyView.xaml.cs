using System.Windows;
using Transmittal.Requesters;
using Transmittal.ViewModels;

namespace Transmittal.Views;
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
