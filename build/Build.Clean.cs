using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Clean => _ => _
    .Executes(() =>
    {
        CleanDirectory(ArtifactsDirectory);

        foreach (var project in Solution.AllProjects.Where(project => project != Solution._build))
        {
            CleanDirectory(project.Directory / "bin");
            CleanDirectory(project.Directory / "obj");
        }

        //foreach (var configuration in GlobBuildConfigurations())
        //{
        //    DotNetClean(settings => settings
        //        .SetProject(Solution)
        //        .SetConfiguration(configuration)
        //        .SetVerbosity(DotNetVerbosity.minimal)
        //        .EnableNoLogo());
        //}
    });

    static void CleanDirectory(AbsolutePath path)
    {
        Log.Information("Cleaning directory: {Directory}", path);
        path.CreateOrCleanDirectory();
    }
}
