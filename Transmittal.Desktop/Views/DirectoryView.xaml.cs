using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for DirectoryView.xaml
/// </summary>
public partial class DirectoryView : Window
{
    GridRowSizingOptions _gridRowResizingOptions = new GridRowSizingOptions();
    //To get the calculated height from GetAutoRowHeight method.    
    private double _autoHeight = double.NaN;

    public DirectoryView()
    {
        InitializeComponent();

        this.sfDataGridPeople.QueryRowHeight += sfDataGridPeople_QueryRowHeight;
        this.sfDataGridPeople.CurrentCellEndEdit += sfDataGridPeople_CurrentCellEndEdit;        
        this.sfDataGridCompanies.QueryRowHeight += sfDataGridCompanies_QueryRowHeight;
        this.sfDataGridCompanies.CurrentCellEndEdit += sfDataGridCompanies_CurrentCellEndEdit;

        this.sfDataGridPeople.SelectionController = new Controllers.GridCellSelectionControllerExt(this.sfDataGridPeople);
        this.sfDataGridCompanies.SelectionController = new Controllers.GridCellSelectionControllerExt(this.sfDataGridCompanies);
    }

    private void sfDataGridPeople_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs args)
    {
        sfDataGridPeople.InvalidateRowHeight(args.RowColumnIndex.RowIndex);
        sfDataGridPeople.GetVisualContainer().InvalidateMeasureInfo();
    }

    private void sfDataGridPeople_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
    {
        if (this.sfDataGridPeople.GridColumnSizer.GetAutoRowHeight(e.RowIndex, _gridRowResizingOptions, out _autoHeight))
        {
            if (e.RowIndex > 0)
            {
                if (_autoHeight > 24)
                {
                    e.Height = _autoHeight;
                    e.Handled = true;
                }
            }
        }
    }

    private void sfDataGridCompanies_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs args)
    {
        sfDataGridCompanies.InvalidateRowHeight(args.RowColumnIndex.RowIndex);
        sfDataGridCompanies.GetVisualContainer().InvalidateMeasureInfo();
    }

    private void sfDataGridCompanies_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
    {
        if (this.sfDataGridCompanies.GridColumnSizer.GetAutoRowHeight(e.RowIndex, _gridRowResizingOptions, out _autoHeight))
        {
            if (e.RowIndex > 0)
            {
                if (_autoHeight > 24)
                {
                    e.Height = _autoHeight;
                    e.Handled = true;
                }
            }
        }

    }
}
