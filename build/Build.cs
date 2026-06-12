using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.Execution;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Collections;
using Fallout.Solutions;
using System;
using System.Linq;
using static Fallout.Common.EnvironmentInfo;
using static Fallout.Common.IO.PathConstruction;

partial class Build : FalloutBuild
{
    readonly AbsolutePath OutputDirectory = RootDirectory / "output";
    readonly AbsolutePath SourceDirectory = RootDirectory / "source";

    readonly string[] CompiledAssemblies = { "Transmittal.dll", "Transmittal.Desktop.exe", "Transmittal.Desktop.dll", "Transmittal.Browser.exe", "Transmittal.Library.dll", "Transmittal.Reports.dll" };

    [GitRepository]
    [Required]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = true)]
    Solution Solution;

    public static int Main () => Execute<Build>(x => x.Clean);

    //[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;



}
