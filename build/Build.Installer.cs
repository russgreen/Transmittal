using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Installer => _ => _
        .DependsOn(Sign)
        .Executes(() =>
        {

        });
}
