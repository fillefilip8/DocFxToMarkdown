using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Octokit.Internal;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using LogLevel = Nuke.Common.LogLevel;

[GitHubActions(
    "docker",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    EnableGitHubToken = true,
    InvokedTargets = new[] { nameof(BuildDocker), })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main ()
    {
        Logging.Level = LogLevel.Trace;
        return Execute<Build>(x => x.BuildDocker);
    }

    [Nuke.Common.Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [GitRepository] readonly GitRepository Repository;
    
    GitHubActions GitHubActions => GitHubActions.Instance;
    
    [Solution]
    readonly Solution Solution;
    
    [GitVersion(Framework = "net6.0")]
    [Required]
    readonly GitVersion GitVersion;

    public string ProjectName => Solution.Name == null ? "project" : Solution.Name.ToLower();

    Target BuildDocker => _ => _
        .Executes(() =>
        {
            Log.Information("Commit Hash: {Hash}", Repository.Commit);
            Log.Information("SemVer: {SemVer}", GitVersion.SemVer);
            DockerTasks.DockerBuild(settings =>
            {
                return settings
                    .SetPath(RootDirectory)
                    .AddTag($"{ProjectName}:{Repository.Commit}");
            });
        });

    Target DeployDocker => _ => _
        .TriggeredBy(BuildDocker)
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .Executes(async () =>
        {
            DockerTasks.DockerLogin(settings =>
            {
                return settings
                    .SetServer("ghcr.io")
                    .SetUsername(GitHubActions.Actor)
                    .SetPassword(GitHubActions.Token);
            });
            
            var target = $"ghcr.io/{GitHubActions.Repository}:{GitVersion.SemVer}".ToLower();

            DockerTasks.DockerImageTag(x => x
                .SetSourceImage($"{ProjectName}:{Repository.Commit}")
                .SetTargetImage(target));

            DockerTasks.DockerImagePush(x => x.SetName(target));
            
            var credentials = new Credentials(GitHubActions.Instance.Token);
            GitHubTasks.GitHubClient = new GitHubClient(
                new ProductHeaderValue(nameof(NukeBuild)),
                new InMemoryCredentialStore(credentials));
            
            var newRelease = new NewRelease(GitVersion.SemVer)
            {
                TargetCommitish = Repository.Commit,
                Draft = false,
                Name = GitVersion.SemVer,
                Prerelease = !Repository.IsOnMainOrMasterBranch(),
                Body = ""
            };

            var repoName = GitHubActions.GitHubEvent.Value<JObject>("repository").Value<string>("name");

            Log.Information("Repo Owner: {RepoOwner}", GitHubActions.RepositoryOwner);
            Log.Information("Repo Name: {RepoName}", repoName);

            await GitHubTasks.GitHubClient.Repository.Release.Create(GitHubActions.RepositoryOwner, repoName, newRelease);
        });

    /*Target Release => _ => _
        .After(DeployDocker)
        .TriggeredBy(DeployDocker)
        .OnlyWhenDynamic(() => Repository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            var source = $"ghcr.io/{GitHubActions.Repository}:{Repository.Commit}".ToLower();
            var target = $"ghcr.io/{GitHubActions.Repository}:{GitVersion.SemVer}".ToLower();

            DockerTasks.DockerImageTag(x => x
                .SetSourceImage(source)
                .SetTargetImage(target));

            DockerTasks.DockerImagePush(x => x.SetName(target));
        });*/
}
