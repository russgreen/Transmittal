using System.IO;
using System.Text.Json;
using Transmittal.Browser.Models;

namespace Transmittal.Browser.Services;

public sealed class BrowserLaunchOptionsProvider : IBrowserLaunchOptionsProvider
{
    private const string PortArgument = "--remote-debugging-port=";
    private const string UserDataDirectoryArgument = "--user-data-dir=";
    private const string StartUrlArgument = "--start-url=";
    private const string FilesManifestArgument = "--files-manifest=";
    private const string ShowRedropHintArgument = "--show-redrop-hint=";

    public BrowserLaunchOptions GetLaunchOptions()
    {
        var args = Environment.GetCommandLineArgs();

        var userDataDirectory = GetValue(args, UserDataDirectoryArgument);
        if (string.IsNullOrWhiteSpace(userDataDirectory))
        {
            userDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Transmittal",
                "Browser",
                "UserData");
        }

        var remoteDebuggingPort = 9222;
        var remoteDebuggingPortArgument = GetValue(args, PortArgument);
        if (!string.IsNullOrWhiteSpace(remoteDebuggingPortArgument) &&
            int.TryParse(remoteDebuggingPortArgument, out var parsedPort))
        {
            remoteDebuggingPort = parsedPort;
        }

        var startUrl = GetValue(args, StartUrlArgument);
        if (string.IsNullOrWhiteSpace(startUrl))
        {
            startUrl = "about:blank";
        }

        var filesManifestPath = GetValue(args, FilesManifestArgument);
        var showRedropHint = bool.TryParse(GetValue(args, ShowRedropHintArgument), out var showHint) && showHint;

        return new BrowserLaunchOptions
        {
            RemoteDebuggingPort = remoteDebuggingPort,
            UserDataDirectory = userDataDirectory,
            StartUrl = startUrl,
            FilesManifestPath = filesManifestPath,
            ShowRedropHint = showRedropHint,
            TransferFilePaths = GetTransferFilePaths(filesManifestPath)
        };
    }

    private static string GetValue(IEnumerable<string> args, string argumentPrefix)
    {
        return args
            .FirstOrDefault(arg => arg.StartsWith(argumentPrefix, StringComparison.OrdinalIgnoreCase))
            ?.Substring(argumentPrefix.Length)
            ?.Trim('"')
            ?? string.Empty;
    }

    private static IReadOnlyList<string> GetTransferFilePaths(string filesManifestPath)
    {
        if (string.IsNullOrWhiteSpace(filesManifestPath) || !File.Exists(filesManifestPath))
        {
            return Array.Empty<string>();
        }

        try
        {
            var json = File.ReadAllText(filesManifestPath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
