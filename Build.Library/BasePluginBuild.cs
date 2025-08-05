﻿using System.Net;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Serilog;

public abstract class BasePluginBuild : NukeBuild
{

    protected abstract int minorVersion { get; }
    protected abstract int maintenanceVersion { get; }

    protected abstract int gitlabProjectId { get; }

    [Solution] public virtual Solution Solution { get; } = null!;

    [Parameter("Build configuration like Release or Debug")]
    public readonly string Configuration = "Release";

    [Parameter("Target environment: Development or Production")]
    public readonly string Stage = "Development";

    [Parameter("Project to build name")]
    public readonly string ProjectName = "Build.Library";

    private Project LibraryProject =>
        Solution.AllProjects.FirstOrDefault(project => project.Name.Contains(ProjectName));

    private string GetFtpTargetFolder()
    {
        return Stage.Equals("Development", StringComparison.OrdinalIgnoreCase)
            ? "DevelopmentPlugins"
            : "ProductionPlugins";
    }

    private string GetVersion(string configuration)
    {
        var match = Regex.Match(configuration, @"\d+");
        string major = match.Success ? "20" + match.Value : "0";

        return $"{major}.{minorVersion}.{maintenanceVersion}";
    }

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    public Target Clean => _ => _
        .Executes(() =>
        {
            CleanDirectory(LibraryProject.Directory / "bin");
            CleanDirectory(LibraryProject.Directory / "obj");

            static void CleanDirectory(AbsolutePath path)
            {
                Log.Information("Cleaning directory: {Directory}", path);
                path.CreateOrCleanDirectory();
            }
        });

public Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            var csprojPath = LibraryProject.Path;
            var projectXml = XDocument.Load(csprojPath);

            var ns = projectXml.Root?.Name.Namespace ?? XNamespace.None;
            var tfm = projectXml.Descendants(ns + "TargetFramework").FirstOrDefault()?.Value
                   ?? projectXml.Descendants(ns + "TargetFrameworks").FirstOrDefault()?.Value.Split(';').FirstOrDefault();

            if (string.IsNullOrWhiteSpace(tfm))
                throw new Exception("Cannot determine TargetFramework from .csproj");

            var version = GetVersion(Configuration);
            
            DotNetRestore(s => s
            .SetProjectFile(LibraryProject));

            if (tfm.StartsWith("net4")) // .NET Framework
            {
                MSBuildTasks.MSBuild(s => s
                    .SetTargetPath(LibraryProject.Path)
                    .SetTargets("Build")
                    .SetConfiguration(Configuration)
                    .SetProperty("Version", version));
            }
            else // .NET Core / .NET 5+
            {
                DotNetBuild(s => s
                    .SetProjectFile(LibraryProject)
                    .SetVersion(version)
                    .SetConfiguration(Configuration));
            }
        });

    public Target PushDllsToPluginServer => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        var ftpHost = Environment.GetEnvironmentVariable("FTP_HOST");
        var ftpUser = Environment.GetEnvironmentVariable("FTP_USER");
        var ftpPassword = Environment.GetEnvironmentVariable("FTP_PASSWORD");

        if (string.IsNullOrWhiteSpace(ftpHost) ||
            string.IsNullOrWhiteSpace(ftpUser) ||
            string.IsNullOrWhiteSpace(ftpPassword))
        {
            throw new Exception("FTP credentials are not set in environment variables.");
        }

        var folderName = GetFtpTargetFolder();

        var dllDirectory = LibraryProject.Directory / "bin" / Configuration;
        var dllFiles = Directory.EnumerateFiles(dllDirectory)
        .Where(f =>
        !f.EndsWith(".nupkg")
        && !f.EndsWith(".pdb")
        && !f.EndsWith(".dll.config")
        && !f.Contains("RevitAPIUI.dll")
        && !f.Contains("RevitAPI.dll")
        && !f.Contains("Autodesk.Navisworks.Automation")
        && !f.Contains("Autodesk.Navisworks.Api")
        && !f.Contains("Autodesk.Navisworks.Automation")
        && !f.Contains("AdWindows.dll")
        )
        .ToList();

        foreach (var localFile in dllFiles)
        {
            var fileName = Path.GetFileName(localFile);
            var remotePath = $"plugins/{GetFtpTargetFolder()}/{fileName}";

            var ftpUri = new Uri($"{ftpHost}/{remotePath}");

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

                DateTime serverLastModified;
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    serverLastModified = response.LastModified;
                }

                var localLastModified = File.GetLastWriteTimeUtc(localFile);
                if (localLastModified > serverLastModified.ToUniversalTime())
                {
                    Log.Information("Newer file detected. Replacing: {0}", fileName);

                    UploadFile(ftpUri, localFile, ftpUser, ftpPassword);
                }
                else
                {
                    Log.Information("Up-to-date file found: {0}", fileName);
                }
            }
            catch (WebException ex) when (((FtpWebResponse)ex.Response)?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                Log.Information("File not found on server. Uploading: {0}", fileName);
                UploadFile(ftpUri, localFile, ftpUser, ftpPassword);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to check/upload {0}: {1}", fileName, ex.Message);
            }
        }

        void UploadFile(Uri uri, string filePath, string username, string password)
        {
            NetworkCredential networkCredential = new NetworkCredential(username, password);

            using var client = new WebClient();
            client.Credentials = networkCredential;
            client.UploadFile(uri, WebRequestMethods.Ftp.UploadFile, filePath);
        }
    });

    public Target Pack => _ => _
    .DependsOn(PushDllsToPluginServer)
    .Executes(() =>
    {
        DotNetPack(s => s
            .SetProject(LibraryProject)
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .SetVersion(GetVersion(Configuration))
            .SetOutputDirectory(ArtifactsDirectory / Configuration));
    });

    public Target PushToGitLabNugetRepository => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            Log.Information("PATH: " + Environment.GetEnvironmentVariable("PATH"));

            var apiKey = Environment.GetEnvironmentVariable("GITLAB_NUGET_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("GITLAB_NUGET_API_KEY is not set");

            var version = GetVersion(Configuration);
            var matchedNupkgs = Directory
                .GetFiles(ArtifactsDirectory, "*.nupkg", SearchOption.AllDirectories)
                .Where(nupkg => Path.GetFileName(nupkg).Contains(version))
                .ToList();

            foreach (var nupkg in matchedNupkgs)
            {
                DotNetNuGetPush(s => s
                    .SetTargetPath(nupkg)
                    .SetSource($"https://gitlab.com/erik7912510/api/v4/projects/{gitlabProjectId}/packages/nuget/index.json")
                    .SetApiKey(apiKey)
                    .EnableSkipDuplicate());
            }
        });
}
