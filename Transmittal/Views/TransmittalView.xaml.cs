using Autodesk.Revit.DB;
using Autodesk.Windows;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Transmittal.Library.Models;
using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for TransmittalView.xaml
/// </summary>
public partial class TransmittalView : Window
{
    private readonly ViewModels.TransmittalViewModel _viewModel;

    public TransmittalView()
    {
        InitializeComponent();
           
        _viewModel = (ViewModels.TransmittalViewModel)this.DataContext;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

        BuildSheetsDataGrid();
        BuildDirectoryDataGrids();
    }

    private void BuildSheetsDataGrid()
    {
        this.sfDataGridSheets.Columns.Clear();
        this.sfDataGridSheets.Columns.Add(new GridCheckBoxSelectorColumn() { MappingName = "SelectorColumn", HeaderText = string.Empty, AllowCheckBoxOnHeader = false, Width = 34}); //, CheckBoxSize = new Size(14, 14) 
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgNumber", HeaderText = "Number", Width = 100 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgRev", HeaderText = "Revision", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgName", HeaderText = "Name", Width = 250 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgVolume", HeaderText = "Volume", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgLevel", HeaderText = "Level", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgType", HeaderText = "Type", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgStatus", HeaderText = "Status", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "StatusDescription", HeaderText = "Status Description", Width = 120 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgPaper", HeaderText = "Paper", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgScale", HeaderText = "Scale", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "IssueDate", HeaderText = "Issue Date", Width = 100 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgDrawn", HeaderText = "Dr", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgChecked", HeaderText = "Ch", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "RevNotes", HeaderText = "Rev Notes", MinimumWidth = 300 });
    }

    private void BuildDirectoryDataGrids()
    {
        this.sfDataGridDirectory.Columns.Clear();
        this.sfDataGridDirectory.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListModel.CompanyName", HeaderText = "Company" });
        this.sfDataGridDirectory.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListContactModel.FullNameReversed", HeaderText = "Person" });

        this.sfDataGridDistribution.Columns.Clear();
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListModel.CompanyName", HeaderText = "Company" });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListContactModel.FullNameReversed", HeaderText = "Person" });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "TransCopies", HeaderText = "Copies", Width = 60 });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "TransFormat", HeaderText = "Format", Width = 60 });
    }

    private void WizardControl_Help(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("https://russgreen.github.io/transmittal/");
    }

    private void WizardControl_Cancel(object sender, RoutedEventArgs e)
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
            _viewModel.AbortFlag = true;
            if (_viewModel.Processingsheets == false)
            {
                this.Close();
            }
        }
    }    
    
    private void sfDataGridSheets_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        // couldn't get the databinding to work on the selecteditems property.
        // TODO investigate why not working
        var items = sfDataGridSheets.SelectedItems;
        _viewModel.SelectedDrawingSheets.Clear();

        if (items.Count > 0)
        {
            //viewModel.SheetsSelected = true;
            foreach (var item in items)
            {
                _viewModel.SelectedDrawingSheets.Add((DrawingSheetModel)item);
            }
        }
    }

    private void ButtonRevise_Click(object sender, RoutedEventArgs e)
    {
        Views.RevisionsView dialog = new Views.RevisionsView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void ButtonStatus_Click(object sender, RoutedEventArgs e)
    {
        Views.StatusView dialog = new Views.StatusView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();

        this.sfDataGridSheets.View.Refresh();
    }

    private void Button_AddToDirectory_Click(object sender, RoutedEventArgs e)
    {

    }
}
