
using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Services;

namespace Transmittal.Forms;

public partial class FormProjectDirectoryNew : Form
{
    private readonly List<ProjectDirectoryModel> _projectDirectoryList;
    private ProjectDirectoryModel _projectDirectory = new();

    private readonly bool _isFormLoaded = false;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;

    public FormProjectDirectoryNew(List<ProjectDirectoryModel> projectDirectoryList)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();
        //_settingsService.GetSettings();

        _projectDirectoryList = projectDirectoryList;

        //load companies to the combo
        LoadCompaniesCombo();

        this.comboBoxCompanies.SelectedItem = null;

        _isFormLoaded = true;
    }

    private void ComboBoxCompanies_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isFormLoaded)
        {
            //load contacts to the combo
            LoadContactsCombo();

            this.buttonAddContact.Enabled = true;


        }
    }

    private void ComboBoxContacts_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if (_isFormLoaded)
        //{
        //    //check if the approved list ID is already in the project directory
        //    //bool contains = pricePublicList.Any(p => p.Size == 200);
        //    if (_projectDirectoryList.Any(d => d.ApprovedListContactID == ((ApprovedListContactModel)comboBoxContacts.SelectedItem).ApprovedListContactID))
        //    {
        //        this.OK_button.Enabled = false;
        //    }
        //    else
        //    {
        //        this.OK_button.Enabled = true;
        //    }
        //}


    }

    private void LoadCompaniesCombo()
    {
        //this.comboBoxCompanies.DataSource = App.contactDirectoryService.GetApproveListCompanies_All();
        //this.comboBoxCompanies.DisplayMember = "CompanyName";
        //this.comboBoxCompanies.ValueMember = "ApprovedListID";
    }

    private void LoadContactsCombo()
    {
        //this.comboBoxContacts.DataSource = App.contactDirectoryService.GetApprovedListContacts_ByApprovedList(((ApprovedListModel)comboBoxCompanies.SelectedItem).ApprovedListID);
        //this.comboBoxContacts.DisplayMember = "FullNameReversed";
        //this.comboBoxContacts.ValueMember = "ApprovedListContactID";
    }

    private void OK_button_Click(object sender, EventArgs e)
    {
        ////save project directory
        //_projectDirectory.ShowInReport = true;
        //_projectDirectory.ProjectID = _projectID;
        //_projectDirectory.ApprovedListContactID = ((ApprovedListContactModel)comboBoxContacts.SelectedItem).ApprovedListContactID;
        //_projectDirectory.ApprovedListContactModel = (ApprovedListContactModel)comboBoxContacts.SelectedItem;
        //_projectDirectory.ApprovedListModel = (ApprovedListModel)comboBoxCompanies.SelectedItem;

        //App.contactDirectoryService.CreateProjectDirectory(_projectDirectory);

        //_projectDirectoryList.Add(_projectDirectory);

        //this.DialogResult = DialogResult.OK;
        //Close();
    }

    private void Cancel_button_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void ButtonAddCompany_Click(object sender, EventArgs e)
    {
        //ApprovedListModel approvedListModel = new();
        //Form frm = new FormNewCompany(approvedListModel);
        //if (frm.ShowDialog(this) == DialogResult.OK)
        //{
        //    //reload the combo boxes
        //    LoadCompaniesCombo();

        //    //select the newly added company
        //    this.comboBoxCompanies.SelectedValue = approvedListModel.ApprovedListID;

        //    LoadContactsCombo();
        //}
    }

    private void ButtonAddContact_Click(object sender, EventArgs e)
    {
        //ApprovedListContactModel approvedListContactModel = new()
        //{
        //    ApprovedListID = ((ApprovedListModel)comboBoxCompanies.SelectedItem).ApprovedListID
        //};


        //Form frm = new FormNewPerson(approvedListContactModel);
        //if (frm.ShowDialog(this) == DialogResult.OK)
        //{
        //    //reload the combo boxes
        //    LoadContactsCombo();

        //    //select the newly added contact
        //    this.comboBoxContacts.SelectedValue = approvedListContactModel.ApprovedListContactID;
        //}
    }


}
