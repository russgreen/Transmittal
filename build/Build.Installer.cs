using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Fallout.Common.IO.PathConstruction;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

partial class Build : FalloutBuild
{
    Target Installer => _ => _
        .TriggeredBy(Sign)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            var wixProjectPath = Path.Combine(RootDirectory, @"installer\Transmittal.Setup.wixproj");
            var version = GetProjectVersion(Path.Combine(RootDirectory, @"Directory.Build.props"));

            Log.Information("WiX project : {wixProjectPath}", wixProjectPath);
            Log.Information("Version : {version}", version);

            // the product version comes from Directory.Build.props and a new ProductCode is generated for every build
            // the UpgradeCode never changes so the new MSI upgrades earlier installations
            DotNetBuild(settings => settings
                .SetProjectFile(wixProjectPath)
                .SetConfiguration(Configuration.Release)
                .SetVerbosity(DotNetVerbosity.minimal));

            SignMSI(version);
        });

    static string GetProjectVersion(string projectFilePath)
    {
        var doc = XDocument.Load(projectFilePath);
        var root = doc.Root;
        var propertyGroup = root?
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName == "PropertyGroup");

        var versionProperties = new[]
        {
            "Version",
            "VersionPrefix",
            "ApplicationVersion",
            "FileVersion",
            "InformationalVersion"
        };

        var versionElement = propertyGroup?
            .Elements()
            .FirstOrDefault(x => versionProperties.Contains(x.Name.LocalName) && !string.IsNullOrWhiteSpace(x.Value));

        return versionElement?.Value ?? "1.0.0";
    }

    static void SignMSI(string version)
    {
        var msiOutputPath = Path.Combine(RootDirectory, @"installer\Transmittal-SetupFiles");
        Log.Information(msiOutputPath);

        var msiPath = Directory.GetFiles(msiOutputPath, $"*{version}.msi").FirstOrDefault();
        Log.Information(msiPath);

        if (File.Exists(msiPath))
        {
            SignFiles(new System.Collections.Generic.List<string> { msiPath });
        }
        else
        {
            Log.Error("MSI file not found.");
        }
    }
}
