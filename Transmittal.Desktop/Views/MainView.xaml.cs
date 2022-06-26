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

namespace Transmittal.Desktop.Views
{
    /// <summary>
    /// Interaction logic for TransmittalView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Button_Transmittal_Click(object sender, RoutedEventArgs e)
        {
            TransmittalView view = new();
            view.ShowDialog();
        }

        private void Button_TransmittalArchive_Click(object sender, RoutedEventArgs e)
        {
            ArchiveView view = new();
            view.ShowDialog();
        }

        private void Button_Directory_Click(object sender, RoutedEventArgs e)
        {
            DirectoryView view = new();
            view.ShowDialog();
        }
    }
}
