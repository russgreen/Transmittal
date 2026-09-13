using Autodesk.Revit.DB;
using Syncfusion.UI.Xaml.Grid;
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

            BuildDataGrid();

            _viewModel.PopulateParameterListCommand.Execute(category);
            _viewModel.ClosingRequest += (sender, e) => this.Close();
        }

        private void BuildDataGrid()
        {
            var sfDataGridParameterGrid = new SfDataGrid
            {
                ColumnSizer = GridLengthUnitType.AutoWithLastColumnFill,
                AllowEditing = false,
                SelectionMode = GridSelectionMode.Single,

                Columns =
                {
                    new GridTextColumn { MappingName = "Name", HeaderText = "Name", MinimumWidth = 200 },
                    new GridTextColumn { MappingName = "Guid", HeaderText = "GUID", MinimumWidth = 200 }
                }
            };

            sfDataGridParameterGrid.SetBinding(SfDataGrid.ItemsSourceProperty, new System.Windows.Data.Binding(nameof(ViewModels.ParameterSelectorViewModel.Parameters)));
            sfDataGridParameterGrid.SetBinding(SfDataGrid.SelectedItemProperty, new System.Windows.Data.Binding(nameof(ViewModels.ParameterSelectorViewModel.SelectedParameter)));

            sfDataGridParameterGridHost.Content = sfDataGridParameterGrid;
        }
    }
}
