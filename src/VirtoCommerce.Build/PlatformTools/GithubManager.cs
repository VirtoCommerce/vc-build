using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace PlatformTools
{
    internal static class GithubManager
    {
        private static readonly string _githubUser = "virtocommerce";
        private static readonly string _platformRepo = "vc-platform";
        private static readonly GitHubClient _client = new GitHubClient(new ProductHeaderValue("vc-build"));

        public static Task<Release> GetPlatformRelease(string releaseTag)
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
                PageSize = 5,
                PageCount = 1,
            });

            var release = releases.OrderByDescending(r => new Version(r.TagName.Trim())).FirstOrDefault();
            return release;
        }

        /// <summary>
        ///     Gets a repo owner and a repo name from packageUrl
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The First Value is Owner, The Second is Repo Name</returns>
        public static Tuple<string, string> GetRepoFromUrl(string url)
        {
            var regex = new Regex(@"http[s]{0,1}:\/\/github.com\/([A-z0-9]*)\/([A-z0-9\-]*)\/", RegexOptions.IgnoreCase);
            var match = regex.Match(url);
            var groups = match.Groups;
            return new Tuple<string, string>(groups[1].Value, groups[2].Value);
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
    }
}
