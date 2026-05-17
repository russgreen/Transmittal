using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Transmittal.Library.Services;

public class FileTransferService : IFileTransferService
{
    private readonly ILogger<FileTransferService> _logger;
    private readonly ISettingsService _settingsService;

    private readonly string _browserPath = string.Empty;
    private readonly string _browserUserDataDirectory;
    private const int _debugPort = 9222;
    private const string _localAddress = "127.0.0.1";
    private const string _browserExeName = "Transmittal.Browser.exe";


    public FileTransferService(ILogger<FileTransferService> logger,
        ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;

        _browserPath = GetTransmittalBrowserPath();
        _browserUserDataDirectory = GetBrowserUserDataDirectory();
    }

    public async Task<bool> PrepareFileTransferUploadAsync(List<string> filePaths)
    {
        switch (_settingsService.GlobalSettings.FileTransferType)
        {
            case Enums.FileTransferType.WeTransfer:
                return await WeTransfer(filePaths);

            case Enums.FileTransferType.Smash:
                return await Smash(filePaths);

            default:
                throw new NotSupportedException($"File transfer service {_settingsService.GlobalSettings.FileTransferType} is not supported.");
        }
    }

    public Task<bool> PrepareFileTransferUploadAsync(List<string> filePaths, List<string> recipientsEmails)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> WeTransfer(List<string> filePaths)
    {
        var browserRunning = await LaunchBrowserWithRemoteDebuggingAsync(filePaths);
        if (!browserRunning)
        {
            return false;
        }

        _logger.LogDebug("Browser is running at http://{LocalAddress}:{DebugPort}", _localAddress, _debugPort);

        using var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://{_localAddress}:{_debugPort}");
        var context = browser.Contexts.First();

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();

        await page.GotoAsync("https://wetransfer.com/");

        await TryClickIfVisibleAsync(page.GetByTestId("Accept All-btn"), "Accept All");
        await TryClickIfVisibleAsync(page.GetByTestId("accept-terms"), "Accept Terms");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
               
        try
        {
            var dropZone = page.GetByTestId("drop-zone");
            var fileInput = dropZone.Locator("input[type='file']");
            await fileInput.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await fileInput.SetInputFilesAsync(filePaths);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogWarning(ex, "Automatic WeTransfer upload did not complete. Users can drag files from the browser side panel.");
        }

        return true;
    }


    private async Task<bool> Smash(List<string> filePaths)
    {
        var browserRunning = await LaunchBrowserWithRemoteDebuggingAsync(filePaths);
        if (!browserRunning)
        {
            return false;
        }

        _logger.LogDebug("Browser is running at http://{LocalAddress}:{DebugPort}", _localAddress, _debugPort);

        using var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://{_localAddress}:{_debugPort}");
        var context = browser.Contexts.First();

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();

        await page.GotoAsync("https://fromsmash.com/");

        await TryClickIfVisibleAsync(page.GetByText("I loooove cookies!"), "Accept Cookies");
        await TryClickIfVisibleAsync(page.GetByText("Continue to use for free"), "Continue");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var files = filePaths.Select(Path.GetFullPath).ToArray();

        // Preferred path: set files directly on a real file input in the uploader area.
        var uploadInputCandidates = new[]
        {
        "main input[type='file']",
        "form input[type='file']",
        "input[type='file']"
    };

        try
        {
            foreach (var selector in uploadInputCandidates)
            {
                var input = page.Locator(selector).First;
                if (await input.CountAsync() > 0)
                {
                    await input.SetInputFilesAsync(files);
                    _logger.LogDebug("Files assigned through selector: {Selector}", selector);
                    return true;
                }
            }

            // Fallback: click the central CTA and handle file chooser.
            var chooser = await page.RunAndWaitForFileChooserAsync(async () =>
            {
                await page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("send|file|upload|select", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
                          .First
                          .ClickAsync();
            });

            await chooser.SetFilesAsync(files);
            _logger.LogDebug("Files assigned through file chooser fallback.");
        }
        catch (PlaywrightException ex)
        {
            _logger.LogWarning(ex, "Automatic Smash upload did not complete. Users can drag files from the browser side panel.");
        }

        return true;
    }



    private string GetTransmittalBrowserPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("TRANSMITTAL_BROWSER_PATH");
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        var appContextDirectory = AppContext.BaseDirectory;
        var candidatePaths = new List<string>
        {
            Path.Combine(appContextDirectory, _browserExeName),
            Path.Combine(appContextDirectory, "Transmittal.Browser", _browserExeName)
        };

        var entryAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(entryAssemblyDirectory))
        {
            candidatePaths.Add(Path.Combine(entryAssemblyDirectory, _browserExeName));
        }

