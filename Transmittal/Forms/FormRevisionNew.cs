using Autodesk.Revit.DB;
using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.Forms;

public partial class FormRevisionNew : System.Windows.Forms.Form
{
    private readonly IRevisionRequester _callingForm;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;
    
    public FormRevisionNew(IRevisionRequester caller)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
        //_settingsService.GetSettings();

        _callingForm = caller;

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        foreach (var i in Enum.GetValues(typeof(Autodesk.Revit.DB.RevisionNumberType)))
            comboBoxNumbering.Items.Add(i);
        comboBoxNumbering.SelectedIndex = 0;
#else
           //IEnumerable<ElementId> revisionNumberingSequences = RevisionNumberingSequence.GetAllRevisionNumberingSequences(App.revitDocument);
           var revisionNumberingSequences = RevisionNumberingSequence.GetAllRevisionNumberingSequences(App.RevitDocument).Select(App.RevitDocument.GetElement).ToList();

            comboBoxNumbering.DataSource = revisionNumberingSequences;
            comboBoxNumbering.DisplayMember = "Name";
            comboBoxNumbering.ValueMember = "Id";
            comboBoxNumbering.SelectedIndex = 0;
#endif
        
    }

    private void OK_button_Click(object sender, EventArgs e)
    {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new RevisionDataModel
        {
            RevDate = this.DateTimePicker1.Value.ToString(_settingsService.GlobalSettings.DateFormatString),
            Description = this.textBoxDescription.Text,
            IssuedBy = this.textBoxBy.Text,
            IssuedTo = this.textBoxTo.Text,
            Numbering = (RevisionNumberType)this.comboBoxNumbering.SelectedItem
        };

#else
        //create a new revision model & pupulate the values from the form
        RevisionDataModel revisionModel = new RevisionDataModel
            {
                RevDate = this.DateTimePicker1.Value.ToString(_settingsService.GlobalSettings.DateFormatString),
                Description = this.textBoxDescription.Text,
                IssuedBy = this.textBoxBy.Text,
                IssuedTo = this.textBoxTo.Text,
                SequenceId = (ElementId)this.comboBoxNumbering.SelectedValue
            };           
#endif

        _callingForm.RevisionComplete(revisionModel);
        this.Close();
    }

    private void Cancel_button_Click(object sender, EventArgs e)
    {
        Close();
    }
}
