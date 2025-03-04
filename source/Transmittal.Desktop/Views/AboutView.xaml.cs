using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for AboutView.xaml
/// </summary>
public partial class AboutView : Window
{
    public AboutView()
    {
        InitializeComponent();
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
