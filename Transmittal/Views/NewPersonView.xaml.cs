using System.Windows;
using Transmittal.Requesters;
using Transmittal.ViewModels;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for NewPersonView.xaml
/// </summary>
public partial class NewPersonView : Window
{
    private readonly NewPersonViewModel _viewModel;
    public NewPersonView(IPersonRequester caller)
    {
        InitializeComponent();

        _viewModel = new NewPersonViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

    }

    private void Button_AddCompany_Click(object sender, RoutedEventArgs e)
    {
        Views.NewCompanyView dialog = new Views.NewCompanyView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }
}
