using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Transmittal.Library.Services;
using Transmittal.Requesters;
using Transmittal.Services;
using Transmittal.ViewModels;

namespace Transmittal.Views;

public partial class RevisionsView : Window
{
    private readonly RevisionsViewModel _viewModel;
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public RevisionsView()
    {

    }

    public RevisionsView(IRevisionRequester caller)
    {
        InitializeComponent();

        var factory = Host.GetService<ICallingViewModelFactory>();
        _viewModel = factory.CreateRevisionsViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

        BuildDataGrid();
    }

    private void BuildDataGrid()
    {
        var sfDataGridRevisions = new SfDataGrid
        {
            AutoGenerateColumns = true,
            AllowGrouping = false,
            AllowResizingColumns = true,
            AllowFiltering = false,
            NavigationMode = NavigationMode.Row,
            SelectionMode =  GridSelectionMode.Single,
            GridValidationMode = GridValidationMode.InView,
            Margin = new Thickness(0, 0, 0, 0)
        };

        sfDataGridRevisions.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding(nameof(RevisionsViewModel.Revisions)));
        sfDataGridRevisions.SetBinding(SfDataGrid.SelectedItemProperty, new Binding(nameof(RevisionsViewModel.SelectedRevision)) { Mode = BindingMode.TwoWay });

        sfDataGridRevisions.Columns.Clear();
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Sequence", HeaderText = "Sequence", Width = 80 });
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Numbering", HeaderText = "Numbering", Width = 100 });
#else
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "SequenceName", HeaderText = "Numbering", Width = 100 });
#endif
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "RevDate", HeaderText = "Date", Width = 80 }); //, Pattern = Syncfusion.Windows.Shared.DateTimePattern.CustomPattern , CustomPattern = _settingsService.GlobalSettings.DateFormatString });
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Description", HeaderText = "Description",  MinimumWidth = 100 });
        sfDataGridRevisions.Columns.Add(new GridCheckBoxColumn() { MappingName = "Issued", HeaderText = "Issued", Width = 60 });
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "IssuedBy", HeaderText = "Issued By", Width = 80 });
        sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "IssuedTo", HeaderText = "Issued To", Width = 80 });

        sfDataGridRevisions.SelectionController = new GridSelectionControllerExt(sfDataGridRevisions);

        sfDataGridRevisionsHost.Content = sfDataGridRevisions;
    }

    private void ButtonAddRevision_Click(object sender, RoutedEventArgs e)
    {
        if(_viewModel.CanEditRevisions() == false)
        {
            return;
        }


        Views.NewRevisionView dialog = new Views.NewRevisionView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void TextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox box)
        {
            if (string.IsNullOrEmpty(box.Text))
            {
                box.Background = (ImageBrush)FindResource("watermark");
            }
            else
            {
                box.Background = null;
            }
        }

        var sfDataGridRevisions = sfDataGridRevisionsHost.Content as SfDataGrid;
        sfDataGridRevisions.SearchHelper.SearchBrush = Brushes.Green;
        sfDataGridRevisions.SearchHelper.AllowFiltering = true;
        sfDataGridRevisions.SearchHelper.Search(TextBoxSearch.Text);
    }
}

public class GridSelectionControllerExt : GridSelectionController
{
    public GridSelectionControllerExt(SfDataGrid datagrid)
        : base(datagrid)
    {
    }
    protected override void ProcessSelectedItemChanged(SelectionPropertyChangedHandlerArgs handle)
    {
        base.ProcessSelectedItemChanged(handle);
        if (handle.NewValue != null)
        {
            this.DataGrid.ScrollInView(this.CurrentCellManager.CurrentRowColumnIndex);
        }
    }

}
