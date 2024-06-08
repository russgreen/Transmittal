using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

partial class Build
{
    Target Installer => _ => _
        .DependsOn(Sign)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            var aipProjectPath = Path.Combine(RootDirectory, @"Installer\Transmittal.aip");
            var version = Solution.Transmittal.GetProperty("Version");

            Log.Information("AIP : {aipProjectPath}", aipProjectPath);
            Log.Information("Version : {version}", version);

            AdvancedInstallerCLI($"/edit {aipProjectPath} /SetVersion {version}");
            AdvancedInstallerCLI($"/edit {aipProjectPath} /SetProductCode -langid 1033");
            AdvancedInstallerCLI($"/build {aipProjectPath}");

            SignMSI(version);
        });

    static void SignMSI(string version)
    {
        var aipOutputPath = Path.Combine(RootDirectory, @"Installer\Transmittal-SetupFiles");
        Log.Information(aipOutputPath);

        var msiPath = Directory.GetFiles(aipOutputPath, $"*{version}.msi").FirstOrDefault();
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


    static void AdvancedInstallerCLI(string args)
    {
        Log.Information("Command args {args}", args);

        var applicationPath = GetAdvancedInstallerPath();
        var applicationFullPath = Path.Combine(applicationPath, "AdvancedInstaller.com");

        var p = new ProcessStartInfo();
        p.WorkingDirectory = applicationPath;
        p.FileName = applicationFullPath;
        p.Arguments = args;

        var process = Process.Start(p);
        process.WaitForExit();
    }

    static string GetAdvancedInstallerPath()
    {
        var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var caphyonPath = Path.Combine(programFilesPath, "Caphyon");

        var latestVersion = Directory.GetDirectories(caphyonPath)
            .Select(Path.GetFileName)
            .Where(x => x.StartsWith("Advanced Installer")) // Ensure we only consider Advanced Installer directories
            .Select(x => new { Path = x, Version = new Version(x.Substring("Advanced Installer ".Length)) }) // Extract the version number from the directory name
            .OrderByDescending(x => x.Version) // Sort by version number
            .FirstOrDefault();

        if (latestVersion == null)
        {
            throw new Exception("Advanced Installer is not installed.");
        }

        var advancedInstallerPath = Path.Combine(caphyonPath, latestVersion.Path, "bin", "x86");
        Log.Information(advancedInstallerPath);

        return advancedInstallerPath;
    }
}
