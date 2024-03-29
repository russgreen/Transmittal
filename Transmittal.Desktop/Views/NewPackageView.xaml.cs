﻿using System.Windows;
using Transmittal.Desktop.Requesters;
using Transmittal.Desktop.ViewModels;

namespace Transmittal.Desktop.Views
{
    /// <summary>
    /// Interaction logic for NewPackageView.xaml
    /// </summary>
    public partial class NewPackageView : Window
    {
        private readonly NewPackageViewModel _viewModel;

        public NewPackageView(IPackageRequester caller)
        {
            InitializeComponent();

            _viewModel = new NewPackageViewModel(caller);
            this.DataContext = _viewModel;
            _viewModel.ClosingRequest += (sender, e) => this.Close();
        }
    }
}
