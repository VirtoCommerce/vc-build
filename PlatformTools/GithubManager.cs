using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace PlatformTools
{
    internal class GithubManager
    {
        private static readonly string _githubUser = "virtocommerce";
        private static readonly string _platformRepo = "vc-platform";
        private static readonly GitHubClient _client = new GitHubClient(new ProductHeaderValue("vc-build"));

        public static async Task<Release> GetPlatformRelease(string releaseTag)
        {
            var release = string.IsNullOrEmpty(releaseTag)
                ? await GetLatestReleaseAsync(_githubUser, _platformRepo)
                : await _client.Repository.Release.Get(_githubUser, _platformRepo, releaseTag);

            return release;
        }

        public static async Task<Release> GetPlatformRelease(string token, string releaseTag)
        {
            SetAuthToken(token);
            return await GetPlatformRelease(releaseTag);
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

            var release = releases.OrderByDescending(r => r.TagName.Trim()).FirstOrDefault();
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

        public static async Task<Release> GetModuleRelease(string token, string moduleRepo, string releaseTag)
        {
            SetAuthToken(token);
            return await GetModuleRelease(moduleRepo, releaseTag);
        }

        public static async Task<Release> GetModuleRelease(string moduleRepo, string releaseTag)
        {
            Release release;

            if (string.IsNullOrEmpty(releaseTag))
            {
                release = await _client.Repository.Release.GetLatest(_githubUser, moduleRepo);
            }
            else
            {
                release = await _client.Repository.Release.Get(_githubUser, moduleRepo, releaseTag);
            }

            return release;
        }
    }
}
