using Transmittal.Browser.Models;

namespace Transmittal.Browser.Services;

public interface IBrowserLaunchOptionsProvider
{
    BrowserLaunchOptions GetLaunchOptions();
}
