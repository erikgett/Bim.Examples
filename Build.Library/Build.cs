using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System.Text.RegularExpressions;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Build.Library;

public class Build : NukeBuild
{
    private const int minorVersion = 0;
    private const int maintenanceVersion = 2;
    public static int Main() => Execute<Build>(x => x.Pack);

    [Solution("Bim.Examples.sln")]
    private readonly Solution Solution;

    private Project libraryProject =>
        Solution.AllProjects.FirstOrDefault(project => project.Name.Contains("Bim.Library"));

    private string GetVersion(string configuration)
    {
        var match = Regex.Match(configuration, @"\d+");
        var major = match.Success ? match.Value : "0";
        return $"{major}.{minorVersion}.{maintenanceVersion}";
    }


    private List<string> configurations => [.. Solution.Configurations
        .Where(pair => !pair.Key.Contains("Generator"))
        .Where(pair => !pair.Key.Contains("Abstractions"))
        .Select(pair => pair.Key)
        .Select(config => config.Remove(config.LastIndexOf('|')))
        .Where(config => config.Contains("Release"))
        .Where(config => config.Contains("R21"))];

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Executes(() =>
        {
            CleanDirectory(libraryProject.Directory / "bin");
            CleanDirectory(libraryProject.Directory / "obj");

            static void CleanDirectory(AbsolutePath path)
            {
                Log.Information("Cleaning directory: {Directory}", path);
                path.CreateOrCleanDirectory();
            }
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {            
            foreach (var configuration in configurations)
            {
                DotNetBuild(s => s
                    .SetProjectFile(libraryProject)
                    .SetVersion(GetVersion(configuration))
                    .SetConfiguration(configuration));
            }
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var configuration in configurations)
            {
                DotNetPack(s => s
                    .SetProject(libraryProject)
                    .SetConfiguration(configuration)
                    .SetVersion(GetVersion(configuration))
                    .EnableNoBuild()
                    .SetOutputDirectory(ArtifactsDirectory / configuration));
            }
        });


    Target PushToGitLabNugetRepository => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetTargetPath(ArtifactsDirectory / "*.nupkg")
                .SetSource("https://gitlab.example.com/api/v4/projects/ID/packages/nuget/index.json")
                .SetApiKey("YOUR_GITLAB_TOKEN")
                .EnableSkipDuplicate());
        });
}
