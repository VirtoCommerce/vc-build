using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Nuke.Common.IO;
using Octokit;
using VirtoCommerce.Platform.Core.Common;

namespace PlatformTools
{
    internal static partial class GithubReleaseService
    {
        private static readonly string _githubUser = "virtocommerce";
        private static readonly string _platformRepo = "vc-platform";
        private static readonly GitHubClient _client = new GitHubClient(new ProductHeaderValue("vc-build"));

        public static Task<Release> GetPlatformRelease(string releaseTag = null)
        {
            return string.IsNullOrEmpty(releaseTag)
                ? GetLatestReleaseAsync(_githubUser, _platformRepo)
                : _client.Repository.Release.Get(_githubUser, _platformRepo, releaseTag);
        }

        public static Task<Release> GetPlatformRelease(string token, string releaseTag)
        {
            SetAuthToken(token);
            return GetPlatformRelease(releaseTag);
        }

        public static void SetAuthToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.Credentials = new Credentials(token);
            }
        }

        private static async Task<Release> GetLatestReleaseAsync(string repoUser, string repoName)
        {
            var releases = await _client.Repository.Release.GetAll(repoUser, repoName, new ApiOptions
            {
                PageSize = 50,
                PageCount = 1,
            });

            var release = releases.Where(r => !r.Prerelease).OrderByDescending(r => new Version(r.TagName.Trim())).FirstOrDefault();
            return release;
        }

        /// <summary>
        ///     Gets a repo owner and a repo name from packageUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The First Value is Owner, The Second is Repo Name</returns>
        public static Tuple<string, string> GetRepoFromUrl(string url)
        {
            var regex = RepoUrlRegex();
            var match = regex.Match(url);
            var groups = match.Groups;
            const int repoOwnerGroupIndex = 1;
            const int repoNameGroupIndex = 2;
            return new Tuple<string, string>(groups[repoOwnerGroupIndex].Value, groups[repoNameGroupIndex].Value);
        }

        public static Task<Release> GetModuleRelease(string token, string moduleRepo, string releaseTag)
        {
            SetAuthToken(token);
            return GetModuleRelease(moduleRepo, releaseTag);
        }

        public static Task<Release> GetModuleRelease(string moduleRepo, string releaseTag)
        {
            return string.IsNullOrEmpty(releaseTag)
                ? _client.Repository.Release.GetLatest(_githubUser, moduleRepo)
                : _client.Repository.Release.Get(_githubUser, moduleRepo, releaseTag);
        }

        internal static async Task<SemanticVersion> GetLatestPlatformVersion()
        {
            const string platformBuildPropsUrl = "https://raw.githubusercontent.com/VirtoCommerce/vc-platform/refs/heads/master/Directory.Build.props";
            var response = await HttpTasks.HttpDownloadStringAsync(platformBuildPropsUrl);
            var version = ParseVersionFromProps(response);

            if (version.IsNullOrEmpty())
            {
                var platformRelease = await GetPlatformRelease();
                version = platformRelease.TagName;
            }

            return SemanticVersion.Parse(version);
        }

        private static string ParseVersionFromProps(string rawXml)
        {
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };
            xmlDocument.LoadXml(rawXml);

            var prefixNode = xmlDocument.GetElementsByTagName("VersionPrefix")[0];

            return prefixNode?.InnerText;
        }

        [GeneratedRegex(@"http[s]{0,1}:\/\/github.com\/([A-z0-9]*)\/([A-z0-9\-]*)\/", RegexOptions.IgnoreCase, "ru-RU")]
        private static partial Regex RepoUrlRegex();
    }
}
