using CommunityToolkit.Mvvm.DependencyInjection;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Windows;
using System.Windows.Controls;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Views;
/// <summary>
/// Interaction logic for DirectoryView.xaml
/// </summary>
public partial class DirectoryView : Window
{
    private readonly ITransmittalService _transmittalService = Host.GetService<ITransmittalService>();
    private readonly IContactDirectoryService _contactDirectoryService = Host.GetService<IContactDirectoryService>();

    GridRowSizingOptions _gridRowResizingOptions = new GridRowSizingOptions();
    //To get the calculated height from GetAutoRowHeight method.    
    private double _autoHeight = double.NaN;

    private readonly ViewModels.DirectoryViewModel _viewModel;

    public DirectoryView()
    {
        InitializeComponent();

        _viewModel = (ViewModels.DirectoryViewModel)this.DataContext;

        //this.sfDataGridPeople.QueryRowHeight += sfDataGridPeople_QueryRowHeight;
        //this.sfDataGridPeople.CurrentCellEndEdit += sfDataGridPeople_CurrentCellEndEdit;    
        //this.sfDataGridPeople.RecordDeleting += sfDataGridPeople_RecordDeleting;        

        //this.sfDataGridCompanies.QueryRowHeight += sfDataGridCompanies_QueryRowHeight;
        //this.sfDataGridCompanies.CurrentCellEndEdit += sfDataGridCompanies_CurrentCellEndEdit;
        //this.sfDataGridCompanies.RecordDeleting += sfDataGridPeople_RecordDeleting;

        //this.sfDataGridPeople.SelectionController = new Controllers.GridCellSelectionControllerExt(this.sfDataGridPeople);
        //this.sfDataGridCompanies.SelectionController = new Controllers.GridCellSelectionControllerExt(this.sfDataGridCompanies);
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
        TaskDialogButton deleteButton = new($"Remove the selected contact from the transmittal. This action cannot be undone.");
        TaskDialogButton cancelButton = new(ButtonType.Cancel);

        TaskDialog taskDialog = new()
        {
            WindowTitle = "Delete contact from transmittal",
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { deleteButton, cancelButton }
        };

        if (_viewModel.SelectedPerson != null)
        {
            if (_transmittalService.GetTransmittals_ByPerson(_viewModel.SelectedPerson.ID).Count == 0)
            {
                TaskDialogButton button = taskDialog.ShowDialog(this);
                if (button == deleteButton)
                {
                    _viewModel.RemovePersonCommand.Execute(null);
                    return;
                }
            }
        }

        e.Cancel = true;
    }

    private void sfDataGridCompanies_RecordDeleting(object sender, RecordDeletingEventArgs e)
    {
        TaskDialogButton deleteButton = new($"Remove the selected company from the database. This action cannot be undone.");
        TaskDialogButton cancelButton = new(ButtonType.Cancel);

        TaskDialog taskDialog = new()
        {
            WindowTitle = "Delete company",
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { deleteButton, cancelButton }
        };

        if (_viewModel.SelectedCompany != null)
        {
            if (_contactDirectoryService.GetPeople_ByCompany(_viewModel.SelectedCompany.ID).Count == 0)
            {
                TaskDialogButton button = taskDialog.ShowDialog(this);
                if (button == deleteButton)
                {
                    _viewModel.RemoveCompanyCommand.Execute(null);
                    return;
                }
            }
        }

        e.Cancel = true;
    }


}
