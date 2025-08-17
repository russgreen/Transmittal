using System.Windows;
using System.Windows.Markup;
using Transmittal.Library.Services;
using Transmittal.Requesters;
using Transmittal.Services;
using Transmittal.ViewModels;

namespace Transmittal.Views;

public partial class NewRevisionView : Window
{
    private readonly NewRevisionViewModel _viewModel;

    public NewRevisionView(IRevisionRequester caller)
    {
        InitializeComponent();

        var factory = Host.GetService<ICallingViewModelFactory>();
        _viewModel = factory.CreateNewRevisionViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }
        
}
