using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Requesters;

namespace Transmittal.Forms;

public partial class FormStatus : Form
{
    private readonly IStatusRequester _callingForm;
    private readonly ISettingsService _settingsService;

    public FormStatus(IStatusRequester caller)
    {
        InitializeComponent();

        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();

        _callingForm = caller;

        this.comboBoxStatus.DataSource = _settingsService.GlobalSettings.DocumentStatuses;
        this.comboBoxStatus.DisplayMember = "DisplayName";
        this.comboBoxStatus.ValueMember = "Code";
    }

    private void OK_button_Click(object sender, EventArgs e)
    {
        _callingForm.StatusComplete((DocumentStatusModel)this.comboBoxStatus.SelectedItem);

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void Cancel_button_Click(object sender, EventArgs e)
    {
        Close();
    }

}
