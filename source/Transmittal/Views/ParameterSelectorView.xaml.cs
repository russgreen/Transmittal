using Autodesk.Revit.DB;
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
using Transmittal.Requesters;

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

            _viewModel = new ViewModels.ParameterSelectorViewModel(caller, targetVariable);

            //_viewModel = (ViewModels.ParameterSelectorViewModel)this.DataContext;
            this.DataContext = _viewModel;
            _viewModel.PopulateParameterListCommand.Execute(category);
            _viewModel.ClosingRequest += (sender, e) => this.Close();
        }
    }
}
