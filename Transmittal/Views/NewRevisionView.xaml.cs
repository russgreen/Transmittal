using System.Windows;
using Transmittal.Library.Services;
using Transmittal.Requesters;
using Transmittal.ViewModels;

namespace Transmittal.Views;

public partial class NewRevisionView : Window
{
    private readonly NewRevisionViewModel _viewModel;
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public NewRevisionView(IRevisionRequester caller)
    {
        InitializeComponent();

        _viewModel = new NewRevisionViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }
        
}
