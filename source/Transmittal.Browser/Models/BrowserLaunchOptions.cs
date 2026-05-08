using System;
using System.Collections.Generic;

namespace Transmittal.Browser.Models;

public sealed class BrowserLaunchOptions
{
    public int RemoteDebuggingPort { get; init; } = 9222;
    public string UserDataDirectory { get; init; } = string.Empty;
    public string StartUrl { get; init; } = "about:blank";
    public string FilesManifestPath { get; init; } = string.Empty;
    public bool ShowRedropHint { get; init; }
    public IReadOnlyList<string> TransferFilePaths { get; init; } = Array.Empty<string>();
}
