using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Services;

namespace Transmittal.Forms;

public partial class FormNewCompany : Form
{
    readonly CompanyModel _newCompany;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;
    PersonModel _newContact = new();


    public FormNewCompany(CompanyModel model)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
        //_settingsService.GetSettings();

        _newCompany = model;

        //databind the textboxes
        this.textBoxCompanyName.DataBindings.Add("Text", _newCompany, "CompanyName", false, DataSourceUpdateMode.OnValidation);
        this.textBoxCompanyDiscipline.DataBindings.Add("Text", _newCompany, "CompanyDiscipline", false, DataSourceUpdateMode.OnValidation);
        this.textBoxWebsite.DataBindings.Add("Text", _newCompany, "Website", false, DataSourceUpdateMode.OnValidation);
        this.textBoxAddress1.DataBindings.Add("Text", _newCompany, "Address1", false, DataSourceUpdateMode.OnValidation);
        this.textBoxAddress2.DataBindings.Add("Text", _newCompany, "Address2", false, DataSourceUpdateMode.OnValidation);
        this.textBoxCity.DataBindings.Add("Text", _newCompany, "City", false, DataSourceUpdateMode.OnValidation);
        this.textBoxRegion.DataBindings.Add("Text", _newCompany, "Region", false, DataSourceUpdateMode.OnValidation);
        this.textBoxCountry.DataBindings.Add("Text", _newCompany, "Country", false, DataSourceUpdateMode.OnValidation);
        this.textBoxPostCode.DataBindings.Add("Text", _newCompany, "PostalCode", false, DataSourceUpdateMode.OnValidation);
        this.textBoxTelephone.DataBindings.Add("Text", _newCompany, "Tel", false, DataSourceUpdateMode.OnValidation);

        this.textBoxFirstName.DataBindings.Add("Text", _newContact, "FirstName", false, DataSourceUpdateMode.OnValidation);
        this.textBoxLastName.DataBindings.Add("Text", _newContact, "LastName", false, DataSourceUpdateMode.OnValidation);
        this.textBoxEmail.DataBindings.Add("Text", _newContact, "Email", false, DataSourceUpdateMode.OnValidation);
        this.textBoxDDI.DataBindings.Add("Text", _newContact, "DDI", false, DataSourceUpdateMode.OnValidation);
        this.textBoxMobile.DataBindings.Add("Text", _newContact, "Mobile", false, DataSourceUpdateMode.OnValidation);
        this.textBoxPosition.DataBindings.Add("Text", _newContact, "Position", false, DataSourceUpdateMode.OnValidation);
        this.textBoxNotes.DataBindings.Add("Text", _newContact, "Notes", false, DataSourceUpdateMode.OnValidation);
    }

    private void OK_button_Click(object sender, EventArgs e)
    {
        bool OK_Enabled = true;

        //validate the form
        //TODO_LOW - improve model validation using attributes
        if (this.textBoxCompanyName.Text.Trim().Length == 0)
        {
            OK_Enabled = false;
        }

        if (this.textBoxLastName.Text.Trim().Length == 0 || this.textBoxFirstName.Text.Trim().Length == 0)
        {
            OK_Enabled = false;
        }

        if (OK_Enabled)
        {
            //save the records to the DB
            //App.contactDirectoryService.CreateApprovedListCompany(_newCompany);
            //_newContact.ApprovedListID = _newCompany.ApprovedListID;
            //App.contactDirectoryService.CreateApprovedListContact(_newContact);

            this.DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void Cancel_button_Click(object sender, EventArgs e)
    {
        Close();
    }
}
