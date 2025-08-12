using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Models;

namespace Transmittal.Library.Helpers;
public static class AnalyticsSettingsLoader
{
    public static AnalyticsSettings LoadAnalyticsSettings()
    {
#if DEBUG
        //look for the optional analytics settings file
        var analyticsFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "AnalyticsSettings.json");
#else
        //look for the optional analytics settings file
        var analyticsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Transmittal",
            "AnalyticsSettings.json");
#endif

        if (File.Exists(analyticsFile))
        {
            Debug.Write($"Loading optional analytics settings from {analyticsFile}");

            var settingsJson = File.ReadAllText(analyticsFile, Encoding.UTF8);

            if (settingsJson is not null)
            {
                try
                {
                    var analyticsSettings = System.Text.Json.JsonSerializer.Deserialize<AnalyticsSettings>(settingsJson)!;
#if DEBUG
                    if (analyticsSettings.EnableAnalytics & string.IsNullOrEmpty(analyticsSettings.AnalyticsPath))
                    {
                        analyticsSettings.AnalyticsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    }
#endif

                    Debug.Write("Optional analytics settings loaded successfully.");

                    return analyticsSettings;
                }
                catch (Exception ex)
                {
                    Debug.Write(ex, "Error deserializing settings file.");
                }
            }

        }

        // If the file doesn't exist or deserialization fails, return default settings which disables analytics
        return new AnalyticsSettings();
    }
}
