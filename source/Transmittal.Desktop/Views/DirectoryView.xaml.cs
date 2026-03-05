using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for DirectoryView.xaml
/// </summary>
public partial class DirectoryView : Window
{
    private readonly ViewModels.DirectoryViewModel _viewModel;
    private readonly ITransmittalService _transmittalService;
    private readonly IContactDirectoryService _contactDirectoryService;

    GridRowSizingOptions _gridRowResizingOptions = new GridRowSizingOptions();
    //To get the calculated height from GetAutoRowHeight method.    
    private double _autoHeight = double.NaN;


    public DirectoryView()
    {
        InitializeComponent();

        _transmittalService = Host.GetService<ITransmittalService>();
        _contactDirectoryService = Host.GetService<IContactDirectoryService>();
        _viewModel = Host.GetService<ViewModels.DirectoryViewModel>();

        DataContext = _viewModel;
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

    private void sfDataGridPeople_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        // Only allow delete if a person is selected and not referenced by any transmittals.
        if (_viewModel.SelectedPerson == null ||
            _transmittalService.GetTransmittals_ByPerson(_viewModel.SelectedPerson.ID).Count != 0)
        {
            e.Cancel = true;
            return;
        }

        // Build TaskDialog (System.Windows.Forms) page with command link style.
        var deleteButton = new System.Windows.Forms.TaskDialogCommandLinkButton(
            "Remove the selected contact from the transmittal. This action cannot be undone.");

        var page = new System.Windows.Forms.TaskDialogPage
        {
            Caption = "Delete contact from transmittal",
            Buttons = { deleteButton, System.Windows.Forms.TaskDialogButton.Cancel }
        };

        // Show with WPF owner via window handle.
        var ownerHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        var result = System.Windows.Forms.TaskDialog.ShowDialog(ownerHandle, page);

        if (result == deleteButton)
        {
            _viewModel.RemovePersonCommand.Execute(null);
            return;
        }

        e.Cancel = true;
    }

    private void sfDataGridCompanies_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        // Only allow delete if a company is selected and not referenced by any people.
        if (_viewModel.SelectedCompany == null ||
            _contactDirectoryService.GetPeople_ByCompany(_viewModel.SelectedCompany.ID).Count != 0)
        {
            e.Cancel = true;
            return;
        }

        var deleteButton = new System.Windows.Forms.TaskDialogCommandLinkButton(
            "Remove the selected company from the database. This action cannot be undone.");

        var page = new System.Windows.Forms.TaskDialogPage
        {
            Caption = "Delete company",
            Buttons = { deleteButton, System.Windows.Forms.TaskDialogButton.Cancel }
        };

        var ownerHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        var result = System.Windows.Forms.TaskDialog.ShowDialog(ownerHandle, page);

        if (result == deleteButton)
        {
            _viewModel.RemoveCompanyCommand.Execute(null);
            return;
        }

        e.Cancel = true;
    }


}
