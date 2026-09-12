using Ookii.Dialogs.Wpf;
using SfDatagrid.WPF.Extensions;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Transmittal.Converters;
using Transmittal.Library.Services;
using Transmittal.Models;

namespace Transmittal.Views;
/// <summary>
/// Interaction logic for TransmittalView.xaml
/// </summary>
public partial class TransmittalView : Window
{
    private readonly ViewModels.TransmittalViewModel _viewModel;
    private readonly ISettingsService _settingsService;
    private SfDataGrid sfDataGridSheets;

    public TransmittalView()
    {
        InitializeComponent();

        _viewModel = Host.GetService<ViewModels.TransmittalViewModel>();
        _settingsService = Host.GetService<ISettingsService>();
        DataContext = _viewModel;

        BuildDataGridSheets();
        BuildDataGridDirectory();
        BuildDataGridDistribution();

        _viewModel.ClosingRequest += (sender, e) => this.Close();

#if REVIT2025_OR_GREATER
        sfDataGridSheets.GroupColumnDescriptions.Add(new GroupColumnDescription() { ColumnName = "DrgSheetCollection" });
        sfDataGridSheets.Columns["DrgSheetCollection"].GroupMode = DataReflectionMode.Display;
        sfDataGridSheets.AutoExpandGroups = true;
        sfDataGridSheets.AllowFrozenGroupHeaders = true;
#endif

        sfDataGridSheets.EnableCtrlDragFill(requiredModifiers: ModifierKeys.Alt);

        var column = sfDataGridSheets.Columns["IssueDate"] as Syncfusion.UI.Xaml.Grid.GridDateTimeColumn;
        if (column != null)
        {
            column.Pattern = Syncfusion.Windows.Shared.DateTimePattern.CustomPattern;
            column.CustomPattern  = _settingsService.GlobalSettings.DateFormatString;
        }
    }

