using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Transmittal.Library.Services;
using Transmittal.Services;

namespace Transmittal.Forms;
public partial class FormSettings : Form
{
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;

    public FormSettings(ExternalCommandData CommandData)
    {
        InitializeComponent();

        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();

        //_settingsService.GetSettings();
    }
}
