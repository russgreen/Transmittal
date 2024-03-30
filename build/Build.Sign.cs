using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.SignTool;
using Serilog;
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
            var assemblies = new List<string>();
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

            SignTool(s => s
                .SetFileDigestAlgorithm("sha256")
                .SetTimestampServerUrl(@$"http://timestamp.comodoca.com")
                .SetFile(@"D:\Development\Code Signing\RussellGreen.pfx")
                .SetPassword(System.IO.File.ReadLines(@"D:\Development\Code Signing\PFXPassword.txt").First())
                .SetFiles(compiledAssemblies.ToArray()));

        });
}
