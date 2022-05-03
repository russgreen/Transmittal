using Autodesk.Revit.DB;
using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.Forms;

public partial class FormRevisions : System.Windows.Forms.Form, IRevisionRequester
{
    private readonly IRevisionRequester _callingForm;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;

    private IList<ElementId> _ids;

    public FormRevisions(IRevisionRequester caller)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
        //_settingsService.GetSettings();

        _callingForm = caller;

        CreateGridColumns();
        LoadRevisions();
    }

    private void OK_button_Click(object sender, EventArgs e)
    {
        //check we have a revision selected in the grid
        if (this.sfDataGrid1.SelectedItems.Count == 1)
        {
            //if we do then pass the revision back
            _callingForm.RevisionComplete((RevisionDataModel)this.sfDataGrid1.SelectedItem);
            this.Close();
        }
    }

    private void Cancel_button_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        FormRevisionNew frm = new FormRevisionNew(this);
        frm.ShowDialog(this);
    }

    private void CreateGridColumns()
    {
        this.sfDataGrid1.Columns.Clear();
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "Sequence", HeaderText = "Sequence", Width = 80 });
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "Numbering", HeaderText = "Numbering", Width = 100 });
#else
            this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "SequenceName", HeaderText = "Numbering", Width = 100 });
#endif
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridDateTimeColumn() { MappingName = "RevDate", HeaderText = "Date", Width = 80, Format = _settingsService.GlobalSettings.DateFormatString });
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "Description", HeaderText = "Description", MinimumWidth = 100 });
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridCheckBoxColumn() { MappingName = "Issued", HeaderText = "Issued", Width = 60 });
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "IssuedBy", HeaderText = "Issued By", Width = 80 });
        this.sfDataGrid1.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "IssuedTo", HeaderText = "Issued To", Width = 80 });
    }

    private void LoadRevisions()
    {
        _ids = Revision.GetAllRevisionIds(App.RevitDocument);
        int n = _ids.Count;
        var revision_data = new List<RevisionDataModel>(n);
        foreach (ElementId id in _ids)
        {
            Revision r = (Revision)App.RevitDocument.GetElement(id);
            revision_data.Add(new RevisionDataModel(r));
        }

        this.sfDataGrid1.DataSource = revision_data;
    }

    public void RevisionComplete(RevisionDataModel model)
    {
        //save the new revision into the model
        Transaction trans = null;
        try
        {
            trans = new Transaction(App.RevitDocument, "Create Revision");
            //var failOpt = trans.GetFailureHandlingOptions();
            //failOpt.SetFailuresPreprocessor(new WarningSwallower());
            //trans.SetFailureHandlingOptions(failOpt);
            trans.Start();
            var newRevision = Revision.Create(App.RevitDocument);
            newRevision.Description = model.Description;
            newRevision.IssuedBy = model.IssuedBy;
            newRevision.IssuedTo = model.IssuedTo;
            newRevision.RevisionDate = model.RevDate;
            //newRevision.NumberType = model.Numbering;

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
            //no sequence ID until 2022
            newRevision.NumberType = model.Numbering;
#else
            newRevision.RevisionNumberingSequenceId = model.SequenceId;
#endif

            trans.Commit();
        }
        catch (Exception)
        {
            trans.RollBack();
        }

        LoadRevisions();

    }
}
