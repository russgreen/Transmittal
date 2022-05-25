using Transmittal.Library.Models;
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
    void UpdateSettingsRvt();

    /// <summary>
    /// (Re)set the default parameter names and GUID's in the ISettingsService.GlobalSettings
    /// </summary>
    void SetParameters();

    /// <summary>
    /// Checks to see if the database file can be found
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="checkConnection"></param>
    /// <returns></returns>
    bool CheckDatabaseFileExists(string filepath, bool checkConnection);
}
