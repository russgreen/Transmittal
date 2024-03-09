using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Media;
using Transmittal.Library.Services;
using Transmittal.Requesters;
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

        _viewModel = new RevisionsViewModel(caller);
        this.DataContext = _viewModel;
        _viewModel.ClosingRequest += (sender, e) => this.Close();

        BuildDataGrid();
    }

    private void BuildDataGrid()
    {
        this.sfDataGridRevisions.Columns.Clear();
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Sequence", HeaderText = "Sequence", Width = 80 });
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Numbering", HeaderText = "Numbering", Width = 100 });
#else
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "SequenceName", HeaderText = "Numbering", Width = 100 });
#endif
        this.sfDataGridRevisions.Columns.Add(new GridDateTimeColumn() { MappingName = "RevDate", HeaderText = "Date", Width = 80, Pattern = Syncfusion.Windows.Shared.DateTimePattern.CustomPattern , CustomPattern = _settingsService.GlobalSettings.DateFormatString });
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "Description", HeaderText = "Description",  MinimumWidth = 100 });
        this.sfDataGridRevisions.Columns.Add(new GridCheckBoxColumn() { MappingName = "Issued", HeaderText = "Issued", Width = 60 });
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "IssuedBy", HeaderText = "Issued By", Width = 80 });
        this.sfDataGridRevisions.Columns.Add(new GridTextColumn() { MappingName = "IssuedTo", HeaderText = "Issued To", Width = 80 });
    }

    private void ButtonAddRevision_Click(object sender, RoutedEventArgs e)
    {
        Views.NewRevisionView dialog = new Views.NewRevisionView(_viewModel);
        dialog.Owner = this;
        dialog.ShowDialog();
    }

    private void TextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox box)
        {
            if (string.IsNullOrEmpty(box.Text))
                box.Background = (ImageBrush)FindResource("watermark");
            else
                box.Background = null;
        }

        this.sfDataGridRevisions.SearchHelper.SearchBrush = Brushes.Green;
        this.sfDataGridRevisions.SearchHelper.AllowFiltering = true;
        this.sfDataGridRevisions.SearchHelper.Search(this.TextBoxSearch.Text);
    }
}
