using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

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

    public static int Main () => Execute<Build>(x => x.BuildDocker);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [GitRepository] readonly GitRepository Repository;
    
    GitHubActions GitHubActions => GitHubActions.Instance;
    
    [Solution]
    readonly Solution Solution;

    public string ProjectName => Solution.Name == null ? "project" : Solution.Name.ToLower();

    Target BuildDocker => _ => _
        .Executes(() =>
        {
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
        .Executes(() =>
        {
            DockerTasks.DockerLogin(settings =>
            {
                return settings
                    .SetServer("ghcr.io")
                    .SetUsername(GitHubActions.Actor)
                    .SetPassword(GitHubActions.Token);
            });
            
            var target = $"ghcr.io/{GitHubActions.Repository}:{Repository.Commit}".ToLower();

            DockerTasks.DockerImageTag(x => x
                .SetSourceImage($"{ProjectName}:{Repository.Commit}")
                .SetTargetImage(target));

            DockerTasks.DockerImagePush(x => x.SetName(target));
        });
}
