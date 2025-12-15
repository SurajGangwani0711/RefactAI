using System.Text.Json;

namespace RefactAI.Common
{
    public static class RepoLoader
    {
        // Looks for repo.json inside the API project folder
        private static readonly string RepoPath =
            Path.Combine(Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.FullName,
                         "config", "Repo.json");
        public static string? GetRepoUrl()
        {
            
            var repo_url = Environment.GetEnvironmentVariable("REPO_URL");

            if (string.IsNullOrWhiteSpace(repo_url))
                throw new Exception("❌ GITHUB_REPO_URL not set. Use the React UI to set it.");

            return repo_url;
        }

        public static void SaveRepoUrl(string repoUrl)
        {
            var obj = new RepoFile { REPO_URL = repoUrl };

            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(RepoPath, json);
        }

        private class RepoFile
        {
            public string? REPO_URL { get; set; }
        }
    }
}
