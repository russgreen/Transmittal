namespace Transmittal.Models;

internal class ImportSettingsModel : Transmittal.Library.Models.SettingsModel
{
    public string SharedParametersFile { get; set; }
    public string ProjectParametersGroup { get; set; }
    public string SheetParametersGroup { get; set; }
}    
