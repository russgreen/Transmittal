using Fallout.Common;
using Fallout.Common.IO;

partial class Build : FalloutBuild
{
    Target Clean => _ => _
    .Executes(() =>
    {
        SourceDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
        OutputDirectory.CreateOrCleanDirectory();
    });
}
