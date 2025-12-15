using Orleans;
using Microsoft.Extensions.Logging;
using RefactAI.Orleans.Contracts;
using RefactAI.Common.Runner;
using RefactAI.Refactor;

namespace RefactAI.Orleans.Grains
{
    public class PrGrain : Grain, IPrGrain
    {
        private readonly ILogger<PrGrain> _logger;
        private readonly IDotnetRunner _runner;
        private readonly IRefactorService _refactor;
        private readonly IGitHubService _gh;

        private const string OutputBasePath = "/Users/suraj/RefactAI_Output/";

        public PrGrain(
            ILogger<PrGrain> logger,
            IDotnetRunner runner,
            IRefactorService refactor,
            IGitHubService github)
        {
            _logger = logger;
            _runner = runner;
            _refactor = refactor;
            _gh = github;
        }

        public Task<PrResult> ProcessPr(PrRequest request)
        {
            RegisterTimer(
                async _ => await ProcessInternalAsync(request),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(-1)
            );

            return Task.FromResult(new PrResult("Started", $"PR job launched for {request.RepoUrl}"));
        }

        private async Task ProcessInternalAsync(PrRequest request)
        {
            try
            {
                _logger.LogInformation($"CLONING: {request.RepoUrl} @ {request.Sha}");
                var clonePath = await _runner.CloneRepo(request.RepoUrl, request.Sha);
                _logger.LogInformation($"CLONED TO: {clonePath}");

                var files = await _runner.GetAllFiles(clonePath);
                _logger.LogInformation($"FOUND {files.Count} files");

                foreach (var file in files)
                {
                    try
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        string lang = Detect(ext);

                        if (!Supports(lang))
                            continue;

                        string code = await File.ReadAllTextAsync(file);
                        string updated = await _refactor.RefactorAsync(code, lang);
                        await File.WriteAllTextAsync(file, updated);
                        _logger.LogInformation($"Refactored: {file}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error processing {file}");
                    }
                }

                // commit & push
                string prBranch = $"refact-ai-{DateTime.UtcNow:yyyyMMddHHmmss}";
                string commitMessage = "Automated refactor by RefactAI";

                try
                {
                    await _runner.CommitAllAndPush(clonePath, prBranch, commitMessage, baseBranch: "main");

                    // create PR
                    string prTitle = $"RefactorAI automated refactor ({prBranch})";
                    string prBody = $"Automated refactor produced by RefactAI at {DateTime.UtcNow:O}";
                    string prUrl = await _gh.CreatePullRequest(request.RepoUrl, prBranch, "main", prTitle, prBody);

                    _logger.LogInformation($"PR created: {prUrl}");

                    // Optionally also save output locally
                    var output = Path.Combine(OutputBasePath, ExtractRepoName(request.RepoUrl), request.Sha);
                    if (Directory.Exists(clonePath))
                    {
                        CopyDirectory(clonePath, output);
                        _logger.LogInformation($"Saved output to {output}");
                    }

                    // You could set a grain state or call another grain with final result
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to commit/push or create PR");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected failure in ProcessInternalAsync");
            }
        }

        private string ExtractRepoName(string repoUrl)
        {
            string name = Path.GetFileNameWithoutExtension(repoUrl);
            return name.Replace(".git", "");
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir)) return;
            Directory.CreateDirectory(destDir);
            foreach (var f in Directory.GetFiles(sourceDir))
            {
                File.Copy(f, Path.Combine(destDir, Path.GetFileName(f)), true);
            }
            foreach (var d in Directory.GetDirectories(sourceDir))
            {
                var subDest = Path.Combine(destDir, Path.GetFileName(d));
                CopyDirectory(d, subDest);
            }
        }

        private string Detect(string ext) => ext switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".java" => "java",
            ".py" => "python",
            ".c" => "c",
            ".cpp" => "cpp",
            ".h" => "header",
            _ => "unknown"
        };

        private bool Supports(string lang) =>
            lang is "csharp" or "javascript" or "java" or "python" or "c" or "cpp";
    }
}
