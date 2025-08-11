// Copyright 2003-2023 by Autodesk, Inc.
// This code is taken from the RevitLookup project and is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Transmittal.Library.DTO;
using Transmittal.Library.Enums;

namespace Transmittal.Library.Services;
public sealed class SoftwareUpdateService : ISoftwareUpdateService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SoftwareUpdateService> _logger;

    private readonly Regex _versionRegex = new(@"(\d+\.)+\d+", RegexOptions.Compiled);

    private string _downloadUrl;

    public SoftwareUpdateState State { get; set; }

    public string CurrentVersion { get; private set; }

    public string NewVersion { get; set; }

    public string LatestCheckDate { get; set; }

    public string ReleaseNotesUrl { get; set; }

    public string ErrorMessage { get; set; }

    public string LocalFilePath { get; set; }


    public SoftwareUpdateService(IConfiguration configuration, 
        ILogger<SoftwareUpdateService> logger) 
    {
        _configuration = configuration;
        _logger = logger;

        CurrentVersion = _configuration["SoftwareVersion"];
        ReleaseNotesUrl = "https://github.com/russgreen/Transmittal/releases/";
    }

    public async Task CheckUpdates()
    {
        try
        {
            if (!string.IsNullOrEmpty(LocalFilePath))
            {
                if (File.Exists(LocalFilePath))
                {
                    var fileName = Path.GetFileName(LocalFilePath);
                    if (fileName.Contains(NewVersion))
                    {
                        State = SoftwareUpdateState.ReadyToInstall;
                        return;
                    }
                }
            }

            string releasesJson;
            using (var gitHubClient = new HttpClient())
            {
                gitHubClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Transmittal");
                releasesJson = await gitHubClient.GetStringAsync("https://api.github.com/repos/russgreen/Transmittal/releases");
            }

            var releases = JsonSerializer.Deserialize<List<GutHubResponse>>(releasesJson);

            if (releases is null)
            {
                State = SoftwareUpdateState.ErrorChecking;
                ErrorMessage = "GitHub server unavailable to check for updates";
                return;
            }

            var latestRelease = releases
                .Where(response => !response.Draft)
                .Where(response => !response.PreRelease)
                .OrderByDescending(release => release.PublishedDate)
                .FirstOrDefault();

            if (latestRelease is null)
            {
                State = SoftwareUpdateState.UpToDate;
                return;
            }

            // Finding a new version
            Version newVersionTag = null;
            var currentTag = new Version(CurrentVersion);
            foreach (var asset in latestRelease.Assets)
            {
                var match = _versionRegex.Match(asset.Name);
                if (!match.Success)
                {
                    continue;
                }

                if (!match.Value.StartsWith(currentTag.Major.ToString()))
                {
                    continue;
                }

                newVersionTag = new Version(match.Value);
                _downloadUrl = asset.DownloadUrl;
                break;
            }

            // Checking available releases
            if (newVersionTag is null)
            {
                State = SoftwareUpdateState.UpToDate;
                return;
            }

            // Checking for a new release version
            if (newVersionTag <= currentTag)
            {
                State = SoftwareUpdateState.UpToDate;
                return;
            }

            // Checking downloaded releases
            var downloadFolder = _configuration["DownloadFolder"];
            NewVersion = newVersionTag.ToString(3);
            if (Directory.Exists(downloadFolder))
            {
                foreach (var file in Directory.EnumerateFiles(downloadFolder))
                {
                    if (file.EndsWith(Path.GetFileName(_downloadUrl)!))
                    {
                        LocalFilePath = file;
                        State = SoftwareUpdateState.ReadyToInstall;
                        return;
                    }
                }
            }

            State = SoftwareUpdateState.ReadyToDownload;
            ReleaseNotesUrl = latestRelease.Url;

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GitHub request limit exceeded");
            
            // GitHub request limit exceeded
            State = SoftwareUpdateState.UpToDate;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking for updates");
            
            State = SoftwareUpdateState.ErrorChecking;
            ErrorMessage = "An error occurred while checking for updates";
        }
    }

    public async Task DownloadUpdate()
    {
        try
        {
            var downloadFolder = _configuration["DownloadFolder"];
            Directory.CreateDirectory(downloadFolder);
            var fileName = Path.Combine(downloadFolder, Path.GetFileName(_downloadUrl));

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStreamAsync(_downloadUrl);

            //using var fileStream = new FileStream(fileName, FileMode.Create);
            using var fileStream = new FileStream(fileName, FileMode.Create);
            await response.CopyToAsync(fileStream);

            LocalFilePath = fileName;
            State = SoftwareUpdateState.ReadyToInstall;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while downloading the update");
            
            State = SoftwareUpdateState.ErrorDownloading;
            ErrorMessage = "An error occurred while downloading the update";
        }
    }

}
