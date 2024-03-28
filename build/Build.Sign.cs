using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Sign => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {

        });
}