        return candidatePaths.FirstOrDefault(File.Exists) ?? string.Empty;
    }

    private static string GetBrowserUserDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Transmittal",
            "Browser",
            "UserData");
    }

    private async Task<bool> TryClickIfVisibleAsync(ILocator locator, string description, int timeoutMs = 5000)
    {
        try
        {
            if (!await locator.IsVisibleAsync())
            {
                _logger.LogDebug("{description} is not visible.", description);
                return false;
            }

            await locator.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = timeoutMs });
            await locator.ScrollIntoViewIfNeededAsync();
            await locator.ClickAsync();
            return true;
        }
        catch (PlaywrightException ex)
        {
            _logger.LogDebug(ex, "Failed to click {description}.", description);
            return false;
        }
    }

    private async Task<bool> LaunchBrowserWithRemoteDebuggingAsync(List<string> filePaths)
    {
        if (string.IsNullOrWhiteSpace(_browserPath))
        {
            _logger.LogError("{BrowserExecutable} could not be found. Ensure it is deployed with the app.", _browserExeName);
            return false;
        }

        var filesManifestPath = CreateTransferFilesManifest(filePaths);

        if (await IsBrowserWithRemoteDebuggingRunningAsync(_debugPort))
        {
            _logger.LogInformation("Browser with remote debugging is already running. Restarting to refresh transfer file panel.");
            KillExistingBrowser();
        }

        try
        {
            LaunchWithRemoteDebugging(_browserPath, filesManifestPath, showRedropHint: true);
            WaitForPort(_debugPort);
            return await WaitForDevToolsEndpointAsync(_debugPort, TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start {BrowserPath} for file transfer automation", _browserPath);
        }

        return false;
    }

    private async Task<bool> IsBrowserWithRemoteDebuggingRunningAsync(int port)
    {
        if (!IsPortOpen(port))
        {
            return false;
        }

        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromMilliseconds(500);

            var response = await http.GetStringAsync($"http://{_localAddress}:{port}/json/version");
            return !string.IsNullOrWhiteSpace(response);
        }
        catch
        {
            return false;
        }
    }

    private void KillExistingBrowser()
    {
        var processes = new Dictionary<int, Process>();

        var browserProcessName = System.IO.Path.GetFileNameWithoutExtension(_browserPath);

        foreach (var proc in Process.GetProcessesByName(browserProcessName))
        {
            processes.Add(proc.Id, proc);
        }

        foreach (var proc in processes.Values)
        {
            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                    proc.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Cannot terminate {proc}", proc.ProcessName);
            }
        }
    }
    
    private Process LaunchWithRemoteDebugging(string browserPath, string filesManifestPath, bool showRedropHint)
    {
        Directory.CreateDirectory(_browserUserDataDirectory);

        var psi = new ProcessStartInfo
        {
            FileName = browserPath,
            Arguments =
                $"--remote-debugging-port={_debugPort} " +
                $"--user-data-dir=\"{_browserUserDataDirectory}\" " +
                $"--start-url=about:blank " +
                $"--files-manifest=\"{filesManifestPath}\" " +
                $"--show-redrop-hint={showRedropHint.ToString().ToLowerInvariant()}",
            UseShellExecute = false
        };
        return Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start browser process at {browserPath}.");
    }

    private string CreateTransferFilesManifest(List<string> filePaths)
    {
        Directory.CreateDirectory(_browserUserDataDirectory);

        var validFilePaths = filePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var manifestPath = Path.Combine(_browserUserDataDirectory, "transfer-files-manifest.json");
        var json = JsonSerializer.Serialize(validFilePaths);
        File.WriteAllText(manifestPath, json);

        return manifestPath;
    }

    private void WaitForPort(int port, int timeoutMs = 20000)
    {

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            if (IsPortOpen(port))
            {
                return;
            }

            Thread.Sleep(200);
        }

        throw new TimeoutException($"Port {port} did not open within {timeoutMs}ms.");
    }

    private bool IsPortOpen(int port)
    {
        try
        {
            using var client = new TcpClient();
            var result = client.ConnectAsync(_localAddress, port).Wait(200);
            return result && client.Connected;
        }
        catch 
        { 
            return false; 
        }
    }

    private async Task<bool> WaitForDevToolsEndpointAsync(int port, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        while (sw.Elapsed < timeout)
        {
            try
            {
                var response = await http.GetStringAsync($"http://{_localAddress}:{port}/json/version");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    return true;
                }
            }
            catch { }
            await Task.Delay(200);
        }
        return false;
    }

    public bool PrepareWeTransferUpload(List<string> filePaths)
    {
        throw new NotImplementedException();
    }


}
