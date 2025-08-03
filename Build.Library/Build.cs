using Nuke.Common;
using Nuke.Common.ProjectModel;

namespace Build.Library;

public class Build : BasePluginBuild
{
    protected override int minorVersion => 0;

    protected override int maintenanceVersion => 2;

    protected override int gitlabProjectId => 605;


    [Solution("Bim.Examples.sln")]
    public override Solution Solution { get; }

    public static int Main() => Execute<Build>(x => x.PushToGitLabNugetRepository);
}