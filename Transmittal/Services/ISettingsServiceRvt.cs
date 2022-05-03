using Transmittal.Models;

namespace Transmittal.Services;

internal interface ISettingsServiceRvt
{
    /// <summary>
    /// Load the settings for the application from revit paramaters
    /// </summary>
    /// <returns>SettingsModel</returns>
    bool GetSettingsRvt(Autodesk.Revit.DB.Document rvtDoc);

    /// <summary>
    /// Update the settings for the application to revit paramaters
    /// </summary>
    void UpdateSettingsRvt(Autodesk.Revit.DB.Document rvtDoc);
}
