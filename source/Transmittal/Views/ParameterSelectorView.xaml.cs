using Autodesk.Revit.DB;
using System.Windows;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.Views
{
    /// <summary>
    /// Interaction logic for ParameterSelectorView.xaml
    /// </summary>
    public partial class ParameterSelectorView : Window
    {
        private readonly ViewModels.ParameterSelectorViewModel _viewModel;

        public ParameterSelectorView()
        {

        }

        public ParameterSelectorView(IParameterGuidRequester caller,  string targetVariable, BuiltInCategory category)
        {
            InitializeComponent();

            var factory = Host.GetService<ICallingViewModelFactory>();
            _viewModel = factory.CreateParameterSelectorViewModel(caller, targetVariable);
            this.DataContext = _viewModel;
            _viewModel.PopulateParameterListCommand.Execute(category);
            _viewModel.ClosingRequest += (sender, e) => this.Close();
        }
    }
}
