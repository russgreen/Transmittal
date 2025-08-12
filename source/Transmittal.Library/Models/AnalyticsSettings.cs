using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Library.Models;

    // optional settings used for internal analytics logging.
    // requires the user to opt in with a separate AnalyticsSettings.json
public partial class AnalyticsSettings : ObservableValidator
{
    [ObservableProperty]
    private bool _enableAnalytics = false;

    [ObservableProperty]
    private string _analyticsPath = string.Empty;
}
