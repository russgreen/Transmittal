﻿using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.SignTool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.SignTool.SignToolTasks;

partial class Build
{
    Target Sign => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var compiledAssemblies = new List<string>();

            //var project = Solution.Transmittal;

            foreach (var project in Solution.AllProjects.Where(project => project == Solution.Transmittal || project == Solution.Transmittal_Desktop))
            {
                AbsolutePath projectDirectory = project.Directory;
                Log.Information(projectDirectory);

                var files = projectDirectory
                        .GlobDirectories(@"**\bin\**")
                        .SelectMany(x => x.GlobFiles(CompiledAssemblies));

                Log.Information("Files found : {files}", files.Count());

                 foreach (var file in files)
                 {
                        Log.Information("File : {file}", file);
                        compiledAssemblies.Add(file);
                 }
            }

            SignFiles(compiledAssemblies);

        });

    static void SignFiles(List<string> compiledAssemblies) => SignTool(s => s
                .SetFileDigestAlgorithm("sha256")
                .SetTimestampServerUrl(@$"http://timestamp.comodoca.com")
                .SetFile(Environment.GetEnvironmentVariable("CODECERT_PFX"))
                .SetPassword(System.IO.File.ReadLines(Environment.GetEnvironmentVariable("CODECERT_PASS")).First())
                .SetFiles(compiledAssemblies.ToArray()));
}
