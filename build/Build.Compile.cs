using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.SignTool;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Collections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Fallout.Common.EnvironmentInfo;
using static Fallout.Common.IO.PathConstruction;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.SignTool.SignToolTasks;

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
