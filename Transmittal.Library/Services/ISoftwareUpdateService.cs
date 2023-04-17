// Copyright 2003-2023 by Autodesk, Inc.
// This code is taken from the RevitLookup project and is licensed under the MIT license.

using Transmittal.Library.Enums;

namespace Transmittal.Library.Services;
public interface ISoftwareUpdateService
{
    public SoftwareUpdateState State { get; }
    public string CurrentVersion { get; }
    public string NewVersion { get; }
    public string LatestCheckDate { get; }
    public string ReleaseNotesUrl { get; }
    public string ErrorMessage { get; }
    public string LocalFilePath { get; }

    Task CheckUpdates();
    Task DownloadUpdate();
}
