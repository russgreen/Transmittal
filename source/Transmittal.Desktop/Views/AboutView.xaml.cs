using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for AboutView.xaml
/// </summary>
public partial class AboutView : Window
{

    private readonly ViewModels.AboutViewModel _viewModel;

    public AboutView()
    {
        InitializeComponent();

        _viewModel = Host.GetService<ViewModels.AboutViewModel>();
        DataContext = _viewModel;
    }

    private void OpenLink(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not Hyperlink link) return;


        var uri = link.NavigateUri;
        if (uri == null) return;

        var psi = new ProcessStartInfo
        {
            FileName = uri.OriginalString,
            UseShellExecute = true
        };

        Process.Start(psi);
    }
}
