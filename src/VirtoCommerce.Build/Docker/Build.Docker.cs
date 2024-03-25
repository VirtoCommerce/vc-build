using Nuke.Common;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities;
using Serilog;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("Connection String")] public static string DockerConnectionString { get; set; }

        private static string _dockerUsername = string.Empty;
        [Parameter("Docker Username")]
        public static string DockerUsername
        {
            get
            {
                return _dockerUsername;
            }
            set
            {
                _dockerUsername = value?.ToLowerInvariant();
            }
        }

        [Parameter("Docker Password")] public static string DockerPassword { get; set; }
        [Parameter("Docker Registry Url")] public static string DockerRegistryUrl { get; set; }
        [Parameter("Docker Image Name")] public static string DockerImageName { get; set; }
        [Parameter("Docker Image Tag")] public static string DockerImageTag { get; set; }
        [Parameter("Dockerfile Path")] public static string DockerfilePath { get; set; }
        [Parameter("Docker build context path")] public static string DockerBuildContextPath { get; set; }

        private static string DockerImageFullName => string.IsNullOrEmpty(DockerImageTag) ? DockerImageName : DockerImageName.Append($":{DockerImageTag}");

        public static bool DockerCredentialsPassed => !string.IsNullOrEmpty(DockerUsername) && !string.IsNullOrEmpty(DockerPassword);
        Target DockerLogin => _ => _
        .Before(BuildImage, PushImage)
        .OnlyWhenDynamic(() => DockerCredentialsPassed)
        .Executes(() =>
        {
            DockerTasks.DockerLogger = (_, m) => Log.Debug(m);

            var settings = new DockerLoginSettings()
                .SetServer(DockerRegistryUrl)
                .SetUsername(DockerUsername)
                .SetPassword(DockerPassword);
            DockerTasks.DockerLogin(settings);
        });

        Target BuildImage => _ => _
        .Before(PushImage)
        .Executes(() =>
        {

            var settings = new DockerBuildSettings()
                .SetFile(DockerfilePath)
                .SetPull(true)
                .SetPath(DockerBuildContextPath ?? RootDirectory)
                .SetTag(DockerImageFullName);
            DockerTasks.DockerBuild(settings);
        });

        Target PushImage => _ => _
        .DependsOn(DockerLogin)
        .Executes(() =>
        {
            var settings = new DockerImagePushSettings()
                .SetName(DockerImageFullName);
            DockerTasks.DockerImagePush(settings);
        });

        public Target BuildAndPush => _ => _
        .Before(CloudInit)
        .DependsOn(DockerLogin, BuildImage, PushImage);
    }
}
