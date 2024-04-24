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
        Process.Start(link.NavigateUri.OriginalString);
    }
}
