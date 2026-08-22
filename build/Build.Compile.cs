using Fallout.Common;
using Fallout.Common.Tools.DotNet;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

partial class Build : FalloutBuild
{
    Target Compile => _ => _
    .TriggeredBy(Clean)
    .Executes(() =>
    {
        foreach (var configuration in Solution.GetModel().BuildTypes)
        {
            Log.Information("Configuration name: {configuration}", configuration);

            if (configuration.StartsWith("Release"))
            {
                //DotNetRestore(settings => settings
                //    .SetProjectFile(Solution));

                DotNetBuild(settings => settings
                    .SetProjectFile(Solution)
                    .SetConfiguration(configuration)
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
        }
    });
}
