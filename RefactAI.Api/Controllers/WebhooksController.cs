using Microsoft.AspNetCore.Mvc;
using Orleans;
using RefactAI.Common;
using RefactAI.Orleans.Contracts;
using System.Text.Json;

namespace RefactAI.Api.Controllers
{
    [ApiController]
    [Route("webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly IClusterClient _cluster;

        public WebhooksController(IClusterClient cluster)
        {
            _cluster = cluster;
        }

        // ------------------------------------------------------------
        // TEST ENDPOINT
        // ------------------------------------------------------------
        // Example:
        //   http://localhost:5121/webhooks/run?repo=https://github.com/dotnet/runtime
        //
        [HttpGet("run")]
        public async Task<IActionResult> Run([FromQuery] string repo)
        {
            if (string.IsNullOrWhiteSpace(repo))
                return BadRequest("Missing repo URL");

            Console.WriteLine("ENV GITHUB_TOKEN = " + GlobalTokenStore.Load());
            Console.WriteLine("ENV REPO_URL = " + Environment.GetEnvironmentVariable("REPO_URL"));

            Console.WriteLine("TOken is",GlobalTokenStore.Load());
            Console.WriteLine("Repo is",GlobalRepoStore.Load());
            string normalized = NormalizeRepoUrl(repo);

            var grain = _cluster.GetGrain<IRepoGrain>(normalized);

            await grain.EnqueueWork(new RepoWorkItem(
                RepoUrl: normalized,
                Branch: "main",
                Sha: "HEAD",
                Kind: "PR"
            ));

            return Ok($"Refactor started for {normalized}");
        }

        // ------------------------------------------------------------
        // GITHUB WEBHOOK (optional future use)
        // ------------------------------------------------------------
        [HttpPost("github")]
        public async Task<IActionResult> GitHubWebhook()
        {
            using var sr = new StreamReader(Request.Body);
            var body = await sr.ReadToEndAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            string repoUrl = root.GetProperty("repository").GetProperty("clone_url").GetString()!;
            string branch = root.GetProperty("ref").GetString()?.Replace("refs/heads/", "") ?? "main";
            string sha = root.GetProperty("after").GetString() ?? "HEAD";

            string normalized = NormalizeRepoUrl(GlobalRepoStore.Load());

            var grain = _cluster.GetGrain<IRepoGrain>(normalized);

            await grain.EnqueueWork(new RepoWorkItem(
                RepoUrl: normalized,
                Branch: branch,
                Sha: sha,
                Kind: "Push"
            ));

            return Ok("OK");
        }

        // ------------------------------------------------------------
        // Normalize GitHub URL (PREVENTS YOUR ERRORS)
        // ------------------------------------------------------------
        private static string NormalizeRepoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = url.Trim().TrimEnd('/');

            // Remove blob or tree paths (these break git clone)
            if (url.Contains("/blob/"))
                url = url[..url.IndexOf("/blob/")];

            if (url.Contains("/tree/"))
                url = url[..url.IndexOf("/tree/")];

            return url;
        }
    }
}
