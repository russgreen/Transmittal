using Ookii.Dialogs.Wpf;
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

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for ProgressView.xaml
/// </summary>
public partial class ProgressView : Window
{
    private readonly ViewModels.ProgressViewModel _viewModel;

    public ProgressView()
    {
        InitializeComponent();

        _viewModel = (ViewModels.ProgressViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Ookii.Dialogs.Wpf.TaskDialogButton yesButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.Yes);
        Ookii.Dialogs.Wpf.TaskDialogButton noButton = new Ookii.Dialogs.Wpf.TaskDialogButton(ButtonType.No);

        Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog()
        {
            WindowTitle = "Cancel Transmittal",
            MainInstruction = "Are you sure you want to cancel?",
            MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Information,
            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.Standard,
            Buttons = { yesButton, noButton }
        };

        Ookii.Dialogs.Wpf.TaskDialogButton button = dialog.ShowDialog(this);
        if (button == yesButton)
        {
            this.CancelButton.IsEnabled = false;

            _viewModel.CancelTransmittal();
        }
    }
}
