using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    Target Clean => _ => _
    .Executes(() =>
    {
        SourceDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
        OutputDirectory.CreateOrCleanDirectory();
    });
}