    private void BuildDataGridSheets()
    {
        sfDataGridSheets = new SfDataGrid
        {
            AutoGenerateColumns = false,
            AllowEditing = true,
            AllowDeleting = false,
            AllowGrouping = false,
            AllowResizingColumns = true,
            AllowFiltering = true,
            AllowSorting = true,
            NavigationMode = NavigationMode.Cell,
            SelectionMode =  GridSelectionMode.Extended,
            GridValidationMode = GridValidationMode.InView,
            Margin = new Thickness(0, 10, 0, 0)
        };

        sfDataGridSheets.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding(nameof(ViewModels.TransmittalViewModel.DrawingSheets)));
        sfDataGridSheets.SetBinding(SfDataGrid.SelectedItemsProperty, new Binding(nameof(ViewModels.TransmittalViewModel.SelectedDrawingSheets))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var duplicateStyle = new Style(typeof(GridCell));
        var duplicateTrigger = new DataTrigger
        {
            Binding = new Binding(nameof(DrawingSheetModel.IsDuplicateSheet)),
            Value = true
        };
        duplicateTrigger.Setters.Add(new Setter(GridCell.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xF8, 0xD7, 0xDA))));
        duplicateTrigger.Setters.Add(new Setter(GridCell.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0x8B, 0x1E, 0x1E))));
        duplicateTrigger.Setters.Add(new Setter(GridCell.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0xB2, 0x22, 0x34))));
        duplicateTrigger.Setters.Add(new Setter(GridCell.BorderThicknessProperty, new Thickness(1)));
        duplicateStyle.Triggers.Add(duplicateTrigger);
        sfDataGridSheets.Resources["DuplicateSheetGridCellStyle"] = duplicateStyle;

        sfDataGridSheets.SortColumnDescriptions.Add(new SortColumnDescription { ColumnName = "DrgNumber", SortDirection = ListSortDirection.Ascending });

        var selectorColumn = new GridCheckBoxSelectorColumn
        {
            MappingName = "SelectorColumn",
            HeaderText = string.Empty,
            AllowCheckBoxOnHeader = false,
            Width = 34,
        };
        selectorColumn.IsHidden = _viewModel.EnablePerSheetExportFormats;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.TransmittalViewModel.EnablePerSheetExportFormats))
            {
                selectorColumn.IsHidden = _viewModel.EnablePerSheetExportFormats;
            }
        };

        sfDataGridSheets.Columns.Add(selectorColumn);

        var pdfColumn = new GridImageColumn
        {
            MappingName = "ExportPDF",
            HeaderText = string.Empty,
            Width = 24,
            AllowEditing = false,
            AllowFiltering = false,
            ValueBinding = new Binding(nameof(DrawingSheetModel.ExportPDF))
            {
                Converter = new BoolToObjectConverter
                {
                    TrueValue = "/Transmittal;component/Resources/pdfFile.png",
                    FalseValue = string.Empty,
                    CanConvertToTargetType = false
                }
            }
        };

        pdfColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.TransmittalViewModel.EnablePerSheetExportFormats))
            {
                pdfColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
            }
        };

        sfDataGridSheets.Columns.Add(pdfColumn);

        var dwgColumn = new GridImageColumn
        {
            MappingName = "ExportDWG",
            HeaderText = string.Empty,
            Width = 24,
            AllowEditing = false,
            AllowFiltering = false,
            ValueBinding = new Binding(nameof(DrawingSheetModel.ExportDWG))
            {
                Converter = new BoolToObjectConverter
                {
                    TrueValue = "/Transmittal;component/Resources/dwgFile.png",
                    FalseValue = string.Empty,
                    CanConvertToTargetType = false
                }
            }
        };

        dwgColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.TransmittalViewModel.EnablePerSheetExportFormats))
            {
                dwgColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
            }
        };

        sfDataGridSheets.Columns.Add(dwgColumn);

        var dwfColumn = new GridImageColumn
        {
            MappingName = "ExportDWF",
            HeaderText = string.Empty,
            Width = 24,
            AllowEditing = false,
            AllowFiltering = false,
            ValueBinding = new Binding(nameof(DrawingSheetModel.ExportDWF))
            {
                Converter = new BoolToObjectConverter
                {
                    TrueValue = "/Transmittal;component/Resources/dwfFile.png",
                    FalseValue = string.Empty,
                    CanConvertToTargetType = false
                }
            }
        };

        dwfColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.TransmittalViewModel.EnablePerSheetExportFormats))
            {
                dwfColumn.IsHidden = !_viewModel.EnablePerSheetExportFormats;
            }
        };


        sfDataGridSheets.Columns.Add(dwfColumn);

        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgNumber", HeaderText = "Number", Width = 100, AllowEditing = false, CellStyle = (Style)duplicateStyle });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgRev", HeaderText = "Revision", Width = 50, AllowEditing = false });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgName", HeaderText = "Name", Width = 250 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgVolume", HeaderText = "Volume / Functional", Width = 50 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgLevel", HeaderText = "Level / Spatial", Width = 50 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgType", HeaderText = "Type", Width = 50, CellStyle = (Style)duplicateStyle });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgStatus", HeaderText = "Status", Width = 50, AllowEditing = false });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgStatusDescription", HeaderText = "Status Description", Width = 120, AllowEditing = false });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgPackage", HeaderText = "Package", Width = 120 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgSheetCollection", HeaderText = "Sheet Collection", Width = 120, IsHidden = true });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgScale", HeaderText = "Scale", Width = 75, AllowEditing = false });
        sfDataGridSheets.Columns.Add(new GridDateTimeColumn { HeaderText = "Date", MappingName = "IssueDate", Width = 100 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgDrawn", HeaderText = "Dr", Width = 75 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "DrgChecked", HeaderText = "Ch", Width = 75 });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "RevDate", HeaderText = "Rev Date", Width = 100, AllowEditing = false });
        sfDataGridSheets.Columns.Add(new GridTextColumn { MappingName = "RevNotes", HeaderText = "Rev Notes", MinimumWidth = 300, AllowEditing = false });

        sfDataGridSheetsHost.Content = sfDataGridSheets;

        sfDataGridSheets.CurrentCellValidated += sfDataGridSheets_CurrentCellValidated;
    }

    private void BuildDataGridDistribution()
    {
        var sfDataGridDirectory = new SfDataGrid
        {
            AutoGenerateColumns = false,
            AllowEditing = false,
            AllowDeleting = false,
            AllowGrouping = false,
            AllowResizingColumns = true,
            AllowFiltering = true,
            AllowSorting = true,
            NavigationMode = NavigationMode.Row,
            SelectionMode = GridSelectionMode.Extended,
            ColumnSizer =  GridLengthUnitType.AutoWithLastColumnFill,

            Columns =
            {
                new GridTextColumn { HeaderText = "Company", MappingName = "Company.CompanyName" },
                new GridTextColumn { HeaderText = "Person", MappingName = "Person.FullNameReversed" }
            }
        };

        sfDataGridDirectory.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding(nameof(ViewModels.TransmittalViewModel.ProjectDirectory)));
        sfDataGridDirectory.SetBinding(SfDataGrid.SelectedItemsProperty, new Binding(nameof(ViewModels.TransmittalViewModel.SelectedProjectDirectory))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        sfDataGridDirectory.SetBinding(SfDataGrid.IsEnabledProperty, new Binding("IsChecked")
        {
            Source = RecordIssue
        });

        sfDataGridDirectoryHost.Content = sfDataGridDirectory;
    }

    private void BuildDataGridDirectory()
    {
        var sfDataGridDistribution = new SfDataGrid
        {
            AutoGenerateColumns = false,
            AllowEditing = false,
            AllowDeleting = false,
            AllowGrouping = false,
            AllowResizingColumns = true,
            AllowFiltering = true,
            AllowSorting = true,
            NavigationMode = NavigationMode.Row,
            SelectionMode = GridSelectionMode.Extended,
            ColumnSizer = GridLengthUnitType.AutoWithLastColumnFill,
            Columns =
            {
                new GridTextColumn { HeaderText = "Company", MappingName = "Company.CompanyName" },
                new GridTextColumn { HeaderText = "Person", MappingName = "Person.FullNameReversed" },
                new GridTextColumn { HeaderText = "Copies", MappingName = "TransCopies" },
                new GridTextColumn { HeaderText = "Format", MappingName = "TransFormat" }
            }
        };

        sfDataGridDistribution.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding(nameof(ViewModels.TransmittalViewModel.Distribution)));
        sfDataGridDistribution.SetBinding(SfDataGrid.SelectedItemsProperty, new Binding(nameof(ViewModels.TransmittalViewModel.SelectedDistribution))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        sfDataGridDistribution.SetBinding(SfDataGrid.IsEnabledProperty, new Binding("IsChecked")
        {
            Source = RecordIssue
        });

        sfDataGridDistributionHost.Content = sfDataGridDistribution;
    }

    private void WizardControl_Help(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://russgreen.github.io/Transmittal/revit-addin/",
            UseShellExecute = true
        });
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
        //TODO stop the main window closing if the no button is clicked
    }

    private async void WizardControl_Finish(object sender, RoutedEventArgs e)
    {
        _viewModel.IsFinishEnabled = false;

        var conflicts = await _viewModel.GetCurrentFileConflicts();
        var action = FileConflictAction.Overwrite;

        if (conflicts.Count > 0)
        {
            action = ShowFileConflictDialog(conflicts);
        }

        var shouldContinue = _viewModel.ApplyFileConflictAction(action, conflicts);
        if (!shouldContinue)
        {
            _viewModel.IsFinishEnabled = true;

            if (action == FileConflictAction.ReviseSheets)
            {
                wizardControl.SelectedWizardPage = wizardPage1;
            }

            return;
        }

        _viewModel.ProcessSheetsCommand.Execute(null);
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
        Views.NewPersonView dialog = new Views.NewPersonView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void RecordIssue_Unchecked(object sender, RoutedEventArgs e)
    {
        if(this.RecordIssue.IsChecked == false)
        {
            _viewModel.SelectedProjectDirectory.Clear();
        }
    }

    private void sfDataGridSheets_CurrentCellValidated(object sender, CurrentCellValidatedEventArgs e)
    {
        if (e.NewValue != e.OldValue)
        {
            DrawingSheetModel sheet = e.RowData as DrawingSheetModel;
            if (sheet != null)
            {
                _viewModel.UpdateSheet(sheet);
            }
        }
    }

    private void CopiesTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !IsPositiveInt((TextBox)sender, e.Text);
    }

    private void CopiesTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
        {
            e.CancelCommand();
            return;
        }

        var pasteText = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
        if (!IsPositiveInt((TextBox)sender, pasteText))
        {
            e.CancelCommand();
        }
    }

    private bool IsPositiveInt(TextBox textBox, string newText)
    {
        var positiveIntRegex = new Regex(@"^[1-9]\d*$");

        var proposed = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, newText);

        return string.IsNullOrEmpty(proposed) || positiveIntRegex.IsMatch(proposed);
    }

    private FileConflictAction ShowFileConflictDialog(IReadOnlyCollection<ExportFileCheckResult> conflicts)
    {
        var overwriteButton = new TaskDialogButton("Continue and overwrite existing files");
        var useExistingButton = new TaskDialogButton("Continue and use existing exported files");
        var reviseButton = new TaskDialogButton("Revise selected sheets");
        var cancelButton = new TaskDialogButton(ButtonType.Cancel);

        var taskDialog = new TaskDialog
        {
            WindowTitle = "Export file conflicts detected",
            MainInstruction = $"{conflicts.Count} export file(s) already exist.",
            Content = BuildConflictContent(conflicts),
            MainIcon = TaskDialogIcon.Warning,
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            Buttons = { overwriteButton, useExistingButton, reviseButton, cancelButton }
        };

        var button = taskDialog.ShowDialog(this);

        if (button == overwriteButton)
        {
            return FileConflictAction.Overwrite;
        }

        if (button == useExistingButton)
        {
            return FileConflictAction.UseExisting;
        }

        if (button == reviseButton)
        {
            return FileConflictAction.ReviseSheets;
        }

        return FileConflictAction.Cancel;
    }

    private static string BuildConflictContent(IReadOnlyCollection<ExportFileCheckResult> conflicts)
    {
        var lines = conflicts
            .Take(20)
            .Select(x => $"{x.SheetNumber} ({x.ExportFormat})");

        var content = string.Join(Environment.NewLine, lines);
        if (conflicts.Count > 20)
        {
            content = $"{content}{Environment.NewLine}... and {conflicts.Count - 20} more";
        }

        return content;
    }
}
