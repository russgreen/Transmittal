using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Library.Services;

public class WeTransferService : IWeTransferService
{
    private readonly ILogger<WeTransferService> _logger;
    private readonly string _browserPath = string.Empty;
    private const string _weTransferUrl = "https://wetransfer.com/";
    private const int _debugPort = 9222;
    private const string _localAddress = "127.0.0.1";

    public WeTransferService(ILogger<WeTransferService> logger)
    {
        _logger = logger;

        _browserPath = GetBrowserPath();
    }

    public async Task<bool> PrepareWeTransferUploadAsync(List<string> filePaths)
    {
        var browserRunning = await LaunchBrowserWithRemoteDebuggingAsync();
        if (!browserRunning)
        {
            return false;
        }

        _logger.LogDebug("Browser is running at http://{LocalAddress}:{DebugPort}", _localAddress, _debugPort);

        using var playwright = await Playwright.CreateAsync();   

        var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://{_localAddress}:{_debugPort}");
        var context = browser.Contexts.First();

        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();

        await page.GotoAsync(_weTransferUrl);

        await TryClickIfVisibleAsync(page.GetByTestId("Accept All-btn"), "Accept All");
        await TryClickIfVisibleAsync(page.GetByTestId("accept-terms"), "Accept Terms");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dropZone = page.GetByTestId("drop-zone");
        var fileInput = dropZone.Locator("input[type='file']");
        await fileInput.WaitForAsync(new() { State = WaitForSelectorState.Attached });
        await fileInput.SetInputFilesAsync(filePaths);

        return true;
    }

    public Task<bool> PrepareWeTransferUploadAsync(List<string> filePaths, List<string> recipientsEmails)
    {
        throw new NotImplementedException();
    }

    private string GetBrowserPath()
    {
        //var defaultBrowser = GetDefaultBrowserPath();
        var browserPath = GetEdgePath();

        //if (defaultBrowser.Contains("chrome.exe", StringComparison.OrdinalIgnoreCase))
        //{
        //    browserPath = GetChromePath();
        //}

        return browserPath;
    }

    private string GetDefaultBrowserPath()
    {
        const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";

        using var key = Registry.CurrentUser.OpenSubKey(userChoice);
        var progId = key?.GetValue("ProgId")?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(progId))
        {
            return "";
        }

        using var commandKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
        var command = commandKey?.GetValue(null)?.ToString() ?? "";

        if (command.StartsWith("\""))
        {
            int end = command.IndexOf('"', 1);
            return command.Substring(1, end - 1);
        }

        int space = command.IndexOf(' ');
        if (space > 0)
        {
            return command.Substring(0, space);
        }

        return command;

    }

    private string GetEdgePath()
    {

        var x86 = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft", "Edge", "Application", "msedge.exe");

        if (File.Exists(x86))
        {
            return x86;
        }

        var x64 = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Microsoft", "Edge", "Application", "msedge.exe");

        if (File.Exists(x64))
        {
            return x64;
        }

        return string.Empty;
    }

    private string GetChromePath()
    {

        var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Google", "Chrome", "Application", "chrome.exe");

        return File.Exists(path) ? path : "";
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

    private async Task<bool> LaunchBrowserWithRemoteDebuggingAsync()
    {
        if(await IsBrowserWithRemoteDebuggingRunningAsync(_debugPort))
        {
            _logger.LogInformation("Browser with remote debugging is already running.");
            return true;
        }

        try
        {
            KillExistingBrower();
            LaunchWithRemoteDebugging(_browserPath);
            WaitForPort(_debugPort);
            return await WaitForDevToolsEndpointAsync(_debugPort, TimeSpan.FromSeconds(5));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
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

    private void KillExistingBrower()
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
    
    private Process LaunchWithRemoteDebugging(string browserPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = browserPath,
            Arguments = "--remote-debugging-port=9222 --no-first-run --no-default-browser-check",
            UseShellExecute = false
        };
        return Process.Start(psi);
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
