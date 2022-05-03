using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Services;

namespace Transmittal.Forms;

public partial class FormNewPerson : Form
{
    readonly PersonModel _newContact;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;

    public FormNewPerson(PersonModel model)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
        //_settingsService.GetSettings();

        _newContact = model;

        //databind the textboxes
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

        if (this.textBoxLastName.Text.Trim().Length == 0 || this.textBoxFirstName.Text.Trim().Length == 0)
        {
            OK_Enabled = false;
        }

        if (OK_Enabled)
        {
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
