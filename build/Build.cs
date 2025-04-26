using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

partial class Build : NukeBuild
{
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";

    readonly string[] CompiledAssemblies = { "Transmittal.dll", "Transmittal.Desktop.exe", "Transmittal.Desktop.dll", "Transmittal.Library.dll", "Transmittal.Reports.dll" };

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = true)]
    Solution Solution;

    public static int Main () => Execute<Build>(x => x.Clean);

    //[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;



}
