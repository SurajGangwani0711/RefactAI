using Orleans;
using Microsoft.Extensions.Logging;
using RefactAI.Orleans.Contracts;

namespace RefactAI.Orleans.Grains
{
    public class RepoGrain : Grain, IRepoGrain
    {
        private readonly ILogger<RepoGrain> _logger;

        public RepoGrain(ILogger<RepoGrain> logger)
        {
            _logger = logger;
        }

        // public override Task OnActivateAsync()
        // {
        //     _logger.LogInformation($"RepoGrain Activated: {this.GetPrimaryKeyString()}");
        //     return base.OnActivateAsync();
        // }

        public async Task EnqueueWork(RepoWorkItem item)
        {
            _logger.LogInformation($"EnqueueWork {item.RepoUrl} {item.Kind}");

            // Normalize repo URL
            string normalizedRepo = NormalizeRepoUrl(item.RepoUrl);

            // SHA fallback → HEAD
            string sha = string.IsNullOrWhiteSpace(item.Sha) ? "HEAD" : item.Sha;

            // PR grain is keyed by repo URL
            var prGrain = GrainFactory.GetGrain<IPrGrain>(normalizedRepo);

            // Correct PrRequest — ONLY use fields that exist in Orleans.Contracts PrRequest
            var request = new PrRequest
            (
                RepoUrl: normalizedRepo,
                Sha: sha,
                PrNumber: item.Kind   // Tag: PR / Push / Manual
            );

            // Call grain
            await prGrain.ProcessPr(request);   // returns Task<PrResult>
        }


        private static string NormalizeRepoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Remove trailing slashes
            url = url.Trim().TrimEnd('/');

            // Remove /tree/main or /blob/main paths
            if (url.Contains("/tree/"))
                url = url[..url.IndexOf("/tree/", StringComparison.Ordinal)];

            if (url.Contains("/blob/"))
                url = url[..url.IndexOf("/blob/", StringComparison.Ordinal)];

            return url;
        }
    }
}
