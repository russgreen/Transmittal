using Fallout.Common;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;

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

}
