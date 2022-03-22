using Autodesk.Revit.UI;
using System;
using System.Windows;
using Transmittal.ViewModels;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for MainWindowView.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        var viewModel = new MainViewModel();
        this.DataContext = viewModel;

    }

}
