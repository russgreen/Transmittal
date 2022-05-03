using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public interface ISettingsService
{
    /// <summary>
    /// Current settings data
    /// </summary>
    SettingsModel GlobalSettings { get; set; }

    /// <summary>
    /// Load the global settings for the application
    /// </summary>
    /// <returns>SettingsModel</returns>
    void GetSettings();
    
    /// <summary>
    /// Update the global settings for the application
    /// </summary>
    void UpdateSettings();

}
