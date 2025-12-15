using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RefactAI.Common;

namespace RefactAI.Orleans.Grains
{
    public interface IGitHubService
    {
        Task<string> CreatePullRequest(string repoUrl, string headBranch, string baseBranch, string title, string body);
    }

    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _http;

            public GitHubService(HttpClient http)
            {
                _http = http;

                _http.DefaultRequestHeaders.UserAgent.ParseAdd("RefactAI-Bot");

                string? token = GlobalTokenStore.Load();

                if (string.IsNullOrWhiteSpace(token))
                    throw new InvalidOperationException("GITHUB_TOKEN not set");

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("token", token);
            }

        private (string owner, string repo) ParseOwnerRepo(string repoUrl)
        {
            string? url = GlobalRepoStore.Load();  // this returns a string

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Repository URL cannot be null.");

            var uri = new Uri(url); // 👉 FIX: convert to Uri

            var parts = uri.AbsolutePath.Trim('/').Split('/');

            if (parts.Length < 2)
                throw new ArgumentException("Invalid repo url");

            return (parts[0], parts[1].Replace(".git", ""));
        }


        public async Task<string> CreatePullRequest(string repoUrl, string headBranch, string baseBranch, string title, string body)
        {
            var (owner, repo) = ParseOwnerRepo(repoUrl);
            var endpoint = $"https://api.github.com/repos/{owner}/{repo}/pulls";

            var payload = new
            {
                title,
                head = headBranch,
                @base = baseBranch,
                body
            };

            var resp = await _http.PostAsJsonAsync(endpoint, payload);
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Failed to create PR: {resp.StatusCode} {text}");

            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.TryGetProperty("html_url", out var u))
                return u.GetString() ?? string.Empty;

            return text;
        }
    }
}
